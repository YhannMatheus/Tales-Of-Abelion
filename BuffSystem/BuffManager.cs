using UnityEngine;
using System.Collections.Generic;

// Gerenciador de buffs/debuffs temporários
// Este sistema permite aplicar modificadores que duram por um tempo determinado
public class BuffManager : MonoBehaviour
{
    [System.Serializable]
    public class ActiveBuff
    {
        public string buffName;
        public float duration;
        public float remainingTime;
        public List<Modifier> modifiers;
        public bool isDebuff;

        public ActiveBuff(string name, float duration, List<Modifier> mods, bool debuff = false)
        {
            buffName = name;
            this.duration = duration;
            remainingTime = duration;
            modifiers = new List<Modifier>(mods);
            isDebuff = debuff;
        }
    }

    [Header("References")]
    [SerializeField] private CharacterManager CharacterManager;

    [Header("Active Buffs/Debuffs")]
    [SerializeField] private List<ActiveBuff> activeBuffs = new List<ActiveBuff>();
    // Novo: slots de buffs ativos para gerenciamento stackável e compatível com UI
    [SerializeField] private List<BuffSlot> buffSlots = new List<BuffSlot>();
    private class DistributedModifier
    {
        public Modifier original;
        public float totalAbsoluteValue;
        public float appliedSoFar = 0f;
        public int appliedRounded = 0; 
    }

    private class DistributedBuff
    {
        public string buffName;
        public float duration;
        public float remainingTime;
        public float tickInterval;
        public List<DistributedModifier> mods = new List<DistributedModifier>();

        public DistributedBuff(string name, float duration, float tickInterval)
        {
            this.buffName = name;
            this.duration = duration;
            this.remainingTime = duration;
            this.tickInterval = tickInterval;
        }
    }

    private List<DistributedBuff> distributedBuffs = new List<DistributedBuff>();

    private void Update()
    {
        UpdateBuffs();
        UpdateDistributedBuffs(Time.deltaTime);
        UpdateSlots(Time.deltaTime);
    }

    void UpdateSlots(float dt)
    {
        if (buffSlots == null || buffSlots.Count == 0) return;
        for (int i = buffSlots.Count - 1; i >= 0; i--)
        {
            var slot = buffSlots[i];
            if (slot == null) continue;
            slot.Tick(dt);
        }
    }

    private void UpdateDistributedBuffs(float deltaTime)
    {
        if (distributedBuffs.Count == 0) return;

        for (int i = distributedBuffs.Count - 1; i >= 0; i--)
        {
            var db = distributedBuffs[i];
            float dt = Mathf.Min(deltaTime, db.remainingTime);

            for (int m = 0; m < db.mods.Count; m++)
            {
                var dmod = db.mods[m];
                if (db.duration <= 0f) continue;
                float delta = dmod.totalAbsoluteValue * (dt / db.duration);

                ApplyModifierDelta(dmod.original, delta, dmod);
            }

            db.remainingTime -= dt;

            if (db.remainingTime <= 0f)
            {
                for (int m = 0; m < db.mods.Count; m++)
                {
                    var dmod = db.mods[m];
                    float residual = dmod.totalAbsoluteValue - dmod.appliedSoFar;
                    if (Mathf.Abs(residual) > 0.0001f)
                    {
                        ApplyModifierDelta(dmod.original, residual, db.mods[m]);
                        db.mods[m].appliedSoFar += residual;
                    }
                }

                for (int m = 0; m < db.mods.Count; m++)
                {
                    RevertAppliedModifier(db.mods[m].original, db.mods[m]);
                }

                distributedBuffs.RemoveAt(i);
            }
        }
    }

    private void ApplyModifierDelta(Modifier mod, float deltaAbsolute, DistributedModifier state)
    {
        CharacterData data = CharacterManager.Data;

        switch (mod.variable)
        {
            case ModifierVar.physicalDamage:
                state.appliedSoFar += deltaAbsolute;
                data.externalPhysicalDamageBonus += deltaAbsolute;
                break;
            case ModifierVar.physicalResistence:
                state.appliedSoFar += deltaAbsolute;
                data.externalPhysicalResistenceBonus += deltaAbsolute;
                break;
            case ModifierVar.magicalDamage:
                state.appliedSoFar += deltaAbsolute;
                data.externalMagicalDamageBonus += deltaAbsolute;
                break;
            case ModifierVar.magicalResistence:
                state.appliedSoFar += deltaAbsolute;
                data.externalMagicalResistenceBonus += deltaAbsolute;
                break;
            case ModifierVar.maxHealth:
                state.appliedSoFar += deltaAbsolute;
                int newRounded = Mathf.RoundToInt(state.appliedSoFar);
                int deltaInt = newRounded - state.appliedRounded;
                if (deltaInt != 0)
                {
                    data.externalMaxHealthBonus += deltaInt;
                    state.appliedRounded = newRounded;
                }
                break;
            case ModifierVar.maxEnergy:
                state.appliedSoFar += deltaAbsolute;
                newRounded = Mathf.RoundToInt(state.appliedSoFar);
                deltaInt = newRounded - state.appliedRounded;
                if (deltaInt != 0)
                {
                    data.externalMaxEnergyBonus += deltaInt;
                    state.appliedRounded = newRounded;
                }
                break;
            case ModifierVar.criticalChance:
                state.appliedSoFar += deltaAbsolute;
                data.externalCriticalChanceBonus += deltaAbsolute;
                break;
            case ModifierVar.criticalDamage:
                state.appliedSoFar += deltaAbsolute;
                data.externalCriticalDamageBonus += deltaAbsolute;
                break;
            case ModifierVar.speed:
                state.appliedSoFar += deltaAbsolute;
                data.externalSpeedBonus += deltaAbsolute;
                break;
            case ModifierVar.attackSpeed:
                state.appliedSoFar += deltaAbsolute;
                data.externalAttackSpeedBonus += deltaAbsolute;
                break;
            case ModifierVar.luck:
                state.appliedSoFar += deltaAbsolute;
                data.externalLuckBonus += deltaAbsolute;
                break;
            case ModifierVar.healthRegen:
                state.appliedSoFar += deltaAbsolute;
                newRounded = Mathf.RoundToInt(state.appliedSoFar);
                deltaInt = newRounded - state.appliedRounded;
                if (deltaInt != 0)
                {
                    data.externalHealthRegenBonus += deltaInt;
                    state.appliedRounded = newRounded;
                }
                break;
            default:
                data.externalPhysicalDamageBonus += deltaAbsolute;
                break;
        }
    }

    private void RevertAppliedModifier(Modifier mod, DistributedModifier state)
    {
        CharacterData data = CharacterManager.Data;
        switch (mod.variable)
        {
            case ModifierVar.physicalDamage:
                data.externalPhysicalDamageBonus -= state.appliedSoFar;
                break;
            case ModifierVar.physicalResistence:
                data.externalPhysicalResistenceBonus -= state.appliedSoFar;
                break;
            case ModifierVar.magicalDamage:
                data.externalMagicalDamageBonus -= state.appliedSoFar;
                break;
            case ModifierVar.magicalResistence:
                data.externalMagicalResistenceBonus -= state.appliedSoFar;
                break;
            case ModifierVar.maxHealth:
                data.externalMaxHealthBonus -= state.appliedRounded;
                break;
            case ModifierVar.maxEnergy:
                data.externalMaxEnergyBonus -= state.appliedRounded;
                break;
            case ModifierVar.criticalChance:
                data.externalCriticalChanceBonus -= state.appliedSoFar;
                break;
            case ModifierVar.criticalDamage:
                data.externalCriticalDamageBonus -= state.appliedSoFar;
                break;
            case ModifierVar.speed:
                data.externalSpeedBonus -= state.appliedSoFar;
                break;
            case ModifierVar.attackSpeed:
                data.externalAttackSpeedBonus -= state.appliedSoFar;
                break;
            case ModifierVar.luck:
                data.externalLuckBonus -= state.appliedSoFar;
                break;
            case ModifierVar.healthRegen:
                data.externalHealthRegenBonus -= state.appliedRounded;
                break;
            default:
                data.externalPhysicalDamageBonus -= state.appliedSoFar;
                break;
        }
    }

    // Atualiza todos os buffs ativos e remove os expirados
    private void UpdateBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].remainingTime -= Time.deltaTime;

            if (activeBuffs[i].remainingTime <= 0)
            {
                RemoveBuff(activeBuffs[i]);
                activeBuffs.RemoveAt(i);
            }
        }
    }

    // Aplica um buff/debuff temporário
    public void ApplyBuff(string buffName, float duration, List<Modifier> modifiers, bool isDebuff = false, bool distributeOverDuration = false, float tickInterval = 1f)
    {
        if (distributeOverDuration && duration > 0f)
        {
            var existing = distributedBuffs.Find(b => b.buffName == buffName);
            if (existing != null)
            {
                existing.remainingTime = duration;
                return;
            }
            var db = new DistributedBuff(buffName, duration, tickInterval);
            foreach (var mod in modifiers)
            {
                DistributedModifier dm = new DistributedModifier();
                dm.original = mod;
                dm.totalAbsoluteValue = CalculateModifierValue(mod);
                dm.appliedSoFar = 0f;
                dm.appliedRounded = 0;
                db.mods.Add(dm);
            }
            distributedBuffs.Add(db);
            return;
        }

        // Preferir gerenciamento por slots quando possível: tentar encontrar um slot correspondente
        var existingSlot = buffSlots.Find(s => s.buffName == buffName);
        if (existingSlot != null)
        {
            // Mesclar: renovar duração e adicionar um stack se permitido
            existingSlot.remainingTime = duration;
            existingSlot.AddStack(1, true);
            // Reaplicar apenas o delta: remover aplicação prévia e aplicar a nova
            RemoveModifiers(modifiers); // remove qualquer aplicação anterior por segurança
            ModifyModifiers(existingSlot.GetEffectiveModifiers(), 1);
            return;
        }

        // fallback to ActiveBuff behavior for non-slot buffs
        ActiveBuff existingBuff = activeBuffs.Find(b => b.buffName == buffName);
        if (existingBuff != null)
        {
            existingBuff.remainingTime = duration;
            return;
        }

        ActiveBuff newBuff = new ActiveBuff(buffName, duration, modifiers, isDebuff);
        activeBuffs.Add(newBuff);

        ModifyModifiers(modifiers, 1);
    }

    // Nova sobrecarga: aplica um BuffData (ScriptableObject) que pode opcionalmente distribuir o efeito ao longo do tempo
    // ❌ REMOVIDO: ApplyBuff(BuffData) - BuffData não tem mais duration/modifiers
    // 📌 ARQUITETURA: BuffData é APENAS visual/áudio
    // 📌 VALORES vêm do SkillData via BuffEffect.Execute()
    // 📌 Use: ApplyBuff(string buffName, float duration, List<Modifier> modifiers, bool isDebuff, bool distributeOverTime, float tickInterval)

    // Aplica ou reverte um conjunto de modificadores ao personagem
    /// <summary>
    /// Aplica ou reverte um conjunto de modificadores. Use multiplier = 1 para aplicar, -1 para reverter.
    /// Centraliza a aplicação dos modificadores para reduzir duplicação e alocação desnecessária.
    /// </summary>
    private void ModifyModifiers(List<Modifier> modifiers, int multiplier = 1)
    {
        if (modifiers == null || modifiers.Count == 0) return;
        CharacterData data = CharacterManager.Data;
        foreach (Modifier mod in modifiers)
        {
            float value = CalculateModifierValue(mod) * multiplier;
            switch (mod.variable)
            {
                case ModifierVar.physicalDamage:
                    data.externalPhysicalDamageBonus += value;
                    break;
                case ModifierVar.physicalResistence:
                    data.externalPhysicalResistenceBonus += value;
                    break;
                case ModifierVar.magicalDamage:
                    data.externalMagicalDamageBonus += value;
                    break;
                case ModifierVar.magicalResistence:
                    data.externalMagicalResistenceBonus += value;
                    break;
                case ModifierVar.maxHealth:
                    data.externalMaxHealthBonus += Mathf.RoundToInt(value);
                    break;
                case ModifierVar.maxEnergy:
                    data.externalMaxEnergyBonus += Mathf.RoundToInt(value);
                    break;
                case ModifierVar.criticalChance:
                    data.externalCriticalChanceBonus += value;
                    break;
                case ModifierVar.criticalDamage:
                    data.externalCriticalDamageBonus += value;
                    break;
                case ModifierVar.speed:
                    data.externalSpeedBonus += value;
                    break;
                case ModifierVar.attackSpeed:
                    data.externalAttackSpeedBonus += value;
                    break;
                case ModifierVar.luck:
                    data.externalLuckBonus += value;
                    break;
                case ModifierVar.healthRegen:
                    data.externalHealthRegenBonus += Mathf.RoundToInt(value);
                    break;
            }
        }
    }

    // Remove (reverte) uma lista de modificadores previamente aplicados
    private void RemoveModifiers(List<Modifier> modifiers)
    {
        if (modifiers == null) return;
        CharacterData data = CharacterManager.Data;
        foreach (Modifier mod in modifiers)
        {
            float value = CalculateModifierValue(mod);

            switch (mod.variable)
            {
                case ModifierVar.physicalDamage:
                    data.externalPhysicalDamageBonus -= value;
                    break;
                case ModifierVar.physicalResistence:
                    data.externalPhysicalResistenceBonus -= value;
                    break;
                case ModifierVar.magicalDamage:
                    data.externalMagicalDamageBonus -= value;
                    break;
                case ModifierVar.magicalResistence:
                    data.externalMagicalResistenceBonus -= value;
                    break;
                case ModifierVar.maxHealth:
                    data.externalMaxHealthBonus -= Mathf.RoundToInt(value);
                    break;
                case ModifierVar.maxEnergy:
                    data.externalMaxEnergyBonus -= Mathf.RoundToInt(value);
                    break;
                case ModifierVar.criticalChance:
                    data.externalCriticalChanceBonus -= value;
                    break;
                case ModifierVar.criticalDamage:
                    data.externalCriticalDamageBonus -= value;
                    break;
                case ModifierVar.speed:
                    data.externalSpeedBonus -= value;
                    break;
                case ModifierVar.attackSpeed:
                    data.externalAttackSpeedBonus -= value;
                    break;
                case ModifierVar.luck:
                    data.externalLuckBonus -= value;
                    break;
                case ModifierVar.healthRegen:
                    data.externalHealthRegenBonus -= Mathf.RoundToInt(value);
                    break;
            }
        }
    }

    // Remove os modificadores de um buff
    private void RemoveBuff(ActiveBuff buff)
    {
        CharacterData data = CharacterManager.Data;

        foreach (Modifier mod in buff.modifiers)
        {
            float value = CalculateModifierValue(mod);

            switch (mod.variable)
            {
                case ModifierVar.physicalDamage:
                    data.externalPhysicalDamageBonus -= value;
                    break;
                case ModifierVar.physicalResistence:
                    data.externalPhysicalResistenceBonus -= value;
                    break;
                case ModifierVar.magicalDamage:
                    data.externalMagicalDamageBonus -= value;
                    break;
                case ModifierVar.magicalResistence:
                    data.externalMagicalResistenceBonus -= value;
                    break;
                case ModifierVar.maxHealth:
                    data.externalMaxHealthBonus -= Mathf.RoundToInt(value);
                    break;
                case ModifierVar.maxEnergy:
                    data.externalMaxEnergyBonus -= Mathf.RoundToInt(value);
                    break;
                case ModifierVar.criticalChance:
                    data.externalCriticalChanceBonus -= value;
                    break;
                case ModifierVar.criticalDamage:
                    data.externalCriticalDamageBonus -= value;
                    break;
                case ModifierVar.speed:
                    data.externalSpeedBonus -= value;
                    break;
                case ModifierVar.attackSpeed:
                    data.externalAttackSpeedBonus -= value;
                    break;
                case ModifierVar.luck:
                    data.externalLuckBonus -= value;
                    break;
                case ModifierVar.healthRegen:
                    data.externalHealthRegenBonus -= Mathf.RoundToInt(value);
                    break;
            }
        }
    }

    // Auxiliares para gerenciamento de slots
    public event System.Action<BuffSlot> OnBuffSlotAdded;
    public event System.Action<BuffSlot> OnBuffSlotRemoved;
    public event System.Action<BuffSlot> OnBuffSlotUpdated;

    public BuffSlot CreateSlotFromBuffData(BuffData buffData, GameObject source = null)
    {
        if (buffData == null) return null;
        var slot = new BuffSlot();
        slot.InitializeFromBuffData(buffData, source);
    // configura padrões simples de stack (se o BuffData indicar stackable futuramente)
        slot.SetStackConfig(1, 1);
        slot.OnExpired += Slot_OnExpired;
        slot.OnStackChanged += Slot_OnStackChanged;
        buffSlots.Add(slot);
        // aplicar modificadores imediatamente (aplica delta a partir de zero)
        ModifyModifiers(slot.GetStackDeltaModifiersAndConsume(), 1);
        OnBuffSlotAdded?.Invoke(slot);
        return slot;
    }

    private void Slot_OnExpired(BuffSlot slot)
    {
        if (slot == null) return;
        // reverte apenas os modificadores que foram aplicados por este slot
        ModifyModifiers(slot.GetCurrentlyAppliedModifiers(), -1);
        buffSlots.Remove(slot);
        OnBuffSlotRemoved?.Invoke(slot);
    }

    private void Slot_OnStackChanged(BuffSlot slot)
    {
        if (slot == null) return;
        // aplicar apenas o delta produzido pelo slot (adição de stacks)
        var delta = slot.GetStackDeltaModifiersAndConsume();
        if (delta != null && delta.Count > 0)
        {
            // delta contém os valores a aplicar (positivos para stacks adicionados)
            ModifyModifiers(delta, 1);
        }
        OnBuffSlotUpdated?.Invoke(slot);
    }

    public BuffSlot GetSlotByName(string name)
    {
        return buffSlots.Find(s => s != null && s.buffName == name);
    }

    // Calcula o valor final do modificador baseado no tipo
    private float CalculateModifierValue(Modifier mod)
    {
        CharacterData data = CharacterManager.Data;

        switch (mod.type)
        {
            case ModifierType.Flat:
                return mod.value;

            case ModifierType.PercentAdd:
                return GetBaseValue(mod.variable) * (mod.value / 100f);

            case ModifierType.PercentMult:
                return GetCurrentValue(mod.variable) * (mod.value / 100f);

            default:
                return mod.value;
        }
    }

    // Obtém o valor base de uma stat (sem modificadores de equipamento/externos)
    private float GetBaseValue(ModifierVar variable)
    {
        CharacterData data = CharacterManager.Data;

        switch (variable)
        {
            case ModifierVar.physicalDamage: return data.physicalDamage;
            case ModifierVar.physicalResistence: return data.physicalResistence;
            case ModifierVar.magicalDamage: return data.magicalDamage;
            case ModifierVar.magicalResistence: return data.magicalResistence;
            case ModifierVar.maxHealth: return data.maxHealth;
            case ModifierVar.maxEnergy: return data.maxEnergy;
            case ModifierVar.speed: return data.speed;
            case ModifierVar.attackSpeed: return data.attackSpeed;
            case ModifierVar.criticalChance: return data.criticalChance;
            case ModifierVar.criticalDamage: return data.criticalDamage;
            case ModifierVar.luck: return data.luck;
            case ModifierVar.healthRegen: return data.healthRegen;
            default: return 0f;
        }
    }

    // Obtém o valor total atual de uma stat (com todos os modificadores)
    private float GetCurrentValue(ModifierVar variable)
    {
        CharacterData data = CharacterManager.Data;

        switch (variable)
        {
            case ModifierVar.physicalDamage: return data.TotalPhysicalDamage;
            case ModifierVar.physicalResistence: return data.TotalPhysicalResistance;
            case ModifierVar.magicalDamage: return data.TotalMagicalDamage;
            case ModifierVar.magicalResistence: return data.TotalMagicalResistance;
            case ModifierVar.maxHealth: return data.TotalMaxHealth;
            case ModifierVar.maxEnergy: return data.TotalMaxEnergy;
            case ModifierVar.speed: return data.TotalSpeed;
            case ModifierVar.attackSpeed: return data.TotalAttackSpeed;
            case ModifierVar.criticalChance: return data.TotalCriticalChance;
            case ModifierVar.criticalDamage: return data.TotalCriticalDamage;
            case ModifierVar.luck: return data.TotalLuck;
            case ModifierVar.healthRegen: return data.TotalHealthRegen;
            default: return 0f;
        }
    }

    // Remove todos os buffs ativos
    public void ClearAllBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            RemoveBuff(activeBuffs[i]);
        }
        activeBuffs.Clear();

        for (int i = distributedBuffs.Count - 1; i >= 0; i--)
        {
            var db = distributedBuffs[i];
            foreach (var dmod in db.mods)
            {
                RevertAppliedModifier(dmod.original, dmod);
            }
        }
        distributedBuffs.Clear();
    }

    // Remove um buff específico pelo nome
    public void RemoveBuffByName(string buffName)
    {
        ActiveBuff buff = activeBuffs.Find(b => b.buffName == buffName);
        if (buff != null)
        {
            RemoveBuff(buff);
            activeBuffs.Remove(buff);
        }
        else
        {
            var db = distributedBuffs.Find(d => d.buffName == buffName);
            if (db != null)
            {
                foreach (var dmod in db.mods)
                {
                    RevertAppliedModifier(dmod.original, dmod);
                }
                distributedBuffs.Remove(db);
            }
        }
    }

    public bool HasBuff(string buffName)
    {
        return activeBuffs.Exists(b => b.buffName == buffName);
    }
}

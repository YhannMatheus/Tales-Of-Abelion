using UnityEngine;
using System;

[System.Serializable]
public class ModifiersController
{
    [System.NonSerialized] private CharacterManager _character;

    public void Initialize(CharacterManager characterManager)
    {
        _character = characterManager;
    }

    public void ApplyEquipmentModifier(ModifierVar variable, float value)
    {
        if (_character == null) return;
        var data = _character.Data;
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                data.equipamentPhysicalDamageBonus += value;
                break;
            case ModifierVar.physicalResistence:
                data.equipamentPhysicalResistenceBonus += value;
                break;
            case ModifierVar.armorPenetration:
                data.equipamentArmorPenetrationBonus += value;
                break;
            case ModifierVar.magicalDamage:
                data.equipamentMagicalDamageBonus += value;
                break;
            case ModifierVar.magicalResistence:
                data.equipamentMagicalResistenceBonus += value;
                break;
            case ModifierVar.magicPenetration:
                data.equipamentMagicPenetrationBonus += value;
                break;
            case ModifierVar.maxHealth:
                data.equipamentMaxHealthBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.maxEnergy:
                data.equipamentMaxEnergyBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.speed:
                data.equipamentSpeedBonus += value;
                break;
            case ModifierVar.criticalChance:
                data.equipamentCriticalChanceBonus += value;
                break;
            case ModifierVar.criticalDamage:
                data.equipamentCriticalDamageBonus += value;
                break;
            case ModifierVar.attackSpeed:
                data.equipamentAttackSpeedBonus += value;
                break;
            case ModifierVar.luck:
                data.equipamentLuckBonus += value;
                break;
            case ModifierVar.healthRegen:
                data.equipamentHealthRegenBonus += Mathf.RoundToInt(value);
                break;
            default:
                Debug.LogWarning($"[ModifiersController] Modificador de equipamento não suportado: {variable}");
                break;
        }
    }

    public void ApplyEffectModifier(ModifierVar variable, float value)
    {
        if (_character == null) return;
        var data = _character.Data;
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                data.externalPhysicalDamageBonus += value;
                break;
            case ModifierVar.physicalResistence:
                data.externalPhysicalResistenceBonus += value;
                break;
            case ModifierVar.armorPenetration:
                data.externalArmorPenetrationBonus += value;
                break;
            case ModifierVar.magicalDamage:
                data.externalMagicalDamageBonus += value;
                break;
            case ModifierVar.magicalResistence:
                data.externalMagicalResistenceBonus += value;
                break;
            case ModifierVar.magicPenetration:
                data.externalMagicPenetrationBonus += value;
                break;
            case ModifierVar.maxHealth:
                data.externalMaxHealthBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.maxEnergy:
                data.externalMaxEnergyBonus += Mathf.RoundToInt(value);
                break;
            case ModifierVar.speed:
                data.externalSpeedBonus += value;
                break;
            case ModifierVar.criticalChance:
                data.externalCriticalChanceBonus += value;
                break;
            case ModifierVar.criticalDamage:
                data.externalCriticalDamageBonus += value;
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
            default:
                Debug.LogWarning($"[ModifiersController] Modificador de efeito não suportado: {variable}");
                break;
        }
    }

    public void RemoveEquipmentModifier(ModifierVar variable, float value)
    {
        ApplyEquipmentModifier(variable, -value);
    }

    public void RemoveEffectModifier(ModifierVar variable, float value)
    {
        ApplyEffectModifier(variable, -value);
    }

    public float GetTotalModifier(ModifierVar variable)
    {
        if (_character == null) return 0f;
        var data = _character.Data;
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                return data.TotalPhysicalDamage;
            case ModifierVar.physicalResistence:
                return data.TotalPhysicalResistance;
            case ModifierVar.armorPenetration:
                return data.TotalArmorPenetration;
            case ModifierVar.magicalDamage:
                return data.TotalMagicalDamage;
            case ModifierVar.magicalResistence:
                return data.TotalMagicalResistance;
            case ModifierVar.magicPenetration:
                return data.TotalMagicPenetration;
            case ModifierVar.maxHealth:
                return data.TotalMaxHealth;
            case ModifierVar.maxEnergy:
                return data.TotalMaxEnergy;
            case ModifierVar.speed:
                return data.TotalSpeed;
            case ModifierVar.criticalChance:
                return data.TotalCriticalChance;
            case ModifierVar.criticalDamage:
                return data.TotalCriticalDamage;
            case ModifierVar.attackSpeed:
                return data.TotalAttackSpeed;
            case ModifierVar.luck:
                return data.TotalLuck;
            case ModifierVar.healthRegen:
                return data.TotalHealthRegen;
            default:
                Debug.LogWarning($"[ModifiersController] GetTotalModifier não suporta: {variable}");
                return 0f;
        }
    }

    public float GetBaseModifier(ModifierVar variable)
    {
        if (_character == null) return 0f;
        var data = _character.Data;
        switch (variable)
        {
            case ModifierVar.physicalDamage:
                return data.physicalDamage;
            case ModifierVar.physicalResistence:
                return data.physicalResistence;
            case ModifierVar.armorPenetration:
                return data.armorPenetration;
            case ModifierVar.magicalDamage:
                return data.magicalDamage;
            case ModifierVar.magicalResistence:
                return data.magicalResistence;
            case ModifierVar.magicPenetration:
                return data.magicPenetration;
            case ModifierVar.maxHealth:
                return data.maxHealth;
            case ModifierVar.maxEnergy:
                return data.maxEnergy;
            case ModifierVar.speed:
                return data.speed;
            case ModifierVar.criticalChance:
                return data.criticalChance;
            case ModifierVar.criticalDamage:
                return data.criticalDamage;
            case ModifierVar.attackSpeed:
                return data.attackSpeed;
            case ModifierVar.luck:
                return data.luck;
            case ModifierVar.healthRegen:
                return data.healthRegen;
            default:
                Debug.LogWarning($"[ModifiersController] GetBaseModifier não suporta: {variable}");
                return 0f;
        }
    }
}

using UnityEngine;
using System;

[System.Serializable]
public class EnergyController
{
    [Header("Tipo de Energia Ativa")]
    [SerializeField] private EnergyType energyType = EnergyType.Mana;

    [Header("Mana Settings")]
    [SerializeField] private float maxMana = 100f;
    [SerializeField] private float currentMana = 100f;
    [SerializeField] private float manaRegenRate = 5f;
    [SerializeField] private float manaRegenInterval = 1f;

    [Header("Stamina Settings")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float currentStamina = 100f;
    [SerializeField] private float staminaRegenRate = 10f;
    [SerializeField] private float staminaRegenInterval = 0.5f;

    [Header("Fury Settings")]
    [SerializeField] private float maxFury = 100f;
    [SerializeField] private float currentFury = 0f;
    [SerializeField] private float furyGainOnDealDamage = 5f;
    [SerializeField] private float furyGainOnTakeDamage = 10f;
    [SerializeField] private float furyDecayRate = 2f;
    [SerializeField] private float furyDecayDelay = 5f;

    // Timers internos
    private float _manaRegenTimer = 0f;
    private float _staminaRegenTimer = 0f;
    private float _furyDecayTimer = 0f;
    private float _lastCombatTime = 0f;

    // Valores anteriores para eventos
    private float _previousMana;
    private float _previousStamina;
    private float _previousFury;

    // Flag para controle de evento de fúria máxima
    private bool _furyMaxEventFired = false;

    // Referência ao CharacterManager dono (não-serializada)
    [System.NonSerialized] private CharacterManager _character;

    // ✅ Eventos
    public event EventHandler<EnergyTypeChangedEventArgs> OnManaChanged;
    public event EventHandler<EnergyTypeChangedEventArgs> OnStaminaChanged;
    public event EventHandler<EnergyTypeChangedEventArgs> OnFuryChanged;
    public event EventHandler<FuryMaxReachedEventArgs> OnFuryMaxReached;
    public event EventHandler<FuryGainedEventArgs> OnFuryGained;

    // Propriedades públicas de leitura
    public EnergyType ActiveEnergyType => energyType;
    
    public float CurrentMana => currentMana;
    public float MaxMana => maxMana;
    public float ManaPercentage => maxMana > 0 ? (currentMana / maxMana) * 100f : 0f;
    
    public float CurrentStamina => currentStamina;
    public float MaxStamina => maxStamina;
    public float StaminaPercentage => currentStamina;
    
    public float CurrentFury => currentFury;
    public float MaxFury => maxFury;
    public float FuryPercentage => maxFury > 0 ? (currentFury / maxFury) * 100f : 0f;
    public bool IsFuryMax => currentFury >= maxFury;

    // Inicializa o controller - chamado por CharacterManager.Awake()
    public void Initialize(CharacterManager CharacterManager)
    {
        _character = CharacterManager;
        if (_character == null) return;

        var data = _character.Data;
        // Sincronizar tipo e valores iniciais com CharacterData
        energyType = data.energyType;

        // Definir máxima e atual baseado no TotalMaxEnergy / currentEnergy
        switch (energyType)
        {
            case EnergyType.Mana:
                maxMana = data.TotalMaxEnergy;
                currentMana = data.currentEnergy;
                manaRegenRate = data.energyRegen;
                break;
            case EnergyType.Stamina:
                maxStamina = data.TotalMaxEnergy;
                currentStamina = data.currentEnergy;
                staminaRegenRate = data.energyRegen;
                break;
            case EnergyType.Fury:
                maxFury = data.TotalMaxEnergy;
                currentFury = data.currentEnergy;
                break;
        }

        _previousMana = currentMana;
        _previousStamina = currentStamina;
        _previousFury = currentFury;

        // Inscrever em eventos do CharacterManager para ganho de fúria
        if (_character != null)
        {
            _character.OnTakeDamage += OnCharacterTakeDamage;
        }
    }

    // Deve ser chamado após CharacterData.Initialization para sincronizar valores atualizados
    public void SyncAfterInitialization()
    {
        if (_character == null) return;
        var data = _character.Data;
        energyType = data.energyType;
        switch (energyType)
        {
            case EnergyType.Mana:
                maxMana = data.TotalMaxEnergy;
                currentMana = data.currentEnergy;
                manaRegenRate = data.energyRegen;
                break;
            case EnergyType.Stamina:
                maxStamina = data.TotalMaxEnergy;
                currentStamina = data.currentEnergy;
                staminaRegenRate = data.energyRegen;
                break;
            case EnergyType.Fury:
                maxFury = data.TotalMaxEnergy;
                currentFury = data.currentEnergy;
                break;
        }
        // Garantir que CharacterData reflita o valor atual (caso controller ajuste)
        SyncCharacterCurrentEnergy();
    }

    // Atualiza CharacterData.currentEnergy para manter fonte de verdade sincronizada
    private void SyncCharacterCurrentEnergy()
    {
        if (_character == null) return;
        _character.Data.currentEnergy = CurrentEnergyInt;
    }

    // Limpa eventos - chamado por CharacterManager.OnDestroy()
    public void Cleanup()
    {
        // Desinscrever de eventos
        if (_character != null)
        {
            _character.OnTakeDamage -= OnCharacterTakeDamage;
        }
    }

    // Atualiza regeneração/decay - chamado por CharacterManager.Update()
    public void Tick(float deltaTime)
    {
        if (_character == null || !_character.Data.IsAlive) return;

        // Regeneração baseada no tipo de energia ativa
        switch (energyType)
        {
            case EnergyType.Mana:
                HandleManaRegeneration(deltaTime);
                break;
            case EnergyType.Stamina:
                HandleStaminaRegeneration(deltaTime);
                break;
            case EnergyType.Fury:
                HandleFuryDecay(deltaTime);
                break;
        }
    }

    #region Mana Methods

    private void HandleManaRegeneration(float deltaTime)
    {
        if (currentMana >= maxMana) return;

        _manaRegenTimer += deltaTime;

        if (_manaRegenTimer >= manaRegenInterval)
        {
            RestoreMana(manaRegenRate);
            _manaRegenTimer = 0f;
        }
    }

    // Restaura mana (flat)
    public void RestoreMana(float amount)
    {
        if (amount <= 0) return;

        _previousMana = currentMana;
        currentMana = Mathf.Min(currentMana + amount, maxMana);

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnManaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Mana,
            currentMana,
            maxMana,
            _previousMana,
            false));
    }

    // Gasta mana (flat) - retorna false se não tem o suficiente
    public bool SpendMana(float amount)
    {
        if (amount <= 0) return true;
        if (currentMana < amount) return false;

        _previousMana = currentMana;
        currentMana = Mathf.Max(currentMana - amount, 0f);

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnManaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Mana,
            currentMana,
            maxMana,
            _previousMana,
            false));

        return true;
    }

    // Conveniências para integração com CharacterManager que usa CharacterData
    // Restaura energia genérica (usa o tipo de energia ativo internamente)
    public void RestoreEnergy(int amount)
    {
        if (_character == null) return;
        // Usa os helpers existentes dependendo do tipo
        switch (energyType)
        {
            case EnergyType.Mana:
                RestoreMana(amount);
                break;
            case EnergyType.Stamina:
                RestoreStamina(amount, EnergyModificationMode.Flat);
                break;
            case EnergyType.Fury:
                GainFury(amount);
                break;
        }
    }

    // Tenta gastar energia genérica
    public bool TrySpendEnergy(int amount)
    {
        if (_character == null) return false;
        switch (energyType)
        {
            case EnergyType.Mana:
                return SpendMana(amount);
            case EnergyType.Stamina:
                return SpendStamina(amount, EnergyModificationMode.Flat);
            case EnergyType.Fury:
                return SpendFury(amount);
            default:
                return false;
        }
    }

    // Expor energia atual/máxima como inteiros para o CharacterManager
    public int CurrentEnergyInt => _character != null ? Mathf.RoundToInt(energyType == EnergyType.Mana ? currentMana : (energyType == EnergyType.Stamina ? currentStamina : currentFury)) : 0;
    public int MaxEnergyInt => _character != null ? Mathf.RoundToInt(energyType == EnergyType.Mana ? maxMana : (energyType == EnergyType.Stamina ? maxStamina : maxFury)) : 0;

    // Aumenta mana máxima (flat) - também aumenta o valor atual
    public void IncreaseMaxMana(float amount)
    {
        if (amount <= 0) return;

        _previousMana = currentMana;
        maxMana += amount;
        currentMana += amount; // Aumenta também o valor atual

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnManaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Mana,
            currentMana,
            maxMana,
            _previousMana,
            false));
    }

    #endregion

    #region Stamina Methods

    private void HandleStaminaRegeneration(float deltaTime)
    {
        if (currentStamina >= maxStamina) return;

        _staminaRegenTimer += deltaTime;

        if (_staminaRegenTimer >= staminaRegenInterval)
        {
            RestoreStamina(staminaRegenRate, EnergyModificationMode.Percentage);
            _staminaRegenTimer = 0f;
        }
    }

    // Restaura stamina (% ou flat)
    public void RestoreStamina(float amount, EnergyModificationMode mode = EnergyModificationMode.Percentage)
    {
        if (amount <= 0) return;

        _previousStamina = currentStamina;

        if (mode == EnergyModificationMode.Percentage)
        {
            // amount é porcentagem (0-100)
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        }
        else
        {
            // amount é valor flat
            currentStamina = Mathf.Min(currentStamina + amount, maxStamina);
        }


        OnStaminaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Stamina,
            currentStamina,
            maxStamina,
            _previousStamina,
            true));

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();
    }

    // Gasta stamina (% ou flat) - retorna false se não tem o suficiente
    public bool SpendStamina(float amount, EnergyModificationMode mode = EnergyModificationMode.Percentage)
    {
        if (amount <= 0) return true;

        float requiredStamina = amount;
        
        if (mode == EnergyModificationMode.Percentage)
        {
            // amount é porcentagem (0-100)
            requiredStamina = amount;
        }

        if (currentStamina < requiredStamina) return false;

        _previousStamina = currentStamina;
        currentStamina = Mathf.Max(currentStamina - requiredStamina, 0f);

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnStaminaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Stamina,
            currentStamina,
            maxStamina,
            _previousStamina,
            true));

        return true;
    }

    #endregion

    #region Fury Methods

    private void HandleFuryDecay(float deltaTime)
    {
        if (currentFury <= 0) return;

        // Atualizar timer de último combate
        _lastCombatTime += deltaTime;

        // Verificar se passou tempo suficiente sem combate
        if (_lastCombatTime >= furyDecayDelay)
        {
            _furyDecayTimer += deltaTime;

            if (_furyDecayTimer >= 1f) // Decai a cada segundo
            {
                _previousFury = currentFury;
                currentFury = Mathf.Max(currentFury - furyDecayRate, 0f);

                // Reset flag de evento quando fúria não está mais no máximo
                if (currentFury < maxFury)
                {
                    _furyMaxEventFired = false;
                }

                // Sincronizar com CharacterData
                SyncCharacterCurrentEnergy();

                OnFuryChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
                    EnergyType.Fury,
                    currentFury,
                    maxFury,
                    _previousFury,
                    false));

                _furyDecayTimer = 0f;
            }
        }
    }

    // Ganha fúria (sempre flat) - reseta timer de combate
    public void GainFury(float amount, FuryGainSource source = FuryGainSource.Other)
    {
        if (amount <= 0) return;

        _previousFury = currentFury;
        currentFury = Mathf.Min(currentFury + amount, maxFury);

        // Atualizar timer de combate (resetar contador)
        _lastCombatTime = 0f;
        _furyDecayTimer = 0f;

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnFuryChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Fury,
            currentFury,
            maxFury,
            _previousFury,
            false));

        OnFuryGained?.Invoke(_character, new FuryGainedEventArgs(
            amount,
            currentFury,
            maxFury,
            source));

        // Verificar se atingiu o máximo
        if (currentFury >= maxFury && !_furyMaxEventFired)
        {
            _furyMaxEventFired = true;
            OnFuryMaxReached?.Invoke(_character, new FuryMaxReachedEventArgs(
                currentFury,
                maxFury,
                _character));

            if (_character != null)
            {
                Debug.Log($"[EnergyController] {_character.gameObject.name} atingiu FÚRIA MÁXIMA!");
            }
        }
    }

    // Gasta fúria (sempre flat) - retorna false se não tem o suficiente
    public bool SpendFury(float amount)
    {
        if (amount <= 0) return true;
        if (currentFury < amount) return false;

        _previousFury = currentFury;
        currentFury = Mathf.Max(currentFury - amount, 0f);

        // Reset flag de evento quando não está mais no máximo
        if (currentFury < maxFury)
        {
            _furyMaxEventFired = false;
        }


        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnFuryChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Fury,
            currentFury,
            maxFury,
            _previousFury,
            false));

        return true;
    }

    // Reseta fúria para zero
    public void ResetFury()
    {
        _previousFury = currentFury;
        currentFury = 0f;
        _furyMaxEventFired = false;

        // Sincronizar com CharacterData
        SyncCharacterCurrentEnergy();

        OnFuryChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(
            EnergyType.Fury,
            currentFury,
            maxFury,
            _previousFury,
            false));
    }

    // Chame quando o personagem causar dano (para ganhar fúria)
    public void OnDealDamage(float damageDealt)
    {
        if (energyType != EnergyType.Fury) return;
        
        GainFury(furyGainOnDealDamage, FuryGainSource.DealDamage);
    }

    // Handler automático quando recebe dano (inscrito no Initialize)
    private void OnCharacterTakeDamage(object sender, DamageTakenEventArgs e)
    {
        if (energyType != EnergyType.Fury) return;
        
        GainFury(furyGainOnTakeDamage, FuryGainSource.TakeDamage);
    }

    // Conveniências para integração com CharacterManager
    public void SetCurrentEnergy(int value)
    {
        if (_character == null) return;
        switch (energyType)
        {
            case EnergyType.Mana:
                _previousMana = currentMana;
                currentMana = Mathf.Clamp(value, 0f, maxMana);
                SyncCharacterCurrentEnergy();
                OnManaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(EnergyType.Mana, currentMana, maxMana, _previousMana, false));
                break;
            case EnergyType.Stamina:
                _previousStamina = currentStamina;
                currentStamina = Mathf.Clamp(value, 0f, maxStamina);
                SyncCharacterCurrentEnergy();
                OnStaminaChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(EnergyType.Stamina, currentStamina, maxStamina, _previousStamina, true));
                break;
            case EnergyType.Fury:
                _previousFury = currentFury;
                currentFury = Mathf.Clamp(value, 0f, maxFury);
                SyncCharacterCurrentEnergy();
                OnFuryChanged?.Invoke(_character, new EnergyTypeChangedEventArgs(EnergyType.Fury, currentFury, maxFury, _previousFury, false));
                break;
        }
    }

    #endregion

    #region Generic Methods

    // Verifica se tem energia suficiente (baseado no tipo ativo)
    public bool HasEnoughEnergy(float amount, EnergyModificationMode mode = EnergyModificationMode.Flat)
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                return currentMana >= amount;
            
            case EnergyType.Stamina:
                if (mode == EnergyModificationMode.Percentage)
                    return currentStamina >= amount;
                else
                    return currentStamina >= amount;
            
            case EnergyType.Fury:
                return currentFury >= amount;
            
            default:
                return false;
        }
    }

    // Gasta energia baseado no tipo ativo
    public bool SpendEnergy(float amount, EnergyModificationMode mode = EnergyModificationMode.Flat)
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                return SpendMana(amount);
            
            case EnergyType.Stamina:
                return SpendStamina(amount, mode);
            
            case EnergyType.Fury:
                return SpendFury(amount);
            
            default:
                return false;
        }
    }

    // Restaura energia baseado no tipo ativo
    public void RestoreEnergy(float amount, EnergyModificationMode mode = EnergyModificationMode.Flat)
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                RestoreMana(amount);
                break;
            
            case EnergyType.Stamina:
                RestoreStamina(amount, mode);
                break;
            
            case EnergyType.Fury:
                GainFury(amount);
                break;
        }
    }

    // Retorna energia atual do tipo ativo
    public float GetCurrentEnergy()
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                return currentMana;
            case EnergyType.Stamina:
                return currentStamina;
            case EnergyType.Fury:
                return currentFury;
            default:
                return 0f;
        }
    }

    // Retorna energia máxima do tipo ativo
    public float GetMaxEnergy()
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                return maxMana;
            case EnergyType.Stamina:
                return maxStamina;
            case EnergyType.Fury:
                return maxFury;
            default:
                return 0f;
        }
    }

    // Retorna porcentagem de energia do tipo ativo (0-100)
    public float GetEnergyPercentage()
    {
        switch (energyType)
        {
            case EnergyType.Mana:
                return ManaPercentage;
            case EnergyType.Stamina:
                return StaminaPercentage;
            case EnergyType.Fury:
                return FuryPercentage;
            default:
                return 0f;
        }
    }

    #endregion

    #region Public Accessors

    // Define qual tipo de energia está ativo
    public void SetEnergyType(EnergyType type)
    {
        energyType = type;
    }

    // Define valores máximos dos 3 tipos de energia
    public void SetMaxValues(float mana, float stamina, float fury)
    {
        maxMana = mana;
        maxStamina = stamina;
        maxFury = fury;
    }

    // Valida e corrige valores - chamado por CharacterManager.OnValidate()
    public void ValidateValues()
    {
        // Garantir que stamina máxima seja sempre 100 (sistema percentual)
        if (energyType == EnergyType.Stamina && maxStamina != 100f)
        {
            maxStamina = 100f;
        }

        // Clampar valores atuais
        currentMana = Mathf.Clamp(currentMana, 0f, maxMana);
        currentStamina = Mathf.Clamp(currentStamina, 0f, maxStamina);
        currentFury = Mathf.Clamp(currentFury, 0f, maxFury);
    }

    #endregion
}

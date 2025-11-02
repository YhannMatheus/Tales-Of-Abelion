using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(IAAnimatorController))]
[RequireComponent(typeof(IADetectSystem))]
[RequireComponent(typeof(StateManager))]
[RequireComponent(typeof(IAAbilityManager))]
public class IAManager : MonoBehaviour
{
    [Header("Informações Básicas")]
    public IaType iaType;
    public Vector3 eyePositionOffset;

    [Header("Character Data (NPC)")]
    [SerializeField] private CharacterData characterData = new CharacterData();
    
    [Header("Regeneração")]
    [Tooltip("Habilitar regeneração de vida")]
    public bool enableHealthRegen = true;
    [Tooltip("Habilitar regeneração de energia")]
    public bool enableEnergyRegen = true;
    
    private float _healthRegenTimer = 0f;
    private float _energyRegenTimer = 0f;

    public CharacterData Data => characterData;

    [Header("Aliado - Configurações")]
    public Transform playerToFollow;
    public float minFollowDistance = 2f;
    public float maxFollowDistance = 5f;
    public Vector3 followOffset = new Vector3(2f, 0f, -1f);

    [Header("Patrulha")]
    public float waitTimeAtPoint = 2f;
    public Vector3[] patrolPoints;
    public float patrolPointRadius = 0.5f;
    [HideInInspector] public int currentPatrolIndex = 0;

    [Header("Detecção")]
    public float visionArea = 10f;
    public float detectionAngle = 90f;
    public LayerMask[] targetLayerMask;
    public LayerMask[] obstructionLayerMask;
    [HideInInspector] public Transform lastKnownTargetPosition;

    [Header("Combate")]
    public float attackRange = 2f;
    [HideInInspector] public float lastAttackTime = 0f;

    [Header("Movimentação")]
    [HideInInspector] public Vector3 moveDirection;
    [HideInInspector] public float walkSpeed;
    [HideInInspector] public float runSpeed;
    [HideInInspector] public float currentSpeed;

    [Header("Componentes")]
    [HideInInspector] public CharacterController controller;
    [HideInInspector] public IAAnimatorController animator;
    [HideInInspector] public IADetectSystem detectSystem;
    [HideInInspector] public StateManager stateManager;
    [HideInInspector] public IAAbilityManager abilityManager;
    
    [Header("Runtime Data")]
    [HideInInspector] public Transform currentTarget;

    [Header("Recompensas (Enemy/NPC)")]
    public int experienceReward;
    public GameObject[] itemDrops;
    public float xpDistributionRange = 10f;
    [Range(0f, 1f)] public float dropChance = 0.5f;

    [Header("Aliado - Revive System")]
    public GameObject reviveTokenPrefab;
    [HideInInspector] public GameObject spawnedReviveToken;

    [Header("Eventos")]
    public Action OnDeath;
    public Action<int, int> OnHealthChanged;
    public Action<int, int> OnEnergyChanged;

    public bool IsAlive => characterData.IsAlive;
    public bool IsFullHealth => characterData.IsFullHealth;
    public bool IsFullEnergy => characterData.IsFullEnergy;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<IAAnimatorController>();
        detectSystem = GetComponent<IADetectSystem>();
        stateManager = GetComponent<StateManager>();
        abilityManager = GetComponent<IAAbilityManager>();

        InitializeNPC();

        runSpeed = characterData.TotalSpeed;
        walkSpeed = runSpeed * 0.7f;

        stateManager.Initialize(this);
        abilityManager.InitializeAbilities();

        // Inscreve nos eventos de detecção
        SubscribeToDetectionEvents();

        Debug.Log($"[IAManager] {characterData.characterName} inicializado como {iaType}");
    }

    void OnDestroy()
    {
        // Desinscreve dos eventos de detecção
        UnsubscribeFromDetectionEvents();
    }

    private void SubscribeToDetectionEvents()
    {
        if (detectSystem != null)
        {
            detectSystem.OnDetectTarget += HandleTargetDetected;
            detectSystem.OnLoseTarget += HandleTargetLost;
        }
    }

    private void UnsubscribeFromDetectionEvents()
    {
        if (detectSystem != null)
        {
            detectSystem.OnDetectTarget -= HandleTargetDetected;
            detectSystem.OnLoseTarget -= HandleTargetLost;
        }
    }

    private void HandleTargetDetected(Transform target)
    {
        currentTarget = target;
        Debug.Log($"[IAManager] {characterData.characterName} detectou alvo: {target.name}");
    }

    private void HandleTargetLost()
    {
        currentTarget = null;
        Debug.Log($"[IAManager] {characterData.characterName} perdeu o alvo");
    }

    private void InitializeNPC()
    {
        if (characterData.characterClass == null || characterData.characterRace == null)
        {
            Debug.LogError($"[IAManager] {gameObject.name} precisa de ClassData e RaceData configurados no CharacterData!");
            return;
        }

        characterData.Initialization();

        Debug.Log($"[IAManager] NPC {characterData.characterName} inicializado - HP: {characterData.currentHealth}/{characterData.TotalMaxHealth}");
    }

    void Update()
    {
        if (!IsAlive) return;

        HandleRegeneration();
        ApplyGravity();
        
        detectSystem?.CanSeeTarget(this);
        stateManager?.VerifyStates(this);
        stateManager?.UpdateState();
        animator?.UpdateAnimation();



    }

    private void HandleRegeneration()
    {
        if (!characterData.IsAlive) return;

        if (enableHealthRegen && !characterData.IsFullHealth)
        {
            _healthRegenTimer += Time.deltaTime;

            if (_healthRegenTimer >= 1f)
            {
                int regenAmount = Mathf.RoundToInt(characterData.TotalHealthRegen);
                characterData.Heal(regenAmount);
                OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
                _healthRegenTimer = 0f;
            }
        }

        if (enableEnergyRegen && !characterData.IsFullEnergy)
        {
            _energyRegenTimer += Time.deltaTime;

            if (_energyRegenTimer >= 1f)
            {
                characterData.RestoreEnergy(characterData.energyRegen);
                OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
                _energyRegenTimer = 0f;
            }
        }
    }

    #region StateManager Delegation

    public void SwitchState(State newState)
    {
        stateManager?.SwitchState(newState);
    }

    public State GetStateByIAState(IAState stateType)
    {
        return stateManager?.GetStateByType(stateType);
    }

    public bool CanUseState(IAState state)
    {
        return stateManager != null && stateManager.CanUseState(state);
    }

    #endregion

    #region Combat Methods
    
    public void TakeDamage(float damage, bool isMagical = false)
    {
        if (!characterData.IsAlive) return;

        characterData.TakeDamage(damage, isMagical);

        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);

        Debug.Log($"[IA] {characterData.characterName} levou {damage} de dano! HP: {characterData.currentHealth}/{characterData.TotalMaxHealth}");

        if (!characterData.IsAlive)
        {
            Die();
        }
    }

    public void Die()
    {
        Debug.Log($"[IA] {characterData.characterName} morreu!");
        
        if (iaType == IaType.Enemy)
        {
            HandleEnemyDeath();
        }
        else if (iaType == IaType.Ally)
        {
            HandleAllyDeath();
        }
        
        State deadState = GetStateByIAState(IAState.Dead);
        if (deadState != null && CanUseState(IAState.Dead))
        {
            SwitchState(deadState);
        }
        
        OnDeath?.Invoke();
    }

    public void Heal(int amount)
    {
        if (!characterData.IsAlive) return;

        characterData.Heal(amount);
        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
    }

    public void Revive()
    {
        int reviveHealth = Mathf.RoundToInt(characterData.TotalMaxHealth * 0.5f);
        int reviveEnergy = Mathf.RoundToInt(characterData.TotalMaxEnergy * 0.5f);

        characterData.currentHealth = reviveHealth;
        characterData.currentEnergy = reviveEnergy;

        OnHealthChanged?.Invoke(characterData.currentHealth, characterData.TotalMaxHealth);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);

        if (spawnedReviveToken != null)
        {
            Destroy(spawnedReviveToken);
            spawnedReviveToken = null;
        }

        State idleState = GetStateByIAState(IAState.Idle);
        if (idleState != null && CanUseState(IAState.Idle))
        {
            SwitchState(idleState);
        }

        Debug.Log($"[IA] {characterData.characterName} revivido com {reviveHealth} HP e {reviveEnergy} energia!");
    }

    public void RestoreEnergy(int amount)
    {
        if (!characterData.IsAlive) return;

        characterData.RestoreEnergy(amount);
        OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
    }

    public bool TrySpendEnergy(int amount)
    {
        if (!characterData.IsAlive) return false;

        bool success = characterData.SpendEnergy(amount);

        if (success)
        {
            OnEnergyChanged?.Invoke(characterData.currentEnergy, characterData.TotalMaxEnergy);
        }

        return success;
    }

    #endregion

    #region Loot & XP Distribution

    public void HandleEnemyDeath()
    {
        List<Character> nearbyPlayers = FindNearbyPlayers(xpDistributionRange);

        if (nearbyPlayers.Count > 0 && experienceReward > 0)
        {
            int xpPerPlayer = Mathf.RoundToInt(experienceReward / nearbyPlayers.Count);

            foreach (Character player in nearbyPlayers)
            {
                player.GainExperience(xpPerPlayer);
                Debug.Log($"[IA] {player.Data.characterName} ganhou {xpPerPlayer} XP de {characterData.characterName}");
            }
        }

        DropLoot();

        Destroy(gameObject, 5f);
    }

    public void HandleAllyDeath()
    {
        if (reviveTokenPrefab != null)
        {
            Vector3 spawnPosition = transform.position + Vector3.up * 0.5f;
            spawnedReviveToken = Instantiate(reviveTokenPrefab, spawnPosition, Quaternion.identity);
            
            ReviveToken tokenScript = spawnedReviveToken.GetComponent<ReviveToken>();
            if (tokenScript != null)
            {
                tokenScript.Initialize(this);
            }
            
            Debug.Log($"[IA] {characterData.characterName} deixou um ReviveToken em {spawnPosition}");
        }
        else
        {
            Debug.LogWarning($"[IA] {characterData.characterName} é um aliado mas não tem reviveTokenPrefab configurado!");
        }
    }

    private void DropLoot()
    {
        if (itemDrops == null || itemDrops.Length == 0) return;

        foreach (GameObject item in itemDrops)
        {
            if (item != null)
            {
                if (UnityEngine.Random.value <= dropChance)
                {
                    Vector3 dropPosition = transform.position + Vector3.up * 0.5f;
                    Instantiate(item, dropPosition, Quaternion.identity);
                    Debug.Log($"[IA] {characterData.characterName} dropou item: {item.name}");
                }
            }
        }
    }

    private List<Character> FindNearbyPlayers(float range)
    {
        List<Character> players = new List<Character>();

        Collider[] hits = Physics.OverlapSphere(transform.position, range);

        foreach (Collider hit in hits)
        {
            Character character = hit.GetComponent<Character>();

            if (character != null && character.characterType == CharacterType.Player)
            {
                players.Add(character);
            }
        }

        return players;
    }

    #endregion

    #region Utility Methods

    public bool IsInAttackRange(Transform target)
    {
        if (target == null) return false;
        return Vector3.Distance(transform.position, target.position) <= attackRange;
    }

    public bool CanAttack()
    {
        float attackSpeed = characterData.TotalAttackSpeed;
        float cooldown = attackSpeed > 0 ? 1f / attackSpeed : 1.5f;
        return Time.time >= lastAttackTime + cooldown;
    }

    public void ApplyGravity()
    {
        if (controller != null && !controller.isGrounded)
        {
            controller.Move(Vector3.down * 9.81f * Time.deltaTime);
        }
    }

    public void RotateTowards(Vector3 targetPosition, float rotationSpeed = 5f)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;
        
        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }

    #endregion

    #region Gizmos & Debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        if (patrolPoints != null && patrolPoints.Length > 1)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                Vector3 currentPoint = patrolPoints[i];
                Vector3 nextPoint = patrolPoints[(i + 1) % patrolPoints.Length];
                Gizmos.DrawLine(currentPoint, nextPoint);
                Gizmos.DrawSphere(currentPoint, patrolPointRadius);
            }
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (iaType == IaType.Enemy)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, xpDistributionRange);
        }
    }

    private void OnDrawGizmosSelected()
    {
        #if UNITY_EDITOR
        if (characterData != null)
        {
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 2f,
                $"[{iaType}] {characterData.characterName}\n" +
                $"HP: {characterData.currentHealth}/{characterData.TotalMaxHealth}\n" +
                $"Energy: {characterData.currentEnergy}/{characterData.TotalMaxEnergy}\n" +
                $"Atk: {characterData.TotalPhysicalDamage:F1} | Mag: {characterData.TotalMagicalDamage:F1}\n" +
                $"Crit: {characterData.TotalCriticalChance:F1}% ({characterData.TotalCriticalDamage:F1}x)\n" +
                $"AtkSpd: {characterData.TotalAttackSpeed:F2}/s | Speed: {runSpeed:F1}\n" +
                $"XP Reward: {experienceReward}"
            );
        }
        #endif
    }

    #endregion
}
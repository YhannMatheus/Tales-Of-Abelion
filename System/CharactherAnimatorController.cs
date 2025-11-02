using System.Linq;
using UnityEngine;

public class CharacterAnimatorController : MonoBehaviour
{
    [Header("References")]
    private Animator animator;
    private Character character;
    
    [Header("Movement")]
    [SerializeField] private float movementSmoothTime = 8f;
    
    [Header("Animation Parameters")]
    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int IsGroundedHash = Animator.StringToHash("IsGrounded");
    private static readonly int IsDeathHash = Animator.StringToHash("isDeath");
    private static readonly int HitHash = Animator.StringToHash("Hit");
    private static readonly int AttackHash = Animator.StringToHash("Attack");
    private static readonly int LevelUpHash = Animator.StringToHash("LevelUp");
    private static readonly int AbilityIndexHash = Animator.StringToHash("AbilityIndex");
    private static readonly int AbilityActiveHash = Animator.StringToHash("AbilityActive");

    private float currentSpeed = 0f;
    private bool isGrounded = true;
    [Header("Debug")]
    [SerializeField] private bool debugAnimator = false;
    private bool hasSpeedParameter = true;

    private void Awake()
    {

        animator = GetComponentInChildren<Animator>();
        character = GetComponent<Character>();

        if (animator == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No Animator found on {gameObject.name}");
            enabled = false;
            return;
        }

        if (character == null)
        {
            Debug.LogWarning($"[CharacterAnimatorController] No Character component found on {gameObject.name}");
            enabled = false;
            return;
        }

        // Verifica se o Animator possui o parâmetro "Speed" — se não, avisa para ajudar debugging
        var paramNames = animator.parameters.Select(p => p.name).ToArray();
        hasSpeedParameter = paramNames.Any(n => n == "Speed");
        if (!hasSpeedParameter)
        {
            Debug.LogWarning($"[CharacterAnimatorController] Animator on {gameObject.name} does not contain parameter 'Speed'. Available: {string.Join(", ", paramNames)}");
        }
    }

    private void OnEnable()
    {

        if (character != null)
        {
            character.OnDeath += HandleDeath;
            character.OnRevive += HandleRevive;
            character.OnLevelUp += HandleLevelUp;
        }
    }

    private void OnDisable()
    {

        if (character != null)
        {
            character.OnDeath -= HandleDeath;
            character.OnRevive -= HandleRevive;
            character.OnLevelUp -= HandleLevelUp;
        }
    }

    public void UpdateMovementSpeed(float targetSpeed, bool grounded = true)
    {

        if (animator == null) return;
        if (isGrounded != grounded)
        {
            isGrounded = grounded;
            animator.SetBool(IsGroundedHash, isGrounded);
        }

        float newSpeed;
        if (isGrounded)
        {
            newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, movementSmoothTime * Time.deltaTime);
        }
        else
        {
            newSpeed = Mathf.Lerp(currentSpeed, 0f, movementSmoothTime * Time.deltaTime);
        }

        if (debugAnimator)
        {
            Debug.Log($"[CharacterAnimatorController] UpdateMovementSpeed: target={targetSpeed:F2}, grounded={grounded}, current(before)={currentSpeed:F2}, new={newSpeed:F2}");
        }

        currentSpeed = newSpeed;

        if (hasSpeedParameter)
        {
            animator.SetFloat(SpeedHash, currentSpeed);
        }
    }

    public void TriggerAttack()
    {

        if (animator == null || !character.Data.IsAlive) return;
        animator.SetTrigger(AttackHash);
    }

    public void TriggerAbility(int abilityIndex)
    {

        if (animator == null || !character.Data.IsAlive) return;
        animator.SetInteger(AbilityIndexHash, abilityIndex);
        animator.SetBool(AbilityActiveHash, true);
    }

    public void TriggerHit()
    {

        if (animator == null || !character.Data.IsAlive) return;
        animator.SetTrigger(HitHash);
    }

    public void EndAbility()
    {
        if (animator == null) return;
        animator.SetBool(AbilityActiveHash, false);
    }

    private void HandleDeath()
    {
        if (animator == null) return;
        animator.SetBool(IsDeathHash, true);
        currentSpeed = 0f;
        animator.SetFloat(SpeedHash, 0f);
    }

    private void HandleRevive()
    {
        if (animator == null) return;
        animator.SetBool(IsDeathHash, false);
    }

    private void HandleLevelUp(int newLevel)
    {
        if (animator == null) return;
        animator.SetTrigger(LevelUpHash);
    }
}
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class IAAnimatorController : MonoBehaviour
{
	[Header("Animator parameters")]
	[SerializeField] private string speedParam = "Speed";
	[SerializeField] private string isWalkingParam = "isWalking";
	[SerializeField] private string isRunningParam = "isRunning";
	[SerializeField] private string isAttackingParam = "isAttacking";
	[SerializeField] private string isTakingDamageParam = "isTakingDamage";
    [SerializeField] private string isDeadParam = "isDead";
	
    [Header("IA state parameter names (optional)")]
	[SerializeField] private string isIdleParam = "isIdle";
	[SerializeField] private string isPatrolParam = "isPatrol";
	[SerializeField] private string isChaseParam = "isChase";
	[SerializeField] private string isAttackParam = "isAttack";
	[SerializeField] private string isFleeParam = "isFlee";

	[Header("Smoothing / tuning")]
	[SerializeField] private float speedChangeRate = 2f;
	[SerializeField] private float speedDampTime = 0.1f;

	private Animator animator;

    [Header("Flags")]
    public bool IsIdle;
    public bool IsPatrol;
    public bool IsChase;
    public bool IsAttack;
    public bool IsFlee;

    public bool IsTakingDamage;
    public bool IsDead;

	[Header("Animation Speed")]
	public float targetSpeedNormalized = 0f;
	public float smoothedSpeed { get; private set; }

	private void Awake()
	{
		animator = GetComponent<Animator>();
		smoothedSpeed = 0f;
	}

	public void UpdateAnimation()
	{
		UpdateSpeed();
		UpdateDeadState();
		UpdateIAStateFlags();
		UpdateCombatFlags();
	}

	private void UpdateSpeed()
	{
		smoothedSpeed = Mathf.MoveTowards(smoothedSpeed, Mathf.Clamp01(targetSpeedNormalized), speedChangeRate * Time.deltaTime);
		if (animator != null)
		{
			animator.SetFloat(speedParam, smoothedSpeed, speedDampTime, Time.deltaTime);
		}
	}

	private void UpdateDeadState()
	{
		if (animator == null) return;

		animator.SetBool(isDeadParam, IsDead);
		if (IsDead)
		{
			animator.SetBool(isWalkingParam, false);
			animator.SetBool(isRunningParam, false);
			animator.SetBool(isAttackingParam, false);
			animator.SetBool(isTakingDamageParam, false);

			SetAnimatorBoolSafe(isIdleParam, false);
			SetAnimatorBoolSafe(isPatrolParam, false);
			SetAnimatorBoolSafe(isChaseParam, false);
			SetAnimatorBoolSafe(isAttackParam, false);
			SetAnimatorBoolSafe(isFleeParam, false);
		}
	}

	private void UpdateIAStateFlags()
	{
		if (animator == null) return;

		SetAnimatorBoolSafe(isIdleParam, IsIdle);
		SetAnimatorBoolSafe(isPatrolParam, IsPatrol);
		SetAnimatorBoolSafe(isChaseParam, IsChase);
		SetAnimatorBoolSafe(isAttackParam, IsAttack);
		SetAnimatorBoolSafe(isFleeParam, IsFlee);
	}

	private void UpdateCombatFlags()
	{
		if (animator == null) return;

		animator.SetBool(isWalkingParam, IsPatrol || IsChase);
		animator.SetBool(isRunningParam, IsChase);
		animator.SetBool(isAttackingParam, IsAttack);
		animator.SetBool(isTakingDamageParam, IsTakingDamage);
	}

    private void SetAnimatorBoolSafe(string paramName, bool value)
    {
        if (string.IsNullOrEmpty(paramName) || animator == null) return;
        try
        {
            animator.SetBool(paramName, value);
        }
        catch { }
    }

	public void SetState(IAState state)
	{
		IsIdle = IsPatrol = IsChase = IsAttack = IsFlee = false;

		switch (state)
		{
			case IAState.Idle:
				IsIdle = true;
				break;
			case IAState.Patrol:
				IsPatrol = true;
				break;
			case IAState.Chase:
				IsChase = true;
				break;
			case IAState.Attack:
				IsAttack = true;
				break;
			case IAState.Flee:
				IsFlee = true;
				break;
		}
	}
	
	public void ExitState(IAState state)
	{
		switch (state)
		{
			case IAState.Idle:
				IsIdle = false;
				break;
			case IAState.Patrol:
				IsPatrol = false;
				break;
			case IAState.Chase:
				IsChase = false;
				break;
			case IAState.Attack:
				IsAttack = false;
				break;
			case IAState.Flee:
				IsFlee = false;
				break;
		}
	}


	public void ResetStateFlags()
	{
		IsIdle = false;
		IsPatrol = false;
		IsChase = false;
		IsAttack = false;
		IsFlee = false;
		IsTakingDamage = false;
		targetSpeedNormalized = 0f;
	}
}


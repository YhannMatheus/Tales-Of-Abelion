using UnityEngine;

/// <summary>
/// Estado de ataque básico estilo MOBA/ARPG
/// - Persegue o alvo até estar em range de ataque
/// - Executa ataque básico repetidamente
/// - Retorna para Idle se alvo morrer/desaparecer ou se jogador clicar para mover
/// </summary>
public class AttackState : StateBase
{
    private CharacterManager target;
    private float attackRange = 2f; // range para iniciar ataque
    private float followDistance = 10f; // distância máxima para seguir alvo
    private float attackCooldown = 0f;

    public AttackState(CharacterManager target, float attackRange = 2f)
    {
        this.target = target;
        this.attackRange = attackRange;
    }

    public override void EnterState(PlayerManager playerManager)
    {
        // Nada especial na entrada
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;

        // Verifica cancelamento por novo input (clique para mover/outro alvo)
        if (InputManager.Instance != null && InputManager.Instance.interactButton)
        {
            if (playerManager._playerMouseController != null)
            {
                Vector3 newTarget = playerManager._playerMouseController.GetTargetPosition();
                GameObject targetObj = playerManager._playerMouseController.GetTargetObject();

                // Se clicou em outro inimigo, troca alvo
                if (targetObj != null)
                {
                    CharacterManager newTargetChar = targetObj.GetComponent<CharacterManager>();
                    if (newTargetChar != null && IsEnemy(playerManager.sheet, newTargetChar) && newTargetChar != target)
                    {
                        target = newTargetChar;
                        return;
                    }
                }
                else
                {
                    // Clicou no chão, cancela ataque e move
                    playerManager._playerStateMachine.SwitchState(new MoveState(newTarget));
                    return;
                }
            }
        }

        // Valida alvo
        if (target == null || target.gameObject == null || !target.gameObject.activeInHierarchy)
        {
            playerManager._playerStateMachine.SwitchState(new IdleState());
            return;
        }

        // Verifica se alvo está morto
        if (target.Health != null && target.Health.CurrentHealth <= 0)
        {
            playerManager._playerStateMachine.SwitchState(new IdleState());
            return;
        }

        Vector3 targetPos = target.transform.position;
        Vector3 playerPos = playerManager.transform.position;
        float distance = Vector3.Distance(playerPos, targetPos);

        // Se alvo está muito longe, cancela
        if (distance > followDistance)
        {
            playerManager._playerStateMachine.SwitchState(new IdleState());
            return;
        }

        // Se está fora do range de ataque, move em direção ao alvo
        if (distance > attackRange)
        {
            Vector3 direction = (targetPos - playerPos).normalized;
            Vector3 localDir = playerManager.transform.InverseTransformDirection(direction);

            float speed = 0f;
            if (playerManager.sheet != null)
                speed = playerManager.sheet.Data.TotalSpeed;

            playerManager._playerMotor.Move(localDir, speed);
            playerManager._playerMotor.Rotation(direction);
        }
        else
        {
            // Em range: para de mover e executa ataque
            playerManager._playerMotor.Stop();

            // Rotaciona para o alvo
            Vector3 lookDir = (targetPos - playerPos);
            lookDir.y = 0f; // mantém rotação apenas no plano horizontal
            if (lookDir != Vector3.zero)
            {
                playerManager._playerMotor.Rotation(lookDir.normalized);
            }

            // Executa ataque básico respeitando cooldown
            attackCooldown -= Time.deltaTime;
            if (attackCooldown <= 0f)
            {
                if (playerManager.skillManager != null && playerManager.skillManager.basicAttackSkill != null)
                {
                    if (playerManager.skillManager.basicAttackSkill.CanUse())
                    {
                        var ctx = playerManager.skillManager.CreateContext(playerManager.skillManager.basicAttackSkill.Data);
                        playerManager.skillManager.UseBasicAttack(ctx);

                        // Define próximo ataque baseado no attack speed
                        float attackSpeed = playerManager.sheet != null ? playerManager.sheet.Data.TotalAttackSpeed : 1f;
                        attackCooldown = 1f / Mathf.Max(0.1f, attackSpeed);
                    }
                }
            }
        }
    }

    public override void ExitState(PlayerManager playerManager)
    {
        // Para movimento ao sair
        if (playerManager != null && playerManager._playerMotor != null)
        {
            playerManager._playerMotor.Stop();
        }
    }

    private bool IsEnemy(CharacterManager self, CharacterManager target)
    {
        if (self == null || target == null) return false;
        if (self == target) return false;
        if (self.characterType == CharacterType.Player || self.characterType == CharacterType.Ally)
        {
            return target.characterType == CharacterType.Enemy;
        }
        return false;
    }

    public override bool CanMove => true; // pode se mover para perseguir
    public override bool CanRotate => true;
    public override bool CanCasting => true; // pode usar skills durante ataque
}

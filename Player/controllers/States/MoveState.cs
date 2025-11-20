using UnityEngine;

public class MoveState : StateBase
{
    private Vector3 targetPosition;
    private float stopDistance = 0.5f;

    public MoveState(Vector3 targetPosition, float stopDistance = 0.5f)
    {
        this.targetPosition = targetPosition;
        this.stopDistance = stopDistance;
    }

    public override void EnterState(PlayerManager playerManager)
    {
        // nada especial na entrada por enquanto
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;
        var motor = playerManager._playerMotor;
        var transform = playerManager.transform;

        // Enquanto o botão direito estiver pressionado, atualiza o destino frame-a-frame
        if (Input.GetMouseButton(1))
        {
            if (playerManager._playerMouseController != null)
            {
                Vector3 newTarget = playerManager._playerMouseController.GetTargetPosition();
                GameObject targetObj = playerManager._playerMouseController.GetTargetObject();

                // Se o cursor estiver sobre um inimigo, troca para AttackState
                if (targetObj != null)
                {
                    CharacterManager targetChar = targetObj.GetComponent<CharacterManager>();
                    if (targetChar != null && IsEnemy(playerManager.sheet, targetChar))
                    {
                        playerManager._playerStateMachine.SwitchState(new AttackState(targetChar));
                        return;
                    }
                }

                // Atualiza destino continuamente enquanto o botão estiver pressionado
                targetPosition = newTarget;
            }
        }

        Vector3 worldDelta = targetPosition - transform.position;
        float dist = worldDelta.magnitude;
        if (dist <= stopDistance)
        {
            motor.Stop();
            playerManager._playerStateMachine.SwitchState(new IdleState());
            return;
        }

        // direção em espaço world normalizada
        Vector3 worldDir = worldDelta.normalized;
        // converte para direção local (x = lateral, z = forward)
        Vector3 localDir = transform.InverseTransformDirection(worldDir);

        float speed = 0f;
        if (playerManager.sheet != null)
            speed = playerManager.sheet.Data.TotalSpeed;

        motor.Move(localDir, speed);
        motor.Rotation(worldDir);
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

    public override void ExitState(PlayerManager playerManager)
    {
        // assegura parar
        playerManager?._playerMotor?.Stop();
    }

    public override bool CanMove => true;
    public override bool CanRotate => true;
    public override bool CanCasting => false;
}

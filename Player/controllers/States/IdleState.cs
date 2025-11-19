using UnityEngine;

public class IdleState : StateBase
{
    public override void EnterState(PlayerManager playerManager)
    {
        
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;

        // Clique direito para mover (estilo MOBA/ARPG)
        if (InputManager.Instance != null && InputManager.Instance.interactButton)
        {
            if (playerManager._playerMouseController != null)
            {
                Vector3 targetPos = playerManager._playerMouseController.GetTargetPosition();
                GameObject targetObj = playerManager._playerMouseController.GetTargetObject();

                // Se clicou em inimigo, inicia ataque básico em movimento
                if (targetObj != null)
                {
                    CharacterManager targetChar = targetObj.GetComponent<CharacterManager>();
                    if (targetChar != null && IsEnemy(playerManager.sheet, targetChar))
                    {
                        playerManager._playerStateMachine.SwitchState(new AttackState(targetChar));
                        return;
                    }
                }

                // Senão, só move para a posição
                playerManager._playerStateMachine.SwitchState(new MoveState(targetPos));
                return;
            }
        }
    }

    public override void ExitState(PlayerManager playerManager)
    {
        
    }


    private bool IsEnemy(CharacterManager self, CharacterManager target)
    {
        if (self == null || target == null) return false;
        if (self == target) return false;

        // Player e Ally são aliados; Enemy é inimigo
        if (self.characterType == CharacterType.Player || self.characterType == CharacterType.Ally)
        {
            return target.characterType == CharacterType.Enemy;
        }
        return false;
    }

    public override bool CanMove => true;
    public override bool CanRotate => true;
    public override bool CanCasting => true;
}
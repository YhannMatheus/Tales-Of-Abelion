using UnityEngine;
using System;

public class CastState : StateBase
{
    private SkillData skillData;
    private bool isCasting = true;
    private float castTimer = 0f;
    private float maxCastTime = 5f; // segurança contra travamento

    public CastState(SkillData skillData)
    {
        this.skillData = skillData;
        if (skillData != null)
            maxCastTime = Mathf.Max(skillData.castTime, 0.5f) + 2f; // cast time + margem
    }

    public override void EnterState(PlayerManager playerManager)
    {
        if (playerManager != null && playerManager.skillManager != null)
        {
            // Assina eventos para saber quando a skill terminar
            playerManager.skillManager.OnSkillExecuted += OnSkillExecuted;
            playerManager.skillManager.OnSkillExecuteFailed += OnSkillFailed;
        }

        // Para movimento durante cast
        if (playerManager != null && playerManager._playerMotor != null)
        {
            playerManager._playerMotor.Stop();
        }
    }

    public override void UpdateState(PlayerManager playerManager)
    {
        if (playerManager == null) return;

        castTimer += UnityEngine.Time.deltaTime;

        // Se terminou o cast ou passou do tempo máximo, volta para Idle
        if (!isCasting || castTimer >= maxCastTime)
        {
            playerManager._playerStateMachine.SwitchState(new IdleState());
            return;
        }
    }

    public override void ExitState(PlayerManager playerManager)
    {
        if (playerManager != null && playerManager.skillManager != null)
        {
            playerManager.skillManager.OnSkillExecuted -= OnSkillExecuted;
            playerManager.skillManager.OnSkillExecuteFailed -= OnSkillFailed;
        }
    }

    private void OnSkillExecuted(int slotIndex, SkillContext ctx)
    {
        // Se a skill que executou é a nossa (compara por SkillData)
        if (ctx.Skill == skillData)
        {
            isCasting = false;
        }
    }

    private void OnSkillFailed(int slotIndex, string reason)
    {
        // Considera falha como término
        isCasting = false;
    }

    public override bool CanMove => false;
    public override bool CanRotate => true;
    public override bool CanCasting => false; // não pode usar outra skill durante cast
}

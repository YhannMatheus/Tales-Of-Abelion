using UnityEngine;

public abstract class SkillManager : MonoBehaviour
{
    public SkillExecutionController[] skillSlots = new SkillExecutionController[8]; // skils listada como usaveis pelo jogador
    public SkillExecutionController basicAttackSkill;
    public CharacterManager character;

    private void Update()
    {
        
    }

    private void UpdateCooldowns(float deltaTime)
    {   
        for (int i = 0; i < skillSlots.Length; i++)
        {
            if (skillSlots[i] == null) continue;

            var sc = skillSlots[i];
            if (sc != null)
                sc.TickCooldown(deltaTime);
        }
    }

    
}
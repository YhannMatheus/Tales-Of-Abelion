using UnityEngine;
[RequireComponent(typeof(CharacterManager))]
public class PlayerManager : MonoBehaviour
{
    public CharacterManager sheet;
    public PlayerSkillManager skillManager;
    public PlayerMotor _playerMotor;
    public PlayerStateMachine _playerStateMachine;


    protected void Awake()
    {
        sheet = GetComponent<CharacterManager>();
        skillManager = GetComponent<PlayerSkillManager>();
        _playerStateMachine.SetPlayerManager(this);
    }

    public void Update()
    {
        _playerStateMachine.UpdateState();
    }


}
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(PlayerMotor))]
[RequireComponent(typeof(PlayerClickDetect))]
[RequireComponent(typeof(CharacterAnimatorController))]
[RequireComponent(typeof(PlayerAbilityManager))]
public class PlayerManager : MonoBehaviour
{
    [Header("Components")]
    private PlayerMotor motor;
    private InputManager input;
    private CharacterAnimatorController animator;
    private PlayerClickDetect click;
    private Character character;
    private PlayerAbilityManager ability;
    private Vector3 moveDirection;

    private void Awake()
    {
        motor = GetComponent<PlayerMotor>();
        character = GetComponent<Character>();
        click = GetComponent<PlayerClickDetect>();
        ability = GetComponent<PlayerAbilityManager>();
        input = Object.FindFirstObjectByType<InputManager>();
        animator = GetComponent<CharacterAnimatorController>();
    }

    private void Update()
    {
        if (character.Data.IsAlive)
        {
            HandleMovement();
        }
        
        motor.ApplyGravity();
        
        animator?.UpdateMovementSpeed(motor.CurrentSpeedNormalized, motor.IsGrounded);

        if (input.interactButton)
        {
            HandleEventClick();
        }

        if (input.attackInput)
        {
            HandleAttack();
        }
    }

    private void HandleMovement()
    {
        moveDirection = new Vector3(input.horizontalInput, 0, input.verticalInput);

        if (moveDirection.magnitude > 0.1f)
        {
            motor.Move(moveDirection, character.Data.TotalSpeed);
            motor.Rotate(moveDirection);
        }
        else
        {
            motor.Stop();
        }
    }


    private void HandleEventClick()
    {
        GameObject clickedObject = click.GetClickedObject();

        if (clickedObject != null)
        {
            Event eventComponent = clickedObject.GetComponent<Event>();

            if (!motor.IsMoving)
            {
                motor.RotateToPosition(click.GetMousePosition());
            }

            if (eventComponent != null)
            {
                float distanceToObject = Vector3.Distance(transform.position, clickedObject.transform.position);

                if (distanceToObject <= eventComponent.minDistanceToTrigger)
                {
                    eventComponent.OnClick();
                }
            }
        }
    }

    private void HandleAttack()
    {
        Vector3 mouseWorldPos = click.GetMousePosition();

        Vector3 attackDirection = (mouseWorldPos - transform.position);
        attackDirection.y = 0;

        if (attackDirection.magnitude > 0.1f)
        {
            motor.Rotate(attackDirection);
            animator.TriggerAbility(0);
            ability.UseAbilityInSlot(0, click.GetMousePosition(), click.GetClickedObject());
        }
        
    }
    
}
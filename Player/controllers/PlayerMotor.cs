using System;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

[System.Serializable]
public class PlayerMotor
{   
    [Header("Movement Settings")]
    public float gravity = -9.81f;
    private float _verticalVelocity = 0f;
    public float groundedStick = 0f; // pequena força para manter o player preso ao chão
    public float rotationSpeed = 10f;

    [Header("Ground Check")]
    public float positionOffset = 0.1f;
    public float sphereRadius = 0.5f;
    private bool isGrounded;
    public LayerMask groundLayer;
    
    [Header("References")]
    public CharacterController characterController;
    public Collider playercollider;
    public Rigidbody playerRigidbody;

    //para completamente a movimentação do player
    public void Stop(){
        characterController.Move(Vector3.zero);
        IsMoving?.Invoke(false);
    }

    // Move o player na direção especificada com a velocidade dada
    public void Move(Vector3 direction, float speed)
    {
        if (characterController == null) return;

        // Atualiza velocidade vertical
        UpdateVerticalVelocity(Time.deltaTime);

        // Direção em espaço local do CharacterController
        Vector3 right = characterController.transform.right;
        Vector3 forward = characterController.transform.forward;
        Vector3 horizontal = (right * direction.x + forward * direction.z) * speed;

        Vector3 motion = horizontal + Vector3.up * _verticalVelocity;
        characterController.Move(motion * Time.deltaTime);
        IsMoving?.Invoke(horizontal.sqrMagnitude > 0f);
    }

    // Rotaciona o player na direção especificada
    public void Rotation(Vector3 lookDirection)
    {
        if (characterController == null) return;
        if (lookDirection == Vector3.zero) return;

        Quaternion target = Quaternion.LookRotation(lookDirection.normalized, Vector3.up);
        var t = characterController.transform;
        t.rotation = Quaternion.Slerp(t.rotation, target, rotationSpeed * Time.deltaTime);
    }

    // Deslocamento instantâneo do player para a posição final em um determinado tempo
    public IEnumerator InstantainDeslocation(Vector3 finalPosition, float duration)
    {
        if (characterController == null) yield break;

        // se duration <= 0, teleporta instantaneamente
        if (duration <= 0f)
        {
            characterController.transform.position = finalPosition;
            yield break;
        }

        Vector3 start = characterController.transform.position;
        float elapsed = 0f;
        Vector3 previous = start;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            Vector3 target = Vector3.Lerp(start, finalPosition, t);

            // move pelo CharacterController aplicando apenas o delta entre frames
            Vector3 delta = target - previous;
            characterController.Move(delta);
            previous = target;

            yield return null;
        }

        // garante posição final
        Vector3 remaining = finalPosition - previous;
        if (remaining.sqrMagnitude > 0f)
            characterController.Move(remaining);
    }

    // Deslocamento para cima (knock up)
    public IEnumerator KnockUp(Vector3 force, float minAirTime = 0f, float targetHeight = 0f)
    {
        if (characterController == null) yield break;

        // marca como no ar
        isGrounded = false;
        IsGrounded?.Invoke(isGrounded);

        // direitos horizontais em espaço world
        Vector3 horizontalImpulseLocal = new Vector3(force.x, 0f, force.z);
        Vector3 horizontalVelocity = characterController.transform.TransformDirection(horizontalImpulseLocal);

        // define velocidade vertical necessária para alcançar targetHeight (se especificado)
        if (targetHeight > 0f)
        {
            float neededV = Mathf.Sqrt(2f * -gravity * targetHeight); // gravity é negativo
            _verticalVelocity = Mathf.Max(_verticalVelocity, neededV);
        }
        else
        {
            _verticalVelocity = Mathf.Max(_verticalVelocity, force.y);
        }

        float elapsed = 0f;
        float startY = characterController.transform.position.y;
        bool reachedHeight = targetHeight <= 0f;

        // loop até: ter ficado pelo menos minAirTime e ter pousado (e se aplicável, ter alcançado a altura)
        const float safetyMax = 10f;
        while (true)
        {
            float dt = Time.deltaTime;
            elapsed += dt;

            // atualiza gravidade/vel vertical
            UpdateVerticalVelocity(dt);

            // aplica movimento (horizontal persistente + gravidade)
            Vector3 move = horizontalVelocity + Vector3.up * _verticalVelocity;
            characterController.Move(move * dt);

            // verifica se alcançou a altura desejada
            if (!reachedHeight && characterController.transform.position.y - startY >= targetHeight - 0.01f)
                reachedHeight = true;

            // condição de término: pousou, passou tempo mínimo e (se solicitado) alcançou altura
            if (characterController.isGrounded && _verticalVelocity <= 0f && elapsed >= minAirTime && reachedHeight)
                break;

            // segurança para evitar loop infinito
            if (elapsed >= safetyMax)
                break;

            yield return null;
        }

        // notifica movimento (pode ser removido ou ajustado conforme necessidade)
        IsMoving?.Invoke(false);
    }

    // Aplica a gravidade ao player
    public void ApplyGravity()
    {
        if (characterController == null) return;

        UpdateVerticalVelocity(Time.deltaTime);
        characterController.Move(Vector3.up * _verticalVelocity * Time.deltaTime);
        
    }

    private void UpdateVerticalVelocity(float deltaTime)
    {
        if (isGrounded)
        {
            if (_verticalVelocity < 0f)
                _verticalVelocity = groundedStick;
        }
        else
        {
            _verticalVelocity += gravity * deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = true;
            IsGrounded?.Invoke(isGrounded);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & groundLayer) != 0)
        {
            isGrounded = false;
            IsGrounded?.Invoke(isGrounded);
        }
    }

    // ____ Eventos ____
    public Action<bool> IsGrounded;
    public Action<bool> IsMoving;
}
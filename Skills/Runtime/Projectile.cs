using UnityEngine;

public class Projectile : MonoBehaviour
{
    private InstanceContext _context;
    private float _speed = 0f;
    private float _lifetime = 5f;
    private ProjectileBehavior _behavior = ProjectileBehavior.Linear;
    private Vector3 _direction = Vector3.forward;
    private CharacterManager _homingTarget = null;

    public void Initialize(InstanceContext context, float speed, float lifetime, ProjectileBehavior behavior, Vector3 direction, CharacterManager homingTarget = null)
    {
        _context = context;
        _speed = speed;
        _lifetime = lifetime;
        _behavior = behavior;
        _direction = direction.sqrMagnitude > 0f ? direction.normalized : transform.forward;
        _homingTarget = homingTarget;

        if (_lifetime > 0f)
            Destroy(gameObject, _lifetime);
    }

    private void Update()
    {
        if (_behavior == ProjectileBehavior.Homing && _homingTarget != null)
        {
            Vector3 toTarget = (_homingTarget.transform.position - transform.position).normalized;
            transform.forward = Vector3.Lerp(transform.forward, toTarget, Time.deltaTime * 8f);
            transform.position += transform.forward * _speed * Time.deltaTime;
            return;
        }

        transform.position += _direction * _speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        HandleCollision(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision.gameObject);
    }

    private void HandleCollision(GameObject other)
    {
        if (_context.Caster != null && other == _context.Caster.gameObject) return; // ignora o caster

        var cm = other.GetComponent<CharacterManager>();
        if (cm != null)
        {
            // Aplica dano
            if (_context.totalDamageAmount > 0f)
            {
                bool isMagical = _context.DamageType == DamageType.Magical;
                cm.TakeDamage(_context.totalDamageAmount, isMagical, _context.Caster);
            }

            // Aplica cura
            if (_context.totalHealAmount > 0f)
            {
                int healInt = Mathf.RoundToInt(_context.totalHealAmount);
                cm.Heal(healInt);
            }

            // Futuro: aplicar buffs/effects contidos em _context.Skill (buffModifiers, etc.)

            Destroy(gameObject);
        }
    }
}

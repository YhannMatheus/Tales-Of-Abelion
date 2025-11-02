using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Projectile : MonoBehaviour
{
    public float speed;
    public float maxRange;
    public float damageAmount;
    public Vector3 launchPosition;
    [HideInInspector] public GameObject Caster;
    [HideInInspector] public GameObject Target;
    private LayerMask allowedLayers = ~0;
    private TeamFilter teamFilter = TeamFilter.Enemies;
    private ProjectileType projType = ProjectileType.Skillshot;
    private Vector3 targetPosition = Vector3.zero;

    private Vector3 _startPosition;
    private Rigidbody _rb;

    public void Initialize(float damage, GameObject caster, GameObject target, float speed, float maxRange, Vector3 launchPos, LayerMask allowedLayers, TeamFilter teamFilter, ProjectileType projectileType, Vector3 targetPosition)
    {
        this.damageAmount = damage;
        this.Caster = caster;
        this.Target = target;
        this.speed = speed;
        this.maxRange = maxRange;
        this.launchPosition = launchPos;
        this.allowedLayers = allowedLayers;
        this.teamFilter = teamFilter;
        this.projType = projectileType;
        this.targetPosition = targetPosition;
        transform.position = launchPos;
        _startPosition = launchPos;

        if (Target != null)
        {
            Vector3 dir = (Target.transform.position - transform.position).normalized;
            if (dir.sqrMagnitude > 0f) transform.forward = dir;
        }
        else if (Caster != null)
        {
            transform.forward = Caster.transform.forward;
        }
    }

    // Sobrecarga compatível com versões anteriores: padrão Skillshot e sem posição alvo explícita
    public void Initialize(float damage, GameObject caster, GameObject target, float speed, float maxRange, Vector3 launchPos, LayerMask allowedLayers, TeamFilter teamFilter)
    {
        Initialize(damage, caster, target, speed, maxRange, launchPos, allowedLayers, teamFilter, ProjectileType.Skillshot, Vector3.zero);
    }

    public void ConfigureProjectile(ProjectileType type, Vector3 targetPos)
    {
        this.projType = type;
        this.targetPosition = targetPos;
        if (projType == ProjectileType.Skillshot && targetPos != Vector3.zero)
        {
            Vector3 dir = (targetPos - transform.position).normalized;
            if (dir.sqrMagnitude > 0f) transform.forward = dir;
        }
    }

    private void Start()
    {
        _startPosition = transform.position;
        _rb = GetComponent<Rigidbody>();
        if (_rb != null)
        {
            _rb.isKinematic = true;
            _rb.useGravity = false;
        }
    }

    private void Update()
    {
        if (projType == ProjectileType.TargetShot)
        {
            if (Target != null && Target.activeInHierarchy)
            {
                Vector3 desired = Target.transform.position;
                float step = speed * Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, desired, step);
                Vector3 look = (desired - transform.position);
                if (look.sqrMagnitude > 0.0001f) transform.forward = look.normalized;
                if (Vector3.Distance(transform.position, desired) <= 0.5f)
                {
                    OnHitTarget();
                }
            }
            else
            {
                AbilityPool.Release(gameObject, null);
            }
        }
        else // Skillshot: viaja em linha reta na direção definida (forward configurado no spawn)
        {
            transform.position += transform.forward * speed * Time.deltaTime;
        }

        if (Vector3.Distance(_startPosition, transform.position) >= maxRange)
        {
            AbilityPool.Release(gameObject, null);
        }
    }

    private void OnHitTarget()
    {
        if (Target == null) return;

        var targetChar = Target.GetComponent<Character>();
        if (targetChar != null)
        {
            targetChar.TakeDamage(damageAmount);
        }

        AbilityPool.Release(gameObject, null);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == Caster) return;

    // Skillshot: causa dano no primeiro inimigo válido atingido (filtrado por layer/time)
        if (projType == ProjectileType.Skillshot)
        {
            if (((1 << other.gameObject.layer) & allowedLayers.value) == 0) return;
            var ch = other.GetComponentInParent<Character>();
            if (ch != null)
            {
                var casterChar = Caster != null ? Caster.GetComponentInParent<Character>() : null;
                if (teamFilter == TeamFilter.Enemies && ch.characterType == casterChar?.characterType) return;
                if (teamFilter == TeamFilter.Allies && ch.characterType != casterChar?.characterType) return;
                ch.TakeDamage(damageAmount);
                AbilityPool.Release(this.gameObject, null);
            }
            return;
        }

    // TargetShot: ignora colisões incidentais; só dispara se atingir o alvo pretendido
        if (projType == ProjectileType.TargetShot && Target != null)
        {
            var targetChar = Target.GetComponent<Character>();
            if (targetChar != null && other.GetComponentInParent<Character>() == targetChar)
            {
                OnHitTarget();
            }
        }
    }
}

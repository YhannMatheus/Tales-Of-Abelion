using UnityEngine;
using System;

public abstract class InstanceBase : MonoBehaviour
{
    protected InstanceContext Context { get; private set; }
    // Parâmetros de movimento/comportamento fornecidos pelo spawner
    protected Vector3 Direction { get; private set; } = Vector3.zero;
    protected CharacterManager HomingTarget { get; private set; } = null;
    protected float Speed { get; private set; } = 0f;
    protected float Lifetime { get; private set; } = 0f;
    protected ProjectileBehavior Behavior { get; private set; } = ProjectileBehavior.Linear;

    private float _age = 0f;
    private bool _initialized = false;
    private Collider _projCollider = null;
    private Collider _casterCollider = null;
    private bool _collisionIgnoredWithCaster = false;

    // Eventos
    public event Action<CharacterManager> OnHit;
    public event Action OnExpired;

    // Expor status
    public bool IsInitialized => _initialized;

    // Initialize compatível com versões anteriores (sem parâmetros de movimento)
    public virtual void Initialize(InstanceContext context)
    {
        Context = context;
        _age = 0f;
        _initialized = true;
        StartInstance();
    }

    // Inicializa com parâmetros de movimento/homing (chamado pelo spawner)
    public virtual void Initialize(InstanceContext context, Vector3 direction, CharacterManager homingTarget, float speed, float lifetime, ProjectileBehavior behavior = ProjectileBehavior.Linear)
    {
        Context = context;
        Direction = direction;
        HomingTarget = homingTarget;
        Speed = speed;
        Lifetime = lifetime;
        Behavior = behavior;
        _age = 0f;
        _initialized = true;

        // garantir que o collider seja trigger para evitar bloqueio físico
        var col = GetComponent<Collider>();
        if (col != null) col.isTrigger = true;
        _projCollider = col;

        StartInstance();
    }

    protected virtual void Start()
    {
        if (!_initialized)
        {
            StartInstance();
            _initialized = true;
        }
    }

    protected virtual void Update()
    {
        if (_initialized)
        {
            _age += Time.deltaTime;
            if (Lifetime > 0f && _age >= Lifetime)
            {
                OnExpired?.Invoke();
                Destroy(gameObject);
                return;
            }

            Move(Time.deltaTime);
        }

        UpdateInstance();
    }

    protected virtual void OnDestroy()
    {
        // reativar colisão entre projétil e caster ao destruir a instância
        if (_collisionIgnoredWithCaster && _casterCollider != null && _projCollider != null)
        {
            Physics.IgnoreCollision(_casterCollider, _projCollider, false);
            _collisionIgnoredWithCaster = false;
        }

        EndInstance();
    }

    protected abstract void StartInstance(); // como o objeto será instanciado no mundo
    
    protected abstract void UpdateInstance(); // como o objeto se comporta no mundo
    
    protected abstract void EndInstance(); // como o objeto será destruido no mundo
    
    // Aplica efeitos definidos no contexto da instância (OnHit / OverTime)
    protected virtual void ApplyEffects(CharacterManager target)
    {
        if (Context.Effects == null || Context.Effects.Count == 0) return;
        foreach (var effect in Context.Effects)
        {
            if (effect == null || effect.effectBehavior == null) continue;
            if (effect.effectTiming == EffectTiming.OnHit || effect.effectTiming == EffectTiming.OverTime)
            {
                effect.effectBehavior.Initialize(effect, target, Context.Caster, Context.SkillLevel);
            }
        }
    }

    protected virtual void OnCollisionEnter(Collision other)
    {
        if (other == null) return;

        if (other.gameObject.TryGetComponent<CharacterManager>(out var targetCharacter))
        {
            OnHit?.Invoke(targetCharacter);
            ApplyEffects(targetCharacter);
            if (Context.consumedOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other == null) return;

        if (other.gameObject.TryGetComponent<CharacterManager>(out var targetCharacter))
        {
            OnHit?.Invoke(targetCharacter);
            ApplyEffects(targetCharacter);
            if (Context.consumedOnHit)
            {
                Destroy(gameObject);
            }
        }
    }

    // Auxiliar usado pelo movimento/homing ao alcançar o alvo
    protected void ApplyEffectsOnHit(GameObject hitObject)
    {
        if (hitObject == null) return;
        if (!hitObject.TryGetComponent<CharacterManager>(out var targetCm)) return;
        ApplyEffects(targetCm);
    }

    // Movimento separado para que classes derivadas possam sobrescrever comportamentos específicos
    protected virtual void Move(float deltaTime)
    {
        if (HomingTarget != null && HomingTarget.gameObject != null)
        {
            Vector3 targetPos = HomingTarget.transform.position;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, Speed * deltaTime);
            float dist = Vector3.Distance(transform.position, targetPos);
            const float defaultImpactDistance = 0.5f;
            if (dist <= defaultImpactDistance)
            {
                OnReachedTarget(HomingTarget);
            }
        }
        else if (Direction != Vector3.zero && Speed > 0f)
        {
            transform.position += Direction.normalized * Speed * deltaTime;
        }
    }

    // Chamado quando o movimento/homing determina que a instância alcançou um alvo personagem
    protected virtual void OnReachedTarget(CharacterManager target)
    {
        if (target == null) return;
        OnHit?.Invoke(target);
        ApplyEffects(target);
        if (Context.SkillData != null && Context.SkillData.consumeOnHit)
            Destroy(gameObject);
    }

    // helper para que o spawner registre a colisão ignorada entre projétil e caster
    public void RegisterIgnoredCollisionWithCaster(Collider casterCollider)
    {
        if (casterCollider == null || _projCollider == null) return;
        _casterCollider = casterCollider;
        Physics.IgnoreCollision(_casterCollider, _projCollider, true);
        _collisionIgnoredWithCaster = true;
    }
}
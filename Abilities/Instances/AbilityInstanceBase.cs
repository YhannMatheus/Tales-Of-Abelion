using UnityEngine;

public abstract class AbilityInstanceBase : MonoBehaviour
{
    protected Ability AbilityData { get; private set; }
    protected AbilityContext Context { get; private set; }

    public virtual void Initialize(Ability abilityData, AbilityContext context)
    {
        AbilityData = abilityData;
        Context = context;
        OnInitialized();
        
        var caster = Context != null ? Context.Caster : null;
        if (caster != null)
        {
            var pam = caster.GetComponent<PlayerAbilityManager>();
            if (pam != null)
            {
                pam.RegisterInstance(this);
            }
        }
    }

    protected virtual void OnInitialized() { }

    public virtual void Cancel()
    {
        var caster = Context != null ? Context.Caster : null;
        if (caster != null)
        {
            var anim = caster.GetComponent<CharacterAnimatorController>();
            if (anim != null) anim.EndAbility();

            var pam = caster.GetComponent<PlayerAbilityManager>();
            if (pam != null) pam.CancelInstance(this);
        }

        Destroy(gameObject);
    }

    public virtual void Finish()
    {
        var caster = Context != null ? Context.Caster : null;
        if (caster != null)
        {
            var anim = caster.GetComponent<CharacterAnimatorController>();
            if (anim != null) anim.EndAbility();

            var pam = caster.GetComponent<PlayerAbilityManager>();
            if (pam != null) pam.CancelInstance(this);
        }

        Destroy(gameObject);
    }

    public virtual void Abort()
    {
        var caster = Context != null ? Context.Caster : null;
        if (caster != null)
        {
            var anim = caster.GetComponent<CharacterAnimatorController>();
            if (anim != null) anim.EndAbility();

            var pam = caster.GetComponent<PlayerAbilityManager>();
            if (pam != null) pam.CancelInstance(this);
        }

        Destroy(gameObject);
    }

    public virtual void OnHit(GameObject target) { }
}

using Unity.VisualScripting;
using UnityEngine;

public abstract class InstanceBase : MonoBehaviour
{
    protected InstanceContext Context { get; private set; }
    public void Initialize(InstanceContext context)
    {
        Context = context;
        StartInstance();
    }
    private void Update()
    {
        UpdateInstance();
    }

    public abstract void StartInstance(); // como o projetil será instanciado no mundo
    
    public abstract void UpdateInstance(); // como o projetil se comporta no mundo
    
    public abstract void EndInstance(); // como o projetil será destruido no mundo

    public virtual void ApplyEffects(CharacterManager target)
    {
    }

    public virtual void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == Context.Caster.gameObject && Context.TargetCharacter != Context.Caster) return; // ignorar colisão com o caster
        if(Context.Skill.consumeOnHit)
        {
            
            EndInstance();
        }
    }
}
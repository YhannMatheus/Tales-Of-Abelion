using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Buff", menuName = "Buff Data")]
public class BuffData : ScriptableObject
{
    [Header("Buff Info")]
    public string buffName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;

    [Header("Buff Properties")]
    public float duration;
    public bool isDebuff = false;
    public bool isStackable = false;
    [Header("Distribution")]
    public bool distributeOverDuration = false;
    public float tickInterval = 1f;

    [Header("Modifiers")]
    public List<Modifier> modifiers = new List<Modifier>();
    
    [Header("Visual & Audio")]
    public GameObject buffEffectPrefab;
    public AudioClip buffSound;

    public void ApplyTo(Character target)
    {
        if (target == null) return;

        BuffManager buffManager = target.GetComponent<BuffManager>();
        if (buffManager != null)
        {
            buffManager.ApplyBuff(buffName, duration, modifiers, isDebuff, distributeOverDuration, tickInterval);
        }
    }
}

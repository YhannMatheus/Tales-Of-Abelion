using UnityEngine;

// BuffData - APENAS informações visuais e de áudio
// Duração e modificadores são configurados no SkillData!
[CreateAssetMenu(fileName = "New Buff", menuName = "Buff Data")]
public class BuffData : ScriptableObject
{
    [Header("Buff Info")]
    public string buffName;
    [TextArea(3, 5)]
    public string description;
    public Sprite icon;
    public bool isDebuff = false;

    [Header("Visual & Audio")]
    public GameObject buffEffectPrefab;
    public AudioClip buffSound;
    
    [Header("Info")]
    [Tooltip("Duração e modificadores são configurados no SkillData")]
    [TextArea(2, 3)]
    public string info = "Este BuffData é apenas VISUAL.\nConfigure duração e valores no SkillData:\n- buffDuration\n- buffModifiers\n- distributeBuffOverTime";
}

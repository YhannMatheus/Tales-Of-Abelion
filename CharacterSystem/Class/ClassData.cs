using System.Collections.Generic;
using UnityEditor.Animations;
using UnityEngine;

[CreateAssetMenu(fileName = "NewClassData", menuName = "CharacterManager System/ClassData")]
public class ClassData : ScriptableObject
{
    [Header("Basic Information")]
    public string className;
    public AnimatorController animatorController;

    [TextArea]
    public string classDescription;
    public int baseMaxHealth;
    public int baseHealthRegen;
    public int healthMultiplierPerEndurance;
    public int healthMultiplierPerLevel;
    public EnergyType energyType;
    public int baseMaxEnergy;
    public int baseEnergyRegen;

    [Header("Attributes Bonus")]
    public int StrengthBonus;
    public int DexterityBonus;
    public int IntelligenceBonus;
    public int CharismaBonus;
    public int EnduranceBonus;

    [Header("Skills")]
    public List<SkillData> classSkillTree = new List<SkillData>();

    [Header("Base Modifiers")] 
    public float physicalDamage;
    public float physicalResistence;
    public float magicalDamage;
    public float magicalResistence;
    public float criticalChance;
    public float criticalDamage;
    public float attackSpeed;
    public float luck;

}
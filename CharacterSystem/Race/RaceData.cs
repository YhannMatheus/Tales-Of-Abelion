using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[CreateAssetMenu(fileName = "NewRaceData", menuName = "CharacterManager System/RaceData")]
public class RaceData : ScriptableObject
{
    [Header("Basic Information")]
    public string raceName;
    [TextArea(5,10)]
    public string raceDescription;

    [Header("Attributes")]
    public int Strength;
    public int Dexterity;
    public int Intelligence;
    public int Charisma;
    public int Endurance;

    [Header("Skills")]
    public List<SkillData> raceSkills = new List<SkillData>();
    public float moveSpeed;

    [Header("Visual Information")]
    public GameObject[] racePrefabs = new GameObject[2]; // o modelo feminino ou masculino serão escolidos com base no index 0 - Masculino, 1 - Feminino
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LegacyDuplicates
{
    [System.Serializable]
    public class SkillNode
    {
        public string id;
        public Ability ability;
        public int skillPointCost;
        public int maxRank = 1;
        public List<string> connectedNodeIds = new List<string>();
    }

    [CreateAssetMenu(menuName = "Abilities/SkillTree")]
    public class SkillTree : ScriptableObject
    {
        [Header("Skill Tree Settings")]
        public string treeName;
        public Sprite treeIcon;
        public List<SkillNode> skillNodes = new List<SkillNode>();

        public SkillNode GetNodeById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return skillNodes.FirstOrDefault(n => n != null && n.id == id);
        }

        [ContextMenu("Ensure Unique Node IDs")]
        public void EnsureUniqueIds()
        {
            if (skillNodes == null) return;

            foreach (var n in skillNodes)
            {
                if (n == null) continue;
                if (string.IsNullOrEmpty(n.id)) n.id = Guid.NewGuid().ToString();
            }

            var groups = skillNodes.Where(x => x != null).GroupBy(x => x.id).Where(g => g.Count() > 1);
            foreach (var g in groups)
            {
                bool first = true;
                foreach (var node in g)
                {
                    if (first) { first = false; continue; }
                    node.id = Guid.NewGuid().ToString();
                }
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        private void OnValidate()
        {
            EnsureUniqueIds();
        }

    }
}
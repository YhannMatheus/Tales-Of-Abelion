using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSkillTree : MonoBehaviour
{
    // Referência para a definição da árvore de habilidades (dados imutáveis)
    [Header("Data")]
    public SkillTree skillTree;
    public int availableSkillPoints = 0;

    [System.Serializable]
    private class NodeProgress
    {
        public string id;
        public int rank;
    }

    [SerializeField] private List<NodeProgress> progress = new List<NodeProgress>();

    // Inicializa o componente com uma SkillTree e pontos iniciais
    public void Initialize(SkillTree tree, int startingPoints = 0)
    {
        skillTree = tree;
        availableSkillPoints = startingPoints;
        progress.Clear();
    }

    // Retorna o rank atual de um nó (0 se não desbloqueado)
    public int GetNodeRank(string nodeId)
    {
        var n = progress.Find(x => x.id == nodeId);
        return n != null ? n.rank : 0;
    }

    // Retorna true se o nó estiver com rank >= 1
    public bool IsNodeUnlocked(string nodeId)
    {
        return GetNodeRank(nodeId) > 0;
    }

    // Busca o SkillNode na SkillTree por id (retorna null se não existir)
    private SkillNode FindNode(string nodeId)
    {
        if (skillTree == null) return null;
        return skillTree.skillNodes.FirstOrDefault(n => n.id == nodeId);
    }

    // Verifica se é possível desbloquear o nó: respeita maxRank e pré-requisitos
    public bool CanUnlock(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node == null) return false;

        int currentRank = GetNodeRank(nodeId);
        if (currentRank >= node.maxRank) return false;

        // se não há pré-requisitos, pode desbloquear
        if (node.connectedNodeIds == null || node.connectedNodeIds.Count == 0) return true;

        // requer ao menos um nó conectado desbloqueado (política simples)
        foreach (var req in node.connectedNodeIds)
        {
            if (IsNodeUnlocked(req)) return true;
        }

        return false;
    }

    // Retorna a lista de nós que atualmente podem ser desbloqueados
    public List<SkillNode> GetAvailableNodes()
    {
        if (skillTree == null) return new List<SkillNode>();
        var list = new List<SkillNode>();
        foreach (var n in skillTree.skillNodes)
        {
            if (CanUnlock(n.id)) list.Add(n);
        }
        return list;
    }

    // Tenta desbloquear/incrementar o rank do nó, consumindo pontos. Retorna true se bem sucedido.
    public bool UnlockNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node == null) return false;

        if (!CanUnlock(nodeId)) return false;
        if (availableSkillPoints < node.skillPointCost) return false;

        var p = progress.Find(x => x.id == nodeId);
        if (p == null)
        {
            p = new NodeProgress { id = nodeId, rank = 0 };
            progress.Add(p);
        }

        if (p.rank >= node.maxRank) return false;

        p.rank++;
        availableSkillPoints -= node.skillPointCost;
        return true;
    }

    // Reverte um ponto do nó e devolve pontos ao jogador. Retorna true se bem sucedido.
    public bool RefundNode(string nodeId)
    {
        var node = FindNode(nodeId);
        if (node == null) return false;

        var p = progress.Find(x => x.id == nodeId);
        if (p == null || p.rank <= 0) return false;

        p.rank--;
        availableSkillPoints += node.skillPointCost;

        if (p.rank == 0)
        {
            progress.Remove(p);
        }

        return true;
    }

    // Retorna os ids dos nós que o jogador desbloqueou (útil para salvar/JSON)
    public IEnumerable<string> GetUnlockedNodeIds()
    {
        return progress.Select(p => p.id).ToList();
    }

    // Adiciona pontos de skill ao jogador
    public void AddSkillPoints(int amount)
    {
        availableSkillPoints += amount;
    }
}

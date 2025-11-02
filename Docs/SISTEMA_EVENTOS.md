# Sistema de Eventos e Interações - Status e Tarefas

## 📋 Visão Geral
Sistema de objetos interativos no mundo usando herança de classe abstrata `Event` com feedback visual e validação de distância.

## ✅ Componentes Completos

### Event (Base) (`Assets/Scripts/Events/Event.cs`)
- ✅ Classe abstrata para interações
- ✅ Método abstrato OnClick()
- ✅ Feedback visual com material outline
- ✅ OnMouseEnter/Exit para hover
- ✅ Validação de distância mínima
- ✅ Gizmos para visualizar alcance
- ✅ Flag para mostrar/esconder gizmo

### Eventos Implementados
- ✅ DoorEvent - portas
- ✅ LeverEvent - alavancas
- ✅ NpcSpeakerEvent - NPCs que falam
- ✅ ShopEvent - lojas
- ⚠️ Verificar implementação completa de cada um

## ⚠️ Funcionalidades Parciais

### Sistema de Portas
- ✅ Estrutura básica criada
- ❌ Lógica de abrir/fechar não verificada
- ❌ Animação de porta

**Tarefas Pendentes:**
- [ ] Verificar implementação de DoorEvent
- [ ] Animação de abertura/fechamento
- [ ] Som de porta
- [ ] Portas trancadas (requerem chave)
- [ ] Portas que trancam ao fechar
- [ ] Portas de mão dupla
- [ ] Portas que requerem nível/quest

### Sistema de Alavancas
- ✅ Estrutura básica criada
- ❌ Lógica de ativação não verificada
- ❌ Conexão com outros objetos

**Tarefas Pendentes:**
- [ ] Verificar implementação de LeverEvent
- [ ] Sistema de conexão com outros eventos
- [ ] Animação de alavanca
- [ ] Som de ativação
- [ ] Estado persistente (on/off)
- [ ] Puzzles com múltiplas alavancas
- [ ] Reset automático após tempo

### Sistema de Diálogo (NPC)
- ✅ NpcSpeakerEvent criado
- ❌ Sistema de diálogo não implementado

**Tarefas Pendentes:**
- [ ] Verificar implementação de NpcSpeakerEvent
- [ ] Sistema de diálogo com UI
- [ ] Árvore de diálogo/escolhas
- [ ] Localização de textos
- [ ] Portraits de NPCs
- [ ] Animações durante diálogo
- [ ] Som de voz/texto typewriter
- [ ] Quest dialogs
- [ ] Relationship/reputation system

### Sistema de Loja
- ✅ ShopEvent criado
- ❌ Sistema de compra/venda não implementado

**Tarefas Pendentes:**
- [ ] Verificar implementação de ShopEvent
- [ ] UI de loja
- [ ] Inventário do mercador
- [ ] Sistema de preços
- [ ] Compra e venda de itens
- [ ] Estoque limitado/infinito
- [ ] Restock de itens
- [ ] Descontos por reputação
- [ ] Itens especiais por nível/quest

## ❌ Funcionalidades Não Implementadas

### Tipos de Eventos Adicionais
**Tarefas Pendentes:**
- [ ] ChestEvent - baús de loot
- [ ] TrapEvent - armadilhas
- [ ] TeleportEvent - portais/teletransporte
- [ ] CraftingStationEvent - estações de craft
- [ ] BedEvent - descanso/save point
- [ ] SignEvent - placas informativas
- [ ] QuestGiverEvent - NPCs de quest
- [ ] BankEvent - armazenamento
- [ ] TrainerEvent - treinar habilidades
- [ ] ShrineEvent - buffs temporários

### Sistema de Baús e Loot
**Tarefas Pendentes:**
- [ ] ChestEvent com inventário
- [ ] Animação de abertura
- [ ] Loot table
- [ ] Chances de raridade
- [ ] Baús trancados
- [ ] Armadilhas em baús
- [ ] Respawn de loot
- [ ] Loot compartilhado em grupo

### Sistema de Armadilhas
**Tarefas Pendentes:**
- [ ] TrapEvent básico
- [ ] Diferentes tipos (espinhos, flecha, etc)
- [ ] Detecção de armadilhas (por skill)
- [ ] Desarmamento de armadilhas
- [ ] Ativação por proximidade/pressão
- [ ] Dano e efeitos de armadilha
- [ ] Reset de armadilhas

### Sistema de Quest Objects
**Tarefas Pendentes:**
- [ ] Objetos coletáveis para quests
- [ ] Objetivos de interação
- [ ] Objetivos de investigação
- [ ] Tracking de progresso
- [ ] Indicadores visuais de quest
- [ ] Spawn condicional por quest

### Sistema de Puzzles
**Tarefas Pendentes:**
- [ ] Framework genérico de puzzle
- [ ] Sequências (ativar em ordem)
- [ ] Combinações (múltiplas alavancas)
- [ ] Puzzles de rotação
- [ ] Puzzles de posicionamento
- [ ] Reward ao completar
- [ ] Reset de puzzle

### Sistema de Portais/Teleporte
**Tarefas Pendentes:**
- [ ] TeleportEvent básico
- [ ] Destino configurável
- [ ] Custo de teleporte (gold/item)
- [ ] Cooldown de teleporte
- [ ] Rede de fast travel
- [ ] Descoberta de waypoints
- [ ] UI de seleção de destino

## 🔧 Melhorias Sugeridas

### Feedback Visual
- [ ] Partículas ao interagir
- [ ] Pulse effect em objetos interativos
- [ ] Indicator 3D acima de objetos (ícone flutuante)
- [ ] Trail effect ao abrir/ativar
- [ ] Screen space outline em vez de material

### Feedback Sonoro
- [ ] Sons únicos por tipo de evento
- [ ] Som ambiente para objetos mágicos
- [ ] Feedback de sucesso/falha
- [ ] Som de unlock

### UX
- [ ] Tooltip ao passar mouse sobre objeto
- [ ] Indicador de tecla para interagir
- [ ] Mensagem quando muito longe
- [ ] Mensagem quando requisitos não atendidos
- [ ] Preview do resultado da interação

### Acessibilidade
- [ ] Highlight mais forte para daltonismo
- [ ] Opção de aumentar alcance de interação
- [ ] Som de proximidade de objeto interativo
- [ ] Opção de auto-interagir ao se aproximar

### Performance
- [ ] Desabilitar hover check quando longe
- [ ] LOD para objetos interativos distantes
- [ ] Pooling de VFX de interação
- [ ] Culling de eventos fora da câmera

### Multiplayer (Preparação)
- [ ] Sincronização de estado de eventos
- [ ] Interações simultâneas (quem chegou primeiro)
- [ ] Lock temporário durante interação
- [ ] Broadcast de mudanças para todos
- [ ] Validação server-side
- [ ] Loot instancing (cada jogador vê seu loot)

## 📝 Notas Importantes

### Padrão de Implementação
```csharp
public class MeuEvento : Event
{
    public override void OnClick()
    {
        // Sua lógica aqui
        // PlayerManager já valida distância antes de chamar
    }
}
```

### Fluxo de Interação
1. Player clica com botão direito
2. PlayerClickDetect detecta objeto clicado
3. PlayerManager valida distância
4. PlayerManager rotaciona player para objeto
5. Chama event.OnClick() se dentro do alcance
6. Event executa sua lógica específica

### Distância Mínima
- Configurável por evento via `minDistanceToTrigger`
- Visualizada com Gizmo amarelo no editor
- PlayerManager valida antes de executar

### Material Outline
- Material swap simples para feedback
- OnMouseEnter → aplica outlineMaterial
- OnMouseExit → restaura originalMaterial
- Requer configuração manual no Inspector

### Gizmos
- Esfera wireframe mostra alcance de interação
- Cor amarela por padrão
- Pode ser desabilitado com `showDistanceGizmo = false`

### Extensibilidade
Sistema muito simples de estender:
1. Crie classe herdando de Event
2. Override OnClick()
3. Configure minDistanceToTrigger
4. Adicione ao GameObject com collider

### Limitações Atuais
- Usa OnMouseEnter/Exit (requer collider e não funciona em UI)
- Material swap simples (melhor usar Shader outline)
- Sem sistema de requisitos (nível, quest, item)
- Sem cooldown entre interações
- Sem animação do player ao interagir

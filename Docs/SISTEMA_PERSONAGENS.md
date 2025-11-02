# Sistema de Personagens - Status e Tarefas

## 📋 Visão Geral
Sistema central de gerenciamento de personagens (jogador, inimigos, NPCs, aliados) com stats, progressão, eventos e regeneração.

## ✅ Componentes Completos

### Character (`Assets/Scripts/CharacterSystem/Character.cs`)
- ✅ Gerenciamento de CharacterData
- ✅ Sistema de eventos (OnHealthChanged, OnEnergyChanged, OnLevelUp, OnDeath, OnRevive)
- ✅ Regeneração automática de vida e energia
- ✅ Sistema de dano com resistências físicas/mágicas
- ✅ Sistema de cura
- ✅ Gasto e restauração de energia
- ✅ Sistema de experiência e level up
- ✅ Aplicação/remoção de bônus de equipamento
- ✅ Sistema de morte por tipo de personagem
- ✅ Distribuição de XP em área para inimigos
- ✅ Sistema de drop de loot
- ✅ Sistema de revive
- ✅ Gizmos para visualizar alcance de XP (inimigos)

### CharacterData (`Assets/Scripts/CharacterSystem/CharacterData.cs`)
- ✅ Struct serializável com todos os stats
- ✅ Propriedades calculadas (Total___) combinando base + equipamento + externo
- ✅ Sistema de inicialização combinando raça + classe
- ✅ Flags IsAlive, IsFullHealth, IsFullEnergy
- ✅ Suporte para modificadores de equipamento
- ✅ Suporte para modificadores externos (buffs)

### ClassData (`Assets/Scripts/CharacterSystem/Class/ClassData.cs`)
- ✅ ScriptableObject com menu de criação
- ✅ Stats base por classe
- ✅ Bônus de atributos
- ✅ AnimatorController específico
- ✅ Tipo de energia (mana, rage, etc)
- ✅ Skill tree por classe
- ✅ Multiplicadores de crescimento

### RaceData (`Assets/Scripts/CharacterSystem/Race/RaceData.cs`)
- ✅ ScriptableObject com menu de criação
- ✅ Atributos raciais
- ✅ Habilidades raciais
- ✅ Velocidade de movimento
- ✅ Prefabs separados por gênero (masculino/feminino)

## ⚠️ Funcionalidades Parciais

### Sistema de Equipamento
- ✅ Aplicação de bônus de equipamento
- ✅ Remoção de bônus
- ❌ Sem sistema de inventário visual
- ❌ Sem slots de equipamento definidos

**Tarefas Pendentes:**
- [ ] Criar sistema de inventário
- [ ] Definir slots de equipamento (cabeça, peito, armas, etc)
- [ ] Implementar UI de equipamento
- [ ] Adicionar validação de requisitos (nível, classe)
- [ ] Sistema de durabilidade de itens
- [ ] Sistema de sets de equipamento

### Sistema de Atributos
- ✅ Atributos base implementados
- ✅ Pontos livres para distribuição
- ❌ UI para distribuir pontos não implementada

**Tarefas Pendentes:**
- [ ] Criar UI de distribuição de atributos
- [ ] Implementar validação de distribuição
- [ ] Adicionar preview de stats antes de confirmar
- [ ] Sistema de reset de atributos
- [ ] Validar se pontos livres estão corretos por nível

### Sistema de Morte
- ✅ Morte de jogador com overlay
- ✅ Morte de inimigo com XP e drops
- ⚠️ Morte de aliado/NPC apenas comentada

**Tarefas Pendentes:**
- [ ] Implementar lógica completa para morte de aliados
- [ ] Implementar lógica completa para morte de NPCs
- [ ] Sistema de penalidade de morte para jogador
- [ ] Sistema de recuperação de corpo
- [ ] Opções de revive (checkpoint, aliado, item)

## ❌ Funcionalidades Não Implementadas

### Sistema de Salvamento
**Tarefas Pendentes:**
- [ ] Serialização de CharacterData para JSON
- [ ] Sistema de save/load por slot
- [ ] Auto-save em checkpoints
- [ ] Salvamento de inventário
- [ ] Salvamento de progresso de quests
- [ ] Validação de integridade dos dados salvos

### Sistema de Progressão Avançada
**Tarefas Pendentes:**
- [ ] Fórmula de XP por nível (atualmente linear?)
- [ ] Cap de nível máximo
- [ ] Recompensas por nível (além de stats)
- [ ] Sistema de prestígio/reincarnação
- [ ] Achievements/conquistas

### Sistema de Party/Grupo
**Tarefas Pendentes:**
- [ ] Gerenciamento de membros do grupo
- [ ] Compartilhamento de XP em grupo
- [ ] Sistema de roles (tank, DPS, healer)
- [ ] UI de grupo
- [ ] Sincronização para multiplayer

## 🔧 Melhorias Sugeridas

### Performance
- [ ] Pool de objetos para drops
- [ ] Otimizar FindNearbyPlayers com cache
- [ ] Limitar frequência de regeneração em grupos grandes

### Balanceamento
- [ ] Ajustar taxa de regeneração por classe
- [ ] Balancear curva de XP
- [ ] Ajustar alcance de distribuição de XP
- [ ] Balancear chance de drop de itens

### UX
- [ ] Feedback visual ao ganhar XP
- [ ] Animação de level up
- [ ] Indicador de vida baixa
- [ ] Som ao receber dano/cura
- [ ] Números flutuantes de dano/cura

### Multiplayer (Preparação)
- [ ] Identificador único de personagem (UUID)
- [ ] Sincronização de stats via rede
- [ ] Autoridade de servidor para validação
- [ ] Anti-cheat básico
- [ ] Reconciliação de estado

## 📝 Notas
- CharacterType define comportamento de morte e interações
- Propriedades calculadas (Total___) facilitam modificadores temporários
- Sistema de eventos permite UI e IA reagirem a mudanças
- Regeneração baseada em timer (1 segundo por tick)
- Bônus externos são zerados quando buffs expiram

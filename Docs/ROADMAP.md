# Roadmap de Desenvolvimento - Tales Of Abelion

## 📊 Status Geral dos Sistemas

### ✅ Completos (80-100%)
- Sistema de Personagens (Character, CharacterData, Race, Class)
- Sistema de Buffs/Debuffs (BuffManager, mecânicas core)
- Sistema de Habilidades (Ability base, tipos, instâncias)

### ⚠️ Parcialmente Implementados (40-80%)
- Sistema de IA (estrutura completa, estados vazios)
- Sistema de Player (movimento básico, falta polish)
- Sistema de Eventos (estrutura pronta, eventos específicos vazios)

### ❌ Não Implementados (0-40%)
- Sistema de UI (apenas mencionado)
- Sistema de Inventário (apenas hooks)
- Sistema de Quests
- Sistema de Salvamento
- Sistema de Multiplayer
- Sistema de Audio
- Sistema de Localization

## 🎯 Prioridades Sugeridas

### Phase 1 - Core Gameplay (Sistemas Essenciais)
**Objetivo**: Ter um loop de jogo jogável

1. **Completar Estados de IA** [ALTA PRIORIDADE]
   - [x] Implementar ChaseState
   - [x] Implementar AttackState
   - [x] Implementar FleeState (opcional)
   - [x] Implementar DeadState
   - [x] Completar IdleState e PatrolState
   - [x] Testar transições entre estados
   - **Estimativa**: 1-2 semanas

2. **Sistema de Combat Feel** [ALTA PRIORIDADE]
   - [ ] Feedback de dano (shake, flash, números)
   - [ ] Animações de ataque básicas
   - [ ] Sons de combate
   - [ ] VFX de impacto
   - **Estimativa**: 1 semana

3. **UI Básica** [ALTA PRIORIDADE]
   - [ ] HUD (vida, energia, XP)
   - [ ] Hotbar de habilidades
   - [ ] Indicadores de buff/debuff
   - [ ] Minimapa (opcional)
   - **Estimativa**: 1-2 semanas

4. **Sistema de Câmera** [MÉDIA PRIORIDADE]
   - [ ] Controle com mouse
   - [ ] Zoom
   - [ ] Colisão
   - **Estimativa**: 3-5 dias

### Phase 2 - Content Systems (Sistemas de Conteúdo)
**Objetivo**: Adicionar profundidade ao jogo

5. **Sistema de Inventário** [ALTA PRIORIDADE]
   - [ ] Estrutura de dados
   - [ ] UI de inventário
   - [ ] Drag and drop
   - [ ] Integração com drops
   - **Estimativa**: 1-2 semanas

6. **Sistema de Equipamento** [ALTA PRIORIDADE]
   - [ ] Slots de equipamento
   - [ ] UI de equipamento
   - [ ] Validação de requisitos
   - [ ] Troca visual (opcional)
   - **Estimativa**: 1 semana

7. **Completar Eventos Interativos** [MÉDIA PRIORIDADE]
   - [ ] DoorEvent completo
   - [ ] ChestEvent
   - [ ] NPCs de diálogo
   - [ ] ShopEvent completo
   - **Estimativa**: 1 semana

8. **Sistema de Quests** [MÉDIA PRIORIDADE]
   - [ ] Quest data structure
   - [ ] Quest tracking
   - [ ] UI de quests
   - [ ] Objetivos básicos (kill, collect, talk)
   - **Estimativa**: 2-3 semanas

### Phase 3 - Polish & Systems (Polimento)
**Objetivo**: Melhorar qualidade e adicionar features

9. **Sistema de Salvamento** [ALTA PRIORIDADE]
   - [ ] Serialização JSON
   - [ ] Save/Load básico
   - [ ] Múltiplos slots
   - [ ] Auto-save
   - **Estimativa**: 1 semana

10. **Sistema de Audio** [MÉDIA PRIORIDADE]
    - [ ] Audio manager
    - [ ] Música de background
    - [ ] SFX completos
    - [ ] Mixagem por categoria
    - **Estimativa**: 1 semana

11. **Skill Tree** [MÉDIA PRIORIDADE]
    - [ ] Verificar código existente
    - [ ] UI de skill tree
    - [ ] Unlock de habilidades
    - [ ] Balanceamento
    - **Estimativa**: 2 semanas

12. **Migração para New Input System** [BAIXA PRIORIDADE]
    - [ ] Configurar Input Actions
    - [ ] Migrar código
    - [ ] Suporte a gamepad
    - [ ] Rebinding
    - **Estimativa**: 3-5 dias

### Phase 4 - Multiplayer (Preparação e Implementação)
**Objetivo**: Suportar até 5 jogadores

13. **Preparação de Arquitetura** [ALTA PRIORIDADE]
    - [ ] Refatorar para client-server
    - [ ] IDs únicos para objetos
    - [ ] Validação server-side
    - [ ] Separar lógica visual de lógica de jogo
    - **Estimativa**: 2-3 semanas

14. **Networking Básico** [ALTA PRIORIDADE]
    - [ ] Escolher framework (Netcode/Mirror/Photon)
    - [ ] Sincronização de posição
    - [ ] Sincronização de stats
    - [ ] Lobby system
    - **Estimativa**: 3-4 semanas

15. **Multiplayer Features** [MÉDIA PRIORIDADE]
    - [ ] Party system
    - [ ] Compartilhamento de XP
    - [ ] Chat básico
    - [ ] Sincronização de mundo
    - **Estimativa**: 2-3 semanas

16. **Salvamento Local por Máquina** [MÉDIA PRIORIDADE]
    - [ ] Save individual por jogador
    - [ ] Sincronização ao conectar
    - [ ] Resolução de conflitos
    - **Estimativa**: 1 semana

### Phase 5 - Content & Balance (Conteúdo)
**Objetivo**: Preencher o jogo com conteúdo

17. **Criação de Conteúdo**
    - [ ] Classes balanceadas (mínimo 3-4)
    - [ ] Raças balanceadas (mínimo 2-3)
    - [ ] Habilidades variadas (20-30)
    - [ ] Inimigos diversos (10-15 tipos)
    - [ ] Items/equipamentos (50-100)
    - [ ] Quests (20-30)
    - **Estimativa**: 4-8 semanas

18. **Level Design**
    - [ ] Tutorial area
    - [ ] Áreas de exploração (3-5 mapas)
    - [ ] Dungeons (2-3)
    - [ ] Boss arenas
    - **Estimativa**: 4-6 semanas

### Phase 6 - Final Polish
**Objetivo**: Preparar para lançamento/teste

19. **Polish Geral**
    - [ ] Balanceamento final
    - [ ] Bug fixing
    - [ ] Performance optimization
    - [ ] Localização (PT-BR + EN)
    - **Estimativa**: 2-4 semanas

20. **Features Adicionais** (Opcional)
    - [ ] Achievements
    - [ ] Statistics
    - [ ] Leaderboards
    - [ ] Seasonal content
    - **Estimativa**: Variável

## ⏱️ Estimativa Total de Desenvolvimento

### Desenvolvimento Solo
- **Phase 1-2**: 3-4 meses
- **Phase 3**: 2 meses
- **Phase 4**: 2-3 meses
- **Phase 5**: 3-4 meses
- **Phase 6**: 1-2 meses

**Total**: 11-15 meses (desenvolvimento solo, tempo parcial)

### Desenvolvimento em Equipe (2-3 pessoas)
- **Total**: 6-9 meses

## 🎮 Milestones Sugeridos

### Milestone 1: Vertical Slice (3 meses)
- IA funcional
- Combat básico
- UI essencial
- 1 classe, 1 raça
- 1 área pequena
- 3-5 inimigos
- **Meta**: Provar que o jogo é divertido

### Milestone 2: Core Loop (6 meses)
- Inventário e equipamento
- Quests básicas
- Salvamento
- 2-3 classes
- 2 raças
- 2-3 áreas
- **Meta**: Loop de jogo completo

### Milestone 3: Multiplayer Alpha (9 meses)
- Networking funcional
- 5 jogadores simultâneos
- Party system
- Todas as features core
- **Meta**: Teste com amigos

### Milestone 4: Content Complete (12 meses)
- Todo conteúdo planejado
- Balanceamento inicial
- Polish visual
- **Meta**: Feature complete

### Milestone 5: Release (15 meses)
- Bug fixing
- Performance optimization
- Localização completa
- Polish final
- **Meta**: Lançamento

## 📝 Recomendações

### Priorize Gameplay Primeiro
- Foque em fazer o jogo ser divertido antes de adicionar conteúdo
- Teste frequentemente com jogadores reais
- Itere baseado em feedback

### Scope Creep
- Resista à tentação de adicionar features antes de completar as existentes
- Use este documento como guia
- Marque tarefas como completas progressivamente

### Multiplayer - Considere Timing
- Multiplayer adiciona complexidade significativa
- Considere deixar para depois do single-player estar sólido
- Ou implemente desde o início (mas demorará mais)

### Ferramentas de Produtividade
- Use Trello/Notion para tracking de tarefas
- Versionamento com Git (já está usando)
- Backups automáticos
- Playtests regulares

### Quando Pedir Ajuda
- Arte/modelagem 3D
- Música/sound design
- Netcode (se complexo)
- Game design (balanceamento)

## 🎯 Próximos Passos Imediatos

1. **Escolher uma Phase para começar** (recomendo Phase 1)
2. **Criar branch de desenvolvimento para cada feature**
3. **Implementar estados de IA vazios** (quick win)
4. **Criar cena de teste para cada sistema**
5. **Documentar decisões de design**

Boa sorte com o desenvolvimento! 🚀

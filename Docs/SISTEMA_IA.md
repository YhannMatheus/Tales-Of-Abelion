# Sistema de IA - Status e Tarefas

## 📋 Visão Geral
Sistema de inteligência artificial baseado em máquina de estados finitos (FSM) para controlar comportamento de NPCs, inimigos e aliados.

## 🏗️ Arquitetura Modular

### Hierarquia de Responsabilidades

```
Character (Base de Dados)
    ↓
IAManager (Hub Central para NPCs)
    ↓
StateManager (Gerenciador de Estados)
    ↓
States (Comportamentos Específicos)
```

### Componentes Principais

#### **Character** (`Assets/Scripts/CharacterSystem/Character.cs`)
**Papel**: Componente base de dados para Players E NPCs
- ✅ CharacterData - stats, vida, energia, nível
- ✅ Sistema de regeneração
- ✅ Eventos (OnDeath, OnTakeDamage, OnHealthChanged, etc)
- ✅ Métodos de dano, cura, experiência
- ✅ `SetCharacterData()` - Define classe/raça (usado por IAManager)
- ✅ `SetRegenerationSettings()` - Controla regeneração (usado por IAManager)
- ✅ `InitializeCharacter()` - Inicializa CharacterData
- ✅ `Revive()` - Ressuscita personagem

**Responsabilidades**:
- Dados base (vida, energia, stats)
- Regeneração automática
- Eventos de mudança de estado
- **NÃO** contém lógica de IA ou específica de NPCs

#### **IAManager** (`Assets/Scripts/IA/IAManager.cs`)
**Papel**: Hub central para NPCs (equivalente ao Character para Players)

**Responsabilidades**:
- ✅ **Inicializar Character** com classe/raça configuradas no Inspector
- ✅ **Coordenar StateManager** - delega gerenciamento de estados
- ✅ **Controlar detecção, combate e movimento**
- ✅ **Distribuir XP e drops** ao morrer (para inimigos)
- ✅ **Sincronizar CharacterType ↔ IaType**

**Componentes Requeridos**:
```csharp
[RequireComponent(typeof(Character))]
[RequireComponent(typeof(IAAnimatorController))]
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(IADetectSystem))]
[RequireComponent(typeof(StateManager))]  // ← NOVO!
```

**Campos Principais**:
```csharp
// Character Setup
public ClassData npcClass;
public RaceData npcRace;
public int initialLevel = 1;
public bool enableHealthRegen = true;
public bool enableEnergyRegen = true;

// IA Type
public IaType iaType; // Enemy, Neutral, Ally, NPC

// Componentes
public StateManager stateManager;  // ← NOVO!
public Character character;
public IAAnimatorController animator;
public CharacterController controller;
public IADetectSystem detectSystem;

// Recompensas (Enemy/NPC)
public int experienceReward;
public GameObject[] itemDrops;
public float xpDistributionRange = 10f;
public float dropChance = 0.5f;
```

**Métodos Principais**:
- `InitializeNPC()` - Configura classe/raça/nível do Character
- `SyncCharacterType()` - Sincroniza IaType → CharacterType
- `SwitchState()` - Delega para StateManager
- `GetStateByIAState()` - Delega para StateManager
- `CanUseState()` - Delega para StateManager
- `HandleEnemyDeath()` - Distribui XP e dropa itens
- `FindNearbyPlayers()` - Busca players no raio de XP

**Fluxo de Inicialização (NPCs)**:
```
IAManager.Awake()
  → GetComponents (Character, StateManager, etc)
  → InitializeNPC()
      → character.SetCharacterData(npcClass, npcRace)
      → character.SetRegenerationSettings()
      → character.InitializeCharacter()
      → Ajustar nível se > 1
  → SyncCharacterType()
  → Inicializar velocidades (runSpeed, walkSpeed)
  → stateManager.Initialize(this)  // ← Delega estados
```

#### **StateManager** (`Assets/Scripts/IA/StateManager.cs`) ✨ **NOVO**
**Papel**: Gerenciador de estados da IA

**Responsabilidades**:
- ✅ **Criar e armazenar instâncias** de todos os estados
- ✅ **Gerenciar transições** entre estados
- ✅ **Validar permissões** de estados (baseado em IAState flags)
- ✅ **Executar o estado atual**

**Campos Principais**:
```csharp
[Header("Estados Disponíveis")]
public IAState activeStates; // Flags (checkboxes no Inspector)

[Header("Estado Inicial")]
public IAState initialState = IAState.Idle;

// Estados instanciados (privados)
private State _idleState;
private State _patrolState;
private State _chaseState;
private State _attackState;
private State _fleeState;
private State _deadState;

// Estado atual e anterior
private State _currentState;
private State _lastState;

// Propriedades públicas
public State CurrentState { get; }
public State LastState { get; }
```

**Métodos Principais**:
```csharp
Initialize(IAManager)       // Cria estados e entra no inicial
UpdateState()               // Atualiza estado atual
SwitchState(State)          // Troca de estado com validação
CanUseState(IAState)        // Verifica permissões (bitwise AND)
GetStateByType(IAState)     // Retorna estado por tipo
GetStateType(State)         // Retorna tipo do estado
```

**Fluxo de Execução**:
```
1. IAManager.Awake() → stateManager.Initialize(this)
2. StateManager cria instâncias de todos os estados
3. StateManager valida e entra no estado inicial
4. IAManager.Update() → stateManager.UpdateState()
5. StateManager executa currentState.UpdateState(iaManager)
6. Evento/Transição → iaManager.SwitchState(newState)
7. StateManager valida permissões
8. StateManager.ClearState(anterior) → EnterState(novo)
```

**Validação de Estados (Bitwise)**:
```csharp
// No Inspector do StateManager:
activeStates = Idle | Patrol | Chase | Attack  // Checkboxes marcadas

// Em código:
if (CanUseState(IAState.Chase))  // (activeStates & IAState.Chase) == IAState.Chase
{
    SwitchState(GetStateByType(IAState.Chase));
}
```

#### **State** (Classe Abstrata)
**Papel**: Define comportamento em cada estado

**Três Métodos Obrigatórios**:

1. **EnterState(IAManager ia)**
   - Prepara substatemachine da animação
   - Ativa flags específicas
   - Reseta timers e variáveis
   - Registra eventos (se necessário)

2. **UpdateState(IAManager ia)**
   - Comportamento contínuo todo frame
   - **Adapta por ia.iaType** (Enemy/Neutral/Ally)
   - Lógica de movimento, rotação, decisões
   - Verifica condições de transição

3. **ClearState(IAManager ia)**
   - Limpa estado para evitar bugs
   - Desativa flags
   - Remove eventos
   - Reseta substatemachine animação

### Comportamentos por Tipo de IA (IaType)

**IMPORTANTE**: O estado **Idle** não significa necessariamente "parado". É a **substatemachine básica** do Animator que contém:
- Blend Tree de parado ↔ andando lentamente
- Controlado por variável float `speed`
- Permite movimento suave mesmo em "idle"

#### IaType.Neutral (Neutro)
- **Idle**: Substatemachine básica (pode andar devagar)
  - → **Chase**: Se for atacado (OnTakeDamage)
  - → **Patrol**: Se nada ocorrer após tempo
- **Patrol**: Anda entre pontos (walkSpeed)
  - → **Chase**: Se for atacado
- **Chase**: Persegue quem atacou (runSpeed)
- **Attack**: Ataca o agressor

#### IaType.Enemy (Inimigo)
- **Idle**: Substatemachine básica (pode andar devagar)
  - → **Chase**: Se avistar Player ou Aliado
  - → **Patrol**: Se nada ocorrer após tempo
- **Patrol**: Patrulha área (walkSpeed)
  - → **Chase**: Se avistar Player ou Aliado
- **Chase**: Persegue alvo detectado (runSpeed)
- **Attack**: Ataca quando em alcance
- **Dead**: Distribui XP, dropa itens, destrói GameObject

#### IaType.Ally (Aliado)
- **Idle**: Substatemachine básica - segue player em posição relativa (walkSpeed variável)
  - → **Chase**: Se player ou ele for atacado
- **Patrol**: Segue player em distância de patrulha
  - → **Chase**: Se detectar inimigo atacando
- **Chase**: Persegue inimigo que atacou (runSpeed)
- **Attack**: Defende player/si mesmo

## ✅ Componentes Completos

### Character (`Assets/Scripts/CharacterSystem/Character.cs`)
- ✅ CharacterData - struct serializado com todos os stats
- ✅ Sistema de regeneração (vida e energia)
- ✅ Eventos: OnDeath, OnTakeDamage, OnHealthChanged, OnEnergyChanged, OnLevelUp
- ✅ Métodos: TakeDamage, Heal, RestoreEnergy, GainExperience
- ✅ `SetCharacterData(ClassData, RaceData)` - Para NPCs (via IAManager)
- ✅ `SetRegenerationSettings(bool, bool)` - Para NPCs (via IAManager)
- ✅ `InitializeCharacter()` - Inicializa CharacterData.Initialization()
- ✅ `Die()` - Apenas dispara eventos, lógica de morte em IAManager
- ✅ `Revive()` - Restaura vida/energia e dispara OnRevive
- ✅ **Genérico** - Serve Players E NPCs sem lógica específica de IA

### IAManager (`Assets/Scripts/IA/IAManager.cs`)
- ✅ Hub central para NPCs (equivalente ao Character para Players)
- ✅ Inicialização via `InitializeNPC()` - configura Character com classe/raça
- ✅ Sincronização `IaType ↔ CharacterType` via `SyncCharacterType()`
- ✅ **Delegação de estados** para StateManager
  - `SwitchState()` → `stateManager.SwitchState()`
  - `GetStateByIAState()` → `stateManager.GetStateByType()`
  - `CanUseState()` → `stateManager.CanUseState()`
- ✅ Sistema de patrulha com pontos (patrolPoints[])
- ✅ Gerenciamento de velocidade (walkSpeed, runSpeed)
- ✅ Eventos de detecção, morte e dano
- ✅ **HandleDetectTarget** - Comportamento por IaType
- ✅ **HandleTakeDamage** - Reação de Neutros e Aliados
- ✅ **HandleEnemyDeath** - Distribui XP e dropa itens
- ✅ Configurações de Aliado (playerToFollow, followOffset, distances)
- ✅ Runtime Data (currentTarget, aggressorTarget)
- ✅ Recompensas (experienceReward, itemDrops[], xpDistributionRange, dropChance)
- ✅ Gizmos de visualização (patrulha, ataque, XP range)
- ✅ Utility methods (IsInAttackRange, CanAttack, ApplyGravity, RotateTowards)

### StateManager (`Assets/Scripts/IA/StateManager.cs`) ✨ **NOVO**
- ✅ Gerenciador de estados separado do IAManager
- ✅ Cria e armazena instâncias de todos os estados
- ✅ Validação de permissões via bitwise AND
- ✅ Gerenciamento de transições com logs de debug
- ✅ Propriedades públicas: CurrentState, LastState
- ✅ Propriedades de acesso aos estados: IdleState, PatrolState, etc
- ✅ `Initialize(IAManager)` - Cria estados e entra no inicial
- ✅ `UpdateState()` - Atualiza estado atual
- ✅ `SwitchState(State)` - Troca com validação e logs
- ✅ `CanUseState(IAState)` - Verifica flags
- ✅ `GetStateByType(IAState)` - Retorna estado por tipo
- ✅ `GetStateType(State)` - Retorna tipo do estado
- ✅ `GetFirstAvailableState()` - Fallback se inicial inválido
- ✅ OnDestroy cleanup automático

### IADetectSystem (`Assets/Scripts/IA/IADetectSystem.cs`)
- ✅ Detecção por OverlapSphere
- ✅ Validação de ângulo de visão
- ✅ Raycast para line-of-sight
- ✅ Sistema de eventos (OnDetectTarget, OnLoseTarget)
- ✅ Suporte para múltiplas LayerMasks
- ✅ Gizmos de visualização de alcance de visão

### IAAnimatorController (`Assets/Scripts/IA/IAAnimatorController.cs`)
- ✅ Controlador de animação para IA
- ✅ Substatemachines por estado (Idle, Patrol, Chase, Attack, Dead)
- ⚠️ **Arquitetura de Parâmetros Unificados** - Precisa implementar 6 parâmetros globais
- ⚠️ Verificar integração completa com estados

**Parâmetros Globais do Animator** (reutilizados em todas as substatemachines):
1. `speed` (float): Controla blend trees de movimentação (0.0 = parado, 1.0 = correndo)
2. `attackTrigger` (Trigger): Dispara ataque básico
3. `isUsingAbility` (bool): Indica uso de habilidade especial
4. `abilityIndex` (int): Qual habilidade usar (0-N)
5. `takeDamageTrigger` (Trigger): Animação de receber dano
6. `deathTrigger` (Trigger): Animação de morte

### Estados Base
- ✅ `State.cs` - Classe abstrata com padrão EnterState/UpdateState/ClearState
- ✅ `IAEnum.cs` - Enumerações com flags para estados (IAState, IaType, etc)

## ⚠️ Estados Parcialmente Implementados

### IdleState (`Assets/Scripts/IA/States/IdleState.cs`)
- ✅ Estrutura completa com 3 métodos
- ✅ Comportamento específico por IaType
- ✅ **Neutral**: Pode andar devagar (speed baixo), transição para Patrol após tempo
- ✅ **Enemy**: Pode andar devagar (speed baixo), transição para Patrol após tempo
- ✅ **Ally**: Segue player com walkSpeed variável (blend tree reage ao speed)
- ✅ Sistema de timer com chance aleatória
- ✅ Logs de debug por tipo
- ✅ **Animator**: Usa substatemachine Idle
  - **Parâmetros usados**: `speed` (float) para blend tree parado↔andando lento
  - **Animações**: Variações por IaType (Enemy idle ≠ Ally idle)
  - **Habilidades**: Não usa `isUsingAbility` ou `abilityIndex`

**Tarefas Pendentes:**
- [x] ~~Implementar comportamento de espera~~
- [x] ~~Adicionar timer para transição automática~~
- [x] ~~Adicionar animação de idle~~
- [x] ~~Implementar comportamento específico por IaType~~
- [x] ~~Neutral: Parado, vai para Patrol após tempo~~
- [x] ~~Enemy: Observa, vai para Patrol após tempo~~
- [x] ~~Ally: Segue player em posição relativa~~

### PatrolState (`Assets/Scripts/IA/States/PatrolState.cs`)
- ✅ Movimentação entre pontos
- ✅ Rotação de pontos de patrulha
- ✅ Uso de walkSpeed
- ❌ Sistema de espera em pontos (waitTimeAtPoint não usado)

**Animator**: Usa substatemachine Patrol
- **Parâmetros usados**: `speed` (float) para blend tree parado↔andando (0.4-0.6)
- **Animações**: Variações de patrulha (guardas marchando, criaturas farejando, etc.)
- **Pausas**: `speed=0` durante espera em waypoint

**Tarefas Pendentes:**
- [ ] Implementar timer de espera em cada ponto
- [ ] Adicionar variação aleatória no tempo de espera
- [ ] Melhorar rotação do personagem ao andar
- [ ] Adicionar detecção durante patrulha

### ChaseState (`Assets/Scripts/IA/States/ChaseState.cs`)
- ❌ Apenas estrutura vazia
- ❌ Sem implementação

**Animator**: Usa substatemachine Chase
- **Parâmetros usados**: `speed` (float) para blend tree andando↔correndo (0.7-1.0)
- **Animações**: Até 2 variações de movimento (perseguição agressiva, corrida tática)
- **Blend Tree**: Transição suave entre andando rápido (0.6) e correndo (1.0)
- **Habilidades**: Pode usar `takeDamageTrigger` se receber dano durante perseguição

**Tarefas Pendentes:**
- [ ] Implementar perseguição ao alvo detectado
- [ ] Usar runSpeed para velocidade de perseguição (speed=0.7-1.0)
- [ ] Implementar NavMesh ou pathfinding
- [ ] Adicionar rotação suave em direção ao alvo
- [ ] Definir condições de transição para Attack ou Patrol
- [ ] Implementar perda de alvo se sair do alcance
- [ ] Usar lastKnownTargetPosition quando perder visão

### AttackState (`Assets/Scripts/IA/States/AttackState.cs`)
- ❌ Apenas estrutura vazia
- ❌ Sem implementação

**Animator**: Usa substatemachine Attack
- **Parâmetros usados**:
  - `speed=0` (parado durante ataque)
  - `attackTrigger` (Trigger) para ataque básico
  - `isUsingAbility=true` quando usa habilidade especial
  - `abilityIndex` (int) = índice da habilidade (0-N)
  - `takeDamageTrigger` se receber dano durante ataque
- **Substatemachine Interna de Habilidades**:
  - Acessada quando `isUsingAbility = true`
  - Transições baseadas em `abilityIndex` (0=FireballAnimation, 1=HealAnimation, etc.)
  - Retorna ao estado de ataque normal quando `isUsingAbility=false`
- **Animações**: Combo de ataques básicos + habilidades específicas por NPC

**Tarefas Pendentes:**
- [ ] Implementar verificação de alcance de ataque
- [ ] Integrar com sistema de habilidades (AbilitySlot)
- [ ] Adicionar cooldown entre ataques
- [ ] Rotacionar para o alvo antes de atacar
- [ ] Definir transição para Chase se alvo sair do alcance
- [ ] Implementar alternância ataque básico (attackTrigger) vs habilidades (isUsingAbility + abilityIndex)
- [ ] Sincronizar animação com execução real da habilidade

### FleeState (`Assets/Scripts/IA/States/FleeState.cs`)
- ❌ Apenas estrutura vazia
- ❌ Constante fleeDistance definida mas não usada

**Tarefas Pendentes:**
- [ ] Implementar fuga do alvo
- [ ] Calcular direção oposta ao inimigo
- [ ] Usar runSpeed para velocidade de fuga
- [ ] Definir condição de vida baixa para ativar
- [ ] Implementar busca por cover/esconderijo
- [ ] Adicionar transição de volta para Idle quando seguro

### DeadState (`Assets/Scripts/IA/States/DeadState.cs`)
- ❌ Apenas estrutura vazia
- ❌ Sem implementação

**Animator**: Usa substatemachine Dead
- **Parâmetros usados**:
  - `deathTrigger` (Trigger) dispara animação de morte
  - `speed=0` (sem movimento)
- **Animações**: Morte + estado final (corpo no chão)
- **Sem transições de saída**: Estado final do Animator

**Tarefas Pendentes:**
- [ ] Implementar animação de morte (deathTrigger)
- [ ] Desativar colisor
- [ ] Desativar movimento e IAManager
- [ ] Permanecer em Dead permanentemente (sem saída)
- [ ] Integrar com sistema de destruição de GameObject (Enemy)

---

## 🎮 Como Configurar no Unity

### Setup Básico de NPC

**1. Criar GameObject Base**
```
NPC GameObject
├── Character (Component)
├── IAManager (Component)
├── StateManager (Component) ← NOVO!
├── IAAnimatorController (Component)
├── CharacterController (Component)
├── IADetectSystem (Component)
└── Animator (Component - Unity padrão)
```

**2. Configurar Character**
- Marcar `Initialize On Start` como **FALSE** (IAManager cuida disso)
- Character Type será sincronizado automaticamente pelo IAManager

**3. Configurar IAManager**

**Character Setup (NPC)**:
- `npcClass` → Arraste ClassData ScriptableObject
- `npcRace` → Arraste RaceData ScriptableObject  
- `initialLevel` → Nível inicial (padrão: 1)
- `enableHealthRegen` → true/false
- `enableEnergyRegen` → true/false

**Informações Básicas**:
- `iaType` → Enemy / Neutral / Ally / NPC

**Patrulha** (se usar Patrol):
- `waitTimeAtPoint` → Tempo de espera em cada ponto
- `patrolPoints[]` → Array de Transforms marcando pontos
- `patrolPointRadius` → Distância mínima para considerar chegada

**Detecção**:
- `visionArea` → Raio de detecção
- `detectionAngle` → Ângulo de visão (90° = frontal, 180° = semicírculo)
- `targetLayerMask[]` → Layers que pode detectar (Player, Ally, etc)
- `obstructionLayerMask[]` → Layers que bloqueiam visão (Walls, etc)

**Combate**:
- `attackRange` → Distância de ataque

**Recompensas** (Enemy/NPC):
- `experienceReward` → XP ao morrer
- `itemDrops[]` → Itens que pode dropar
- `xpDistributionRange` → Raio para distribuir XP
- `dropChance` → 0.0 a 1.0 (0.5 = 50%)

**4. Configurar StateManager** ← **NOVO!**

**Estados Disponíveis**:
- Marcar checkboxes dos estados permitidos:
  - ☑ Idle
  - ☑ Patrol (se tiver patrolPoints)
  - ☑ Chase
  - ☑ Attack
  - ☐ Flee (opcional)
  - ☑ Dead

**Estado Inicial**:
- Dropdown: Idle / Patrol / etc

**5. Configurar IADetectSystem**
- Já configurado automaticamente via IAManager

**6. Configurar Animator**
- Criar Animator Controller com substatemachines:
  - Idle (blend tree parado ↔ andando lento)
  - Patrol (blend tree andando)
  - Chase (blend tree correndo)
  - Attack (ataques + substatemachine de habilidades)
  - Dead (animação de morte)

- Adicionar parâmetros globais:
  - `speed` (float)
  - `attackTrigger` (Trigger)
  - `isUsingAbility` (bool)
  - `abilityIndex` (int)
  - `takeDamageTrigger` (Trigger)
  - `deathTrigger` (Trigger)

### Exemplos de Configuração

**Guarda (Enemy Básico)**:
```
IAManager:
  - iaType: Enemy
  - npcClass: GuardClass
  - npcRace: HumanRace
  - initialLevel: 5
  
StateManager:
  - activeStates: Idle, Patrol, Chase, Attack, Dead
  - initialState: Patrol
  
Patrulha:
  - patrolPoints[4] (4 pontos ao redor da área)
  - waitTimeAtPoint: 3s
```

**Comerciante (NPC Neutro)**:
```
IAManager:
  - iaType: Neutral
  - npcClass: MerchantClass
  - npcRace: DwarfRace
  - initialLevel: 10
  
StateManager:
  - activeStates: Idle, Chase, Attack, Dead
  - initialState: Idle
  
Nota: Sem Patrol, apenas Idle até ser atacado
```

**Companheiro (Ally)**:
```
IAManager:
  - iaType: Ally
  - npcClass: WarriorClass
  - npcRace: ElfRace
  - initialLevel: 8
  - playerToFollow: Player Transform
  - followOffset: (2, 0, -1)
  
StateManager:
  - activeStates: Idle, Chase, Attack, Dead
  - initialState: Idle
  
Nota: Segue player automaticamente
```

---

## 🔄 Mudanças da Arquitetura (Refatoração Recente)

### **Antes** (Arquitetura Monolítica)

```
Character (Players + NPCs)
  ├── Lógica de Players
  ├── Lógica de Enemies (XP, drops)
  ├── Lógica de Allies
  └── Lógica de NPCs

IAManager
  ├── Cria instâncias de estados
  ├── Valida permissões
  ├── Gerencia transições
  ├── Executa estados
  └── Tudo em um único arquivo
```

**Problemas**:
- ❌ Character tinha lógica específica de NPCs
- ❌ IAManager fazia muitas coisas
- ❌ Difícil de testar e manter
- ❌ Campos como `experienceReward` em Character (não faz sentido para Players)

### **Depois** (Arquitetura Modular)

```
Character (Base de Dados)
  └── Genérico para Players E NPCs

IAManager (Hub para NPCs)
  ├── Inicializa Character
  ├── Coordena sistemas
  ├── Lógica de morte/XP/drops
  └── Delega estados para StateManager

StateManager (Gerenciador de Estados)
  ├── Cria estados
  ├── Valida permissões
  ├── Gerencia transições
  └── Executa estado atual
```

**Benefícios**:
- ✅ **Character** é genérico (Players + NPCs)
- ✅ **IAManager** é o hub equivalente ao Character para NPCs
- ✅ **StateManager** cuida apenas de estados
- ✅ Separação clara de responsabilidades
- ✅ Mais fácil de testar e expandir
- ✅ Inicialização clara: `IAManager.InitializeNPC()` configura tudo

### Migração de Código

**Removido de Character**:
- ❌ `detectionRange`, `attackRange`, `targetLayers`
- ❌ `experienceReward`, `itemDrops`, `xpDistributionRange`
- ❌ `HandleEnemyDeath()`, `DropLoot()`, `FindNearbyPlayers()`

**Adicionado em IAManager**:
- ✅ `npcClass`, `npcRace`, `initialLevel`
- ✅ `experienceReward`, `itemDrops`, `xpDistributionRange`, `dropChance`
- ✅ `InitializeNPC()`, `SyncCharacterType()`
- ✅ `HandleEnemyDeath()`, `DropLoot()`, `FindNearbyPlayers()`

**Adicionado em Character** (para NPCs):
- ✅ `SetCharacterData(ClassData, RaceData)` - Permite IAManager configurar
- ✅ `SetRegenerationSettings(bool, bool)` - Controle de regeneração

**Criado StateManager**:
- ✅ Novo componente separado
- ✅ Toda lógica de gerenciamento de estados migrada
- ✅ IAManager delega via `stateManager.SwitchState()`, etc

---

## 🔧 Melhorias Sugeridas

### Arquitetura
- [ ] Adicionar sistema de prioridade de estados
- [ ] Implementar blackboard para compartilhar dados entre estados
- [ ] Criar biblioteca de behaviors reutilizáveis
- [ ] Adicionar debug visual no editor (estado atual, alvo, etc)

### Detecção
- [ ] Adicionar diferentes níveis de alerta (Low, Medium, High)
- [ ] Implementar memória de última posição conhecida
- [ ] Adicionar som como método de detecção
- [ ] Criar sistema de "suspeita" antes de entrar em Chase

### Performance
- [ ] Otimizar OverlapSphere com intervalos dinâmicos
- [ ] Implementar LOD para IA distante
- [ ] Usar coroutines para detecção em vez de Update

### Pathfinding
- [ ] Integrar Unity NavMesh
- [ ] Adicionar obstacle avoidance
- [ ] Implementar waypoint dinâmico

## 📝 Notas Importantes

- **Character** agora é genérico e serve Players E NPCs
- **IAManager** é para NPCs o que Character.Start() é para Players
- **StateManager** gerencia estados de forma isolada e testável
- Sistema de flags permite IAs com comportamentos limitados (ex: apenas Idle + Attack para inimigos estacionários)
- `activeStates` (no StateManager) define quais estados a IA pode usar (bitwise OR)
- Todos os estados verificam `CanUseState()` antes de transicionar
- Gizmos extensivos facilitam debug visual no editor
- **Inicialização**: IAManager.Awake() → InitializeNPC() → StateManager.Initialize()

---

## 🎬 Arquitetura de Animação Unificada

### Conceito Central
Todas as substatemáquinas do Animator (Idle, Patrol, Chase, Attack, Dead) compartilham **6 parâmetros globais**. Isso permite:
- ✅ Reutilização de parâmetros entre estados
- ✅ Consistência de controle em todo o Animator
- ✅ Flexibilidade para animações específicas por IaType
- ✅ Simplicidade no código do IAAnimatorController

### Parâmetros Globais do Animator

| Parâmetro | Tipo | Função | Valores |
|-----------|------|--------|---------|
| `speed` | float | Controla blend trees de movimentação | 0.0 = parado, 0.5 = andando, 1.0 = correndo |
| `attackTrigger` | Trigger | Dispara ataque básico | One-shot |
| `isUsingAbility` | bool | Indica uso de habilidade especial | true/false |
| `abilityIndex` | int | Qual habilidade usar | 0-N (índice da habilidade) |
| `takeDamageTrigger` | Trigger | Animação de receber dano | One-shot |
| `deathTrigger` | Trigger | Animação de morte | One-shot |

### Substatemáquinas do Animator

#### 1. Idle Substatemachine
- **Blend Tree**: Parado ↔ Andando lentamente
- **Range de speed**: 0.0 - 0.5
- **Parâmetros ativos**: `speed`
- **Variações**: Animações diferentes por IaType
  - Enemy: Idle alerta, pode andar devagar observando
  - Neutral: Idle relaxado, pode andar sem pressa
  - Ally: Idle casual, segue player

#### 2. Patrol Substatemachine
- **Blend Tree**: Parado (waypoint) ↔ Andando
- **Range de speed**: 0.0 (pausa) - 0.6 (patrulha)
- **Parâmetros ativos**: `speed`
- **Animações**: Guarda marchando, criatura farejando, etc.

#### 3. Chase Substatemachine
- **Blend Tree**: Andando rápido ↔ Correndo
- **Range de speed**: 0.6 - 1.0
- **Parâmetros ativos**: `speed`, `takeDamageTrigger` (pode receber dano)
- **Até 2 variações**: Perseguição agressiva, corrida tática
- **Transição suave**: Entre andando rápido e corrida total

#### 4. Attack Substatemachine
- **Parâmetros ativos**: `speed=0`, `attackTrigger`, `isUsingAbility`, `abilityIndex`, `takeDamageTrigger`
- **Estrutura**:
  ```
  Attack Substatemachine
  ├─ Ataque Básico (attackTrigger)
  └─ Substatemachine Interna de Habilidades
     ├─ Habilidade 0 (abilityIndex=0, isUsingAbility=true)
     ├─ Habilidade 1 (abilityIndex=1, isUsingAbility=true)
     └─ Habilidade N (abilityIndex=N, isUsingAbility=true)
  ```
- **Lógica**:
  - `isUsingAbility = false` → Executa ataque básico com `attackTrigger`
  - `isUsingAbility = true` → Entra em substatemachine interna, seleciona animação via `abilityIndex`

#### 5. Dead Substatemachine
- **Parâmetros ativos**: `deathTrigger`, `speed=0`
- **Animações**: Morte + estado final (corpo no chão)
- **Sem transições de saída**: Estado final do Animator

### Fluxo de Uso no Código

```csharp
// IAAnimatorController.cs (planejado)
public class IAAnimatorController : MonoBehaviour
{
    private Animator animator;
    
    // Métodos para atualizar parâmetros globais
    public void SetSpeed(float value) 
        => animator.SetFloat("speed", value);
    
    public void TriggerAttack() 
        => animator.SetTrigger("attackTrigger");
    
    public void SetAbility(int index, bool active) 
    {
        animator.SetInteger("abilityIndex", index);
        animator.SetBool("isUsingAbility", active);
    }
    
    public void TriggerDamage() 
        => animator.SetTrigger("takeDamageTrigger");
    
    public void TriggerDeath() 
        => animator.SetTrigger("deathTrigger");
    
    // Atualiza substatemachine ativa
    public void UpdateAnimation(IAState state)
    {
        // Transições entre substatemachines via triggers ou parâmetros
        // Exemplo: animator.SetInteger("currentState", (int)state);
    }
}
```

### Vantagens da Arquitetura

1. **Não precisa resetar parâmetros** entre transições de estado (são globais)
2. **Mesma lógica de controle** em todos os estados (sempre usa `speed` para movimento)
3. **Flexibilidade total** para animações específicas (Chase de Enemy ≠ Chase de Ally, mas ambos usam `speed`)
4. **Código mais limpo** no IAAnimatorController (não precisa lidar com parâmetros diferentes por estado)
5. **Fácil expansão** (adicionar nova habilidade = novo índice, sem novos parâmetros)

### Exemplo de Integração com Estados

```csharp
// IdleState.cs - já implementado
public override void UpdateState()
{
    // Ally segue player
    ia.currentSpeed = ia.walkSpeed * 0.3f; // Speed baixo para blend tree
    // IAAnimatorController sincroniza: animator.SetFloat("speed", ia.currentSpeed);
}

// ChaseState.cs - planejado
public override void UpdateState()
{
    ia.currentSpeed = ia.runSpeed; // Speed alto (0.7-1.0)
    // IAAnimatorController: animator.SetFloat("speed", ia.currentSpeed);
}

// AttackState.cs - planejado
public override void EnterState()
{
    ia.currentSpeed = 0; // Parado durante ataque
    // Ataque básico:
    iaAnimatorController.TriggerAttack();
    
    // OU habilidade especial:
    iaAnimatorController.SetAbility(2, true); // Usa habilidade index 2
}
```


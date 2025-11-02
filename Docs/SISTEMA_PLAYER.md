# Sistema de Player - Status e Tarefas

## 📋 Visão Geral
Sistema modular de controle do jogador com componentes separados para movimento, habilidades, detecção de clique e morte.

## ✅ Componentes Completos

### PlayerManager (`Assets/Scripts/Player/PlayerManager.cs`)
- ✅ RequireComponent para todas as dependências
- ✅ Orquestração de movimento e ataque
- ✅ Integração com InputManager
- ✅ HandleMovement com direção e velocidade
- ✅ HandleEventClick para interação com objetos
- ✅ HandleAttack para ataques básicos
- ✅ Verificação de vida antes de permitir ações

### PlayerMotor (`Assets/Scripts/Player/PlayerMotor.cs`)
- ✅ Movimento via CharacterController
- ✅ Rotação suave em direção ao movimento
- ✅ Rotação para posição específica (interações)
- ✅ Aplicação de gravidade
- ✅ Normalização de velocidade para animações
- ✅ Estados IsMoving e IsGrounded
- ⚠️ Verificar integração completa

### PlayerAbilityManager (`Assets/Scripts/Player/PlayerAbilityManager.cs`)
- ✅ Gerenciamento de BasicAttack + 6 skill slots
- ✅ Sistema de cooldowns
- ✅ Validação de recursos (energia)
- ✅ Eventos de slot (OnSlotReady, OnSlotCooldownTick)
- ✅ Suporte para charges
- ✅ Tracking de instâncias ativas

### PlayerClickDetect (`Assets/Scripts/Player/PlayerClickDetect.cs`)
- ✅ Raycast de mouse para posição no mundo
- ✅ Detecção de objetos clicados
- ✅ Filtragem por layer
- ⚠️ Verificar implementação completa

### PlayerDeathManager (`Assets/Scripts/Player/PlayerDeathManager.cs`)
- ✅ Singleton pattern
- ✅ Overlay de morte
- ⚠️ Integração com sistema de revive
- ⚠️ Verificar implementação completa

## ⚠️ Funcionalidades Parciais

### Sistema de Movimento
- ✅ Movimento básico WASD funcional
- ✅ Rotação em direção ao movimento
- ❌ Sem dash/dodge
- ❌ Sem sprint/corrida
- ❌ Sem movimento suave (aceleração/desaceleração)

**Tarefas Pendentes:**
- [ ] Implementar dash/dodge (tecla Shift?)
- [ ] Sistema de stamina para sprint
- [ ] Aceleração e desaceleração suaves
- [ ] Pulo (se aplicável ao jogo)
- [ ] Movimento em slopes
- [ ] Efeitos de partícula ao andar/correr

### Sistema de Interação
- ✅ Click para interagir
- ✅ Verificação de distância mínima
- ✅ Rotação para objeto antes de interagir
- ❌ Sem indicador visual de objetos interativos próximos
- ❌ Sem UI de prompt ("Pressione E para interagir")

**Tarefas Pendentes:**
- [ ] Highlight de objetos interativos próximos
- [ ] UI de prompt contextual
- [ ] Tecla alternativa para interação (E)
- [ ] Priorização quando múltiplos objetos próximos
- [ ] Cancelamento de interação
- [ ] Animação de interação

### Sistema de Ataque
- ✅ Ataque básico com mouse
- ✅ Rotação em direção ao ataque
- ❌ Sem combo de ataques básicos
- ❌ Sem integração visual completa

**Tarefas Pendentes:**
- [ ] Sistema de combo de ataques básicos
- [ ] Cancelamento de ataque
- [ ] Buffer de input para combos
- [ ] Feedback de impacto
- [ ] Trail de arma durante ataque
- [ ] Lock-on opcional em alvos

### Sistema de Input
- ✅ InputManager singleton com propriedades read-only
- ✅ Inputs de movimento, ataque, habilidades
- ✅ Inputs de UI (inventário, mapa, etc)
- ⚠️ Input System Actions existe mas integração parcial?
- ❌ Sem rebinding de teclas

**Tarefas Pendentes:**
- [ ] Verificar integração completa com New Input System
- [ ] Implementar rebinding de teclas
- [ ] UI de configuração de controles
- [ ] Suporte para gamepad
- [ ] Profiles de input (teclado, gamepad)
- [ ] Salvamento de preferências de input

## ❌ Funcionalidades Não Implementadas

### Sistema de Câmera
- ⚠️ CameraController existe mas é muito básico
- ❌ Sem controle de câmera por mouse

**Tarefas Pendentes:**
- [ ] Câmera orbital com mouse
- [ ] Zoom in/out com scroll
- [ ] Colisão de câmera com objetos
- [ ] Múltiplos modos (over-shoulder, top-down)
- [ ] Shake de câmera em impactos
- [ ] Suavização de movimento
- [ ] Lock de câmera em alvos

### Sistema de Morte e Revive
- ⚠️ PlayerDeathManager parcialmente implementado
- ❌ Lógica completa não verificada

**Tarefas Pendentes:**
- [ ] Verificar implementação atual
- [ ] Menu de opções na morte (revive, checkpoint, etc)
- [ ] Timer de respawn
- [ ] Penalidades de morte (XP, durabilidade)
- [ ] Revive por aliado
- [ ] Revive por item consumível
- [ ] Animação de morte e revive

### Sistema de Inventário do Player
**Tarefas Pendentes:**
- [ ] Estrutura de dados de inventário
- [ ] UI de inventário
- [ ] Drag and drop de itens
- [ ] Slots de quick access
- [ ] Peso/limite de itens
- [ ] Categorização de itens
- [ ] Tooltip de itens
- [ ] Uso de consumíveis

### Sistema de Equipamento Visual
**Tarefas Pendentes:**
- [ ] Troca visual de armas
- [ ] Troca visual de armadura
- [ ] Sistema de attachment points
- [ ] Customização de aparência
- [ ] Dye system (cores)

### Sistema de Emotes e Social
**Tarefas Pendentes:**
- [ ] Emotes básicos (acenar, dançar, etc)
- [ ] Animações sociais
- [ ] Chat (para multiplayer)
- [ ] Quick chat/commands
- [ ] Gestos contextuais

## 🔧 Melhorias Sugeridas

### Responsividade
- [ ] Input buffering para melhor feel
- [ ] Coyote time para pulos
- [ ] Animation canceling em momentos específicos
- [ ] Priorização de inputs

### Feedback
- [ ] Footstep sounds em diferentes terrenos
- [ ] Partículas de poeira ao andar
- [ ] Screen shake em ações impactantes
- [ ] Slow motion em momentos épicos
- [ ] Vibração de gamepad

### Acessibilidade
- [ ] Opção de auto-targeting
- [ ] Assistência de mira
- [ ] Indicadores visuais de direção
- [ ] Simplificação de combos
- [ ] Opções de controle alternativas

### Performance
- [ ] Desabilitar inputs durante cutscenes
- [ ] Queue de inputs para reduzir checks
- [ ] Debounce de clicks rápidos

### Multiplayer (Preparação)
- [ ] Client-side prediction
- [ ] Server reconciliation
- [ ] Lag compensation
- [ ] Interpolação de movimento
- [ ] Validação server-side de ações
- [ ] Anti-cheat básico (speed hacks, etc)

## 📝 Notas Importantes

### Arquitetura Modular
O sistema de player é dividido em componentes especializados:
- **PlayerManager**: Orquestrador principal
- **PlayerMotor**: Movimento e física
- **PlayerAbilityManager**: Habilidades e cooldowns
- **PlayerClickDetect**: Input de mouse
- **PlayerDeathManager**: Morte e revive

Essa modularidade facilita manutenção e testes.

### Input System
Existe `InputSystem_Actions.inputactions` mas integração não está clara. Verificar se:
- InputManager usa Old Input System (Input.GetAxis)
- Input Actions está configurado mas não usado
- Migração para New Input System está em progresso

### Dependências
PlayerManager requer:
- PlayerMotor
- PlayerClickDetect
- CharacterAnimatorController
- PlayerAbilityManager

Sempre use RequireComponent para garantir setup correto.

### Integração com Character
PlayerManager obtém Character component e usa:
- `character.Data.IsAlive` para validar ações
- `character.Data.TotalSpeed` para movimento
- Eventos de Character para reagir a mudanças

### Fluxo de Ataque
1. Input detectado (mouse button)
2. PlayerManager.HandleAttack()
3. Obtém posição do mouse via PlayerClickDetect
4. Calcula direção de ataque
5. Rotaciona player
6. Executa habilidade via PlayerAbilityManager

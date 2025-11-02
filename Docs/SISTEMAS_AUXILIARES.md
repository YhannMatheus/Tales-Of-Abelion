# Sistemas Auxiliares - Status e Tarefas

## 📋 Visão Geral
Sistemas de suporte que não se encaixam nas categorias principais: câmera, input, animação, checkpoint, etc.

## ✅ Componentes Completos

### InputManager (`Assets/Scripts/System/InputManager.cs`)
- ✅ Singleton para inputs globais
- ✅ Propriedades read-only para todos inputs
- ✅ Inputs de movimento (WASD)
- ✅ Inputs de UI (inventário, mapa, menu)
- ✅ Inputs de habilidades (Q, E, R, 1-3)
- ✅ Inputs de mouse (ataque, interação)
- ⚠️ Usa Old Input System

### CameraController (`Assets/Scripts/System/CameraController.cs`)
- ✅ Segue target com offset
- ✅ LookAt para foco
- ✅ LateUpdate para suavidade
- ❌ Muito básico, sem features avançadas

### CheckpointManager (`Assets/Scripts/System/CheckpointManager.cs`)
- ✅ Singleton pattern
- ✅ Gerenciamento de checkpoint atual
- ✅ Default spawn point
- ✅ Método RespawnPlayer
- ✅ Integração com Character.Revive()

### CharacterAnimatorController (`Assets/Scripts/System/CharactherAnimatorController.cs`)
- ✅ Controlador genérico de animação
- ✅ UpdateMovementSpeed com normalização
- ✅ Tracking de IsGrounded
- ⚠️ Verificar implementação completa

### GroundCheckRaycast (`Assets/Scripts/System/GroundCheckRaycast.cs`)
- ✅ Detecção de chão via raycast
- ⚠️ Verificar integração com PlayerMotor

### ReviveToken (`Assets/Scripts/System/ReviveToken.cs`)
- ✅ Item de revive
- ⚠️ Verificar implementação completa

## ⚠️ Funcionalidades Parciais

### Sistema de Input
- ✅ Inputs básicos funcionais
- ❌ Usa Old Input System em vez de New Input System
- ❌ Input Actions existe mas não está sendo usado
- ❌ Sem rebinding

**Tarefas Pendentes:**
- [ ] Migrar para New Input System completamente
- [ ] Usar Input Actions configurado
- [ ] Implementar rebinding de teclas
- [ ] Suporte para gamepad
- [ ] Salvamento de preferências
- [ ] Perfis de input diferentes
- [ ] Dead zones configuráveis
- [ ] Vibração de gamepad

### Sistema de Câmera
- ✅ Follow básico funcional
- ❌ Sem controle de câmera
- ❌ Sem colisão
- ❌ Sem zoom

**Tarefas Pendentes:**
- [ ] Controle de câmera com mouse
- [ ] Zoom in/out com scroll
- [ ] Colisão com objetos (evitar obstáculos)
- [ ] Múltiplos modos de câmera
- [ ] Transições suaves entre targets
- [ ] Shake de câmera
- [ ] FOV dinâmico
- [ ] Cinemachine integration (opcional)
- [ ] Lock-on em alvos

### Sistema de Checkpoint
- ✅ Funcionalidade básica
- ❌ Sem marcadores físicos no mundo
- ❌ Sem ativação de checkpoint

**Tarefas Pendentes:**
- [ ] Criar componente CheckpointTrigger
- [ ] Visual de checkpoint ativado/desativado
- [ ] Som ao ativar checkpoint
- [ ] Partículas de ativação
- [ ] UI confirmando checkpoint salvo
- [ ] Múltiplos checkpoints por área
- [ ] Checkpoint em bosses

### Sistema de Animação
- ✅ Controlador básico
- ❌ Integração não verificada
- ❌ Sem sistema de IK
- ❌ Sem root motion

**Tarefas Pendentes:**
- [ ] Verificar integração completa
- [ ] Adicionar mais triggers de animação
- [ ] Sistema de blend trees
- [ ] IK para pés/mãos
- [ ] Root motion opcional
- [ ] Animações de impacto
- [ ] Layered animations
- [ ] Animation events
- [ ] Transições suaves

## ❌ Funcionalidades Não Implementadas

### Sistema de Salvamento
**Tarefas Pendentes:**
- [ ] Estrutura de save data
- [ ] Serialização para JSON
- [ ] Sistema de múltiplos slots
- [ ] Auto-save periódico
- [ ] Save em checkpoints
- [ ] Cloud save (para multiplayer)
- [ ] Validação de integridade
- [ ] Backup de saves
- [ ] Migração de saves entre versões

### Sistema de Configurações
**Tarefas Pendentes:**
- [ ] Settings manager
- [ ] Configurações de gráficos
- [ ] Configurações de áudio
- [ ] Configurações de gameplay
- [ ] Configurações de acessibilidade
- [ ] Salvamento de preferências
- [ ] UI de configurações
- [ ] Detecção automática de qualidade

### Sistema de Audio
**Tarefas Pendentes:**
- [ ] Audio manager/mixer
- [ ] Categorias de som (SFX, música, voz)
- [ ] Volume individual por categoria
- [ ] Música de fundo por área
- [ ] Sistema de footsteps
- [ ] Sons ambiente
- [ ] Som 3D posicional
- [ ] Fading de música
- [ ] Audio pools

### Sistema de Partículas
**Tarefas Pendentes:**
- [ ] Particle pool manager
- [ ] Biblioteca de efeitos comuns
- [ ] LOD para partículas
- [ ] Limit de partículas simultâneas
- [ ] Quality settings para VFX

### Sistema de Loading
**Tarefas Pendentes:**
- [ ] Loading screen
- [ ] Async scene loading
- [ ] Progress bar
- [ ] Tips durante loading
- [ ] Stream de assets
- [ ] Unload de recursos não usados

### Sistema de Pause
**Tarefas Pendentes:**
- [ ] Pause menu
- [ ] Time.timeScale = 0
- [ ] UI de pause
- [ ] Options acessíveis no pause
- [ ] Salvar e sair
- [ ] Resume

### Sistema de Debug
**Tarefas Pendentes:**
- [ ] Console de debug in-game
- [ ] Comandos de cheat
- [ ] FPS counter
- [ ] Performance stats
- [ ] God mode
- [ ] Teleport commands
- [ ] Spawn items/enemies
- [ ] Level up command

### Sistema de Localization
**Tarefas Pendentes:**
- [ ] Framework de localização
- [ ] Arquivo de strings por idioma
- [ ] Suporte para PT-BR e EN
- [ ] Localização de UI
- [ ] Localização de diálogos
- [ ] Localização de itens/habilidades
- [ ] Troca dinâmica de idioma

## 🔧 Melhorias Sugeridas

### Input System
- [ ] Criar ScriptableObject de InputConfig
- [ ] Abstrair inputs com interfaces
- [ ] Suporte para AI usar mesmos inputs
- [ ] Replay system (gravar inputs)

### Camera
- [ ] Perfis de câmera por situação (exploração, combate)
- [ ] Cutscene camera system
- [ ] Screenshot mode
- [ ] Letterbox em momentos cinemáticos

### Performance
- [ ] Object pooling genérico
- [ ] Resource manager para assets
- [ ] Garbage collection otimizado
- [ ] Profiling tools

### Quality of Life
- [ ] Tutorial system
- [ ] Tooltips contextuais
- [ ] Hints system
- [ ] Achievements tracker
- [ ] Statistics (tempo jogado, inimigos mortos, etc)

### Multiplayer (Preparação)
- [ ] Network manager setup
- [ ] Client-server architecture
- [ ] Sincronização de estado
- [ ] Matchmaking básico
- [ ] Lobby system
- [ ] Host migration

## 📝 Notas Importantes

### Input System - Migração Necessária
O projeto tem `InputSystem_Actions.inputactions` configurado mas não está sendo usado. O InputManager atual usa Old Input System (Input.GetAxis, Input.GetKey). 

**Recomendação**: Migrar completamente para New Input System para:
- Melhor suporte a gamepad
- Rebinding fácil
- Input Actions reutilizáveis
- Melhor organização

### Singleton Pattern
Vários managers usam singleton:
- InputManager
- CheckpointManager
- PlayerDeathManager

**Padrão usado**:
```csharp
public static ClassName Instance { get; private set; }

void Awake() {
    if (Instance == null) Instance = this;
    else Destroy(gameObject);
}
```

### Camera Follow
CameraController é muito simples. Para produção, considerar:
- Cinemachine (package oficial Unity)
- Implementação custom mais robusta
- Sistema de rails para cutscenes

### Ground Check
GroundCheckRaycast existe mas integração não é clara. Verificar se:
- PlayerMotor usa para IsGrounded
- Raycast distance está calibrado
- Layer mask está correto

### Salvamento JSON Local
Planejado para multiplayer com salvamento local em cada máquina. Estrutura sugerida:
```json
{
  "character": { "level": 10, "stats": {...} },
  "inventory": [...],
  "quests": [...],
  "checkpoint": "checkpoint_id",
  "timestamp": "2025-11-01"
}
```

### Multiplayer - Peer-to-Peer
Arquitetura planejada: 1 jogador age como host/servidor, até 5 jogadores total. Considerar:
- Unity Netcode for GameObjects
- Mirror Networking
- Photon (se precisar de servidor dedicado)
- Custom solution com sockets

### Performance Crítica
Para 5 jogadores simultâneos:
- Limitar VFX
- Sincronizar apenas o necessário
- Priorizar objetos próximos
- LOD agressivo
- Network tick rate otimizado

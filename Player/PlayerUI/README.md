# Player UI System

Sistema modular de UI para o player, baseado em eventos e com capacidade de crescimento.

## 📂 Estrutura

```
PlayerUI/
├── PlayerUIManager.cs      - Coordenador central, escuta eventos do Character
├── PlayerDeathUI.cs        - Módulo de tela de morte (overlay, botões)
├── PlayerHealthUI.cs       - Módulo de barra de vida
├── PlayerEnergyUI.cs       - Módulo de barra de energia
├── PlayerAbilityUI.cs      - Módulo de UI de habilidades (TODO)
└── PlayerBuffUI.cs         - Módulo de UI de buffs (TODO)
```

## 🎯 Arquitetura

### Event-Driven Pattern
O sistema é baseado em **eventos** do `Character`:
- `OnDeath` → PlayerDeathUI mostra overlay
- `OnRevive` → PlayerDeathUI esconde overlay
- `OnHealthChanged` → PlayerHealthUI atualiza barra
- `OnEnergyChanged` → PlayerEnergyUI atualiza barra
- `OnLevelUp` → (futuro) Notificação de level up

### Separação de Responsabilidades

| Componente | Responsabilidade |
|------------|------------------|
| **PlayerUIManager** | Coordenador central, inscreve em eventos, delega para módulos |
| **PlayerDeathUI** | Tela de morte (overlay, botões respawn/menu, pause) |
| **PlayerHealthUI** | Barra de vida com animação suave |
| **PlayerEnergyUI** | Barra de energia com animação suave |
| **PlayerAbilityUI** | Cooldowns, charges, hotkeys (TODO) |
| **PlayerBuffUI** | Ícones de buffs, timers, stacks (TODO) |

### Fluxo de Dados

```
Character.OnDeath (evento)
    ↓
PlayerUIManager.HandleDeath()
    ↓
PlayerDeathUI.ShowDeathOverlay()
    ↓
UI visível + Time.timeScale = 0
```

## 🔧 Setup no Unity

### 1. Adicionar PlayerUIManager ao Player GameObject

```
Player GameObject
├── Character (obrigatório)
├── PlayerManager (obrigatório)
└── PlayerUIManager (novo!)
    ├── Reference: Character (auto-detectado)
    ├── Reference: PlayerManager (auto-detectado)
    └── Módulos de UI (arrastar e soltar)
```

### 2. Criar GameObjects de UI

```
Canvas (Screen Space - Overlay)
├── PlayerDeathUI (GameObject com script PlayerDeathUI)
│   └── DeathOverlayPanel (GameObject)
│       ├── RespawnButton (Button)
│       └── MainMenuButton (Button)
│
├── PlayerHealthUI (GameObject com script PlayerHealthUI)
│   ├── HealthBar (Image com fillAmount)
│   └── HealthText (Text)
│
└── PlayerEnergyUI (GameObject com script PlayerEnergyUI)
    ├── EnergyBar (Image com fillAmount)
    └── EnergyText (Text)
```

### 3. Configurar Referências no Inspector

**PlayerUIManager**:
- `Character`: Auto-detectado
- `Player Manager`: Auto-detectado
- `Death UI`: Arrastar PlayerDeathUI GameObject
- `Health UI`: Arrastar PlayerHealthUI GameObject
- `Energy UI`: Arrastar PlayerEnergyUI GameObject

**PlayerDeathUI**:
- `Death Overlay Panel`: Arrastar o painel de morte
- `Respawn Button`: Arrastar o botão de respawn
- `Main Menu Button`: Arrastar o botão de menu
- `Pause Game On Death`: true/false
- `Main Menu Scene Name`: "MainMenu"

**PlayerHealthUI**:
- `Health Bar`: Arrastar Image da barra
- `Health Text`: Arrastar Text
- `Smooth Transition`: true (animação suave)
- `Transition Speed`: 5

**PlayerEnergyUI**:
- `Energy Bar`: Arrastar Image da barra
- `Energy Text`: Arrastar Text
- `Smooth Transition`: true (animação suave)
- `Transition Speed`: 5

## ✅ Módulos Implementados

### PlayerDeathUI ✅
- Mostra/esconde overlay de morte
- Botão de respawn (chama CheckpointManager)
- Botão de menu principal
- Pausa o jogo opcionalmente

### PlayerHealthUI ✅
- Barra de vida com fillAmount
- Texto de vida atual/máxima
- Animação suave (lerp)

### PlayerEnergyUI ✅
- Barra de energia com fillAmount
- Texto de energia atual/máxima
- Animação suave (lerp)

## 📝 Módulos TODO

### PlayerAbilityUI ❌
- [ ] Slots de habilidades (Q, E, R, 1-8)
- [ ] Cooldown visual (radial fill)
- [ ] Charges counter
- [ ] Hotkey display
- [ ] Out of energy feedback

### PlayerBuffUI ❌
- [ ] Ícones de buffs ativos
- [ ] Timers de duração
- [ ] Stack counter
- [ ] Tooltips on hover
- [ ] Separação buffs/debuffs

### PlayerLevelUpUI ❌
- [ ] Notificação de level up
- [ ] Efeitos visuais/sonoros
- [ ] Stat increase display

### PlayerCastBarUI ❌
- [ ] Barra de cast para habilidades channeling
- [ ] Nome da habilidade sendo usada
- [ ] Cancelar cast feedback
- [ ] Tempo restante

## 🚀 Como Expandir

### Adicionar Novo Módulo de UI

1. **Criar script do módulo** (ex: `PlayerCastBarUI.cs`):
```csharp
public class PlayerCastBarUI : MonoBehaviour
{
    public void ShowCastBar(string abilityName, float castTime) { }
    public void UpdateCastBar(float progress) { }
    public void HideCastBar() { }
}
```

2. **Adicionar referência no PlayerUIManager**:
```csharp
[SerializeField] private PlayerCastBarUI castBarUI;
```

3. **Subscrever a eventos relevantes**:
```csharp
// Em SubscribeToEvents()
if (playerManager != null && playerManager.StateMachine != null)
{
    playerManager.StateMachine.OnStateChanged += HandleStateChanged;
}

// Event handler
private void HandleStateChanged(PlayerStateBase newState)
{
    if (newState is PlayerCastingState castingState)
    {
        castBarUI?.ShowCastBar("Fireball", 2.5f);
    }
}
```

## 🎮 Integração com PlayerDeathManager (REMOVIDO)

O antigo `PlayerDeathManager` foi **removido** pois sua única responsabilidade era gerenciar UI de morte, que agora é responsabilidade do `PlayerDeathUI` dentro do sistema modular.

### Migração:
- ❌ PlayerDeathManager.ShowDeathOverlay() 
- ✅ PlayerDeathUI.ShowDeathOverlay() (chamado via evento)

- ❌ PlayerDeathManager.OnRespawnClicked() 
- ✅ PlayerDeathUI.OnRespawnClicked()

- ❌ PlayerDeathManager.Instance (Singleton global)
- ✅ PlayerUIManager (Component no player, escuta eventos)

## 📊 Diagrama de Fluxo Completo

```
[Character.Die()]
    ↓
[Character.OnDeath event]
    ↓
┌───────────────────────────────────┐
│ PlayerUIManager.HandleDeath()    │
└───────────────────────────────────┘
    ↓
┌───────────────────────────────────┐
│ PlayerDeathUI.ShowDeathOverlay()  │
│ - Mostra painel                   │
│ - Pausa jogo (Time.timeScale=0)  │
└───────────────────────────────────┘
    ↓
[Player clica "Respawn"]
    ↓
┌───────────────────────────────────┐
│ CheckpointManager.RespawnPlayer() │
└───────────────────────────────────┘
    ↓
[Character.Revive()]
    ↓
[Character.OnRevive event]
    ↓
┌───────────────────────────────────┐
│ PlayerUIManager.HandleRevive()    │
└───────────────────────────────────┘
    ↓
┌───────────────────────────────────┐
│ PlayerDeathUI.HideDeathOverlay()  │
│ - Esconde painel                  │
│ - Despausa jogo (Time.timeScale=1)│
└───────────────────────────────────┘
```

## 🔍 Troubleshooting

### UI não atualiza
- Verificar se `PlayerUIManager` está no mesmo GameObject que `Character`
- Verificar se módulos de UI estão atribuídos no Inspector
- Verificar no Console se eventos estão sendo disparados

### Overlay de morte não aparece
- Verificar se `deathOverlayPanel` está atribuído
- Verificar se painel não está com `SetActive(true)` por padrão
- Verificar se `Character.OnDeath` está sendo disparado

### Barras não animam
- Verificar se `smoothTransition` está true
- Verificar se `Image.fillAmount` está configurado (não usar Width)
- Verificar se `Image.Type` = Filled

## 📚 Referências

- **Character.cs**: Dispara eventos (OnDeath, OnHealthChanged, etc.)
- **PlayerManager.cs**: Gerencia state machine do player
- **CheckpointManager.cs**: Gerencia respawn (posição, Revive())

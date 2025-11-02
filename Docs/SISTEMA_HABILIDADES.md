# Sistema de Habilidades - Status e Tarefas

## 📋 Visão Geral
Sistema de habilidades em três camadas: ScriptableObjects (dados), tipos específicos (melee/ranged/area), e instâncias runtime (VFX e lifecycle).

## ✅ Componentes Completos

### Ability (Base) (`Assets/Scripts/Abilities/Ability.cs`)
- ✅ Classe abstrata ScriptableObject
- ✅ Cálculo de dano por DamageNature
- ✅ Sistema de multiplicadores (physical/magical)
- ✅ Suporte para targeting e filtragem por LayerMask
- ✅ Sistema de custo de energia
- ✅ Cooldown e cast time
- ✅ Cancelamento por movimento
- ✅ Flags de controle (isCancelable, isCastable)
- ✅ Suporte para VFX e som
- ✅ AbilityContext para passar dados entre sistemas

### MeleeAbility (`Assets/Scripts/Abilities/AbilitysTypes/MeleeAbility.cs`)
- ✅ Herda de Ability
- ✅ CreateAssetMenu configurado
- ✅ Área de efeito com múltiplas formas (Sphere, Cone, Box)
- ✅ Offset frontal para ataque
- ✅ Integração com MeleeInstance
- ✅ Aplicação de dano em área

### AreaAbility (`Assets/Scripts/Abilities/AbilitysTypes/AreaAbility.cs`)
- ✅ Herda de Ability
- ✅ CreateAssetMenu configurado
- ✅ Duração configurável
- ✅ Sistema de tick damage
- ✅ Aplicação de buff ao entrar
- ✅ Remoção opcional de buff ao sair
- ✅ Suporte para prefab customizado
- ✅ Integração com AreaInstance

### RangedAbility
- ✅ Implementado (não visualizado mas listado em AbilitysTypes/)
- ✅ Integração com Projectile e RangedInstance

### PlayerAbilityManager (`Assets/Scripts/Player/PlayerAbilityManager.cs`)
- ✅ Gerenciamento de slots (BasicAttack + 6 skills)
- ✅ Sistema de cooldowns
- ✅ Sistema de charges
- ✅ Eventos (OnSlotReady, OnSlotCooldownTick, OnAbilityUsed)
- ✅ Validação de recursos (energia)
- ✅ Gerenciamento de instâncias ativas

### AbilitySlot (`Assets/Scripts/Abilities/AbilitySlot.cs`)
- ✅ Gerenciamento de cooldown individual
- ✅ Sistema de charges
- ✅ Validação CanUse()
- ✅ Atribuição dinâmica de habilidades
- ⚠️ Namespace LegacyDuplicates (refatorar?)

### Instâncias Runtime
- ✅ AbilityInstanceBase - classe base
- ✅ MeleeInstance - instância de ataque melee
- ✅ AreaInstance - área persistente
- ✅ RangedInstance - projétil
- ✅ Projectile - física de projétil
- ✅ AreaEffect - efeito de área contínuo
- ✅ AbilityPool - object pooling

### AbilityHelpers (`Assets/Scripts/Abilities/AbilitysTypes/AbilityHelpers.cs`)
- ✅ Métodos utilitários para aplicação de dano/buffs em área
- ✅ Filtragem por TeamFilter

## ⚠️ Funcionalidades Parciais

### Sistema de Targeting
- ✅ Targeting por LayerMask
- ✅ Filtragem por time (TeamFilter)
- ❌ Sem UI de seleção de alvo
- ❌ Sem indicador de alcance visual

**Tarefas Pendentes:**
- [ ] Criar UI de seleção de alvo
- [ ] Implementar indicador de alcance (círculo no chão)
- [ ] Preview de área de efeito antes de usar
- [ ] Highlight de alvos válidos
- [ ] Cancelamento de casting com ESC
- [ ] Cursor customizado por tipo de habilidade

### Sistema de Combo
**Tarefas Pendentes:**
- [ ] Definir chains de habilidades
- [ ] Bônus de dano por combo
- [ ] Timer de combo
- [ ] UI de combo counter
- [ ] Reset de combo ao tomar dano

### Sistema de Skill Tree
- ⚠️ Pasta existe mas implementação não verificada
- ⚠️ Namespace LegacyDuplicates

**Tarefas Pendentes:**
- [ ] Verificar estado atual do Skill Tree
- [ ] Implementar UI de skill tree
- [ ] Sistema de unlock de habilidades
- [ ] Requisitos de nível
- [ ] Dependências entre skills
- [ ] Sistema de reset de skills
- [ ] Preview de skills antes de desbloquear

## ❌ Funcionalidades Não Implementadas

### Sistema de Aprendizado de Habilidades
**Tarefas Pendentes:**
- [ ] Trainers/NPCs para ensinar skills
- [ ] Livros/itens que ensinam habilidades
- [ ] Habilidades por quest
- [ ] Limite de habilidades conhecidas
- [ ] Sistema de "esquecer" habilidades

### Sistema de Customização de Habilidades
**Tarefas Pendentes:**
- [ ] Modificadores de habilidade (runes, gems)
- [ ] Variantes de habilidades
- [ ] Sistema de evolução de skills
- [ ] Sinergia entre habilidades

### Sistema de Hotbar/Atalhos
**Tarefas Pendentes:**
- [ ] UI de hotbar visual
- [ ] Drag and drop de habilidades
- [ ] Múltiplas hotbars
- [ ] Salvamento de layout de hotbar
- [ ] Macros simples

### IA usando Habilidades
**Tarefas Pendentes:**
- [ ] NPCs/inimigos usando habilidades
- [ ] Priorização de habilidades por IA
- [ ] Validação de alcance antes de usar
- [ ] Cooldown tracking para IA
- [ ] Comportamento baseado em recursos (energia)

## 🔧 Melhorias Sugeridas

### Performance
- [ ] Otimizar pooling de projéteis
- [ ] Limitar partículas ativas simultaneamente
- [ ] LOD para VFX distantes
- [ ] Culling de abilities fora da câmera

### Balanceamento
- [ ] Sistema de scaling por nível
- [ ] Ajuste de cooldowns
- [ ] Balanceamento de custo de energia
- [ ] Ajuste de multiplicadores de dano

### VFX e Feedback
- [ ] Melhorar feedback de impacto
- [ ] Shake de câmera em habilidades poderosas
- [ ] Trail effects para projéteis
- [ ] Efeitos de charging durante cast time
- [ ] Som de cooldown pronto

### Acessibilidade
- [ ] Indicador de cooldown visual claro
- [ ] Opção de mostrar números de dano
- [ ] Color-coding por tipo de dano
- [ ] Avisos sonoros de recursos insuficientes

### Multiplayer (Preparação)
- [ ] Validação server-side de habilidades
- [ ] Predição client-side
- [ ] Reconciliação de estado
- [ ] Anti-cheat (cooldown, recursos)
- [ ] Sincronização de VFX

## 📝 Notas Importantes

### DamageNature Explicado
- **Physical/Magical**: Usa multiplicadores (physicalMultiplier, magicalMultiplier)
- **PhysicalTrue/MagicalTrue**: Usa stats diretos sem multiplicador
- **Mixed**: Combina ambos multiplicadores

### Padrão de Execução
1. PlayerAbilityManager.TryUseAbilityInSlot()
2. Valida energia e cooldown
3. Ability.Execute() cria instância runtime
4. Instância aplica VFX e lógica de dano
5. Slot entra em cooldown

### Legacy Code
- AbilitySlot tem namespace LegacyDuplicates
- Verificar se há código duplicado
- Migrar para namespace global

### Extensibilidade
- Fácil criar novos tipos herdando de Ability
- AbilityHelpers centraliza lógica comum
- AbilityContext desacopla sistemas

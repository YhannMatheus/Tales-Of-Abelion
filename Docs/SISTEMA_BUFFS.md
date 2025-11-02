# Sistema de Buffs/Debuffs - Status e Tarefas

## 📋 Visão Geral
Sistema de modificadores temporários com três mecanismos: buffs simples, buffs distribuídos (DoT/HoT), e slots de buff para UI.

## ✅ Componentes Completos

### BuffManager (`Assets/Scripts/BuffSystem/BuffManager.cs`)
- ✅ Gerenciamento de três tipos de buffs
- ✅ Sistema de buffs ativos (duration-based)
- ✅ Sistema de buffs distribuídos (tick-based preciso)
- ✅ Sistema de buff slots (stackable, UI-friendly)
- ✅ Aplicação/remoção automática de modificadores
- ✅ Update por frame para todos os timers
- ✅ Distribuição precisa de valores (evita arredondamento)

### BuffData (`Assets/Scripts/BuffSystem/BuffData.cs`)
- ✅ ScriptableObject configurável
- ✅ Lista de modificadores
- ✅ Configurações de duração e stacks
- ✅ Suporte para ícone e descrição

### BuffSlot (`Assets/Scripts/BuffSystem/BuffSlot.cs`)
- ✅ Gerenciamento de stacks
- ✅ Integração com UI
- ✅ Timer de duração
- ✅ Refresh de duração em reaplica

### Modificadores
- ✅ ModifierVar enum com todas as stats
- ✅ Aplicação a CharacterData.external___Bonus
- ✅ Suporte para valores flat e percentuais
- ✅ Tracking preciso de valores aplicados

## ⚠️ Funcionalidades Parciais

### Sistema de Buffs Ativos
- ✅ Aplicação e remoção funcionando
- ✅ Timer de duração
- ❌ Sem UI visual para mostrar buffs ativos
- ❌ Sem tooltip de descrição

**Tarefas Pendentes:**
- [ ] Criar UI de buffs ativos
- [ ] Tooltip com descrição ao passar mouse
- [ ] Indicador visual de duração restante
- [ ] Separar visualmente buffs de debuffs
- [ ] Animação ao adicionar/remover buff
- [ ] Som ao receber buff/debuff

### Sistema de Buffs Distribuídos
- ✅ Lógica de distribuição precisa
- ✅ Tracking de valores aplicados vs restantes
- ❌ Sem visualização de DoT/HoT em UI
- ❌ Números de dano/cura não aparecem

**Tarefas Pendentes:**
- [ ] Mostrar números de tick damage/heal
- [ ] Indicador visual de DoT/HoT ativo
- [ ] Animação de pulso durante ticks
- [ ] Sons de tick (opcional)
- [ ] Previsão de dano/cura total

### Sistema de Stacks
- ✅ Lógica de stack implementada
- ⚠️ Refresh de duração funciona?
- ❌ Sem indicador de quantidade de stacks na UI

**Tarefas Pendentes:**
- [ ] Verificar refresh de duração ao stack
- [ ] Mostrar contador de stacks na UI
- [ ] Animação ao ganhar stack
- [ ] Efeito visual diferente por quantidade de stacks
- [ ] Som ao atingir max stacks

## ❌ Funcionalidades Não Implementadas

### Buffs Condicionais
**Tarefas Pendentes:**
- [ ] Buffs que ativam sob condições (baixa vida, etc)
- [ ] Buffs que se consomem ao atacar
- [ ] Buffs que mudam efeito por stack
- [ ] Buffs com efeito em pulso (trigger ao expirar)
- [ ] Buffs que se propagam para aliados próximos

### Sistema de Auras
**Tarefas Pendentes:**
- [ ] Auras que afetam área ao redor
- [ ] Auras que aplicam buffs contínuos
- [ ] Múltiplas auras não stackáveis
- [ ] VFX de aura visível
- [ ] Remoção ao sair da área

### Imunidades e Dispel
**Tarefas Pendentes:**
- [ ] Sistema de imunidade a tipos de buff
- [ ] Dispel (remover buffs/debuffs)
- [ ] Cleanse (limpar debuffs negativos)
- [ ] Purge (remover buffs positivos)
- [ ] Proteção contra dispel

### Categorização
**Tarefas Pendentes:**
- [ ] Categorias de buff (físico, mágico, poison, etc)
- [ ] Limite de buffs por categoria
- [ ] Priorização ao atingir limite
- [ ] Buffs únicos (só pode ter 1)
- [ ] Grupos de exclusão mútua

### Interações Complexas
**Tarefas Pendentes:**
- [ ] Sinergia entre buffs
- [ ] Buffs que amplificam outros
- [ ] Conversão de buffs (positivo → negativo)
- [ ] Steal de buffs
- [ ] Reflexão de debuffs

## 🔧 Melhorias Sugeridas

### Performance
- [ ] Pool de BuffSlots para evitar alocações
- [ ] Limitar número máximo de buffs ativos
- [ ] Otimizar loop de update (early exit)
- [ ] Cache de listas para evitar garbage

### UI/UX
- [ ] Barras de progresso para duração
- [ ] Color coding por tipo de buff
- [ ] Ordenação (mais importante primeiro)
- [ ] Filtros (mostrar só debuffs, etc)
- [ ] Indicador quando próximo de expirar
- [ ] Preview de stats com buff antes de aplicar

### Debug
- [ ] Comando de console para adicionar buff
- [ ] Comando para remover todos os buffs
- [ ] Visualização de buffs ativos no inspector
- [ ] Log detalhado de aplicação/remoção
- [ ] Gizmos para auras

### Balanceamento
- [ ] Diminishing returns para CCs
- [ ] Caps para stacks de certos buffs
- [ ] Scaling de duração com stats
- [ ] Resistência a debuffs

### Multiplayer (Preparação)
- [ ] Sincronização de buffs ativos
- [ ] Validação server-side
- [ ] Timestamp de aplicação para sync
- [ ] Reconciliação de buffs expirados
- [ ] Compressão de dados de buff

## 📝 Notas Importantes

### Três Mecanismos de Buff

1. **activeBuffs** (List<ActiveBuff>)
   - Buffs simples com duração fixa
   - Aplicam modificadores imediatamente
   - Removem ao expirar
   - Uso: buffs de stats temporários

2. **distributedBuffs** (List<DistributedBuff>)
   - Distribuição precisa ao longo do tempo
   - Evita problemas de arredondamento
   - Tracking de valores aplicados
   - Uso: DoT/HoT, efeitos graduais

3. **buffSlots** (List<BuffSlot>)
   - Stackable, com limite configurável
   - Integração com UI
   - Refresh de duração ao reaplicar
   - Uso: buffs visíveis na tela

### Padrão de Aplicação
```csharp
// Buff simples
buffManager.ApplyBuff(buffData);

// Buff distribuído (DoT/HoT)
buffManager.ApplyDistributedBuff(buffData, tickInterval);

// Buff com slot (UI)
buffManager.ApplyBuffToSlot(buffData);
```

### ModifierVar Disponíveis
- physicalDamage, physicalResistence
- magicalDamage, magicalResistence
- criticalChance, criticalDamage
- attackSpeed, speed, luck
- maxHealth, maxEnergy
- healthRegen

### Precisão de Distribuição
O sistema de buffs distribuídos usa tracking de float para valores aplicados, mas converte para int quando necessário (vida, energia). Isso garante que o valor total aplicado seja exatamente o configurado, sem perdas por arredondamento.

### Integração com Character
Todos os modificadores são aplicados aos campos `external___Bonus` do CharacterData, que são somados nas propriedades calculadas `Total___`.

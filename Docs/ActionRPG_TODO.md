# Action RPG — TODO (prático)

Este arquivo sumariza o estado atual do sistema de habilidades (baseado nos scripts lidos) e descreve um TODO priorizado e acionável para transformar o projeto numa base de Action RPG jogável.

## Visão rápida do que já existe (arquivos-chave lidos)
- `Assets/Scripts/Abilities/Data/AbilityEnums.cs` — enums e `AbilityContext` (canônico).
- `Assets/Scripts/Abilities/Data/AbilitySlot.cs` — slot de habilidade (cooldown, charges, UpdateAssignedAbility).
- `Assets/Scripts/Abilities/Ability.cs` — ScriptableObject base para habilidades (dados, cálculo de dano, flags isCancelable/isCastable).
- `Assets/Scripts/Abilities/AbilitysTypes/` — implementações: `MeleeAbility`, `RangedAbility`, `AreaAbility`.
- `Assets/Scripts/Abilities/Instances/AbilityInstanceBase.cs` — base para instâncias em runtime (Initialize, Cancel, Finish).
- `Assets/Scripts/Abilities/Instances/RangedInstance.cs` — exemplo de execução: espera castTime, lança `Projectile`, usa `Finish()`/Cancel().
- `Assets/Scripts/Player/PlayerAbilityManager.cs` — sistema de slots do jogador, tick de cooldown, eventos (OnSlotReady, OnAbilityUsed), uso de `AbilitySlot`.
- `Assets/Scripts/Abilities/Skill Tree/PlayerSkillTree.cs` e `Assets/Scripts/Abilities/Data/SkillTree.cs` — sistema de skill tree com IDs únicos e runtime `PlayerSkillTree` para progresso.

## Observações sobre o estado atual / lacunas detectadas
- Sistema de abilities já é data-driven (ScriptableObjects) com separação Data vs Instance — muito bom.
- Existem instâncias runtime (AbilityInstanceBase) que criam GameObjects para comportamento; RangedInstance já instancia `Projectile` e passa parâmetros.
- `AbilitySlot`/`PlayerAbilityManager` provê suporte para cooldowns, charges, eventos — pronto para ligação com UI.
- SkillTree e PlayerSkillTree estão implementados com IDs e runtime persistência básica (lista de ids/ranks) — bom para progressão.

Lacunas e pontos a implementar/priorizar:
- UI: barra de habilidades, indicadores de cooldown, notificações de recursos insuficientes, skill tree UI.
- Saving/Loading do `PlayerSkillTree` e atribuição/estado de `AbilitySlot` (preservar cooldown/charges ao recarregar).
- Pooling para projectiles/instâncias (existe menção a `AbilityPool` — confirmar/implementar).
- Sistema de buffs/debuffs (aplicação, duração, stacks) — mencionado em alguns arquivos (BuffData) mas precisa de manager e slots.
- Integração com sistema de input/camera/controle do jogador (InputManager/PlayerMotor) para lançar habilidades via UI/teclas/atalhos.
- Interações com animação (Animator indices/triggers) e sincronização de castTime/animations.
- Detecção de colisões e aplicação de dano (Projectile, AreaInstance e helpers devem ser revisados).
- Tests/unit e scenes de exemplo (uma cena de combate para testar capacidades básicas).
- Tratamento de multiplayer/rollback (opcional, fora do escopo imediato).

---

## TODO priorizado (MVP -> Nice-to-have)

### MVP (prioridade alta — o mínimo para um Action RPG jogável)
1. Input e controle do jogador
   - Criar/garantir um `InputManager` que dispara: movimento, alvo, uso de slot (1-6), uso de ataque básico.
   - Mapear teclas/atalhos e orbitar para UI.
   - Arquivo(s relevantes: `PlayerMotor`, `InputManager`, `PlayerManager`).

2. HUD / UI de habilidade
   - Barra de habilidades (mostra ícone, cooldown, charges).
   - Feedback visual quando habilidade é usada / sem energia.
   - Ligar eventos de `PlayerAbilityManager` (OnSlotCooldownTick, OnSlotReady, OnInsufficientResources) para atualizar UI.

3. Combate básico
   - Garantir `BasicAttackSlot` e SkillSlots funcionalmente vinculados ao `PlayerAbilityManager`.
   - Implementar/provar inimigos simples com `Health` e `TakeDamage` para testar dano físico/mágico.
   - Testar `MeleeAbility`, `RangedAbility` (projetil) e `AreaAbility` em cena.

4. Projectiles e pooling
   - Revisar `Projectile` (inicialização, impacto, teams/layers) e garantir que tenha `ConfigureProjectile(...)` como RangedInstance espera.
   - Implementar `AbilityPool` simples: Get(prefab) / Release(instance) para reduzir Instantiate/Destroy.

5. Sistema de dano e filtros
   - `AbilityHelpers.ApplyAreaDamageAndBuffs` deve aplicar dano com filtro de layers/teams e invocar eventos de hit.
   - Implementar Buff application API básica para efeitos temporários (stun, dot, stat buff).

6. Scene de exemplo e testes rápidos
   - Criar uma cena "CombatDemo" com player, 2-3 inimigos, UI de habilidade, e cenários de teste (skillshot, melee, area).

Aceitação: Player consegue usar ao menos 2 habilidades diferentes, ver cooldowns na UI, inimigos recebem dano, e não há exceções no console.

---

### Iteração 2 (prioridade média)
1. Skill Tree + Progressão
   - Implementar UI de skill tree que consome `PlayerSkillTree.AddSkillPoints` e chama `UnlockNode`.
   - Persistir progresso (save/load JSON ou PlayerPrefs).

2. Buff/Debuff system completo
   - `BuffData`, `BuffSlot`, `BuffManager` para controlar duração, stacks e remoção.
   - Ações de area que aplicam buffOnEnter/Exit (AreaAbility).

3. Animações e feedback
   - Integrar `Animator` com index/params em `Ability` (animationIndex) e eventos de timelines (para sincronizar hit frames).

4. Polimento de combate
   - Hit reactions, VFX/particles, SFX nas habilidades.
   - FX pooling para partículas.

---

### Nice-to-have (prioridade baixa)
- Inventário / equipamentos / atributos com recalculo de stats (para influência nas capacidades de dano e multiplicadores).
- Save/load completo (JSON com skilltree, slots, equipamentos, posição).
- AI avançada (pathfinding, state machines) para inimigos.
- Multiplayer (replicação de estado de habilidade) — projeto maior.
- Editor tools: custom inspectors para `SkillTree` e `Ability` para facilitar designers.

---

## Tarefas técnicas e de engenharia (curto prazo)
- Consolidar nomes e namespaces (já comecei movendo duplicatas para `LegacyDuplicates`).
- Padronizar `AbilityContext` (já existe canônico em Data). Garantir todos os arquivos usam `global::AbilityContext` se necessário.
- Adicionar testes unitários / playmode tests para: cooldown ticking, uso de slot, aplicação de dano.
- Criar uma cena e um Playtest checklist (lista de passos para validacao).

## Arquivos/Componentes a revisar urgentemente
- `Projectile` (verificar API `Initialize` e `ConfigureProjectile`).
- `AbilityPool` (implementar/confirmar existência).
- `AbilityHelpers` (ApplyAreaDamageAndBuffs) — garantir compatibilidade com `TeamFilter`/LayerMask.
- `PlayerAbilityManager` — adicionar métodos para serialização do estado dos slots se quiser persistir cooldown/charges.

## Plano de implementação (sprint curto — 2 semanas estimado)
Semana 1 (MVP básico):
- Dia 1–2: InputManager + Player control + UI barra de habilidades (integração com PlayerAbilityManager).
- Dia 3–5: Implementar/confirmar Projectile & AbilityPool; testar RangedAbility -> Projectile; ajustar RangedInstance.
- Dia 6–7: Cena CombatDemo + testes de melee/area + ajustes rápidos.

Semana 2 (polimento mínimo):
- Dia 8–10: Buff system mínimo (BuffData + apply/remove), AreaAbility tick tests.
- Dia 11–12: SkillTree UI + persistence básica.
- Dia 13–14: Fixes, performance (profiling), organizar documentação.

## Como eu posso te ajudar agora (sugestões específicas)
- Gerar os scripts faltantes: `AbilityPool.cs`, `Projectile.cs` (se estiver faltando), `AbilityHelpers` se precisar de ajustes.
- Implementar a UI de habilidade (prefab + script que escuta `PlayerAbilityManager`).
- Criar a cena `CombatDemo` com prefabs e exemplos de habilidades.

---

## Nota final
Este TODO foi gerado a partir dos scripts existentes e das lacunas detectadas na leitura. Se você quer que eu gere automaticamente qualquer um dos artefatos acima (por exemplo: um `AbilityPool` pronto, um `Projectile` skeleton, ou a cena `CombatDemo`), diga qual item prefere que eu implemente primeiro e eu crio os arquivos/edits necessários.


> Arquivo gerado automaticamente pelo agente — revise e ajuste prioridades conforme o seu roadmap.

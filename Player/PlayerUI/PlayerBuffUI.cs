using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// Módulo de UI responsável pela exibição de buffs/debuffs do player.
/// Usa GridLayoutGroup para organização automática dos ícones.
/// 
/// Estrutura esperada:
/// - BuffGridContainer (GameObject com GridLayoutGroup)
///   - BuffIconPrefab (Prefab circular)
///     - Background (Image circular)
///     - Overlay (Image circular - para timer visual)
///     - Icon (Image - ícone do buff)
///     - StackText (TextMeshProUGUI - "x3")
///     - DurationText (TextMeshProUGUI opcional - "5.2s")
/// </summary>
public class PlayerBuffUI : MonoBehaviour
{
    [Header("Grid Container")]
    [Tooltip("GameObject com GridLayoutGroup onde os buffs serão instanciados")]
    [SerializeField] private Transform buffGridContainer;

    [Header("Prefab")]
    [Tooltip("Prefab do ícone de buff (circular com Background, Overlay, Icon, StackText)")]
    [SerializeField] private GameObject buffIconPrefab;

    [Header("Configurações")]
    [Tooltip("Número máximo de buffs para exibir simultaneamente")]
    [SerializeField] private int maxBuffsToShow = 20;

    [Tooltip("Cor de borda para buffs positivos")]
    [SerializeField] private Color buffBorderColor = Color.green;

    [Tooltip("Cor de borda para debuffs negativos")]
    [SerializeField] private Color debuffBorderColor = Color.red;

    [Tooltip("Mostrar texto de duração?")]
    [SerializeField] private bool showDurationText = true;

    // Pool de ícones de buff reutilizáveis
    private List<BuffIconUI> activeBuffIcons = new List<BuffIconUI>();
    private Queue<BuffIconUI> inactiveBuffIcons = new Queue<BuffIconUI>();

    // Tracking de buffs ativos
    private Dictionary<BuffData, BuffIconUI> buffToIconMap = new Dictionary<BuffData, BuffIconUI>();

    /// <summary>
    /// Classe interna que representa um slot de buff na UI
    /// Estrutura: Background -> Icon -> TimerRadial -> Overlay -> StackText
    /// </summary>
    private class BuffIconUI
    {
        public GameObject gameObject;
        public Image background;        // Fundo do slot
        public Image icon;              // Ícone do buff (trocado dinamicamente)
        public Image timerRadial;       // Timer circular radial (fillAmount 1→0)
        public Image overlay;           // Borda/moldura decorativa na frente
        public TextMeshProUGUI stackText;
        public TextMeshProUGUI durationText;
        public BuffData buffData;
        public float remainingDuration;
        public bool isBuff; // true = buff, false = debuff
    }

    private void Awake()
    {
        // Valida configuração
        if (buffGridContainer == null)
        {
            Debug.LogError("[PlayerBuffUI] BuffGridContainer não configurado!");
        }

        if (buffIconPrefab == null)
        {
            Debug.LogError("[PlayerBuffUI] BuffIconPrefab não configurado!");
        }

        // Verifica se o container tem GridLayoutGroup
        if (buffGridContainer != null && buffGridContainer.GetComponent<GridLayoutGroup>() == null)
        {
            Debug.LogWarning("[PlayerBuffUI] BuffGridContainer não tem GridLayoutGroup! Adicionando...");
            var grid = buffGridContainer.gameObject.AddComponent<GridLayoutGroup>();
            ConfigureDefaultGrid(grid);
        }
    }

    private void Update()
    {
        // Atualiza timers de duração
        UpdateBuffDurations();
    }

    /// <summary>
    /// Configura GridLayoutGroup com valores padrão
    /// </summary>
    private void ConfigureDefaultGrid(GridLayoutGroup grid)
    {
        grid.cellSize = new Vector2(50, 50); // Ícones circulares 50x50
        grid.spacing = new Vector2(5, 5);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.UpperLeft;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 10; // Máximo 10 buffs por linha
    }

    /// <summary>
    /// Adiciona um buff à exibição
    /// </summary>
    public void AddBuff(BuffData buff, float duration, bool isBuff = true)
    {
        if (buff == null) return;

        // Se já existe, atualiza ao invés de adicionar
        if (buffToIconMap.ContainsKey(buff))
        {
            UpdateBuff(buff, duration);
            return;
        }

        // Verifica limite máximo
        if (activeBuffIcons.Count >= maxBuffsToShow)
        {
            Debug.LogWarning($"[PlayerBuffUI] Limite de {maxBuffsToShow} buffs atingido!");
            return;
        }

        // Pega um ícone do pool ou cria novo
        BuffIconUI iconUI = GetBuffIconFromPool();

        // Configura o ícone
        iconUI.buffData = buff;
        iconUI.remainingDuration = duration;
        iconUI.isBuff = isBuff;

        // 1. Background: Fundo neutro do slot
        if (iconUI.background != null)
        {
            iconUI.background.color = new Color(0.2f, 0.2f, 0.2f, 1f); // Cinza escuro
        }

        // 2. Icon: Sprite do buff (trocado dinamicamente)
        if (iconUI.icon != null && buff.icon != null)
        {
            iconUI.icon.sprite = buff.icon;
            iconUI.icon.enabled = true;
        }

        // 3. TimerRadial: Timer circular que esvazia (fillAmount 1→0)
        if (iconUI.timerRadial != null)
        {
            iconUI.timerRadial.fillAmount = 1f; // Começa cheio
            iconUI.timerRadial.color = new Color(0f, 0f, 0f, 0.5f); // Preto semi-transparente
        }

        // 4. Overlay: Borda/moldura decorativa colorida (na frente de tudo)
        if (iconUI.overlay != null)
        {
            iconUI.overlay.color = isBuff ? buffBorderColor : debuffBorderColor;
        }

        // Adiciona à lista de ativos
        activeBuffIcons.Add(iconUI);
        buffToIconMap[buff] = iconUI;

        // Ativa o GameObject
        iconUI.gameObject.SetActive(true);

        Debug.Log($"[PlayerBuffUI] Buff adicionado: {buff.buffName} (duração: {duration}s, tipo: {(isBuff ? "Buff" : "Debuff")})");
    }

    /// <summary>
    /// Remove um buff da exibição
    /// </summary>
    public void RemoveBuff(BuffData buff)
    {
        if (buff == null || !buffToIconMap.ContainsKey(buff)) return;

        BuffIconUI iconUI = buffToIconMap[buff];

        // Remove do tracking
        buffToIconMap.Remove(buff);
        activeBuffIcons.Remove(iconUI);

        // Retorna ao pool
        ReturnBuffIconToPool(iconUI);

        Debug.Log($"[PlayerBuffUI] Buff removido: {buff.buffName}");
    }

    /// <summary>
    /// Atualiza a duração de um buff existente
    /// </summary>
    private void UpdateBuff(BuffData buff, float newDuration)
    {
        if (!buffToIconMap.ContainsKey(buff)) return;

        BuffIconUI iconUI = buffToIconMap[buff];
        iconUI.remainingDuration = newDuration;
    }

    /// <summary>
    /// Atualiza a duração restante de um buff
    /// </summary>
    public void UpdateBuffDuration(BuffData buff, float remainingTime)
    {
        UpdateBuff(buff, remainingTime);
    }

    /// <summary>
    /// Atualiza os stacks de um buff stackável
    /// </summary>
    public void UpdateBuffStacks(BuffData buff, int currentStacks)
    {
        if (!buffToIconMap.ContainsKey(buff)) return;

        BuffIconUI iconUI = buffToIconMap[buff];

        if (iconUI.stackText != null)
        {
            if (currentStacks > 1)
            {
                iconUI.stackText.text = $"x{currentStacks}";
                iconUI.stackText.enabled = true;
            }
            else
            {
                iconUI.stackText.enabled = false;
            }
        }
    }

    /// <summary>
    /// Atualiza os timers de todos os buffs ativos
    /// </summary>
    private void UpdateBuffDurations()
    {
        foreach (var iconUI in activeBuffIcons)
        {
            if (iconUI.remainingDuration > 0f)
            {
                iconUI.remainingDuration -= Time.deltaTime;

                // Atualiza timer radial visual (fillAmount de 1 → 0)
                if (iconUI.timerRadial != null && iconUI.buffData != null)
                {
                    float totalDuration = iconUI.buffData.duration;
                    float fillAmount = Mathf.Clamp01(iconUI.remainingDuration / totalDuration);
                    iconUI.timerRadial.fillAmount = fillAmount;
                }

                // Atualiza texto de duração
                if (showDurationText && iconUI.durationText != null)
                {
                    iconUI.durationText.text = $"{iconUI.remainingDuration:F1}s";
                    iconUI.durationText.enabled = true;
                }

                // Remove automaticamente quando expirar
                if (iconUI.remainingDuration <= 0f)
                {
                    RemoveBuff(iconUI.buffData);
                }
            }
        }
    }

    /// <summary>
    /// Pega um ícone do pool ou cria um novo
    /// </summary>
    private BuffIconUI GetBuffIconFromPool()
    {
        BuffIconUI iconUI;

        if (inactiveBuffIcons.Count > 0)
        {
            // Reutiliza do pool
            iconUI = inactiveBuffIcons.Dequeue();
        }
        else
        {
            // Cria novo
            iconUI = CreateNewBuffIcon();
        }

        return iconUI;
    }

    /// <summary>
    /// Cria um novo ícone de buff instanciando o prefab
    /// </summary>
    private BuffIconUI CreateNewBuffIcon()
    {
        GameObject instance = Instantiate(buffIconPrefab, buffGridContainer);
        BuffIconUI iconUI = new BuffIconUI();
        iconUI.gameObject = instance;

        // Busca componentes (estrutura: Background -> Icon -> TimerRadial -> Overlay -> StackText)
        iconUI.background = instance.transform.Find("Background")?.GetComponent<Image>();
        iconUI.icon = instance.transform.Find("Icon")?.GetComponent<Image>();
        iconUI.timerRadial = instance.transform.Find("TimerRadial")?.GetComponent<Image>();
        iconUI.overlay = instance.transform.Find("Overlay")?.GetComponent<Image>();
        iconUI.stackText = instance.transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();
        iconUI.durationText = instance.transform.Find("DurationText")?.GetComponent<TextMeshProUGUI>();

        // Valida componentes obrigatórios
        if (iconUI.icon == null)
        {
            Debug.LogWarning("[PlayerBuffUI] Icon não encontrado no prefab! Procure por 'Icon'");
        }

        if (iconUI.timerRadial == null)
        {
            Debug.LogWarning("[PlayerBuffUI] TimerRadial não encontrado no prefab! Procure por 'TimerRadial'");
        }

        // Configura TimerRadial como radial fill (caso não esteja configurado no prefab)
        if (iconUI.timerRadial != null)
        {
            iconUI.timerRadial.type = Image.Type.Filled;
            iconUI.timerRadial.fillMethod = Image.FillMethod.Radial360;
            iconUI.timerRadial.fillOrigin = (int)Image.Origin360.Top;
            iconUI.timerRadial.fillClockwise = false;
            iconUI.timerRadial.fillAmount = 1f;
        }

        return iconUI;
    }

    /// <summary>
    /// Retorna um ícone ao pool
    /// </summary>
    private void ReturnBuffIconToPool(BuffIconUI iconUI)
    {
        iconUI.gameObject.SetActive(false);
        iconUI.buffData = null;
        iconUI.remainingDuration = 0f;

        inactiveBuffIcons.Enqueue(iconUI);
    }

    /// <summary>
    /// Limpa todos os buffs da UI
    /// </summary>
    public void ClearAllBuffs()
    {
        // Copia a lista para evitar modificação durante iteração
        var buffsToRemove = new List<BuffData>(buffToIconMap.Keys);

        foreach (var buff in buffsToRemove)
        {
            RemoveBuff(buff);
        }

        Debug.Log("[PlayerBuffUI] Todos os buffs removidos");
    }

    /// <summary>
    /// Retorna quantos buffs estão ativos
    /// </summary>
    public int GetActiveBuffCount()
    {
        return activeBuffIcons.Count;
    }
}

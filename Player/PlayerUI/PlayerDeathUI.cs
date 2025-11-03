using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Módulo de UI responsável pela tela de morte do player.
/// Gerencia overlay, botões de respawn/menu, e opcionalmente pausa o jogo.
/// </summary>
public class PlayerDeathUI : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Painel de overlay que aparece quando o player morre")]
    [SerializeField] private GameObject deathOverlayPanel;
    
    [Tooltip("Botão de respawn")]
    [SerializeField] private Button respawnButton;
    
    [Tooltip("Botão de voltar ao menu principal")]
    [SerializeField] private Button mainMenuButton;

    [Header("Configurações")]
    [Tooltip("Pausar o jogo quando o player morrer?")]
    [SerializeField] private bool pauseGameOnDeath = true;

    [Tooltip("Nome da cena do menu principal")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private Character deadPlayer;

    private void Awake()
    {
        // Garante que o overlay começa desativado
        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(false);
        }

        // Configura listeners dos botões
        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    private void OnDestroy()
    {
        // Remove listeners ao destruir
        if (respawnButton != null)
        {
            respawnButton.onClick.RemoveListener(OnRespawnClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }
    }

    /// <summary>
    /// Mostra o overlay de morte
    /// </summary>
    public void ShowDeathOverlay(Character player)
    {
        deadPlayer = player;

        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(true);
        }

        if (pauseGameOnDeath)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("[PlayerDeathUI] Overlay de morte exibido");
    }

    /// <summary>
    /// Esconde o overlay de morte
    /// </summary>
    public void HideDeathOverlay()
    {
        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(false);
        }

        if (pauseGameOnDeath)
        {
            Time.timeScale = 1f;
        }

        deadPlayer = null;

        Debug.Log("[PlayerDeathUI] Overlay de morte escondido");
    }

    /// <summary>
    /// Callback do botão de respawn
    /// </summary>
    private void OnRespawnClicked()
    {
        if (deadPlayer != null)
        {
            // Chama o CheckpointManager para respawnar o player
            CheckpointManager.Instance?.RespawnPlayer(deadPlayer);
        }

        HideDeathOverlay();
    }

    /// <summary>
    /// Callback do botão de menu principal
    /// </summary>
    private void OnMainMenuClicked()
    {
        HideDeathOverlay();
        Time.timeScale = 1f; // Garante que o tempo volta ao normal
        SceneManager.LoadScene(mainMenuSceneName);
    }
}

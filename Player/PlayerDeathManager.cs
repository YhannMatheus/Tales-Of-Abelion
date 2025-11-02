using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerDeathManager : MonoBehaviour
{
    public static PlayerDeathManager Instance { get; private set; }

    [Header("UI References")]
    public GameObject deathOverlayPanel;
    public Button respawnButton;
    public Button mainMenuButton;

    private Character deadPlayer;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(false);
        }

        if (respawnButton != null)
        {
            respawnButton.onClick.AddListener(OnRespawnClicked);
        }

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
    }

    public void ShowDeathOverlay(Character player)
    {
        deadPlayer = player;

        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(true);
        }

        Time.timeScale = 0f;
    }

    private void OnRespawnClicked()
    {
        if (deadPlayer != null)
        {
            //CheckpointManager.Instance?.RespawnPlayer(deadPlayer);
        }

        HideDeathOverlay();
    }

    private void OnMainMenuClicked()
    {
        HideDeathOverlay();
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    private void HideDeathOverlay()
    {
        if (deathOverlayPanel != null)
        {
            deathOverlayPanel.SetActive(false);
        }

        Time.timeScale = 1f;
    }
}
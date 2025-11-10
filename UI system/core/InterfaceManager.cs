using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(EnergyInterfaceController))]
[RequireComponent(typeof(HealthInterfaceController))]
[RequireComponent(typeof(ExperienceInterfaceController))]
public class InterfaceManager : MonoBehaviour
{
    [Header(" Estatisticas do Personagem ")]
    public PlayerManager player;
    public float smoothSpeed = 5f;

    [Header(" Barra de Vida ")]
    public HealthInterfaceController healthController;
    public Image healthImage;

    [Header(" Barras de Recursos ")]
    public EnergyInterfaceController energyController;
    public Image energyImage;
    public Sprite manaSprite;
    public Sprite staminaSprite;
    public Sprite furySprite;

    [Header(" Barra de Experiência ")]
    public ExperienceInterfaceController experienceController;
    public Image experienceImage;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currentExperienceText;


    [Header(" Containers ")]
    public GameObject HUDContainer;
    public GameObject PauseMenuContainer;
    public GameObject DeathMenuContainer;
    public GameObject InventoryContainer;
    public GameObject MapContainer;
    public GameObject SkillsContainer;

    private void Initializate()
    {
        // inicializar controladores
        //! vida
        healthController.CharacterManager = player.Character;
        healthController.healthFillImage = healthImage;

        //? energia
        energyController.CharacterManager = player.Character;
        switch (player.Character.Data.energyType)
        {
            case EnergyType.Mana:
                energyImage.sprite = manaSprite;
                break;
            case EnergyType.Stamina:
                energyImage.sprite = staminaSprite;
                break;
            case EnergyType.Fury:
                energyImage.sprite = furySprite;
                break;
        }
        energyController.energyFillImage = energyImage;

        //* experiencia
        experienceController.CharacterManager = player.Character;
        experienceController.experienceFillImage = experienceImage;
        experienceController.levelText = levelText;
        experienceController.currentExperienceText = currentExperienceText;
    }

    public void Awake()
    {
        Initializate();

        HUDContainer.SetActive(true);
        PauseMenuContainer.SetActive(false);
        DeathMenuContainer.SetActive(false);
        InventoryContainer.SetActive(false);
        MapContainer.SetActive(false);
        SkillsContainer.SetActive(false);
    }

    void Update()
    {
        // ESC - Menu de Pausa (fecha todos os outros menus)
        if (InputManager.Instance.pauseInput)
        {
            bool isPausing = !PauseMenuContainer.activeSelf;
            
            PauseMenuContainer.SetActive(isPausing);
            Time.timeScale = isPausing ? 0f : 1f;
            
            // Fecha outros menus ao pausar
            if (isPausing)
            {
                InventoryContainer.SetActive(false);
                MapContainer.SetActive(false);
                SkillsContainer.SetActive(false);
            }
        }

        // I - Inventário (só abre se não estiver pausado)
        if (InputManager.Instance.inventoryInput && !PauseMenuContainer.activeSelf)
        {
            bool isOpening = !InventoryContainer.activeSelf;
            
            InventoryContainer.SetActive(isOpening);
            
            // Fecha outros menus ao abrir inventário
            if (isOpening)
            {
                MapContainer.SetActive(false);
                SkillsContainer.SetActive(false);
            }
        }

        // P - Mapa Mundi (só abre se não estiver pausado)
        if (InputManager.Instance.worldMapInput && !PauseMenuContainer.activeSelf)
        {
            bool isOpening = !MapContainer.activeSelf;
            
            MapContainer.SetActive(isOpening);
            
            // Fecha outros menus ao abrir mapa
            if (isOpening)
            {
                InventoryContainer.SetActive(false);
                SkillsContainer.SetActive(false);
            }
        }

        // J ou K - Árvore de Skills (só abre se não estiver pausado)
        if ((InputManager.Instance.skillTreeInput || InputManager.Instance.skillSelectionInput) 
            && !PauseMenuContainer.activeSelf)
        {
            bool isOpening = !SkillsContainer.activeSelf;
            
            SkillsContainer.SetActive(isOpening);
            
            // Fecha outros menus ao abrir skills
            if (isOpening)
            {
                InventoryContainer.SetActive(false);
                MapContainer.SetActive(false);
            }
        }
    }

}
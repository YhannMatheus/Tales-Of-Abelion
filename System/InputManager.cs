using UnityEngine;

public class InputManager : MonoBehaviour
{
    [Header("Game Inputs")]
    public float horizontalInput { get; private set; }
    public float verticalInput { get; private set; }
    public bool interactButton { get; private set; }
    public bool attackInput { get; private set; }

    [Header("Interface inputs")]
    public bool pauseInput { get; private set; }
    public bool inventoryInput { get; private set; }
    public bool worldMapInput { get; private set; }
    public bool characterMenuInput { get; private set; }
    public bool skillTreeInput { get; private set; }

    [Header("Ability Inputs")]
    public bool basicAttackInput { get; private set; }
    public bool ability1Input { get; private set; }
    public bool ability2Input { get; private set; }
    public bool ability3Input { get; private set; }
    public bool ability4Input { get; private set; }
    public bool ability5Input { get; private set; }
    public bool ability6Input { get; private set; }
    public bool ability7Input { get; private set; }
    public bool ability8Input { get; private set; }

    private void Update()
    {
        // Game Inputs
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");
        interactButton = Input.GetMouseButtonDown(1);
        attackInput = Input.GetMouseButton(0);

        // Interface Inputs
        inventoryInput = Input.GetKeyDown(KeyCode.I);
        pauseInput = Input.GetKeyDown(KeyCode.Escape);
        worldMapInput = Input.GetKeyDown(KeyCode.U);
        characterMenuInput = Input.GetKeyDown(KeyCode.O);
        skillTreeInput = Input.GetKeyDown(KeyCode.J);

        // Ability Inputs
        basicAttackInput = Input.GetMouseButtonDown(0);
        ability1Input = Input.GetKeyDown(KeyCode.Q);
        ability2Input = Input.GetKeyDown(KeyCode.E);
        ability3Input = Input.GetKeyDown(KeyCode.R);
        ability5Input = Input.GetKeyDown(KeyCode.Alpha1);
        ability6Input = Input.GetKeyDown(KeyCode.Alpha2);
        ability7Input = Input.GetKeyDown(KeyCode.Alpha3);
    }
}
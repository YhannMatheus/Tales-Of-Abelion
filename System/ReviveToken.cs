using UnityEngine;

public class ReviveToken : MonoBehaviour
{
    [Header("Token Settings")]
    public float interactionRange = 2f;
    public float reviveTime = 3f;
    public float tokenLifetime = 60f;
    public KeyCode interactKey = KeyCode.E;

    [Header("Visual Feedback")]
    public GameObject interactionPrompt;
    public GameObject visualEffect;
    public float rotationSpeed = 50f;
    public float floatSpeed = 1f;
    public float floatAmplitude = 0.5f;

    private MonoBehaviour _deadEntity;
    private Character _deadCharacter;
    private IAManager _deadAlly;
    private float _spawnTime;
    private Vector3 _initialPosition;
    private bool _isPlayerNearby;
    private bool _isReviving;
    private float _reviveProgress;
    private Character _playerReviving;

    public bool IsReviving => _isReviving;
    public float ReviveProgress => _reviveProgress;

    private void Start()
    {
        _spawnTime = Time.time;
        _initialPosition = transform.position;

        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void Initialize(Character deadCharacter)
    {
        _deadCharacter = deadCharacter;
        _deadEntity = deadCharacter;
        Debug.Log($"[ReviveToken] Token criado para Character: {deadCharacter.Data.characterName}");
    }

    public void Initialize(IAManager deadAlly)
    {
        _deadAlly = deadAlly;
        _deadEntity = deadAlly;
        Debug.Log($"[ReviveToken] Token criado para IAManager: {deadAlly.Data.characterName}");
    }

    public void SetAllyToRevive(Character ally)
    {
        Initialize(ally);
    }

    private void Update()
    {
        if (_deadEntity == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Time.time >= _spawnTime + tokenLifetime)
        {
            string entityName = _deadCharacter != null ? _deadCharacter.Data.characterName : _deadAlly.Data.characterName;
            Debug.Log($"[ReviveToken] Token de {entityName} expirou após {tokenLifetime}s");
            Destroy(gameObject);
            return;
        }

        UpdateVisuals();
        DetectNearbyPlayer();

        if (_isPlayerNearby && !_isReviving)
        {
            if (Input.GetKeyDown(interactKey))
            {
                StartRevive();
            }
        }

        if (_isReviving)
        {
            UpdateRevive();
        }
    }

    private void UpdateVisuals()
    {
        if (visualEffect != null)
        {
            visualEffect.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        float newY = _initialPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.position = new Vector3(_initialPosition.x, newY, _initialPosition.z);
    }

    private void DetectNearbyPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, interactionRange);

        bool foundPlayer = false;

        foreach (Collider hit in hits)
        {
            Character character = hit.GetComponent<Character>();

            if (character != null && character.characterType == CharacterType.Player)
            {
                foundPlayer = true;
                _playerReviving = character;
                break;
            }
        }

        if (foundPlayer != _isPlayerNearby)
        {
            _isPlayerNearby = foundPlayer;

            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(_isPlayerNearby);
            }
        }
    }

    private void StartRevive()
    {
        _isReviving = true;
        _reviveProgress = 0f;

        string entityName = _deadCharacter != null ? _deadCharacter.Data.characterName : _deadAlly.Data.characterName;
        Debug.Log($"[ReviveToken] Iniciando revive de {entityName}");
    }

    private void UpdateRevive()
    {
        if (_playerReviving == null || !_isPlayerNearby)
        {
            CancelRevive();
            return;
        }

        if (Input.GetKey(interactKey))
        {
            _reviveProgress += Time.deltaTime;

            if (_reviveProgress >= reviveTime)
            {
                CompleteRevive();
            }
        }
        else
        {
            CancelRevive();
        }
    }

    private void CompleteRevive()
    {
        if (_deadCharacter != null)
        {
            _deadCharacter.transform.position = transform.position;
            _deadCharacter.Revive();
            Debug.Log($"[ReviveToken] Character {_deadCharacter.Data.characterName} foi revivido!");
        }
        else if (_deadAlly != null)
        {
            _deadAlly.Revive();
            Debug.Log($"[ReviveToken] IAManager {_deadAlly.Data.characterName} foi revivido!");
        }

        Destroy(gameObject);
    }

    private void CancelRevive()
    {
        _isReviving = false;
        _reviveProgress = 0f;

        string entityName = _deadCharacter != null ? _deadCharacter.Data.characterName : (_deadAlly != null ? _deadAlly.Data.characterName : "Unknown");
        Debug.Log($"[ReviveToken] Revive de {entityName} cancelado");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);

        if (_isReviving && _playerReviving != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, _playerReviving.transform.position);
        }

        #if UNITY_EDITOR
        if (_deadEntity != null)
        {
            Vector3 labelPos = transform.position + Vector3.up * 2f;
            string entityName = _deadCharacter != null ? _deadCharacter.Data.characterName : (_deadAlly != null ? _deadAlly.Data.characterName : "Unknown");
            string info = $"Revive Token\n{entityName}\n";
            
            if (_isReviving)
            {
                info += $"Progresso: {(_reviveProgress / reviveTime) * 100:F0}%";
            }
            else if (_isPlayerNearby)
            {
                info += $"Pressione {interactKey} para reviver";
            }
            else
            {
                float remainingTime = tokenLifetime - (Time.time - _spawnTime);
                info += $"Expira em: {remainingTime:F0}s";
            }

            UnityEditor.Handles.Label(labelPos, info);
        }
        #endif
    }
}
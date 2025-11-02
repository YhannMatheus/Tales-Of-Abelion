using UnityEngine;

public class CheckpointManager : MonoBehaviour
{
    public static CheckpointManager Instance { get; private set; }

    [Header("Checkpoint Settings")]
    public Transform defaultSpawnPoint;
    private Transform currentCheckpoint;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (defaultSpawnPoint == null)
        {
            defaultSpawnPoint = transform;
        }

        currentCheckpoint = defaultSpawnPoint;
    }

    public void SetCheckpoint(Transform checkpoint)
    {
        currentCheckpoint = checkpoint;
    }

    public void RespawnPlayer(Character player)
    {
        if (player == null) return;

        Vector3 spawnPosition = currentCheckpoint != null ? currentCheckpoint.position : defaultSpawnPoint.position;
        
        player.transform.position = spawnPosition;
        player.Revive();
    }
}
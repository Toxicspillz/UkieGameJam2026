using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject m_PlayerPrefab;
    [SerializeField] private GameObject m_playerRef;
    [SerializeField] private Vector3 m_PlayerCurrentSpawn;


    public static event Action<GameObject> OnPlayerSpawned;


 
    //void Start()
    //{
    //    SpawnPlayer(m_PlayerPrefab);
    //    //m_playerRef.GetComponent<CharacterMovement>().init();
    //}
    private void SetPlayerSpawnPosition(Transform checkpoint)
    {
        m_PlayerCurrentSpawn = checkpoint.position;
        Debug.Log($"[GameManager] Checkpoint updated: {m_PlayerCurrentSpawn}");

    }

 
    private void Start()
    {
        TimerManager.Instance?.StartTimer(true);
        
        SpawnPlayer(m_PlayerPrefab);
 
    }



    private void HandleCheckpointActivated(int id, Vector3 pos)
    {
        m_PlayerCurrentSpawn = pos;
        Debug.Log($"[GameManager] Updated spawn to checkpoint {id} at {pos}");
    }
    public void SpawnPlayer(GameObject playerPrefab)
    {
        m_playerRef = Instantiate (playerPrefab);
        SetupPlayer(m_playerRef);

    }

    public void RespawnPlayer()
    {
        Debug.Log($"[GameManager] Respawning player at {m_PlayerCurrentSpawn}");

        UnsubscribeFromGroundCheck();
        if (m_playerRef != null)
        {
            var inputHandler = m_playerRef.GetComponent<InputHandler>();
            if (inputHandler != null)
                PlayerInitializer.OnPlayerReady -= inputHandler.PlayerReady;

            Destroy(m_playerRef);
        }

        m_playerRef = Instantiate(m_PlayerPrefab,m_PlayerCurrentSpawn,Quaternion.identity);
        SetupPlayer(m_playerRef);
    }

    private void SetupPlayer(GameObject player)
    {
        OnPlayerSpawned?.Invoke(player);

        var groundCheck = player.GetComponentInChildren<GroundCheck>();
        if (groundCheck != null)
        {
            groundCheck.PlayerEnteredVoid += RespawnPlayer;
        }
        var inputHandler = player.GetComponent<InputHandler>();
        if (inputHandler != null)
        {
            PlayerInitializer.OnPlayerReady += inputHandler.PlayerReady;
        }

        PlayerInitializer.BroadcastPlayerReady(player);

    }

    private void UnsubscribeFromGroundCheck()
    {
        var groundCheck = m_playerRef?.GetComponentInChildren<GroundCheck>();
        if (groundCheck != null)
        {
            groundCheck.PlayerEnteredVoid -= RespawnPlayer;
        }
    }
    //private void OnDisable()
    //{
    //    Checkpoint.CheckpointPosition -= SetPlayerSpawnPosition;
    //}

}

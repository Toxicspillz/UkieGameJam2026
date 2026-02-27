using System;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    [SerializeField] private GameObject m_Player1Prefab;
    [SerializeField] private GameObject m_Player2Prefab;
    
    [SerializeField] private GameObject m_player1Ref;
    [SerializeField] private GameObject m_player2Ref;
    
    [SerializeField] private Transform m_Player1CurrentSpawnTransform;
    [SerializeField] private Transform m_Player2CurrentSpawnTransform;

    private Vector3 m_Player1CurrentSpawn;
    private Vector3 m_Player2CurrentSpawn;

    public static event Action<GameObject> OnPlayerSpawned;


 
    //void Start()
    //{
    //    SpawnPlayer(m_PlayerPrefab);
    //    //m_playerRef.GetComponent<CharacterMovement>().init();
    //}
    private void SetPlayerSpawnPosition(Transform checkpoint)
    {
        m_Player1CurrentSpawn = checkpoint.position;
        Debug.Log($"[GameManager] Checkpoint updated: {m_Player1CurrentSpawn}");

    }

 
    private void Start()
    {
        TimerManager.Instance?.StartTimer(true);

        m_Player1CurrentSpawn = m_Player1CurrentSpawnTransform.position;
        m_Player2CurrentSpawn = m_Player2CurrentSpawnTransform.position;

        SpawnPlayers(m_Player1Prefab, m_Player2Prefab);
 
    }



    private void HandleCheckpointActivated(int id, Vector3 pos)
    {
        m_Player1CurrentSpawn = pos;
        Debug.Log($"[GameManager] Updated spawn to checkpoint {id} at {pos}");
    }
    public void SpawnPlayers(GameObject player1Prefab, GameObject player2Prefab)
    {
        m_player1Ref = Instantiate(m_Player1Prefab,m_Player1CurrentSpawn,Quaternion.identity);
        SetupPlayer(m_player1Ref);

        m_player2Ref = Instantiate(m_Player2Prefab,m_Player2CurrentSpawn,Quaternion.identity);
        SetupPlayer(m_player2Ref);
    }

    public void RespawnPlayer()
    {
        Debug.Log($"[GameManager] Respawning player at {m_Player1CurrentSpawn}");

        UnsubscribeFromGroundCheck();
        
        // --player 1
        if (m_player1Ref != null)
        {
            var inputHandler = m_player1Ref.GetComponent<InputHandler>();
            if (inputHandler != null)
                PlayerInitializer.OnPlayerReady -= inputHandler.PlayerReady;

            Destroy(m_player1Ref);
        }

        m_player1Ref = Instantiate(m_Player1Prefab,m_Player1CurrentSpawn,Quaternion.identity);
        SetupPlayer(m_player1Ref);
        
        // --player 2
        if (m_player2Ref != null)
        {
            var inputHandler = m_player2Ref.GetComponent<InputHandler>();
            if (inputHandler != null)
                PlayerInitializer.OnPlayerReady -= inputHandler.PlayerReady;

            Destroy(m_player2Ref);
        }

        m_player2Ref = Instantiate(m_Player2Prefab,m_Player2CurrentSpawn,Quaternion.identity);
        SetupPlayer(m_player2Ref);
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
        var groundCheck = m_player1Ref?.GetComponentInChildren<GroundCheck>();
        if (groundCheck != null)
        {
            groundCheck.PlayerEnteredVoid -= RespawnPlayer;
        }
        
        var groundCheck2 = m_player2Ref?.GetComponentInChildren<GroundCheck>();
        if (groundCheck2 != null)
        {
            groundCheck2.PlayerEnteredVoid -= RespawnPlayer;
        }
    }
    //private void OnDisable()
    //{
    //    Checkpoint.CheckpointPosition -= SetPlayerSpawnPosition;
    //}

}

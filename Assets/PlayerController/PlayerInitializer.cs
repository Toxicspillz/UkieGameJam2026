using System;
using UnityEngine;

// Made so It sesne when GamManger does its thing , then this will tell the the scripts thta need to know , notifys anythign related to player  
public static class PlayerInitializer 
{


    // Called by PlayerManager after initialization
    public static event Action<GameObject> OnPlayerReady;

    /// <summary>
    /// Broadcasts to all listeners that the player is ready.
    /// Called by GameManager immediately after instantiation.
    /// </summary>
    /// <param name="player">The new player GameObject instance.</param>
    public static void BroadcastPlayerReady(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        Debug.Log($"[PlayerInitializer] Broadcasting OnPlayerReady for {player.name}");
        OnPlayerReady?.Invoke(player);
    }
}

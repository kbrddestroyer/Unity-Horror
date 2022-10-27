using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Mirror;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{
    /*
     *      CustomNetworkManager class <- NetworkManager
     *      CustomNetworkManager class <- PlayerObjectController
     *      Modification of Mirror's standart NetworkManager component
     *      
     *      CustomNetworkManager
     *      | - playerObjectController  [Prefab]
     *      | - players                 [List of active players]
     *      | - OnServerAddPlayer       [Automatic Event]
     *      | - StartGame               [Button Event]
     *      
     *      TODO: 
     *      Comment functions
    */

    [SerializeField] private PlayerObjectController playerObjectController;                     // Prefab for PlayerObjectController, used to create new player
    public List<PlayerObjectController> players { get; } = new List<PlayerObjectController>();  // List, containing all players in lobby/game scene

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        /*
         *  [TODO] Replace hardcoded IF statement with something more suitable
        */
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            // Creates new entity in list if player has connected to lobby
            PlayerObjectController current_player = Instantiate(playerObjectController);
            current_player.ConnectionID = conn.connectionId;
            current_player.PlayerIDNumber = players.Count + 1;
            current_player.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, players.Count);
            NetworkServer.AddPlayerForConnection(conn, current_player.gameObject);
        }
    }

    public void StartGame(string sceneName)
    {
        ServerChangeScene(sceneName);
    }
}

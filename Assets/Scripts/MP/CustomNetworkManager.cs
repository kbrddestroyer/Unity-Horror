using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;
using Steamworks;

public class CustomNetworkManager : NetworkManager
{
    [SerializeField] private PlayerObjectController playerObjectController;
    public List<PlayerObjectController> players { get; } = new List<PlayerObjectController>();

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            PlayerObjectController current_player = Instantiate(playerObjectController);
            playerObjectController.ConnectionID = conn.connectionId;
            playerObjectController.PlayerIDNumber = players.Count + 1;
            playerObjectController.PlayerSteamID = (ulong)SteamMatchmaking.GetLobbyMemberByIndex((CSteamID)SteamLobby.instance.CurrentLobbyID, players.Count);
            NetworkServer.AddPlayerForConnection(conn, current_player.gameObject);
        }
    }
}

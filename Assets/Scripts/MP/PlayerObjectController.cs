using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    [SyncVar] public int ConnectionID;
    [SyncVar] public int PlayerIDNumber;
    [SyncVar] public ulong PlayerSteamID;
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;

    private CustomNetworkManager manager;
    public CustomNetworkManager Manager {
        get {
            if (manager) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    public override void OnStartAuthority()
    {
        Debug.LogWarning("OnStartAuthority");
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Debug.LogWarning("OnStartClient");

        Manager.players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdateLobbyPlayers();
    }

    public override void OnStopClient()
    {
        Manager.players.Remove(this);
        LobbyController.instance.UpdateLobbyPlayers();
    }
    
    [Command] private void CmdSetPlayerName(string PlayerName)
    {
        this.PlayerNameUpdate(this.PlayerName, PlayerName);
    }

    public void PlayerNameUpdate(string old_, string new_)
    {
        if (isServer)
        {
            this.PlayerName = new_;
        }
        if (isClient)
        {
            LobbyController.instance.UpdateLobbyPlayers();
        }
    }
}

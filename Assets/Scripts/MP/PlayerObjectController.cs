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
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;

    private CustomNetworkManager manager;
    public CustomNetworkManager Manager {
        get {
            if (manager) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void CanStartGame(string sceneName)
    {
        if (hasAuthority)
            CmdCanStartGame(sceneName);
    }

    [Command] public void CmdCanStartGame(string sceneName)
    {
        manager.StartGame(sceneName);
    }

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(SteamFriends.GetPersonaName().ToString());
        gameObject.name = "LocalGamePlayer";
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        Manager.players.Add(this);
        LobbyController.instance.UpdateLobbyName();
        LobbyController.instance.UpdateLobbyPlayers();
    }

    public override void OnStopClient()
    {
        Manager.players.Remove(this);
        LobbyController.instance.UpdateLobbyPlayers();
    }
    
    [Command] private void CmdSetPlayerName(string Name)
    {
        this.PlayerNameUpdate(this.PlayerName, Name);
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

    private void PlayerReadyUpdate(bool old_, bool new_)
    {
        if (isServer)
        {
            this.Ready = new_;
        }
        if (isClient)
        {
            LobbyController.instance.UpdatePlayerItem();
        }
    }

    [Command] private void CmdSetPlayerReady()
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ToggleReady()
    {
        if (hasAuthority)
        {
            CmdSetPlayerReady();
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;
using Steamworks;

public class PlayerObjectController : NetworkBehaviour
{
    [SyncVar] public int ConnectionID;                                      // Identifier of current Mirror connection
    [SyncVar] public int PlayerIDNumber;                                    // Player number. Specifies unique ID
    [SyncVar] public ulong PlayerSteamID;                                   // Steam account ID. Uses in SteamAPI functions
    [SyncVar(hook = nameof(PlayerNameUpdate))] public string PlayerName;    // Nickname
    [SyncVar(hook = nameof(PlayerReadyUpdate))] public bool Ready;          // State

    private CustomNetworkManager manager;                                   // Connection, Status, Connection Data etc.                                  
    public CustomNetworkManager Manager {
        // public get modifier for manager
        get {
            if (manager) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;    // singleton - the only unique NetworkManager
        }
    }

    private void Start()
    {
        DontDestroyOnLoad(this.gameObject);                                   // PlayerObjectController must be saved to be used outside of the lobby 
    }

    private void OnDestroy()
    {
        Cursor.lockState = CursorLockMode.None;                               // Lock cursor
        Cursor.visible = true;                                                // Disable cursor
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
        gameObject.name = "LocalGamePlayer";                            // TODO: Replace Hardcode [!]
        LobbyController.instance.FindLocalPlayer();
        LobbyController.instance.UpdateLobbyName();
    }

    public override void OnStartClient()
    {
        /*
         *      If connected successfully:
         */
        Manager.players.Add(this);                      // Add current player to the manager list
        LobbyController.instance.UpdateLobbyName();     // Update current player nickname
        LobbyController.instance.UpdateLobbyPlayers();  // Fetch all players from server
    }

    public override void OnStopClient()
    {
        // DISCONNECT:
        Manager.players.Remove(this);                       // Remove current player from connection list
        LobbyController.instance.UpdateLobbyPlayers();      // Update all players on server
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

    private void PlayerReadyUpdate(bool old_, bool new_)    // Hook
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

    [Command] private void CmdSetPlayerReady()  // Exec. on server
    {
        this.PlayerReadyUpdate(this.Ready, !this.Ready);
    }

    public void ToggleReady()       // Used in UI calls
    {
        if (hasAuthority)
        {
            CmdSetPlayerReady();
        }
    }
}

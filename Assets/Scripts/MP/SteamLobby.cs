using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Steamworks;

public class SteamLobby : MonoBehaviour
{
    /*
     *      SteamLobby <- MonoBehaviour
     *      SteamLobby class is used for UI controlling, SteamworksAPI Callback controlling, etc.
    */

    //*******************************************************************************************//

    public static   SteamLobby  instance;
    public          ulong       CurrentLobbyID;

    protected Callback<LobbyCreated_t>            LobbyCreated;
    protected Callback<GameLobbyJoinRequested_t>  JoinRequest;
    protected Callback<LobbyEnter_t>              LobbyEnter;
    protected Callback<LobbyMatchList_t>          LobbyMatchList;
    protected Callback<LobbyDataUpdate_t>         LobbyDataUpdate;

    public List<CSteamID> lobbyIDs = new List<CSteamID>();                          // [!] Replace PUBLIC with PRIVATE/PROTECTED and use public GET method instead: values can be changed externally

    private const string                          HostAddressKey = "HostAddress";
    private CustomNetworkManager                  manager;

    //*******************************************************************************************//

    private void Start()
    {
        if (!instance)
            instance = this; // Pointer to current class

        manager = GetComponent<CustomNetworkManager>();                 

        try
        {
            InteropHelp.TestIfAvailableClient();
        }
        catch (Exception)
        {
            if (!SteamAPI.Init())
            {
                /*
                 *          If Steamworks failed to fetch API handler and SteamAPI library failed either
                 *          exit init process.
                */
                return;
            }
        }

        LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);             // OnLobbyCreated Event Callback
        JoinRequest = Callback<GameLobbyJoinRequested_t>.Create(OnJoinRequest);     // OnJoinRequest Event Callback
        LobbyEnter = Callback<LobbyEnter_t>.Create(OnLobbyEntered);                 // OnLobbyEntered Event Callback
        LobbyMatchList = Callback<LobbyMatchList_t>.Create(OnGetLobbyList);
        LobbyDataUpdate = Callback<LobbyDataUpdate_t>.Create(OnLobbyDataUpdate);
    }

    public void HostLobby()
    {
        try
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, manager.maxConnections);
            //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, manager.maxConnections);
        }
        catch (Exception)
        {
            // If failed to fetch new lobby -> return
            return;
        }
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey, SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), "name", SteamFriends.GetPersonaName().ToString() + "'s Lobby");
    }

    private void OnJoinRequest(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        if (NetworkServer.active) { return; }
        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        manager.StartClient();
    }

    public void GetLobbiesList()
    {
        if (lobbyIDs.Count > 0) lobbyIDs.Clear();

        SteamMatchmaking.AddRequestLobbyListResultCountFilter(60);
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnGetLobbyList(LobbyMatchList_t callback)
    {
        if (LobbyBrowserController.instance.lobbies.Count > 0) LobbyBrowserController.instance.DestroyLobbies();

        for (int i = 0; i < callback.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyID = SteamMatchmaking.GetLobbyByIndex(i);
            lobbyIDs.Add(lobbyID);
            SteamMatchmaking.RequestLobbyData(lobbyID);
        }
    }

    private void OnLobbyDataUpdate(LobbyDataUpdate_t callback)
    {
        LobbyBrowserController.instance.DisplayLobbies(lobbyIDs, callback);
    }


    public void JoinLobby(CSteamID lobbyID)
    {
        SteamMatchmaking.JoinLobby(lobbyID);
    }
}

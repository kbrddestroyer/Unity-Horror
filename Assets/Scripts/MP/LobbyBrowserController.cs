using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Steamworks;

public class LobbyBrowserController : MonoBehaviour
{
    public static LobbyBrowserController instance;              // [!] Replase PUBLIC with PRIVATE and public GET method. Value can be changed externally

    [SerializeField] private GameObject lobbiesMenu;
    [SerializeField] private GameObject lobbyDataPrefab;
    [SerializeField] private GameObject lobbyListContent;

    [SerializeField] private GameObject mainMenu;

    public List<GameObject> lobbies = new List<GameObject>();   // [!] Replase PUBLIC with PRIVATE and public GET method. Value can be changed externally

    private void Awake()
    {
        if (instance == null) instance = this;
    }

    public void DestroyLobbies()
    {
        foreach (GameObject lobbyItem in lobbies) Destroy(lobbyItem);
    }

    public void DisplayLobbies(List<CSteamID> lobbyIDs, LobbyDataUpdate_t result)
    {
        foreach (CSteamID lobbyID in lobbyIDs) 
        {
            if (lobbyID.m_SteamID == result.m_ulSteamIDLobby)
            {
                GameObject createdItem = Instantiate(lobbyDataPrefab);

                createdItem.GetComponent<LobbyDataEntryController>().lobbyID = lobbyID;
                createdItem.GetComponent<LobbyDataEntryController>().lobbyName = SteamMatchmaking.GetLobbyData((CSteamID) lobbyID.m_SteamID, "name");
                createdItem.GetComponent<LobbyDataEntryController>().SetLobbyData();

                createdItem.transform.SetParent(lobbyListContent.transform);
                createdItem.transform.localScale = Vector3.one;

                lobbies.Add(createdItem);
            }
        }
    }

    public void GetLobbies()        // Use as GET; list of lobbies?
    {
        mainMenu.SetActive(false);
        lobbiesMenu.SetActive(true);

        SteamLobby.instance.GetLobbiesList();
    }

    public void Back()
    {
        this.DestroyLobbies();
        mainMenu.SetActive(true);
        lobbiesMenu.SetActive(false);
    }
}

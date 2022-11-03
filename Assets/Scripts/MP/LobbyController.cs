using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Mirror;
using Steamworks;
using System.Linq;

public class LobbyController : MonoBehaviour
{
    /*
     *  LobbyController <- MonoBehaviout
     *  
     *  Just sort of UI manager. Puts/Removes prefabs on/from list on Canvas when player connects/disconnects
    */

    public static LobbyController instance;                                     // This object
    public Text lobbyNameText;                                                  // LobbyName is fetched from SteamAPI (Setup in SteamLobby.cs)

    public GameObject PlayerListViewContent;                                    // Content of ScrollView
    public GameObject PlayerListItemPrefab;                                     // Element of UI
    public GameObject LocalPlayerObject;                                        // Current player

    public ulong CurrentLobbyID;                                                // Unique identifier
    public bool PlayerItemCreated = false;                                      // ????

    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();  // List of all connected players

    public PlayerObjectController localPlayerController;                        // Current player <- PlayerObjectController

    public Button StartGameBtn;                                                 // Button will be toggled on when everyone is ready
    public Text ReadyButtonText;                                                // Ready/Not ready (will be toggled as well depending on player status)

    private CustomNetworkManager manager;                                       // NetworkManager
    public CustomNetworkManager Manager
    {
        get
        {
            // public GET modifier
            if (manager) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        if (!instance) instance = this;
    }

    public void ReadyPlayer()   // used in UI
    {
        localPlayerController.ToggleReady();
    }

    public void UpdateBtn()     // used in UI
    {
        if (localPlayerController.Ready)
        {
            ReadyButtonText.text = "Not Ready";
        }
        else
        {
            ReadyButtonText.text = "Ready";
        }
    }

    public void CheckIfAllReady()
    {
        bool everyone = true;
        foreach (PlayerObjectController player in Manager.players)
        {
            if (!player.Ready)
            {
                everyone = false;
                break;
            }
        }

        if (everyone)
        {
            if (localPlayerController)
                if (localPlayerController.PlayerIDNumber == 1)
                    StartGameBtn.interactable = true;
        }
        else StartGameBtn.interactable = false;
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
    }

    public void UpdateLobbyPlayers()
    {
        if (!PlayerItemCreated) { CreateHostPlayerItem(); }

        if (playerListItems.Count < Manager.players.Count) { CreateClientPlayerItem(); }

        if (playerListItems.Count > Manager.players.Count) { RemovePlayerItem(); }

        if (playerListItems.Count == Manager.players.Count) { UpdatePlayerItem(); }
    }

    public void FindLocalPlayer()
    {
        LocalPlayerObject = GameObject.Find("LocalGamePlayer");
        localPlayerController = LocalPlayerObject.GetComponent<PlayerObjectController>();
    }

    public void CreateHostPlayerItem()
    {
        foreach(PlayerObjectController player in Manager.players)
        {
            GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
            PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

            newPlayerItemScript.playerName = player.PlayerName;
            newPlayerItemScript.connectionID = player.ConnectionID;
            newPlayerItemScript.steamID = player.PlayerSteamID;
            newPlayerItemScript.ready = player.Ready;
            newPlayerItemScript.SetValues();

            newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
            newPlayerItem.transform.localScale = Vector3.one;

            playerListItems.Add(newPlayerItemScript);
        }

        PlayerItemCreated = true;
    }

    public void CreateClientPlayerItem()
    {
        foreach (PlayerObjectController player in Manager.players)
        {
            if (!playerListItems.Any(b => b.connectionID == player.ConnectionID))
            {
                GameObject newPlayerItem = Instantiate(PlayerListItemPrefab) as GameObject;
                PlayerListItem newPlayerItemScript = newPlayerItem.GetComponent<PlayerListItem>();

                newPlayerItemScript.playerName = player.PlayerName;
                newPlayerItemScript.connectionID = player.ConnectionID;
                newPlayerItemScript.steamID = player.PlayerSteamID;
                newPlayerItemScript.ready = player.Ready;
                newPlayerItemScript.SetValues();

                newPlayerItem.transform.SetParent(PlayerListViewContent.transform);
                newPlayerItem.transform.localScale = Vector3.one;

                playerListItems.Add(newPlayerItemScript);
            }
        }
    }

    public void UpdatePlayerItem()
    {
        foreach (PlayerObjectController player in Manager.players)
        {
            foreach (PlayerListItem player_script in playerListItems)
            {
                if (player_script.connectionID == player.ConnectionID)
                {
                    player_script.playerName = player.PlayerName;
                    player_script.ready = player.Ready;
                    player_script.SetValues();
                    if (player == localPlayerController)
                    {
                        UpdateBtn();
                    }
                }
            }
        }
        CheckIfAllReady();
    }

    public void RemovePlayerItem()
    {
        List<PlayerListItem> playerListItemToRemove = new List<PlayerListItem>();

        foreach (PlayerListItem  playerListItem in playerListItems)
        {
            if (!Manager.players.Any(b => b.ConnectionID == playerListItem.connectionID))
            {
                playerListItemToRemove.Add(playerListItem);
            }
        }
        if (playerListItemToRemove.Count > 0)
        {
            foreach (PlayerListItem playerListItemToRemove_ in playerListItemToRemove)
            {
                playerListItems.Remove(playerListItemToRemove_);
                Destroy(playerListItemToRemove_.gameObject);
            }
        }
    }

    public void StartGame(string sceneName)
    {
        localPlayerController.CanStartGame(sceneName);
    }
}

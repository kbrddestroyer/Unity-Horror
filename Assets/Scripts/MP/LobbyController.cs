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
    public static LobbyController instance;
    public Text lobbyNameText;

    public GameObject PlayerListViewContent;
    public GameObject PlayerListItemPrefab;
    public GameObject LocalPlayerObject;

    public ulong CurrentLobbyID;
    public bool PlayerItemCreated = false;

    private List<PlayerListItem> playerListItems = new List<PlayerListItem>();

    public PlayerObjectController localPlayerController;

    private CustomNetworkManager manager;
    public CustomNetworkManager Manager
    {
        get
        {
            if (manager) return manager;
            return manager = CustomNetworkManager.singleton as CustomNetworkManager;
        }
    }

    private void Awake()
    {
        if (!instance) instance = this;
    }

    public void UpdateLobbyName()
    {
        CurrentLobbyID = Manager.GetComponent<SteamLobby>().CurrentLobbyID;
        lobbyNameText.text = SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name");
        Debug.LogWarning(SteamMatchmaking.GetLobbyData(new CSteamID(CurrentLobbyID), "name"));
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
                    player_script.SetValues();
                }
            }
        }
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
}

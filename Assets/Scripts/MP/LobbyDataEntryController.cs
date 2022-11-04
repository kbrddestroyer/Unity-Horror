using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Steamworks;

public class LobbyDataEntryController : MonoBehaviour
{
    public CSteamID lobbyID;
    public string lobbyName;
    
    [SerializeField] private Text lobbyNameText;
    
    public void SetLobbyData()
    {
        lobbyNameText.text = (lobbyName == "") ? "Unnamed lobby" : lobbyName;
    }

    public void JoinLobby()
    {
        SteamLobby.instance.JoinLobby(lobbyID);
    }
}

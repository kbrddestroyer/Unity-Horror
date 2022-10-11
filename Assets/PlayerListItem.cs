using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using Steamworks;

public class PlayerListItem : MonoBehaviour
{
    public string playerName;
    public int connectionID;
    public ulong steamID;

    public Text PlayerNameText;
    public Text StatusText;
    public RawImage PlayerIcon;

    private bool hasAvatar = false;
    public bool ready = false;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    public void ChangeReadyStatus()
    {
        if (ready)
        {
            StatusText.text = "Ready";
            StatusText.color = Color.green;
        }
        else
        {
            StatusText.text = "Not Ready";
            StatusText.color = Color.red;
        }
    }

    private void Start()
    {
        try
        {
            InteropHelp.TestIfAvailableClient();
        }
        catch (Exception e)
        {
            if (!SteamAPI.Init())
            {
                return;
            }
        }
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == steamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
    }
    void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)steamID);
        if (ImageID == -1) return;
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }

    public void SetValues()
    {
        PlayerNameText.text = playerName;
        ChangeReadyStatus();
        if (!hasAvatar)
        {
            GetPlayerIcon();
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;
        bool valid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (valid)
        {
            byte[] image = new byte[width * height * 4];
            valid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));
            if (valid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        hasAvatar = true;
        return texture;
    }
}

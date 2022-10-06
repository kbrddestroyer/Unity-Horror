using System;
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
    public RawImage PlayerIcon;

    private bool hasAvatar = false;

    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    private void Awake()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
        Debug.LogWarning(ImageLoaded);
    }

    private void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        Debug.Log("Callback image loaded!");
        if (callback.m_steamID.m_SteamID == steamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
    }
    void GetPlayerIcon()
    {
        Debug.Log("Getting icon...");
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)steamID);
        if (ImageID == -1) return;
        Debug.Log("Still getting icon...");
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }

    public void SetValues()
    {
        Debug.Log("Setting values...");
        PlayerNameText.text = playerName;
        if (!hasAvatar)
        {
            Debug.Log("Getting image...");
            GetPlayerIcon();
        }
    }

    private Texture2D GetSteamImageAsTexture(int iImage)
    {
        Debug.LogWarning("Callback...");
        Texture2D texture = null;
        bool valid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (valid)
        {
            Debug.LogWarning("Valid1");

            byte[] image = new byte[width * height * 4];
            valid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));
            if (valid)
            {
                Debug.LogWarning("Valid2");

                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        hasAvatar = true;
        return texture;
    }
}

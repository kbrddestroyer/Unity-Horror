using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Door : NetworkBehaviour
{
    /*
     *      Door logic (I'll comment it later)
     */

    [SerializeField, Range(0f, 15f)] private float smooth;
    [SerializeField, Range(0f, 15f)] private float distance;
    [SerializeField] private bool autoClose;

    private Transform player;
    private float targetYRotation;
    private Vector3 defaultRotation;
    private float timer = 0f;

    [SyncVar] private bool isOpen;

    public bool open { get { return isOpen; } }

    void Awake()
    {
        player = GameObject.Find("LocalGamePlayer").transform;
        defaultRotation = transform.eulerAngles;
        targetYRotation = 0.0f;
    }

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.E) && Vector3.Distance(player.position, transform.position) < distance)
        {
            ToggleDoor(player.position);
        }
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(defaultRotation.x, defaultRotation.y + targetYRotation, defaultRotation.z), smooth * Time.deltaTime);

        timer -= Time.deltaTime;

        if (timer <= 0f && isOpen && autoClose)
        {
            ToggleDoor(player.position);
        }
    }

    [Command(requiresAuthority = false)]
    public void ToggleDoor(Vector3 pos)
    {
        isOpen = !isOpen;

        if (isOpen)
        {
            Vector3 dir = (pos - transform.position);
            targetYRotation = -Mathf.Sign(Vector3.Dot(transform.right, dir)) * 90f;
            timer = 5f;
        }
        else
        {
            targetYRotation = 0f;
        }
    }

    [ServerCallback]
    public void Open(Vector3 pos)
    {
        if (!isOpen)
        {
            ToggleDoor(pos);
        }
    }

    [ServerCallback]
    public void Close(Vector3 pos)
    {
        if (isOpen)
        {
            ToggleDoor(pos);
        }
    }

    [Command]
    public void Interact()
    {
        ToggleDoor(player.position);
    }

    public string GetDescription()
    {
        if (isOpen) return "Close the door";
        return "Open the door";
    }
}

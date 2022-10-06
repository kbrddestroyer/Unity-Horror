using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    [SerializeField, Range(0f, 15f)] private float smooth;
    [SerializeField] private bool autoClose;

    private Transform player;
    private Transform ghost;
    private float targetYRotation;
    private Vector3 defaultRotation;
    private float timer = 0f;
    private bool isOpen;

    public bool open { get { return isOpen; } }
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        ghost = GameObject.FindGameObjectWithTag("Anomaly").transform;
        defaultRotation = transform.eulerAngles;
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(defaultRotation.x, defaultRotation.y + targetYRotation, defaultRotation.z), smooth * Time.deltaTime);

        timer -= Time.deltaTime;

        if (Vector3.Distance(transform.position, ghost.transform.position) <= 1.5f)
        {
            Debug.Log("");
            Open(ghost.position);
        }
        if (timer <= 0f && isOpen && autoClose)
        {
            ToggleDoor(player.position);
        }
    }

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

    public void Open(Vector3 pos)
    {
        if (!isOpen)
        {
            ToggleDoor(pos);
        }
    }
    public void Close(Vector3 pos)
    {
        if (isOpen)
        {
            ToggleDoor(pos);
        }
    }

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

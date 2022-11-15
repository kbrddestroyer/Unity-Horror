using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class InventorySystem : NetworkBehaviour
{
    //---------------------------------------------------------------------------------//
    //                                  Editor Settings                                //
    //---------------------------------------------------------------------------------//

    [Header("Pickup Settings")]
    [SerializeField]                    private Transform   rootpoint;      // Root point for InteractableIten (Holding spot)
    [SerializeField]                    private Transform   rootpoint_global;      // Root point for InteractableIten (Holding spot)
    [Header("Physics Setings")]
    [SerializeField, Range(0f, 200f)]   private float       throwForce;     // Forse, aplied to Rigidbody when throwing

    [SyncVar] private GameObject holding = null;

    private void Awake()
    {
        if (this.gameObject.name != "LocalGamePlayer") rootpoint = rootpoint_global;
    }

    [Command]
    private void MoveItem()
    {
        holding.GetComponent<InteractableItem>().MoveTo(rootpoint);
    }
    
    private void Update()
    {
        if (holding)
        {
            MoveItem();
            if (hasAuthority)
            {
                if (Input.GetKeyDown(KeyCode.G))
                    ThrowItem();
                if (Input.GetKeyDown(KeyCode.Mouse0))
                    holding.GetComponent<InteractableItem>().Interact();
            }
        }
    }

    [Command]
    public void PickupItem(GameObject item)
    {
        holding = item;
        holding.GetComponent<NetworkIdentity>().AssignClientAuthority(connectionToClient);
        holding.transform.parent = rootpoint;
        holding.GetComponent<Rigidbody>().isKinematic = true;
    }
    
    [Command]
    public void ThrowItem()
    {
        holding.transform.parent = null;
        holding.GetComponent<Rigidbody>().isKinematic = false;
        holding.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwForce);

        holding = null;
    }
}

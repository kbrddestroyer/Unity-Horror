using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public abstract class InteractableItem : NetworkBehaviour
{
    protected InventorySystem activePlayer = null;
    [SerializeField] protected Vector3 baseRotation;

    private GameObject player;
    public Vector2 base_rotation { get { return baseRotation; } }

    public abstract void Interact();

    private void Awake()
    {
        player = GameObject.Find("LocalGamePlayer");
    }

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
            Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
    }

    [Obsolete("Method id deprecated. Use base_rotation get method instead")]
    public Vector3 getBaseRotation()
    {
        return baseRotation;
    }

    private void OnMouseOver()
    {
        if (Input.GetKeyDown(KeyCode.E) && player.GetComponent<NetworkIdentity>().hasAuthority && player.gameObject.GetComponent<Player>().alive)
        {
            player.GetComponent<InventorySystem>().PickupItem(this.gameObject);
        }
    }
}
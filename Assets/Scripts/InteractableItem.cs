using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableItem : MonoBehaviour
{
    protected InventorySystem activePlayer = null;
    [SerializeField] protected Vector3 baseRotation;
    public abstract void Interact();

    public void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
            Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
    }

    public Vector3 getBaseRotation()
    {
        return baseRotation;
    }

    public void Pickup(Transform linkpoint, InventorySystem player)
    {
        transform.SetParent(linkpoint);                    // Linking item to rootpoint 
        GetComponent<Rigidbody>().isKinematic = true;
        transform.position = linkpoint.transform.position;
        transform.rotation = linkpoint.transform.rotation; // Copying transform params
        transform.localRotation = Quaternion.Euler(baseRotation);
    }

    public void Throw(float throwForce)
    {
        gameObject.transform.parent = null;
        this.GetComponent<Rigidbody>().isKinematic = false;
        this.GetComponent<Rigidbody>().AddForce(Camera.main.transform.forward * throwForce);
    }
}
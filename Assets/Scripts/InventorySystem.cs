using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventorySystem : MonoBehaviour
{
    //---------------------------------------------------------------------------------//
    //                                  Editor Settings                                //
    //---------------------------------------------------------------------------------//
    [Header("Pickup Settings")]
    [SerializeField]                    private Transform   rootpoint;      // Root point for InteractableIten (Holding spot)
    [Header("Physics Setings")]
    [SerializeField, Range(0f, 10f)]    private float       pickupRange;    // Radius, where items are pickable
    [SerializeField, Range(0f, 200f)]   private float       throwForce;     // Forse, aplied to Rigidbody when throwing
    //---------------------------------------------------------------------------------//

    private bool                isPickingUp = false;                        // Is picking process in progress (Whatever)
    private InteractableItem    holdingItem = null;                         // Stores active InteractableItem
    private Camera              mainCamera;                                 // Camera.main basically. Do not refer to Camera.main method - ise mainCamera instead (links to Camera.main in Start() method)

    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && holdingItem)
            holdingItem.Interact();
        if (Input.GetKeyUp(KeyCode.E))
            isPickingUp = false;
        if (Input.GetKeyDown(KeyCode.G) && holdingItem)
        {
            holdingItem.Throw(throwForce);
            holdingItem = null;
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (!holdingItem)
            {
                isPickingUp = true;
                if (Physics.Raycast(ray, out hit))
                {
                    InteractableItem item = hit.transform.gameObject.GetComponent<InteractableItem>();
                    Door door = hit.transform.gameObject.GetComponent<Door>();
                    Switch sw = hit.transform.gameObject.GetComponent<Switch>();
                    if (item != null)   // If we found item
                    {
                        float dist = Vector3.Distance(item.transform.position, transform.position);
                        if (dist <= pickupRange)    // In range to pick up
                        {
                            //
                            // Pick up InteractableItem
                            //
                            holdingItem = item;
                            item.Pickup(rootpoint, this);
                        }
                    }
                    else if (door != null)
                    {
                        door.Interact();
                    }
                    else if (sw != null && Vector3.Distance(hit.transform.position, transform.position) < pickupRange)
                        sw.switch_light();
                }
            }
            else if (!isPickingUp)  // If holding item
            {
                //
                // Throw
                //
                holdingItem.Throw(throwForce);
                holdingItem = null;
            }
        }
    }
}

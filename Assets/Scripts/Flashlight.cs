using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashlight : InteractableItem
{
    [SerializeField] private new Light light;
    
    public override void Interact()
    {
        light.enabled = !light.enabled;
    }
}

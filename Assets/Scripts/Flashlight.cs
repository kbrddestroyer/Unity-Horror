using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Flashlight : InteractableItem
{
    [SerializeField] private new Light light;
    [SyncVar] private bool light_state;

    private void Update()
    {
        if (light.enabled != light_state) light.enabled = light_state;
    }

    [Command(requiresAuthority = false)]
    public override void Interact()
    {
        light_state = !light_state;
    }
}

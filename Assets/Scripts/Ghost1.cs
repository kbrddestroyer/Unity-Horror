using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Ghost1 : Anomaly
{
    [Command]
    public override void ghostevent()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light light in lights) light.enabled = false;
        StartCoroutine(EventCooldown());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F2))
        {
            can_hunt = true;
            CmdStartHunt();
        }
    }
}

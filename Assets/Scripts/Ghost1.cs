using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ghost1 : Anomaly
{
    public override void ghostevent()
    {
        Light[] lights = GameObject.FindObjectsOfType<Light>();
        foreach (Light light in lights) light.enabled = false;
        StartCoroutine(EventCooldown());
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            can_hunt = true;
            StartCoroutine(hunt());
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class Switch : NetworkBehaviour
{
    [SerializeField] private Light controllerLight;
    [SyncVar(hook = nameof(ChangeLightState))] private bool lightState;

    public void ChangeLightState(bool old_, bool new_)
    {
        lightState = new_;
    }

    [Command]
    public void CmdChangeLightState()
    {
        ChangeLightState(lightState, !lightState);
    }

    public void switch_light()
    {
        CmdChangeLightState();
    }

    private void Update()
    {
        if (controllerLight.enabled != lightState) controllerLight.enabled = lightState;
    }
}

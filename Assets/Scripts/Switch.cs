using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Switch : MonoBehaviour
{
    [SerializeField] private Light controllerLight;
    
    public void switch_light()
    {
        controllerLight.enabled = !controllerLight.enabled;
    }
}

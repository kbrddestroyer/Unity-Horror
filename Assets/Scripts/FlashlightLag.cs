using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlashlightLag : MonoBehaviour
{
    /*
     *          WARNING: NEEDS TESTING!!!
     */
    [SerializeField] private new Light light;

    private int intensity;
    private bool isInAnomaly = false;

    private void Start()
    {
        intensity = ((int)light.intensity);
    }

    IEnumerator Lag()
    {
        while (isInAnomaly)
        {
            yield return new WaitForSeconds(0.05f);
            light.intensity = Random.Range(0f, intensity);
        }
        light.intensity = intensity;
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Anomaly")
        {
            if (other.GetComponent<Anomaly>().isHunting)
            {
                isInAnomaly = true;
                StartCoroutine(Lag());
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Anomaly")
        {
            isInAnomaly = false;
            StopCoroutine(Lag());
        }
    }

}

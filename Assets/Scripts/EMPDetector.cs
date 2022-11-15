using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Mirror;

public class EMPDetector : InteractableItem
{
    [SerializeField, Range(0f, 5f)] private float maxSoundDelay;
    [SerializeField, Range(0f, 25f)] private float maxDistance;
    private AudioSource audioSource;
    private Transform ghost;
    private bool working = false;
    [SyncVar] private bool EMP_Enabled = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        ghost = GameObject.FindGameObjectWithTag("Anomaly").transform;
    }

    [ClientRpc]
    private void StartBeeping()
    {
        StartCoroutine(Beep());
    }

    private IEnumerator Beep()
    {
        float distance;
        working = true;
        do
        {
            distance = Vector3.Distance(transform.position, ghost.position);
            yield return new WaitForSeconds(maxSoundDelay / maxDistance * distance + 0.1f);
            audioSource.Play();
        } while (distance < maxDistance);
        working = false;
    }

    void Update()
    {
        if (Vector3.Distance(transform.position, ghost.position) < maxDistance)
        {
            if (EMP_Enabled && !working && ghost.gameObject.GetComponent<Anomaly>().isPerformingEMP) StartBeeping();
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdToggleEnabled()
    {
        this.EMP_Enabled = !this.EMP_Enabled;
    }

    public override void Interact()
    {
        CmdToggleEnabled();
    }
}

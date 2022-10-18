using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Mirror;

public class PlayerMovementController : NetworkBehaviour
{
    [SerializeField, Range(0f, 10f)] private float msens;              // Mouse sens
    [SerializeField, Range(0f, 10f)] private float speed;
    [SerializeField] private Behaviour[] enableOnSceneStart;
    public GameObject playerObject;
    public Camera camera;
    
    private Vector3 rotation;

    // Start is called before the first frame update
    void Start()
    {
        playerObject.SetActive(false);
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    private void Update()
    {
        if (hasAuthority)
        {
            if (Input.GetKeyDown(KeyCode.F1))
            {
                transform.position = new Vector3(0, 2, 0);
            }
        }
    }

    private IEnumerator updateSpawn()
    {
        yield return new WaitForSeconds(0.1f);

        GameObject[] SpawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

        this.transform.position = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.position;
        this.transform.rotation = SpawnPoints[Random.Range(0, SpawnPoints.Length)].transform.rotation;

        this.playerObject.SetActive(true);
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (next.name == "Lobby") return;
        if (playerObject.activeSelf == false)
        {
            this.playerObject.SetActive(true);
            StartCoroutine(updateSpawn());
            foreach (Behaviour b in enableOnSceneStart)
                b.enabled = true;

            if (hasAuthority)
            {
                camera.gameObject.SetActive(true);
            }
            else
            {
                playerObject.transform.Find("Ch32").gameObject.layer = 0;
            }
        }
    }
}

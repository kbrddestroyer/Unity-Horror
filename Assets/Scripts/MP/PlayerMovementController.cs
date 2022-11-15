using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using Mirror;

public class PlayerMovementController : NetworkBehaviour
{
    /*
     *          --- WARNING: SCRIPT OUT OF DATE ---
     *          USING PLAYER.CS INSTEAD OF THIS ONE
     * 
     * TODO: MOVE EVERYTHING TO PLAYER.CS [!]
    */

    [SerializeField] private Behaviour[] enableOnSceneStart;
    public GameObject playerObject;
    public new Camera camera;
    
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
            if (Input.GetKeyDown(KeyCode.F1))               // WARNING! DELETE THIS ON RELEASE!!! THIS IS DEBUG TOOL
            {
                transform.position = new Vector3(0, 2, 0);
            }
        }
    }

    private IEnumerator updateSpawn()
    {
        while (hasAuthority && NetworkClient.isLoadingScene) yield return new WaitForEndOfFrame();
        if (hasAuthority)
        {
            GameObject[] SpawnPoints = GameObject.FindGameObjectsWithTag("Respawn");

            int id = Random.Range(0, SpawnPoints.Length);

            this.transform.position = SpawnPoints[id].transform.position;
            this.transform.rotation = SpawnPoints[id].transform.rotation;

            this.playerObject.SetActive(true);
        }
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (next.name == "Lobby")                                      // [!] HARDCODE
        {

            return;
        }
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
                playerObject.transform.Find("Ch32").gameObject.layer = 0;        // [!] HARDCODE
            }
        }
    }
}

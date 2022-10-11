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

    // Update is called once per frame
    void Update()
    {
        if (SceneManager.GetActiveScene().name == "Level1")
        {
            if (hasAuthority)
            {
                float new_x = Input.GetAxis("Mouse X") * msens;
                float new_y = Input.GetAxis("Mouse Y") * msens;

                rotation.x -= new_y;
                rotation.y += new_x;
                rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);

                transform.rotation = Quaternion.Euler(0, rotation.y, 0);
                camera.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            }
        }
    }

    private IEnumerator updateSpawn()
    {
        yield return new WaitForSeconds(0.1f);
        this.transform.position = new Vector3(Random.Range(-9f, 9f), 1, Random.Range(-9f, 9f));
        this.playerObject.SetActive(true);       
    }

    private void OnSceneChanged(Scene current, Scene next)
    {
        if (next.name == "Lobby") return;
        if (playerObject.activeSelf == false)
        {
            Debug.LogWarning(this);
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

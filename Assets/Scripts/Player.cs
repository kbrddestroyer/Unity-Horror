using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mirror;

public class Player : NetworkBehaviour
{
    [Header("Base Settings")]
    [SerializeField, Range(0f, 10f)]    private float       speed;              // Standart oving speed
    [SerializeField, Range(0f, 10f)]    private float       run_speed;          // Running speed 
    [SerializeField, Range(0f, 10f)]    private float       msens;              // Mouse sens
    [SerializeField, Range(0f, 10f)]    private float       targetDistance;     // Distance, where player can interact with InteractableItem class
    [SerializeField, Range(0f, 2f)]     private float       crouchHeight;       // Peak point height in crouch
    [SerializeField, Range(0f, 10f)]    private float       crouchSpeed;        // Speed of translating between crouching and normal state
    [SerializeField]                    private new Light   light;              // Attached flashlight
    [SerializeField]                    private AudioClip   footsteps;          // Step sound
    [SerializeField]                    private GameObject  ragdoll;
    [SerializeField]                    private Camera      mainCamera;         // Camera.main link
    //---   PUBLIC VARIABLES    ---//
    public Transform    targetPoint;                                            // Root point for Rig Animation 
    public float        mind = 1.0f;                                            // Parameter for anomalies (heal point)
    public bool alive = true;
    //---   PRIVATE VARIABLES   ---//
    private const float seconds = 0.5f;                                         // Footsteps sound delay (normal)
    private const float run_seconds = 0.4f;                                     // Footsteps sound delay (run)
    private bool        running = false;                                        // Running?
    private bool        crouching = false;                                      // Crouching?
    private float       normalHeight;                                           // Peak point in normal state
    private AudioSource audioSource;                                            // AudioSource component
    private Animator    animator;                                               // Animator component
    private Vector3     rotation;                                               // Base rotation

    public void Die()
    {
        Instantiate(ragdoll, transform).transform.parent = null;
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        mainCamera.GetComponent<Effects>().enabled = true;
        alive = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 8)
        {
            Physics.IgnoreCollision(this.GetComponent<Collider>(), collision.collider);
        }
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.Locked;                               // Lock cursor
        Cursor.visible = false;                                                 // Hide cursor

        rotation = Vector3.zero;                                                // Set base rotation to zero

        animator = GetComponent<Animator>();                                    // Get animator
        audioSource = GetComponent<AudioSource>();                              // Get audioSource
        normalHeight = mainCamera.transform.localPosition.y;                    // Get normalHeight
    }

    private IEnumerator Footsteps() //< Play footstep sound >
    {
        audioSource.clip = footsteps;
        audioSource.Play();
        yield return new WaitForSeconds((running) ? run_seconds : seconds);
        audioSource.Stop();
    }

    private void FixedUpdate()  // Translating camera w/ crouching
    {
        if (hasAuthority)
        {
            if (crouching)
            {
                mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, new Vector3(mainCamera.transform.localPosition.x, crouchHeight, mainCamera.transform.localPosition.z), crouchSpeed * Time.deltaTime);
            }
            else mainCamera.transform.localPosition = Vector3.Lerp(mainCamera.transform.localPosition, new Vector3(mainCamera.transform.localPosition.x, normalHeight, mainCamera.transform.localPosition.z), crouchSpeed * Time.deltaTime);
        }
    }

    [Command] 
    void CmdUpdateRotation(Quaternion rotation)
    {
        this.transform.rotation = rotation;
    }

    void Update()               // Basic movement control
    {
        CmdUpdateRotation(transform.rotation);
        if (hasAuthority)
        {
            float new_x = Input.GetAxis("Mouse X") * msens;
            float new_y = Input.GetAxis("Mouse Y") * msens;

            rotation.x -= new_y;
            rotation.y += new_x;
            rotation.x = Mathf.Clamp(rotation.x, -89f, 89f);
            
            this.transform.rotation = Quaternion.Euler(0, rotation.y, 0);
            mainCamera.transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                running = true;
                crouching = false;
                mainCamera.transform.localPosition = new Vector3(mainCamera.transform.localPosition.x, normalHeight, mainCamera.transform.localPosition.z);
            }
            if (Input.GetKeyUp(KeyCode.LeftShift)) running = false;

            if (Input.GetKeyDown(KeyCode.T)) light.enabled = !light.enabled;

            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                crouching = !crouching;
            }

            transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * ((running) ? run_speed : speed) * Time.deltaTime);

            animator.SetFloat("speed", Input.GetAxis("Vertical") * ((running) ? run_speed : speed));
            animator.SetFloat("side_speed", Input.GetAxis("Horizontal") * ((running) ? run_speed : speed));

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                if (!audioSource.isPlaying)
                    StartCoroutine(Footsteps());
            }
            Ray newPositionRay = mainCamera.GetComponent<Camera>().ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));
            Vector3 newPosition = newPositionRay.origin + newPositionRay.direction * targetDistance;
            targetPoint.position = newPosition;
        }
    }
}

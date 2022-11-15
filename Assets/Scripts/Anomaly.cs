using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Windows.Speech;
using System.Linq;

using Mirror;

public abstract class Anomaly : NetworkBehaviour
{
    /*
     *      Anomaly <- NetworkBehaviour
     *      Anomaly -> GhostClasses
     *      Defines ghost hunting behaviour, events logics, signs, voice recognition etc.
     *      Base class of every ghost behaviour script (Must be at least)
     *      
     *      Status: IN ACTIVE DEVELOPMENT
     *      
     *      TODO LIST:
     *      - Review hunting behaviour
     *      - Review NavMesh communication (checks required)
     *      - Mass testing required (NetworkBehaviour)
     *      - Add signs list and sign logics
    */

    //---   Serializable Fields   ---//
    [SerializeField, Range(0f, 100f)] private float distanceTreshold;   // Distance, where ghost triggers mind-decrement

    [SerializeField, Range(0f, 1f)] protected float attackThreshold;    // 
    [SerializeField, Range(0f, 1f)] protected float eventThreshold;     // 
    [SerializeField, Range(0f, 2f)] protected float huntKillDistance;   // Distacne
    [SerializeField, Range(0, 100)] protected int   huntDuration;       // 
    [SerializeField, Range(0, 100)] protected int   huntCooldown;       // 
    [SerializeField, Range(0, 100)] protected int   eventCooldown;      // 
    [SerializeField, Range(0, 100)] protected int   checkThreshold;     // 
    [SerializeField] protected SkinnedMeshRenderer meshRenderer;        // 

    [SerializeField] protected bool disappears;                         // 
    [SerializeField] protected List<GameObject> players;                    // 
    [SerializeField] protected GameObject[] rooms;                      // 

    protected bool hunting = false;
    protected bool inEvent = false;
    protected bool can_hunt = true;
    protected bool can_perform_event = true;

    protected UnityEngine.AI.NavMeshAgent agent;
    protected float average_mind = 0.0f;
    protected Transform spawnPoint;

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();
    private Animator animator;

    [SerializeField] public bool isPerformingRadio;
    [SerializeField] public bool isPerformingEMP;
    [SerializeField] public bool isPerformingNotes;
    [SerializeField] public bool isPerformingFlashlight;

    public bool isHunting { get { return hunting; } }

    [ServerCallback]
    protected void CheckPlayer(GameObject player)   // If player is in ghost influence - lower mind level
    {
        if (Vector3.Distance(transform.position, player.transform.position) < distanceTreshold)
        {
            Player player_ = player.GetComponent<Player>();
            player_.mind -= Random.Range(0.01f, 0.05f);
            if (player_.mind < 0) player_.mind = 0;         // IDK if this nessesary [!]
        }
    }

    [ServerCallback]
    protected void UpdatePlayersList()
    {
        players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        for (int i = 0; i < players.Count; i++)
        {
            if (!players[i].GetComponent<Player>().alive)
            {
                players.Remove(players[i]);
            }
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdStartCheckingPlayers()  // Commands executed on server by client request 
    {
        StartCoroutine(CheckPlayers());
    }

    [ServerCallback]
    protected IEnumerator CheckPlayers()
    {
        while (true)
        {
            average_mind = 0.0f;
            for (int i = 0; i < players.Count; i++)
            {
                CheckPlayer(players[i]);
                average_mind += players[i].GetComponent<Player>().mind;
            }
            average_mind /= players.Count;
            
            /*
             *  "DICE" event pattern
             *  Program "Rolls the dice" and decides to run / not run the hunt / event
             *  
             *  UPD. Random.Range works on [a, b); (So Random.Range(1, 3) generates 1 and 2)
             *  
             *  Current settings: [1, 2) with 50% chance of performing event or hunt
            */

            if (average_mind < attackThreshold && !hunting && can_hunt)
            {
                int dice = Random.Range(1, 3);
                if (dice == 2) CmdStartHunt();
            }
            if (average_mind < eventThreshold && !inEvent && can_perform_event)
            {
                int dice = Random.Range(1, 3);
                if (dice == 2) ghostevent();
            }
            yield return new WaitForSeconds(checkThreshold);
        }
    }

    protected IEnumerator HuntThreshold()
    {
        // This event performs after hunt() coroutine starts. This one determines how long hunting will last

        hunting = true;
        yield return new WaitForSeconds(huntDuration);
        StartCoroutine(HuntCooldown());
        hunting = false;
        can_perform_event = true;
    }

    protected IEnumerator HuntCooldown()
    {
        // This event calls after hunt() coroutine stops. This one determines, when new hunt can be performed again.
        
        can_hunt = false;
        yield return new WaitForSeconds(huntCooldown);
        can_hunt = true;
    }

    protected IEnumerator EventCooldown()
    {
        // This event calls after ghostevent() stops. This one determines when new ghostevent can be performed again.

        can_perform_event = false;
        yield return new WaitForSeconds(eventCooldown);
        can_perform_event = true;
    }

    [ServerCallback] public void CmdStartHunt()
    {
        // ClientRpc code executes on every client after server request

        StartCoroutine(hunt());
    }

    [ClientRpc]
    private void ToggleRenderer(bool enabled)
    {
        meshRenderer.enabled = enabled;
    }

    [ServerCallback]
    public IEnumerator hunt()
    {
        StartCoroutine(HuntThreshold());        // Starts parallel hunt controller. It will stop this coroutine when needed
        UpdatePlayersList();
        hunting = true;                         
        can_perform_event = false;
        ToggleRenderer(true);

        while (hunting)
        {
            /*
             *  SECTION 1: FINDING SUITABLE PLAYER
             *  
             *  Uses raycasting to EVERY alive player on map. If player is visible and in distance of vision - starts chasing.
            */
            bool found = false;                         // Find anyone?
            Transform closest = transform;              // Closest player

            foreach (GameObject player in players)      // Players array is being updated in CheckPlayers()
            {
                Ray ray = new Ray(transform.position, (player.transform.position - transform.position).normalized);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject.tag == "Player" 
                        && ((found && Vector3.Distance(transform.position, closest.position) > Vector3.Distance(player.transform.position, transform.position)) 
                        || !found))
                    {
                        closest = player.transform;
                        found = true;
                        Debug.DrawLine(transform.position, closest.transform.position, Color.red, 10f, true);
                        break;
                    }
                }
            }

            /*
             *  SECTION 2: CHASE OR WANDER
            */

            if (found)
            {
                if (Vector3.Distance(closest.position, transform.position) < huntKillDistance)
                {
                    //-----------------------------------------------//
                    //                      KILL                     //
                    //-----------------------------------------------//

                    closest.gameObject.GetComponent<Player>().Die();       // see Player.cs::Die();
                    players.Remove(closest.gameObject);                    // Remove dead body from players list
                    if (players.Count == 0) Debug.Log("Game Over");        // TODO: Switch to lobby correctly
                    hunting = false;                                       // if had kill - stop hunting
                    break;
                }
                if (!agent.SetDestination(closest.position)) found = false; // If failed to fetch availibale path on NavMesh
            }
            else
            {
                if (!agent.hasPath)
                {
                    int id = Random.Range(0, rooms.Length);

                    agent.SetDestination(rooms[id].transform.position);     // Wander to random room
                    Debug.DrawLine(transform.position, rooms[id].transform.position, Color.red, 10f, false); ;
                }
            }
            animator.SetFloat("speed", agent.speed);                        // Animated as player btw.
            
            yield return new WaitForEndOfFrame();
        }

        // END HUNT ->

        ToggleRenderer(false);
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        agent.ResetPath();
        //agent.SetDestination(spawnPoint.position);
        yield return null;
    }

    private void KeywordRecognizer_OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;
        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    private void Awake()
    {
        spawnPoint = transform;
        players = new List<GameObject>(GameObject.FindGameObjectsWithTag("Player"));
        rooms = GameObject.FindGameObjectsWithTag("GameController");
        spawnPoint = rooms[Random.Range(0, rooms.Length)].transform;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        if (NetworkClient.Ready())
            CmdStartCheckingPlayers();

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        keywords.Add("give us a sign", () =>
        {
            ghostevent();
        });
        keywords.Add("fuck you leatherman", () =>
        {
            CmdStartHunt();
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        animator = GetComponentInChildren<Animator>();
    }

    public abstract void ghostevent();
}

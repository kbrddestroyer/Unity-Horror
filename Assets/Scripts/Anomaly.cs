using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine.Windows.Speech;
using System.Linq;

public abstract class Anomaly : MonoBehaviour
{
    [SerializeField, Range(0f, 100f)] private float distanceTreshold;
    [SerializeField, Range(0f, 1f)] protected float attackThreshold;
    [SerializeField, Range(0f, 1f)] protected float eventThreshold;
    [SerializeField, Range(0f, 2f)] protected float huntKillDistance;
    [SerializeField, Range(0, 100)] protected int huntDuration;
    [SerializeField, Range(0, 100)] protected int huntCooldown;
    [SerializeField, Range(0, 100)] protected int eventCooldown;
    [SerializeField, Range(0, 100)] protected int checkThreshold;
    [SerializeField] protected SkinnedMeshRenderer meshRenderer;

    [SerializeField] protected bool disappears;
    [SerializeField] protected GameObject[] players;
    [SerializeField] protected GameObject[] rooms;

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

    public bool isHunting { get { return hunting; } }

    protected void CheckPlayer(GameObject player)
    {
        if (Vector3.Distance(transform.position, player.transform.position) < distanceTreshold)
        {
            Player player_ = player.GetComponent<Player>();
            player_.mind -= Random.Range(0.01f, 0.05f);
            if (player_.mind < 0) player_.mind = 0;
        }
    }

    protected void UpdatePlayersList()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        for (int i = 0; i < players.Length; i++)
        {
            if (!players[i].GetComponent<Player>().alive)
                for (int j = i; j < players.Length - 1; j++)
                    players[i] = players[i + 1];
            System.Array.Resize(ref players, players.Length - 1);
        }
    }

    protected IEnumerator CheckPlayers()
    {
        while (true)
        {
            average_mind = 0.0f;
            for (int i = 0; i < players.Length; i++)
            {
                CheckPlayer(players[i]);
                average_mind += players[i].GetComponent<Player>().mind;
            }
            average_mind /= players.Length;
            Debug.Log("Average Status: " + average_mind * 100 + "%");
            if (average_mind < attackThreshold && !hunting && can_hunt)
            {
                int dice = Random.Range(1, 3);
                if (dice == 2) StartCoroutine(hunt());
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
        hunting = true;
        yield return new WaitForSeconds(huntDuration);
        StartCoroutine(HuntCooldown());
        hunting = false;
        can_perform_event = true;
    }

    protected IEnumerator HuntCooldown()
    {
        can_hunt = false;
        yield return new WaitForSeconds(huntCooldown);
        can_hunt = true;
    }

    protected IEnumerator EventCooldown()
    {
        can_perform_event = false;
        yield return new WaitForSeconds(eventCooldown);
        can_perform_event = true;
    }

    public IEnumerator hunt()
    {
        StartCoroutine(HuntThreshold());
        Debug.Log("Hunt!");
        hunting = true;
        can_perform_event = false;
        //GetComponent<MeshRenderer>().enabled = true;
        meshRenderer.enabled = true;
        while (hunting)
        {
            bool found = false;
            Transform closest = transform;
            foreach (GameObject player in players)
            {
                Ray ray = new Ray(transform.position, (player.transform.position - transform.position).normalized);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    if (hit.transform.gameObject.tag == "Player" && ((found && Vector3.Distance(transform.position, closest.position) > Vector3.Distance(player.transform.position, transform.position)) || !found))
                    {
                        closest = player.transform;
                        found = true;
                        break;
                    }
                }
            }
            if (found)
            {
                if (Vector3.Distance(closest.position, transform.position) < huntKillDistance)
                {
                    closest.gameObject.GetComponent<Player>().Die();
                    UpdatePlayersList();
                    if (players.Length == 0) Debug.Log("Game Over");
                    hunting = false;
                    break;
                }
                if (!agent.SetDestination(closest.position)) found = false;
            }
            else
            {
                if (!agent.hasPath)
                {
                    int id = Random.Range(0, rooms.Length);
                    Debug.Log(id);
                    agent.SetDestination(rooms[id].transform.position);
                }
            }
            animator.SetFloat("speed", agent.speed);
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("It's over");
        //GetComponent<MeshRenderer>().enabled = false;
        meshRenderer.enabled = false;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        Debug.Log(spawnPoint);
        agent.SetDestination(spawnPoint.position);
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

    private void Start()
    {
        spawnPoint = transform;
        players = GameObject.FindGameObjectsWithTag("Player");
        rooms = GameObject.FindGameObjectsWithTag("GameController");
        spawnPoint = rooms[Random.Range(0, rooms.Length)].transform;
        transform.position = spawnPoint.position;
        transform.rotation = spawnPoint.rotation;
        StartCoroutine(CheckPlayers());

        agent = GetComponent<UnityEngine.AI.NavMeshAgent>();

        keywords.Add("give us a sign", () =>
        {
            ghostevent();
        });
        keywords.Add("fuck you leatherman", () =>
        {
            StartCoroutine(hunt());
        });
        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += KeywordRecognizer_OnPhraseRecognized;
        keywordRecognizer.Start();

        animator = GetComponentInChildren<Animator>();
        Debug.Log(animator);
    }

    public abstract void ghostevent();
}

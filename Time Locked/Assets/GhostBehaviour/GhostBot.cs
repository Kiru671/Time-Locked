using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    public Transform[] targets;
    public Transform spawnPoint;
    public float spawnDelay = 15f;
    public float walkDelayAfterSpawn = 25f;
    public float delayBetweenTargets = 30f; // ✅ Yeni: hedefler arası bekleme süresi

    private NavMeshAgent agent;
    private int currentTargetIndex = 0;
    private bool isWalkingEnabled = false;
    private bool isWaitingAtTarget = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        gameObject.SetActive(false);
        Invoke(nameof(SpawnGhost), spawnDelay);
    }

    void SpawnGhost()
    {
        transform.position = new Vector3(spawnPoint.position.x, 0f, spawnPoint.position.z);
        gameObject.SetActive(true);
        Invoke(nameof(BeginWalking), walkDelayAfterSpawn);
    }

    void BeginWalking()
    {
        isWalkingEnabled = true;
        GoToCurrentTarget();
    }

    void Update()
    {
        if (!gameObject.activeSelf || !isWalkingEnabled || isWaitingAtTarget) return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                isWaitingAtTarget = true;
                Invoke(nameof(GoToNext), delayBetweenTargets); // ✅ bekleme süresi sonrası geç
            }
        }
    }

    void GoToCurrentTarget()
    {
        if (currentTargetIndex < targets.Length)
        {
            agent.SetDestination(targets[currentTargetIndex].position);
        }
    }

    void GoToNext()
    {
        currentTargetIndex++;
        isWaitingAtTarget = false;

        if (currentTargetIndex < targets.Length)
        {
            GoToCurrentTarget();
        }
        else
        {
            Debug.Log("GhostAI: Tüm hedeflere ulaşıldı.");
        }
    }
}

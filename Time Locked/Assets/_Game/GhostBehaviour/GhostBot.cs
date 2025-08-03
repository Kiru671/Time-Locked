using UnityEngine;
using UnityEngine.AI;

public class GhostAI : MonoBehaviour
{
    [Header("Ghost's next targets.")]
    public Transform[] targets; // Multiple targets
    private int currentTargetIndex = 0;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targets.Length == 0)
        {
            Debug.LogError("GhostAI: Target list empty");
            return;
        }

        MoveToNextTarget();
    }

    void Update()
    {
        // next target
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                GoToNext();
            }
        }
    }

    void GoToNext()
    {
        currentTargetIndex++;

        if (currentTargetIndex < targets.Length)
        {
            MoveToNextTarget();
        }
        else
        {
            Debug.Log("All targets have been achieved.");
        }
    }

    void MoveToNextTarget()
    {
        Transform nextTarget = targets[currentTargetIndex];
        agent.SetDestination(nextTarget.position);
    }
}

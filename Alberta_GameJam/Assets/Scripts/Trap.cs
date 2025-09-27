using UnityEngine;

public class Trap : MonoBehaviour
{
    [SerializeField] private float trapDuration = 2f;

    private void OnTriggerEnter(Collider other)
    {
        Ghost ghost = other.GetComponent<Ghost>();
        if (ghost != null)
        {
            StartCoroutine(TrapGhost(ghost));
            // Disable trap collider to prevent further use
            Collider trapCollider = GetComponent<Collider>();
            if (trapCollider != null)
            {
                trapCollider.enabled = false;
            }
            Debug.Log($"Trap triggered by ghost: {ghost.name} at {transform.position}");
        }
    }

    private System.Collections.IEnumerator TrapGhost(Ghost ghost)
    {
        UnityEngine.AI.NavMeshAgent agent = ghost.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.isStopped = true;
        }
        yield return new WaitForSeconds(trapDuration);
        if (agent != null)
        {
            agent.isStopped = false;
        }
        gameObject.SetActive(false);
    }
}

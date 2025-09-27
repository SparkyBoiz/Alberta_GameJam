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
            Collider trapCollider = GetComponent<Collider>();
            if (trapCollider != null)
            {
                trapCollider.enabled = false;
            }
        }
    }

    private System.Collections.IEnumerator TrapGhost(Ghost ghost)
    {
        ghost.EnterTrapped(trapDuration);
        yield return new WaitForSeconds(trapDuration);
        gameObject.SetActive(false);
    }
}
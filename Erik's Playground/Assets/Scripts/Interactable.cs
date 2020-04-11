using UnityEngine;

// Note - this was made to store a reference to some useful functions.
// It is currently not an implemented script in the game

public class Interactable : MonoBehaviour
{
    public float radius;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

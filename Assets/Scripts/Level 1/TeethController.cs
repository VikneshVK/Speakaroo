using UnityEngine;

public class TeethController : MonoBehaviour
{
    public GameObject foam; // Reference to the foam game object to be activated

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("brush"))
        {
            Debug.Log("Collision with brush detected.");
            ActivateFoam();
        }
    }
    private void ActivateFoam()
    {
        if (foam != null)
            foam.SetActive(true);
        else
            Debug.LogError("Foam GameObject is not assigned.");
    }
}

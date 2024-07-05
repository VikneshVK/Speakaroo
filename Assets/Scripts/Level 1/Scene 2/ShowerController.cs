using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowerController : MonoBehaviour
{
    public List<GameObject> foamObjects = new List<GameObject>(); // List to store foam objects

    public void AddFoamObject(GameObject foam)
    {
        foamObjects.Add(foam);
    }

    public IEnumerator DestroyFoamObjects()
    {
        foreach (GameObject foam in foamObjects)
        {
            LeanTween.scale(foam, Vector3.zero, 0.5f).setOnComplete(() => Destroy(foam));
        }

        foamObjects.Clear();
        yield return new WaitForSeconds(0.5f); // Wait for the foam to shrink and be destroyed
    }
}

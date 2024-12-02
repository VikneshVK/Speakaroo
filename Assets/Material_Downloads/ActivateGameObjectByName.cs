using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateGameObjectByName : MonoBehaviour
{
    public List<GameObject> targetObjects;

    public void ActivateByIndex(int index)
    {
        foreach (GameObject obj in targetObjects)
        {
            if (obj != null && obj.activeSelf)
            {
                obj.SetActive(false);
            }
        }

        if (index >= 0 && index < targetObjects.Count)
        {
            GameObject targetObject = targetObjects[index];
            if (targetObject != null)
            {
                targetObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning($"GameObject at index {index} is null.");
            }
        }
        else
        {
            Debug.LogWarning($"Invalid index {index}. Ensure it's between 0 and {targetObjects.Count - 1}.");
        }
    }
}

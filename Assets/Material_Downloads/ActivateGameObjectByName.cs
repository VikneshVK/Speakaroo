using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateGameObjectByName : MonoBehaviour
{
    public List<GameObject> targetObjects;

    [Header("Activate the ADL Panel")]
    public GameObject ADL_Activate;

    [Header("Activate the Cog Panel")]
    public GameObject Cog_Activate;

    [Header("Activate the Read Panel")]
    public GameObject ReadL_Activate;

    [Header("Activate the Speech Panel")]
    public GameObject Speech_Activate;

    [Header("Activate the FM Panel")]
    public GameObject FML_Activate;

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

    public void ActivateADL()
    {
        ADL_Activate.SetActive(true);
        Cog_Activate.SetActive(false);
        ReadL_Activate.SetActive(false);
        Speech_Activate.SetActive(false);
        FML_Activate.SetActive(false);

    }

    public void ActivateCog()
    {
        ADL_Activate.SetActive(false);
        Cog_Activate.SetActive(true);
        ReadL_Activate.SetActive(false);
        Speech_Activate.SetActive(false);
        FML_Activate.SetActive(false);
    }

    public void ActivateRead()
    {
        ADL_Activate.SetActive(false);
        Cog_Activate.SetActive(false);
        ReadL_Activate.SetActive(true);
        Speech_Activate.SetActive(false);
        FML_Activate.SetActive(false);
    }

    public void ActivateSpeech()
    {
        ADL_Activate.SetActive(false);
        Cog_Activate.SetActive(false);
        ReadL_Activate.SetActive(false);
        Speech_Activate.SetActive(true);
        FML_Activate.SetActive(false);
    }

    public void ActivateFM()
    {
        ADL_Activate.SetActive(false);
        Cog_Activate.SetActive(false);
        ReadL_Activate.SetActive(false);
        Speech_Activate.SetActive(false);
        FML_Activate.SetActive(true);
    }
}

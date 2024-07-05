using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneSetupAnimation : MonoBehaviour
{
    [System.Serializable]
    public class TweenTarget
    {
        public GameObject objectToTween;
        public Transform targetContainer; // This is the transform of the container for the object
    }

    public List<TweenTarget> tweenTargets; // List of objects and their respective containers
    public float tweenDuration = 1f; // Duration for each tween

    private void Start()
    {
        TweenAllObjects();
    }

    private void TweenAllObjects()
    {
        foreach (TweenTarget tweenTarget in tweenTargets)
        {
            if (tweenTarget.objectToTween != null && tweenTarget.targetContainer != null)
            {
                Vector3 targetPosition = tweenTarget.targetContainer.position; // Final position is the container's position
                // Start the tween from current position to the target position
                LeanTween.move(tweenTarget.objectToTween, targetPosition, tweenDuration).setEase(LeanTweenType.easeInOutBack);
            }
            else
            {
                Debug.LogWarning("TweenTarget setup incorrect for one or more entries.");
            }
        }
    }
}

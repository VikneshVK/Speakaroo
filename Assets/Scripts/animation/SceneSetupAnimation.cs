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
        public MonoBehaviour anchorScript; // Reference to the anchor game object script, if any
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

                // Check if the anchor script is attached and disable it before tweening
                if (tweenTarget.anchorScript != null)
                {
                    tweenTarget.anchorScript.enabled = false;
                }

                // Start the tween from current position to the target position
                LeanTween.move(tweenTarget.objectToTween, targetPosition, tweenDuration).setEase(LeanTweenType.easeInOutBack)
                .setOnComplete(() =>
                {
                    // Re-enable the anchor script after tweening if it exists
                    if (tweenTarget.anchorScript != null)
                    {
                        /*tweenTarget.anchorScript.enabled = true;*/
                    }
                });
            }
            else
            {
                Debug.LogWarning("TweenTarget setup incorrect for one or more entries.");
            }
        }
    }
}

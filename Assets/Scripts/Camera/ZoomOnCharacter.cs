using UnityEngine;
using System.Collections;

public class ZoomOnCharacter : MonoBehaviour
{
    public Transform character;  // Target character to zoom towards
    public CameraController cameraController;  // Reference to the CameraController script
    public GameObject speechBubble;  // Reference to the GameObject prefab to instantiate
    public Transform speechBubblePosition;

    private Animator animator;
    private bool isZoomedIn;
    private bool hasInstantiated = false;  // Ensure we only instantiate once per animation

    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on " + gameObject.name);
        }
    }

    void OnMouseDown()
    {
        if (isZoomedIn)
        {
            StartCoroutine(ZoomOutAndAnimate());
        }
        else
        {
            StartCoroutine(ZoomInAndAnimate());
        }
    }

    private IEnumerator ZoomInAndAnimate()
    {
        yield return StartCoroutine(cameraController.ZoomInOn(character));
        isZoomedIn = true;
        UpdateAnimatorFlags(true);
    }

    private IEnumerator ZoomOutAndAnimate()
    {
        yield return StartCoroutine(cameraController.ZoomOut());
        isZoomedIn = false;
        UpdateAnimatorFlags(false);
        // Start the talk animation directly if required
        animator.Play("Talk_0");
    }

    void Update()
    {
        if (!hasInstantiated && isZoomedIn == false && IsAnimationComplete("Talk"))
        {
            Instantiate(speechBubble, speechBubblePosition.position, Quaternion.identity);
            hasInstantiated = true;  // Prevent multiple instantiations
        }
    }

    private bool IsAnimationComplete(string animationName)
    {
        // Check if the animation is playing and whether it has completed
        var animatorStateInfo = animator.GetCurrentAnimatorStateInfo(0);
        return animatorStateInfo.IsName(animationName) && animatorStateInfo.normalizedTime >= 1.0f && !animator.IsInTransition(0);
    }

    private void UpdateAnimatorFlags(bool zoomedIn)
    {
        animator.SetBool("isZoomedIn", zoomedIn);
        animator.SetBool("isClicked", false);  // Reset interaction flag after actions
    }
}

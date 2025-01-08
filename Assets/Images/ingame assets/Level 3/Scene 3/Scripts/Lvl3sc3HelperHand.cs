using System.Collections;
using UnityEngine;

public class Lvl3sc3HelperHand : MonoBehaviour
{
   /* public GameObject glowPrefab;*/
    public GameObject helperHandPrefab;
    public float delayDuration = 10f; // Time before helper appears
    public float audioDelay = 0.5f; // Delay before playing audio

    private Transform target; // The object to be highlighted
    private Vector3 dropTargetPosition; // Position where helper hand will move to
    private GameObject currentGlow;
    private GameObject currentHelperHand;
    private Coroutine delayCoroutine;
    private float halfDelay;
    private bool isGlowComplted = false;

    public Animator kikiAnimator; // Reference to Kiki's animator
    public AudioSource audioSource; // AudioSource attached to the same GameObject

    public AudioClip audio1; // Audio for helper1 trigger
    public AudioClip audio2; // Audio for helper2 trigger
    public AudioClip audio3; // Audio for helper3 trigger

    private void Start()
    {
        halfDelay = delayDuration / 2;
    }

    public void StartDelayTimer(GameObject callingObject)
    {
        target = callingObject.transform; // Set target position to calling object's position
        SetDropTarget(); // Determine the drop target based on the target type

        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }
        delayCoroutine = StartCoroutine(DelayTimerCoroutine());
    }

    private void SetDropTarget()
    {
        if (target.CompareTag("Cloth"))
        {
            ClothesdragHandler clothHandler = target.GetComponent<ClothesdragHandler>();
            if (clothHandler != null)
            {
                dropTargetPosition = clothHandler.clothesBasketRenderer.transform.position;
            }
        }
        else if (target.CompareTag("Toy"))
        {
            Lvl3Sc3ToysdragHandler toyHandler = target.GetComponent<Lvl3Sc3ToysdragHandler>();
            Lvl3Sc3DragManager dragManager = FindObjectOfType<Lvl3Sc3DragManager>();

            if (toyHandler != null && dragManager != null)
            {
                if (toyHandler.isDry)
                {
                    dropTargetPosition = toyHandler.toyBasketPositions[Lvl3Sc3ToysdragHandler.toysDropped].position;
                }
                else
                {
                    var availableHangers = dragManager.GetAvailableHangers();
                    if (availableHangers.Count > 0)
                    {
                        dropTargetPosition = availableHangers[0].position;
                    }
                }
            }
        }
    }

    private IEnumerator DelayTimerCoroutine()
    {
        float timer = 0f;

        while (timer < delayDuration)
        {
            /*if (timer >= halfDelay && currentGlow == null && !isGlowComplted)
            {
                yield return StartCoroutine(GlowEffect());
            }*/

            timer += Time.deltaTime;
            yield return null;

            if (IsInteracted())
            {
                ResetHelperHand();
                FindObjectOfType<Lvl3Sc3DragManager>().OnObjectInteracted();
                yield break;
            }
        }

        if (currentHelperHand == null)
        {
            currentHelperHand = Instantiate(helperHandPrefab, target.position, Quaternion.identity);
            TweenHelperHand();
        }
    }

    /*private IEnumerator GlowEffect()
    {
        currentGlow = Instantiate(glowPrefab, target.position, Quaternion.identity);
        LeanTween.scale(currentGlow, Vector3.one * 10, 0.5f);
        yield return new WaitForSeconds(2f);
        LeanTween.scale(currentGlow, Vector3.zero, 0.5f).setOnComplete(() => Destroy(currentGlow));
        isGlowComplted = true;
    }*/

    private void TweenHelperHand()
    {
        // Determine the correct trigger and audio to play based on conditions
        if (target.CompareTag("Toy"))
        {
            var toyHandler = target.GetComponent<Lvl3Sc3ToysdragHandler>();
            if (toyHandler != null && !toyHandler.isDry)
            {
                SetKikiAnimationAndAudio("helper2", audio2);
            }
        }
        else if (target.CompareTag("Cloth"))
        {
            var clothHandler = target.GetComponent<ClothesdragHandler>();
            if (clothHandler != null && clothHandler.isDry && !FindObjectOfType<Lvl3Sc3DragManager>().isEvening)
            {
                SetKikiAnimationAndAudio("helper1", audio1);
            }
            else if (clothHandler != null && clothHandler.isDry && FindObjectOfType<Lvl3Sc3DragManager>().isEvening)
            {
                SetKikiAnimationAndAudio("helper3", audio3);
            }
        }

        // Move the helper hand to the determined drop target position
        LeanTween.move(currentHelperHand, dropTargetPosition, 3f).setLoopClamp();
    }

    private void SetKikiAnimationAndAudio(string trigger, AudioClip audioClip)
    {
        // Trigger Kiki's animation
        if (kikiAnimator != null)
        {
            kikiAnimator.SetTrigger(trigger);
        }

        // Play audio with a delay to avoid looping
        if (audioSource != null && audioClip != null)
        {
            StartCoroutine(PlayAudioWithDelay(audioClip));
        }
    }

    private IEnumerator PlayAudioWithDelay(AudioClip audioClip)
    {
        yield return new WaitForSeconds(audioDelay);
        audioSource.PlayOneShot(audioClip);
    }

    private bool IsInteracted()
    {
        if (target.TryGetComponent(out ClothesdragHandler clothHandler))
        {
            return clothHandler.IsDragging;
        }
        else if (target.TryGetComponent(out Lvl3Sc3ToysdragHandler toyHandler))
        {
            return toyHandler.IsDragging;
        }
        return false;
    }

    public void ResetHelperHand()
    {
        if (delayCoroutine != null)
        {
            StopCoroutine(delayCoroutine);
        }

        if (currentGlow != null)
        {
            Destroy(currentGlow);
        }

        if (currentHelperHand != null)
        {
            LeanTween.cancel(currentHelperHand);
            Destroy(currentHelperHand);
        }

        isGlowComplted = false;
    }
}

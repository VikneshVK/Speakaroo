using UnityEngine;
using System.Collections;
using TMPro;

public class BeakerDragHandler : MonoBehaviour
{
    public Transform dropPosition;
    public float tweenSpeed = 1f;
    public GameObject rightTestTubeStand1;
    public GameObject leftTestTubeStand1;
    public GameObject rightTestTubeStand2;
    public GameObject leftTestTubeStand2;
    public GameObject originalRightStand1Position;
    public GameObject originalLeftStand1Position;
    public Transform rightStandPosition;
    public Transform leftStandPosition;
    public Lvl8SSc2HelperController helperController;    

    [Header("SFX")]
    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    private bool beakerSfxPlayed = false;

    public Animator kikiAnimator;
    public AudioSource kikiAudioSource;
    public AudioClip color1Audio;
    public AudioClip color3Audio;
    public TextMeshProUGUI subtitleText;

    private Vector3 initialPosition;
    private bool isDragging = false;
    private bool CanDrag = true;

    void Start()
    {
        initialPosition = transform.position;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDragging && CanDrag)
        {
            // Follow the mouse position while dragging
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(mousePosition.x, mousePosition.y, initialPosition.z);
        }
    }

    private void OnMouseDown()
    {
        isDragging = true;
        helperController.ResetGlow();
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Check if the beaker collider and drop point collider overlap
        Collider2D dropPointCollider = dropPosition.GetComponent<Collider2D>();
        Collider2D beakerCollider = GetComponent<Collider2D>();

        if (dropPointCollider != null && beakerCollider != null && dropPointCollider.bounds.Intersects(beakerCollider.bounds))
        {
            if (SfxAudioSource != null && !beakerSfxPlayed)
            {
                beakerSfxPlayed = true;
                SfxAudioSource.PlayOneShot(SfxAudio1);
                Debug.Log("Audio is playing");
            }
            Vector3 targetPosition = dropPointCollider.bounds.center;
            LeanTween.move(gameObject, targetPosition, tweenSpeed)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    transform.position = targetPosition; // Ensure exact alignment
                    CanDrag = false;
                    TweenTestTubeStands();
                });
        }
        else
        {
            // Reset position if not overlapping
            transform.position = initialPosition;
            helperController.SpawnGlow(gameObject);
        }
    }

    public void TweenBackTestTubeStands()
    {
        LeanTween.move(rightTestTubeStand1, originalRightStand1Position.transform.position, tweenSpeed)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                CallSetOriginalPosition(rightTestTubeStand1);
            });

        LeanTween.move(leftTestTubeStand1, originalLeftStand1Position.transform.position, tweenSpeed)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                CallSetOriginalPosition(leftTestTubeStand1);
            });

        LeanTween.move(rightTestTubeStand2, rightStandPosition.position, tweenSpeed)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                CallSetOriginalPosition(rightTestTubeStand2);
                TriggerKikiThirdColor();
            });

        LeanTween.move(leftTestTubeStand2, leftStandPosition.position, tweenSpeed)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnComplete(() =>
            {
                CallSetOriginalPosition(leftTestTubeStand2);
            });
    }

    private void TweenTestTubeStands()
    {
        if (Lvl8Sc2QuestManager.Instance.colorsMade == 0)
        {
            LeanTween.move(rightTestTubeStand1, rightStandPosition.position, tweenSpeed)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    CallSetOriginalPosition(rightTestTubeStand1);
                    TriggerKikiFirstColor();
                });

            LeanTween.move(leftTestTubeStand1, leftStandPosition.position, tweenSpeed)
                .setEase(LeanTweenType.easeInOutQuad)
                .setOnComplete(() =>
                {
                    CallSetOriginalPosition(leftTestTubeStand1);
                });
        }
    }
    private void TriggerKikiFirstColor()
    {
        if (kikiAnimator != null)
        {
            // Trigger the FirstColor animation
            kikiAnimator.SetTrigger("FirstColor");

            // Wait for the animation to finish and then enable the colliders
            StartCoroutine(WaitForKikiAnimation());
        }

        if (kikiAudioSource != null && color1Audio != null)
        {
            // Play the FirstColor audio
            kikiAudioSource.clip = color1Audio;
            kikiAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Pour the Full Blue TestTube into the Beaker", 0.5f));
        }
    }

    private void TriggerKikiThirdColor()
    {
        if (kikiAnimator != null)
        {
            // Trigger the ThirdColor animation
            kikiAnimator.SetTrigger("ThirdColor");

            // Wait for the animation to finish and then enable the colliders
            StartCoroutine(WaitForKikiAnimation1());
        }

        if (kikiAudioSource != null && color3Audio != null)
        {
            // Play the FirstColor audio
            kikiAudioSource.clip = color3Audio;
            kikiAudioSource.Play();
            StartCoroutine(RevealTextWordByWord("Pour the Half Red TestTube into the Beaker", 0.5f));
        }
    }

    private IEnumerator WaitForKikiAnimation()
    {
        // Wait until the FirstColor animation is playing
        while (!kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("FirstColor"))
        {
            yield return null;
        }

        // Wait until the FirstColor animation is complete
        while (kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("FirstColor") &&
               kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Enable the colliders of the test tubes
        EnableTestTubeColliders();
    }

    private IEnumerator WaitForKikiAnimation1()
    {
        // Wait until the FirstColor animation is playing
        while (!kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("ThirdColor"))
        {
            yield return null;
        }

        // Wait until the FirstColor animation is complete
        while (kikiAnimator.GetCurrentAnimatorStateInfo(0).IsName("ThirdColor") &&
               kikiAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
        {
            yield return null;
        }

        // Enable the colliders of the test tubes
        EnableTestTubeColliders1();
    }

    private void EnableTestTubeColliders()
    {
        EnableCollidersInStand(leftTestTubeStand1);
        EnableCollidersInStand(rightTestTubeStand1);
    }

    private void EnableTestTubeColliders1()
    {
        EnableCollidersInStand(leftTestTubeStand2);
        EnableCollidersInStand(rightTestTubeStand2);
    }

    private void EnableCollidersInStand(GameObject testTubeStand)
    {
        foreach (Transform child in testTubeStand.transform)
        {
            Collider2D collider = child.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = true;
                Debug.Log($"Enabled collider for {child.name}");
            }
        }
    }


    public void ResetTestTubeDragging()
    {
        // Enable dragging for all test tubes
        TTDragHandler[] testTubeHandlers = FindObjectsOfType<TTDragHandler>();
        foreach (TTDragHandler handler in testTubeHandlers)
        {
            handler.EnableDragging();
        }
    }

    private void CallSetOriginalPosition(GameObject testTubeStand)
    {
        foreach (Transform child in testTubeStand.transform)
        {
            TTDragHandler dragHandler = child.GetComponent<TTDragHandler>();
            if (dragHandler != null)
            {
                dragHandler.SetOriginalPosition(child.position);
            }
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }
        subtitleText.text = "";
    }
}
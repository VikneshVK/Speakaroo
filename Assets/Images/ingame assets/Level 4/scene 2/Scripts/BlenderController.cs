using UnityEngine;
using System.Collections;
using TMPro;

public class BlenderController : MonoBehaviour
{
    private JuiceController juiceController;
    private SpriteChangeController spriteChangeController;
    private SpriteRenderer jarSpriteRenderer;
    private Transform glass1Transform;
    private Transform glass2Transform;
    private Vector3 originalPosition;
    public bool isBlenderClicked;
    public LVL4Sc2HelperHand helperHand;
    private bool Finalplay;
    public GameObject parrot;
    public Transform birdEndPosition;
    private Vector3 birdInitialPosition;
    private Animator parrotAnimator;
    public LVL4Sc2AudioManager audioManager;
    public TextMeshProUGUI subtitleText;
    public AudioClip Audio1;
    private string subtitle1 = "mmm Tasty..!";



    void Start()
    {
        juiceController = FindObjectOfType<JuiceController>();
        spriteChangeController = FindObjectOfType<SpriteChangeController>();
        jarSpriteRenderer = GameObject.FindGameObjectWithTag("Blender_Jar").GetComponent<SpriteRenderer>();

        glass1Transform = GameObject.FindGameObjectWithTag("Glass1").transform;
        glass2Transform = GameObject.FindGameObjectWithTag("Glass2").transform;
        parrotAnimator = parrot.GetComponent<Animator>();
        isBlenderClicked = false;
        Finalplay = false;
    }
    void OnMouseDown()
    {
        if (GetComponent<Collider2D>().enabled)
        {

            helperHand.OnBlenderJarInteraction();
            originalPosition = jarSpriteRenderer.transform.position;
            jarSpriteRenderer.enabled = false;
            juiceController.OnBlenderClick();

            if (juiceController.juiceManager.isKikiJuice)
            {
                juiceController.TriggerBlenderAnimationForKiki(spriteChangeController.fruitsInBlender);
            }
            else
            {
                // Check if there is at least one fruit in the list before accessing it
                if (spriteChangeController.fruitsInBlender.Count > 0)
                {
                    string fruitTag = spriteChangeController.fruitsInBlender[0];
                    juiceController.TriggerBlenderAnimation(fruitTag);
                }
                else
                {
                    Debug.LogWarning("fruitsInBlender list is empty. Cannot trigger blender animation for first fruit.");
                }
            }

            juiceController.DisableBlenderCollider();
            StartCoroutine(EnableJarSpriteAfterAnimation());
        }
    }


    private IEnumerator EnableJarSpriteAfterAnimation()
    {
        yield return new WaitForSeconds(2.05f);  // Adjust based on animation length

        jarSpriteRenderer.enabled = true;
        juiceController.EnableJarCollider();
        spriteChangeController.UpdateJuiceSprite();

        StartCoroutine(HandleJarTweening());
    }

    private IEnumerator HandleJarTweening()
    {
        Transform targetGlass = juiceController.juiceManager.isKikiJuice ? glass2Transform : glass1Transform;
        float rotationAngle = juiceController.juiceManager.isKikiJuice ? -70f : 70f;

        Vector3 offset = juiceController.juiceManager.isKikiJuice ? new Vector3(-3f, -3f, 0f) : new Vector3(3f, 3f, 0f);
        Vector3 nearGlassPosition = targetGlass.position + offset;

        LeanTween.move(jarSpriteRenderer.gameObject, nearGlassPosition, 2f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(2f);

        LeanTween.rotateZ(jarSpriteRenderer.gameObject, rotationAngle, 1f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(1f);

        UpdateGlassSprite(targetGlass);
        jarSpriteRenderer.sprite = spriteChangeController.GetDefaultBlenderSprite();

        if (!juiceController.juiceManager.isKikiJuice)
        {
            spriteChangeController.ResetBlender();
            helperHand.ResetAndStartDelayTimer();
        }

        LeanTween.rotateZ(jarSpriteRenderer.gameObject, 0f, 1f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(1f);

        LeanTween.move(jarSpriteRenderer.gameObject, originalPosition, 2f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(2f);

        juiceController.juiceManager.isKikiJuice = true;
        isBlenderClicked = false;
        if (!Finalplay)
        {
            juiceController.juiceManager.UpdateFruitRequirements(true);
            Finalplay = true;
        }
        else
        {
            StartBirdTweenSequence("allDone", Audio1, subtitle1);
        }
    }

    private void StartBirdTweenSequence(string animationTrigger, AudioClip audioClip, string subtitleTextContent)
    {
        RectTransform parrotRectTransform = parrot.GetComponent<RectTransform>();
        if (parrotRectTransform == null) return;

        // Set initial position (off-screen, anchored position)
        parrotRectTransform.anchoredPosition = new Vector2(-1300, 320);

        // Tween parrot to the end position
        LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            parrotRectTransform.anchoredPosition = new Vector2(x, 320f);
        }).setOnComplete(() =>
        {
            parrotAnimator.SetTrigger(animationTrigger);
            audioManager.PlayAudio(audioClip);

            // Start subtitle display coroutine
            StartCoroutine(RevealTextWordByWord(subtitleTextContent, 0.5f));
        });
    }
    private void UpdateGlassSprite(Transform glassTransform)
    {
        SpriteRenderer glassSpriteRenderer = glassTransform.GetComponent<SpriteRenderer>();
        string juiceSpriteName = spriteChangeController.GetJuiceSpriteName();
        Sprite newGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + juiceSpriteName);

        if (glassSpriteRenderer != null && newGlassSprite != null)
        {
            glassSpriteRenderer.sprite = newGlassSprite;
        }
    }

    private IEnumerator RevealTextWordByWord(string fullText, float delayBetweenWords)
    {
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(true);

        string[] words = fullText.Split(' ');

        // Reveal words one by one
        for (int i = 0; i < words.Length; i++)
        {
            subtitleText.text = string.Join(" ", words, 0, i + 1);
            yield return new WaitForSeconds(delayBetweenWords);
        }

        yield return new WaitForSeconds(1f); // Wait for a bit after the last word
        subtitleText.text = "";
        subtitleText.gameObject.SetActive(false);
    }
}

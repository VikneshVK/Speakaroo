using UnityEngine;
using UnityEngine.UI;
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
    public SubtitleManager subtitleManager;
    public AudioClip Audio1;
    public GameObject juiceDrinkingPanel;
    public GameObject jojoImage;
    public Image GlassImage;
    private string subtitle1 = "mmm Tasty..!";

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;



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
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }
    void OnMouseDown()
    {
        if (GetComponent<Collider2D>().enabled)
        {

            helperHand.OnBlenderJarInteraction();
            originalPosition = jarSpriteRenderer.transform.position;
            jarSpriteRenderer.enabled = false;
            juiceController.OnBlenderClick();

            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio1);
            }

            if (juiceController.juiceManager.isKikiJuice)
            {
                juiceController.TriggerBlenderAnimationForKiki(spriteChangeController.fruitsInBlender);
            }
            else
            {
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

        UpdateGlassSprite();

        jarSpriteRenderer.sprite = spriteChangeController.GetDefaultBlenderSprite();
        
        LeanTween.rotateZ(jarSpriteRenderer.gameObject, 0f, 1f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(1f);

        LeanTween.move(jarSpriteRenderer.gameObject, originalPosition, 2f).setEase(LeanTweenType.easeInOutQuad);
        yield return new WaitForSeconds(2f);

        juiceDrinkingPanel.SetActive(true); // Assuming juiceDrinkingPanel is assigned in the Inspector.

        GameObject glassImage = juiceDrinkingPanel.transform.Find("Glass").gameObject; // Find the Glass image game object.
        Image glassComponent = glassImage.GetComponent<Image>(); // Get the Image component from the GameObject.
        Sprite glassSprite = glassComponent.sprite; // Access the sprite property from the Image component.
        LeanTween.scale(glassImage, Vector3.one, 0.5f).setEase(LeanTweenType.easeInOutBack);
        yield return new WaitForSeconds(0.5f);

        // Trigger animation based on the juice type
        string juiceSpriteName = spriteChangeController.GetJuiceSpriteName();
        Animator glassAnimator = glassImage.GetComponent<Animator>();

        if (glassAnimator != null)
        {
            SfxAudioSource.PlayOneShot(SfxAudio2);

            switch (juiceSpriteName)
            {
                case "kiwiSBJuice_glass":
                    glassAnimator.SetTrigger("KiwiSB");
                    break;
                case "KiwiBBJuice_glass":
                    glassAnimator.SetTrigger("BBKiwi");
                    break;
                case "BBSBJuice_glass":
                    glassAnimator.SetTrigger("BBSB");
                    break;
                case "kiwiJuice_glass":
                    glassAnimator.SetTrigger("Kiwi");
                    break;
                case "SBJuice_glass":
                    glassAnimator.SetTrigger("SB");
                    break;
                case "BBJuice_glass":
                    glassAnimator.SetTrigger("BB");
                    break;
                default:
                    Debug.LogWarning($"Unhandled juice sprite name: {juiceSpriteName}");
                    break;
            }
        }
        else
        {
            Debug.LogError("Glass image does not have an Animator component!");
        }
        yield return new WaitUntil(() =>
     glassAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1 &&
     !glassAnimator.IsInTransition(0));

        // Load the new sprite
        Sprite newGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/gls");
        if (newGlassSprite != null)
        {
            // Assign the new sprite to the Image component
            glassComponent.sprite = newGlassSprite;
        }
        else
        {
            Debug.LogError("Failed to load new glass sprite from Resources.");
        }

        RectTransform jojoRectTransform = jojoImage.GetComponent<RectTransform>();
        Animator jojoAnimator = jojoImage.GetComponent<Animator>();

        if (jojoRectTransform != null && jojoAnimator != null)
        {
            LeanTween.value(-350f, 105f, 1f).setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float yPos) =>
            {
                jojoRectTransform.anchoredPosition = new Vector2(jojoRectTransform.anchoredPosition.x, yPos);
            })
            .setOnComplete(() =>
            {
                jojoAnimator.SetTrigger("Tasty");

                // Wait for the "Tasty" animation to end before tweening back
                StartCoroutine(WaitForAnimationToEndAndTweenBack(jojoRectTransform));
            });
            if (Finalplay)
            {
                StartBirdTweenSequence("allDone", Audio1, subtitle1);
            }
            else
            {
                StartBirdTweenSequence("Tasty", Audio1, subtitle1);
                if (!juiceController.juiceManager.isKikiJuice)
                {
                    spriteChangeController.ResetBlender();
                    spriteChangeController.ResetGlassSprites();
                    helperHand.ResetAndStartDelayTimer();
                }
            }            
        }
        else
        {
            Debug.LogError("Jojo image does not have a RectTransform or Animator component!");
        }


        yield return new WaitForSeconds(5f);

        juiceDrinkingPanel.SetActive(false);
        juiceController.juiceManager.isKikiJuice = true;
        isBlenderClicked = false;

        if (!Finalplay)
        {
            juiceController.juiceManager.UpdateFruitRequirements(true);
            Finalplay = true;
        }
        
    }

    private IEnumerator WaitForAnimationToEndAndTweenBack (RectTransform jojoRectTransform)
    {
        yield return new WaitForSeconds(2.5f);

        LeanTween.value(105f, -350f, 1f)
            .setEase(LeanTweenType.easeInOutQuad)
            .setOnUpdate((float yPos) =>
            {
                jojoRectTransform.anchoredPosition = new Vector2(jojoRectTransform.anchoredPosition.x, yPos);
            });
    }


    private void StartBirdTweenSequence(string animationTrigger, AudioClip audioClip, string subtitleTextContent)
    {
        RectTransform parrotRectTransform = parrot.GetComponent<RectTransform>();
        if (parrotRectTransform == null) return;

        parrotRectTransform.anchoredPosition = new Vector2(-1300, 21f);

        LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            parrotRectTransform.anchoredPosition = new Vector2(x, 21f);
        }).setOnComplete(() =>
        {
            parrotAnimator.SetTrigger(animationTrigger);
            audioManager.PlayAudio(audioClip); 
            subtitleManager.DisplaySubtitle(subtitleTextContent, "Kiki", audioClip);
        });
    }
    private void UpdateGlassSprite()
    {
        // Get the juice sprite name based on the fruits in the blender
        string juiceSpriteName = spriteChangeController.GetJuiceSpriteName();
        string GlassSpriteName = spriteChangeController.GetGlassSpriteName();
        Sprite newGlassSprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + juiceSpriteName);
        GlassImage.sprite = Resources.Load<Sprite>("Images/LVL 4 scene 2/" + GlassSpriteName);
        // Update Glass 1
        SpriteRenderer glass1SpriteRenderer = glass1Transform.GetComponent<SpriteRenderer>();
        if (glass1SpriteRenderer != null && newGlassSprite != null)
        {
            glass1SpriteRenderer.sprite = newGlassSprite;            
        }
        else
        {
            Debug.LogWarning("Glass 1 sprite or SpriteRenderer is missing.");
        }

        // Update Glass 2
        SpriteRenderer glass2SpriteRenderer = glass2Transform.GetComponent<SpriteRenderer>();
        if (glass2SpriteRenderer != null && newGlassSprite != null)
        {
            glass2SpriteRenderer.sprite = newGlassSprite;

        }
        else
        {
            Debug.LogWarning("Glass 2 sprite or SpriteRenderer is missing.");
        }

        Debug.Log($"Updated both glass sprites to {juiceSpriteName}");
    }   
}

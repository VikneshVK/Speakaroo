using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class PizzaDrag : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 startPosition;
    private Collider2D pizzacollider;
    public GameObject pizzaDropLocation;  // Drop location for the pizza
    public Transform cameraFinalPoint;    // Camera target position
    public Transform pizzaPoint1;         // First position the pizza tweens to
    public Transform pizzaPoint2;         // Second position the pizza tweens to
    public Camera mainCamera;             // Reference to the main camera
    public GameObject pizzaBox;           // Pizza box to tap
    public GameObject pizzaEatingPanel;   // UI panel for pizza eating

    public Lvl7Sc2QuestManager questManager;  // Reference to quest manager

    public Sprite sprite1;  // PizzaMade == 0
    public Sprite sprite2;  // PizzaMade == 1
    public Sprite sprite3;  // PizzaMade == 2
    public Sprite defaultSprite;  // Reset sprite
    public GameObject heat;

    public GameObject kikiImage;
    private Animator kikiAnimator;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;
    public AudioClip SfxAudio3;
    public AudioClip PizzaEndingAudio;

    public bool pizzaDropped = false;
    private bool canTapPizzaBox = false;
    public bool canTapPizzaImage = false;
    private Collider2D pizzaBoxCollider;
    private SpriteRenderer pizzasprite;
    private int toppingsDropped = 0;

    [Header("Pizza Piece Sprites")]
    public Sprite Piece1;
    public Sprite Piece5;
    public Sprite Piece9;

    public RectTransform jojoFinalImage;
    public RectTransform kikiFinalImage;
    private Animator jojoAnimator;
    private Animator kikiFinalAnimator;

    public AudioClip OvenAudio;
    public TextMeshProUGUI subtitleText;
    public Lvl7Sc2AudioManager audioManager;
    public LVL7Sc2HelperFunction helperFunction;

    void Start()
    {
        pizzacollider = GetComponent<Collider2D>();
        pizzaBoxCollider = pizzaBox.GetComponent<Collider2D>();
        startPosition = transform.position;
        pizzasprite = GetComponent<SpriteRenderer>();
        jojoAnimator = jojoFinalImage.GetComponent<Animator>();
        kikiFinalAnimator = kikiFinalImage.GetComponent<Animator>();
        if (pizzaBoxCollider != null)
        {
            pizzaBoxCollider.enabled = false;
        }


        if (kikiImage != null)
        {
            kikiAnimator = kikiImage.GetComponent<Animator>();
            if (kikiAnimator == null)
            {
                Debug.LogError("Kiki image is missing an Animator component.");
            }
        }
        else
        {
            Debug.LogError("Kiki image is not assigned.");
        }

        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0;
            transform.position = mousePosition;
        }

        if (canTapPizzaBox && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

            if (hit.collider != null && hit.collider.gameObject == pizzaBox)
            {
                TapPizzaBox();
            }
        }
    }

    void OnMouseDown()
    {
        if (!pizzaDropped)
        {
            isDragging = true;
            helperFunction.ResetTimer();
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        if (IsDroppedOnTarget())
        {
            LeanTween.move(gameObject, pizzaDropLocation.transform.position, 0.5f);
            pizzaDropped = true;
            pizzacollider.enabled = false;
            PanCameraAndMovePizza();
        }
        else
        {
            transform.position = startPosition;
        }
    }

    private bool IsDroppedOnTarget()
    {
        Collider2D dropLocationCollider = pizzaDropLocation.GetComponent<Collider2D>();
        return pizzacollider.bounds.Intersects(dropLocationCollider.bounds);
    }

    private void PanCameraAndMovePizza()
    {
        LeanTween.moveX(mainCamera.gameObject, cameraFinalPoint.position.x, 1.0f).setOnComplete(() =>
        {
            TweenPizzaToPoints();
        });
    }

    private void TweenPizzaToPoints()
    {
        LeanTween.move(gameObject, pizzaPoint1.position, 0.7f).setOnComplete(() =>
        {
            if (pizzaBoxCollider != null)
            {
                pizzaBoxCollider.enabled = true;
            }
            LeanTween.delayedCall(0.3f, () =>
            {
                heat.GetComponent<SpriteRenderer>().enabled = true;
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.loop = false;
                    SfxAudioSource.clip = SfxAudio3;
                    SfxAudioSource.Play();
                }

                LeanTween.delayedCall(3.0f, () =>
                {
                    heat.GetComponent<SpriteRenderer>().enabled = false;
                    SfxAudioSource.Stop();
                    UpdatePizzaSprite();

                    LeanTween.delayedCall(0.3f, () =>
                    {
                        if (SfxAudioSource != null)
                        {
                            SfxAudioSource.loop = false;
                            SfxAudioSource.PlayOneShot(SfxAudio1);
                        }

                        LeanTween.move(gameObject, pizzaPoint2.position, 0.7f).setOnComplete(() =>
                        {
                            canTapPizzaBox = true;
                        });
                    });
                });
            });
        });
    }

    private void TapPizzaBox()
    {
        canTapPizzaBox = false;

        
        LeanTween.scale(pizzaEatingPanel, Vector3.one, 0.4f).setEase(LeanTweenType.easeOutBounce).setOnComplete(() =>
        {
            LeanTween.scale(kikiFinalImage, Vector3.one * 1.5f, 0.4f);
            LeanTween.scale(jojoFinalImage, Vector3.one * 2.5f, 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
            {
                if (questManager == null)
                {
                    Debug.LogError("QuestManager is not assigned.");
                    return;
                }

                int pizzaIndex = questManager.PizzaMade;
                if (pizzaIndex < 0 || pizzaIndex > 2)
                {
                    Debug.LogError($"Invalid PizzaMade value ({pizzaIndex}).");
                    return;
                }

                // Deactivate only the children of pizzaEatingPanel with "Image" in their name
                for (int i = 0; i < pizzaEatingPanel.transform.childCount; i++)
                {
                    Transform child = pizzaEatingPanel.transform.GetChild(i);
                    if (child.name.Contains("Image"))
                    {
                        child.gameObject.SetActive(false);
                    }
                }

                // Enable the correct child based on PizzaMade by name
                string targetChildName = $"Image {pizzaIndex + 1}";
                Transform targetChild = pizzaEatingPanel.transform.Find(targetChildName);
                if (targetChild == null)
                {
                    Debug.LogError($"Target child with name '{targetChildName}' not found.");
                    return;
                }
                targetChild.gameObject.SetActive(true);

                // Assign the correct sprite to the enabled child
                Image childImage = targetChild.GetComponent<Image>();
                if (childImage == null)
                {
                    Debug.LogError("Target child is missing an Image component.");
                    return;
                }

                switch (pizzaIndex)
                {
                    case 0: childImage.sprite = Piece1; break;
                    case 1: childImage.sprite = Piece5; break;
                    case 2: childImage.sprite = Piece9; break;
                    default: Debug.LogError("Invalid PizzaMade value."); return;
                }

                // Tween the active child's scale to 1
                LeanTween.scale(targetChild.gameObject, Vector3.one * 2.5f, 0.4f).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() =>
                {
                    StartTapSequence(targetChild);
                });
            });
            
        });
    }



    private void StartTapSequence(Transform targetChild)
    {
        if (targetChild == null)
        {
            Debug.LogError("Target child is null. Cannot start tap sequence.");
            return;
        }

        Animator childAnimator = targetChild.GetComponent<Animator>();
        if (childAnimator == null)
        {
            Debug.LogError("Child is missing an Animator component.");
            return;
        }

        // Ensure the PizzaImageClickHandler is assigned to the child
        PizzaImageClickHandler clickHandler = targetChild.GetComponent<PizzaImageClickHandler>();
        if (clickHandler == null)
        {
            Debug.LogError("Target child is missing the PizzaImageClickHandler script.");
            return;
        }

        // Assign reference to PizzaDrag in PizzaImageClickHandler
        clickHandler.pizzaDrag = this;

        int currentTapCount = 0;
        canTapPizzaImage = true;

        void OnChildTapped()
        {
            if (!canTapPizzaImage || currentTapCount >= 4) return;

            string triggerName = $"tap{currentTapCount + 1}";
            if (string.IsNullOrEmpty(triggerName))
            {
                Debug.LogError("Animation trigger name is null or empty.");
                return;
            }

            childAnimator.SetTrigger(triggerName);
            canTapPizzaImage = false;
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.PlayOneShot(SfxAudio2);
            }
            // Wait for the animation length based on the state name
            StartCoroutine(WaitForAnimationAndProceed(childAnimator, triggerName, () =>
            {
                currentTapCount++;
                if (currentTapCount < 4)
                {
                    canTapPizzaImage = true;
                }
                else
                {
                    // After the last tap, complete the sequence with a delay
                    LeanTween.delayedCall(2.0f, () =>
                    {
                        CompletePizzaSequence();
                    });
                }
            }));
        }

        // Define a callback for the PizzaImageClickHandler
        clickHandler.OnPointerClickCallback = OnChildTapped;
    }




    private IEnumerator WaitForAnimationAndProceed(Animator animator, string stateName, System.Action onComplete)
    {
        if (animator == null)
        {
            Debug.LogError("Animator is null.");
            yield break;
        }

        // Wait until the animator enters the specified state
        while (!animator.GetCurrentAnimatorStateInfo(0).IsName(stateName))
        {
            yield return null; // Wait for the next frame
        }

        // Get the animation length from the current state
        float animationLength = animator.GetCurrentAnimatorStateInfo(0).length;

        // Wait for the animation to complete
        yield return new WaitForSeconds(animationLength);

        // Invoke the callback after the animation finishes
        onComplete?.Invoke();
    }


    public void CompletePizzaSequence()
    {
        mainCamera.transform.position = new Vector3(0, 0, mainCamera.transform.position.z);
        transform.position = startPosition;
        pizzasprite.sprite = defaultSprite;        
        questManager.MakePizza();

    }
   /* private void PizzaEndSequence()
    {
        // Tween the final images into the viewport
        LeanTween.move(kikiFinalImage, new Vector2(132, 130), 1.0f).setEase(LeanTweenType.easeOutQuad);
        LeanTween.move(jojoFinalImage, new Vector2(-256, 56), 1.0f).setEase(LeanTweenType.easeOutQuad).setOnComplete(() =>
        {
            // Trigger the level end animations
            if (kikiFinalAnimator != null)
                kikiFinalAnimator.SetTrigger("LevelEnd");

            if (jojoAnimator != null)
                jojoAnimator.SetTrigger("LevelEnd");

            audioManager.PlayAudio(PizzaEndingAudio);

            // Wait for the animations to complete before tweening them back out
            StartCoroutine(WaitForLevelEndAnimationAndReset());
        });
    }

    private IEnumerator WaitForLevelEndAnimationAndReset()
    {
        // Wait for the longest animation length (example: assuming 2 seconds here)
        yield return new WaitForSeconds(2.0f);

        // Tween the final images back to their original positions
        LeanTween.move(kikiFinalImage, new Vector2(132, -130), 1.0f).setEase(LeanTweenType.easeInQuad);
        LeanTween.move(jojoFinalImage, new Vector2(-256, -400), 1.0f).setEase(LeanTweenType.easeInQuad).setOnComplete(() =>
        {
           
        });
    }*/

    public void EnablePizzaCollider()
    {
        if (kikiAnimator != null)
        {
            // Trigger the "intoOven" animation
            kikiAnimator.SetTrigger("intoOven");
            audioManager.PlayAudio(OvenAudio);
            StartCoroutine(RevealTextWordByWord("Let's put it in the Oven", 0.5f));
            // Start a coroutine to wait until the "intoOven" animation state is complete
            StartCoroutine(WaitForAnimationAndEnableCollider());
        }
        else
        {
            Debug.LogError("Kiki Animator is not assigned.");            
        }
    }

    private IEnumerator WaitForAnimationAndEnableCollider()
    {
        if (kikiAnimator == null)
        {
            Debug.LogError("Kiki Animator is null.");
            yield break;
        }
       
        yield return new WaitForSeconds(2f);

        // Enable the pizza collider
        if (pizzacollider != null)
        {
            pizzacollider.enabled = true;
            Debug.Log("Pizza collider enabled after Kiki animation.");
            helperFunction.ResetTimer();
            helperFunction.StartTimer(gameObject.transform.position, pizzaDropLocation.transform.position);
        }
       
    }


    public void OnPizzaImageTapped(GameObject tappedImage)
    {
        // Ensure the tapped image is part of the pizza eating panel
        int pizzaIndex = questManager.PizzaMade;
        Transform targetChild = pizzaEatingPanel.transform.GetChild(pizzaIndex);

        if (targetChild == null || tappedImage != targetChild.gameObject)
        {
            Debug.LogWarning("Tapped object is not the target pizza image.");
            return;
        }

        // Start tap sequence for the tapped child
        StartTapSequence(targetChild);
    }



    private void UpdatePizzaSprite()
    {
        if (questManager != null)
        {
            switch (questManager.PizzaMade)
            {
                case 0:
                    pizzasprite.sprite = sprite1;
                    break;
                case 1:
                    pizzasprite.sprite = sprite2;
                    break;
                case 2:
                    pizzasprite.sprite = sprite3;
                    break;
                default:
                    Debug.LogWarning("Invalid PizzaMade value in Quest Manager.");
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Quest Manager is not assigned.");
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

using UnityEngine;
using System.Collections;
using TMPro;
using System.Collections.Generic;

public class DraggingController1 : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging;
    private SpriteRenderer spriteRenderer;
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float initialZ;

    public static bool isMilkDropped;
    public static bool isCerealDropped;
    public static bool isCherryDropped;

    private Sprite newSprite;
    private Sprite milkSprite;
    private Sprite cerealSprite;
    private Sprite milkAndCerealSprite;
    private Sprite finalBowlSprite;

    private BoxCollider2D cherryCollider;

    private GameObject bird;
    private Animator birdAnimator;
    private Vector3 birdInitialPosition;
    public Transform birdEndPosition;

    public Transform cerealInitialPosition;
    public Transform milkInitialPosition;
    public Transform cherryInitialPosition;

    public Transform pouringPosition1; // New reference for Cereal
    public Transform pouringPosition2; // New reference for Milk
    public Transform pouringPosition3; // New reference for Cherry

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;
    public AudioClip SfxAudio2;
    public AudioClip SfxAudio3;

    public LVL4Sc2AudioManager audioManager;
    public AudioClip Audio1;
    public AudioClip Audio2;
    public AudioClip Audio3;
    public AudioClip Audio4;
    public AudioClip Audio5;

    public SubtitleManager subtitleManager;
    private string subtitle1 = "Lets make Cereal for Breakfast";
    private string subtitle2 = "Put the Cereal in the Bowl";
    private string subtitle3 = "Pour the Milk in the Bowl";
    private string subtitle4 = "Now Add some Berries on the Top";
    private string subtitle5 = "mmm Tasty..!";

    private BoxCollider2D cerealCollider;
    private BoxCollider2D milkCollider;

    public GameObject tweenManager;
    private TweeningController tweeningController;
    private bool startSecondTime;
    private bool finalBowlDisabled;

    public GameObject maskPrefab;
    public GameObject finalBowlSpriteObject;    

    public LVL4Sc2HelperController helperHandController;

    private Animator objectAnimator;
    private bool isCerealSequenceStarted;
    private bool isMilkSequenceStarted;
    private bool isCherrySequenceStarted;
    /*private bool isTweenBackInProgress = false;*/
    private List<GameObject> spawnedMasks = new List<GameObject>();



    private void Start()
    {
        isDragging = false;
        isMilkDropped = false;
        isCerealDropped = false;
        isCherryDropped = false;
        isCerealSequenceStarted = false;
        isMilkSequenceStarted = false;
        isCherrySequenceStarted = false;
        startSecondTime = false;
        finalBowlDisabled = false;


        spriteRenderer = GetComponent<SpriteRenderer>();
        initialPosition = transform.position;
        initialRotation = transform.rotation;
        initialZ = transform.position.z;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        objectAnimator = GetComponent<Animator>();

        if (gameObject.CompareTag("Milk"))
        {
            newSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/milk-Open");
        }
        else if (gameObject.CompareTag("Cereal"))
        {
            newSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/cereal-Opened");
        }

        milkSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/c-Bowl");
        cerealSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/c-Bowl");
        milkAndCerealSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/mc-Bowl");
        finalBowlSprite = Resources.Load<Sprite>("Images/Lvl 4 Scene 2/mcc-Bowl");

        GameObject cherry = GameObject.FindGameObjectWithTag("Cherry");
        if (cherry != null)
        {
            cherryCollider = cherry.GetComponent<BoxCollider2D>();
            if (cherryCollider != null)
            {
                cherryCollider.enabled = false;
            }
        }

        bird = GameObject.FindGameObjectWithTag("Bird");
        if (bird != null)
        {
            birdAnimator = bird.GetComponent<Animator>();
            birdInitialPosition = bird.transform.position;
        }

        cerealCollider = GameObject.FindGameObjectWithTag("Cereal")?.GetComponent<BoxCollider2D>();
        milkCollider = GameObject.FindGameObjectWithTag("Milk")?.GetComponent<BoxCollider2D>();

        if (cerealCollider != null) cerealCollider.enabled = false;
        if (milkCollider != null) milkCollider.enabled = false;

        if (tweenManager != null)
        {
            tweeningController = tweenManager.GetComponent<TweeningController>();
        }

        StartBirdTweenSequence("Intro", Audio1, subtitle1);

        
    }
    private void OnMouseDown()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        offset = transform.position - mousePosition;
        offset.z = 0;
        isDragging = true;

        // Reset the centralized timer when interaction begins
        helperHandController.StopAndResetTimer();
    }



    private void OnMouseDrag()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            Vector3 targetPosition = mousePosition + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 10f);
            Collider2D bowlCollider = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<Collider2D>();
            if (bowlCollider != null && bowlCollider.bounds.Contains(transform.position))
            {
                if (gameObject.CompareTag("Milk"))
                {
                    spriteRenderer.sprite = newSprite;
                    transform.rotation = Quaternion.Euler(0, 0, -100);
                }
                else if (gameObject.CompareTag("Cereal"))
                {
                    spriteRenderer.sprite = newSprite;
                    transform.rotation = Quaternion.Euler(0, 0, 100);
                }
            }
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
        Collider2D bowlCollider = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<Collider2D>();
        transform.rotation = Quaternion.identity;

        if (bowlCollider != null && bowlCollider.bounds.Contains(transform.position))
        {            

            if (gameObject.CompareTag("Cereal"))
            {
                transform.position = pouringPosition1.position; // Move to Cereal pouring position
                if (objectAnimator != null)
                {
                    SfxAudioSource.PlayOneShot(SfxAudio2);
                    objectAnimator.SetTrigger("isPouring");
                }
            }
            else if (gameObject.CompareTag("Milk"))
            {
                transform.position = pouringPosition2.position; // Move to Milk pouring position
                if (objectAnimator != null)
                {
                    SfxAudioSource.PlayOneShot(SfxAudio3);
                    objectAnimator.SetTrigger("isPouring");
                }
            }
            else if (gameObject.CompareTag("Cherry"))
            {
                transform.position = pouringPosition3.position; // Move to Cherry pouring position
                if (objectAnimator != null)
                {
                    SfxAudioSource.PlayOneShot(SfxAudio1);
                    objectAnimator.SetTrigger("isPouring");
                }
            }
        }
        else
        {
            ResetPositionAndRotation();
        }
    }

    private void ResetPositionAndRotation()
    {
        if (gameObject.CompareTag("Milk") && milkInitialPosition != null)
        {
            transform.position = milkInitialPosition.position;
            StartBirdTweenSequence("Milk", Audio3, subtitle3);
            helperHandController.SpawnGlowEffect(milkCollider.gameObject);
        }
        else if (gameObject.CompareTag("Cereal") && cerealInitialPosition != null)
        {
            transform.position = cerealInitialPosition.position;
            StartBirdTweenSequence("Cereal", Audio2, subtitle2);
            helperHandController.SpawnGlowEffect(cerealCollider.gameObject);
        }
        else if (gameObject.CompareTag("Cherry") && cherryInitialPosition != null)
        {
            transform.position = cherryInitialPosition.position;
            StartBirdTweenSequence("Cherry", Audio4, subtitle4);
            helperHandController.SpawnGlowEffect(cherryCollider.gameObject);
        }
        transform.rotation = initialRotation;
        Debug.Log("Position Reset");
    }

    private void ResetPositionAndChangeSprite(string tag)
    {
        SpriteRenderer bowlSpriteRenderer = GameObject.FindGameObjectWithTag("EmptyBowl")?.GetComponent<SpriteRenderer>();

        if (tag == "Cereal")
        {
            isCerealDropped = true;
            if (bowlSpriteRenderer != null)
                bowlSpriteRenderer.sprite = cerealSprite;

            transform.position = cerealInitialPosition.position;
            cerealCollider.enabled = false;
            Debug.Log("Cereal Position Reset and Sprite Changed");
        }
        else if (tag == "Milk")
        {
            isMilkDropped = true;
            if (bowlSpriteRenderer != null)
                bowlSpriteRenderer.sprite = milkAndCerealSprite;

            transform.position = milkInitialPosition.position;
            milkCollider.enabled = false;
            Debug.Log("Milk Position Reset and Sprite Changed");
        }
        else if (tag == "Cherry")
        {
            if (isMilkDropped && isCerealDropped)
            {
                isCherryDropped = true;
                if (bowlSpriteRenderer != null)
                    bowlSpriteRenderer.sprite = finalBowlSprite;
                StartCoroutine(ExecuteBirdTweenAndReveal());
            }

            transform.position = cherryInitialPosition.position;
            cherryCollider.enabled = false;
            Debug.Log("Cherry Position Reset and Sprite Changed");
        }

        transform.rotation = initialRotation;

        // Reset the centralized timer whenever the bowl sprite changes
        helperHandController.StopAndResetTimer();

        
    }

    private void CerealAnimationComplete()
    {
        ResetPositionAndChangeSprite("Cereal");
        if (isCerealDropped)
        {
            StartBirdTweenSequence("Milk", Audio3, subtitle3);
            helperHandController.SpawnGlowEffect(milkCollider.gameObject);
        }
        
    }

    private void MilkAnimationComplete()
    {
        ResetPositionAndChangeSprite("Milk");
        if (isMilkDropped) 
        {
            StartBirdTweenSequence("Cherry", Audio4, subtitle4);
            helperHandController.SpawnGlowEffect(cherryCollider.gameObject);
        }        
    }

    private void CherryAnimationComplete()
    {
        ResetPositionAndChangeSprite("Cherry");
        /* StartBirdTweenSequence("Cherry");*/
    }

    private IEnumerator ExecuteBirdTweenAndReveal()
    {
        /*yield return StartCoroutine(WaitAndTweenBack());*/
        yield return StartCoroutine(SpawnMasksAndReveal());
    }

    private IEnumerator SpawnMasksAndReveal()
    {
        float elapsedTime = 0f;
        float maskInterval = 1f;

        while (elapsedTime < 3f)
        {
            GameObject mask = InstantiateMaskWithinBounds();
            if (mask != null)
            {
                spawnedMasks.Add(mask); // Add the mask to the list
                Destroy(mask, 3f); // Destroy the mask after 3 seconds
            }
            elapsedTime += maskInterval;
            yield return new WaitForSeconds(maskInterval);
        }

        if (finalBowlSpriteObject != null)
        {
            finalBowlSpriteObject.GetComponent<SpriteRenderer>().enabled = false;
            finalBowlDisabled = true;
        }
        startSecondTime = true;
        StartBirdTweenSequence("Tasty", Audio5, subtitle5);
    }

    private GameObject InstantiateMaskWithinBounds()
    {
        if (finalBowlSpriteObject == null)
            return null;

        Collider2D bowlCollider = finalBowlSpriteObject.GetComponent<Collider2D>();
        if (bowlCollider == null)
            return null;

        Bounds bounds = bowlCollider.bounds;
        Vector2 randomPosition;
        bool positionValid;

        do
        {
            randomPosition = new Vector2(
                Random.Range(bounds.min.x, bounds.max.x),
                Random.Range(bounds.min.y, bounds.max.y)
            );

            positionValid = IsPositionWithinBounds(randomPosition, bowlCollider) && !IsPositionInsideMaskCollider(randomPosition);
        } while (!positionValid);

        // Instantiate the mask and return the reference
        return Instantiate(maskPrefab, randomPosition, Quaternion.identity);
    }

    private bool IsPositionWithinBounds(Vector2 position, Collider2D collider)
    {
        return collider.bounds.Contains(position);
    }

    private bool IsPositionInsideMaskCollider(Vector2 position)
    {
        CircleCollider2D maskCollider = maskPrefab.GetComponent<CircleCollider2D>();
        if (maskCollider == null) return false;

        Vector2 maskCenter = maskCollider.transform.position;
        float radius = maskCollider.radius * maskCollider.transform.lossyScale.x;

        return Vector2.Distance(position, maskCenter) <= radius;
    }

    private void StartBirdTweenSequence(string animationTrigger, AudioClip audioClip, string subtitleTextContent)
    {
        RectTransform birdRectTransform = bird.GetComponent<RectTransform>();
        if (birdRectTransform == null || birdEndPosition == null) return;

        // Set initial position (off-screen, anchored position)
        birdRectTransform.anchoredPosition = new Vector2(-1300, 21);

        // Tween bird to the end position
        LeanTween.value(-1300f, -790f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 21f);
        }).setOnComplete(() =>
        {
            birdAnimator.SetTrigger(animationTrigger);
            audioManager.PlayAudio(audioClip);

            // Start subtitle display coroutine
            subtitleManager.DisplaySubtitle(subtitleTextContent, "Kiki", audioClip);

            StartCoroutine(WaitAndTweenBack(birdRectTransform));
        });
    }

    private IEnumerator WaitAndTweenBack(RectTransform birdRectTransform)
    {
        yield return new WaitForSeconds(3f);

        // Tween bird back to the initial position (off-screen)
        LeanTween.value(-790f, -1300f, 1f).setEase(LeanTweenType.easeInOutQuad).setOnUpdate((float x) =>
        {
            birdRectTransform.anchoredPosition = new Vector2(x, 21f);
        }).setOnComplete(() =>
        {
            // Enable colliders and handle state transitions
            if (!isCerealDropped && cerealCollider != null && !isCerealSequenceStarted)
            {
                StartCoroutine(EnableCerealWithDelay());
            }

            if (!isMilkDropped && milkCollider != null && isCerealDropped)
            {
                StartCoroutine(EnableMilkWithDelay());


            }
            else if (!isCherryDropped && cherryCollider != null && isMilkDropped)
            {
                StartCoroutine(EnableCherryWithDelay());

            }

            if (startSecondTime && tweeningController != null)
            {
                tweeningController.isSecondTime = true;
            }
        });

        yield return new WaitForSeconds(1f);
    }

    private IEnumerator EnableCerealWithDelay()
    {
        StartBirdTweenSequence("Cereal", Audio2, subtitle2);
        helperHandController.SpawnGlowEffect(cerealCollider.gameObject);
        yield return new WaitForSeconds(3f);
        cerealCollider.enabled = true;
        helperHandController.StartDelayTimer();
        isCerealSequenceStarted = true;

    }

    private IEnumerator EnableMilkWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        helperHandController.StartDelayTimer();
        milkCollider.enabled = true;
    }

    private IEnumerator EnableCherryWithDelay()
    {
        yield return new WaitForSeconds(0.5f);
        helperHandController.StartDelayTimer();
        cherryCollider.enabled = true;
    }
}

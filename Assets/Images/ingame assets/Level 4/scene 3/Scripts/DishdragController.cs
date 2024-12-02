using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DishdragController : MonoBehaviour
{
    public static List<DishdragController> allDishControllers = new List<DishdragController>();
    public Transform dropTarget;
    public LVL4Sc3HelperHand helperHandManager;
    public float helperHandDelay = 5f;
    public bool isDroppedCorrectly = false;
    public AudioSource audio1;
    public TextMeshProUGUI subtitleText;

    public Sprite newSprite;
    public Animator birdAnimator;

    public Transform glassDropTarget1; // Drop location for dirty-glass1 and dirty-glass2
    public Transform glassDropTarget2;
    public Transform plateDropTarget1; // Drop location for dirty-plate1 and dirty-plate2
    public Transform plateDropTarget2;
    public Transform bowlDropTarget1; // Drop location for dirty-bowl1 and dirty-bowl2
    public Transform bowlDropTarget2;

    public static int dishesArranged = 0;

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private Vector3 startPosition;
    private bool isDragging = false;
    private Coroutine helperHandCoroutine;

    private void Start()
    {
        startPosition = transform.position;
        allDishControllers.Add(this);
        GetComponent<Collider2D>().enabled = false;
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            OnMouseDown();
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            OnMouseDrag();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            OnMouseUp();
        }
    }

    private void OnMouseDown()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);
        if (hitCollider != null && hitCollider.gameObject == gameObject)
        {
            isDragging = true;

            // Change the sorting order to 5 when dragging starts
            GetComponent<SpriteRenderer>().sortingOrder = 5;

            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }
        }
    }

    private void OnMouseDrag()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        transform.position = new Vector3(mousePosition.x, mousePosition.y, transform.position.z);
    }

    private void OnMouseUp()
    {
        isDragging = false;

        // Reset the sorting order when dragging stops
        GetComponent<SpriteRenderer>().sortingOrder = 2;

        if (gameObject.name.Contains("Bowl"))
        {
            // Check if dropped near any bowl drop location
            if (Vector3.Distance(transform.position, bowlDropTarget1.position) < 1.0f ||
                Vector3.Distance(transform.position, bowlDropTarget2.position) < 1.0f)
            {
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.loop = false;
                    SfxAudioSource.PlayOneShot(SfxAudio1);
                }
                OnDropped(true, isBowl: true);
            }
            else
            {
                ResetPosition();
                OnDropped(false);
            }
        }
        else if (gameObject.name.Contains("glass"))
        {
            // Check if dropped near any glass drop location
            if (Vector3.Distance(transform.position, glassDropTarget1.position) < 1.0f ||
                Vector3.Distance(transform.position, glassDropTarget2.position) < 1.0f)
            {
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.loop = false;
                    SfxAudioSource.PlayOneShot(SfxAudio1);
                }
                OnDropped(true, isGlass: true);
            }
            else
            {
                ResetPosition();
                OnDropped(false);
            }
        }
        else if (gameObject.name.Contains("plate"))
        {
            // Check if dropped near any plate drop location
            if (Vector3.Distance(transform.position, plateDropTarget1.position) < 1.0f ||
                Vector3.Distance(transform.position, plateDropTarget2.position) < 1.0f)
            {
                if (SfxAudioSource != null)
                {
                    SfxAudioSource.loop = false;
                    SfxAudioSource.PlayOneShot(SfxAudio1);
                }
                OnDropped(true, isPlate: true);
            }
            else
            {
                ResetPosition();
                OnDropped(false);
            }
        }
    }

    private void OnDropped(bool correctDrop, bool isGlass = false, bool isPlate = false, bool isBowl = false)
    {
        SpriteRenderer targetSpriteRenderer = null;

        if (correctDrop)
        {
            if (isBowl)
            {
                // For bowls, change the sprite and destroy the object
                targetSpriteRenderer = Vector3.Distance(transform.position, bowlDropTarget1.position) <
                                       Vector3.Distance(transform.position, bowlDropTarget2.position)
                    ? bowlDropTarget1.GetComponent<SpriteRenderer>()
                    : bowlDropTarget2.GetComponent<SpriteRenderer>();

                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite;
                }
            }
            else if (isGlass)
            {
                // For glasses, change the sprite and destroy the object
                targetSpriteRenderer = Vector3.Distance(transform.position, glassDropTarget1.position) <
                                       Vector3.Distance(transform.position, glassDropTarget2.position)
                    ? glassDropTarget1.GetComponent<SpriteRenderer>()
                    : glassDropTarget2.GetComponent<SpriteRenderer>();

                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite;
                    
                }
            }
            else if (isPlate)
            {
                // For plates, change the sprite and destroy the object
                targetSpriteRenderer = Vector3.Distance(transform.position, plateDropTarget1.position) <
                                       Vector3.Distance(transform.position, plateDropTarget2.position)
                    ? plateDropTarget1.GetComponent<SpriteRenderer>()
                    : plateDropTarget2.GetComponent<SpriteRenderer>();

                if (targetSpriteRenderer != null)
                {
                    targetSpriteRenderer.sprite = newSprite;
                    targetSpriteRenderer.gameObject.transform.localScale = Vector3.one * 0.2f;
                }
            }

            isDroppedCorrectly = true;
            gameObject.GetComponent<SpriteRenderer>().enabled = false;
            dishesArranged++;
            CheckDishesArranged();

            if (helperHandManager != null)
            {
                helperHandManager.StopHelperHand();
            }

            StartHelperHandCheckForAll();
        }
        else
        {
            StartHelperHandTimer();
        }
    }

    private void CheckDishesArranged()
    {
        if (dishesArranged == 6)
        {
            birdAnimator.SetTrigger("LvlComplete");
            audio1.Play();
            StartCoroutine(RevealTextWordByWord("WOW.! Thank You Friend the Kitchen looks So Clean", 0.5f));
            Debug.Log("Level Completed! All dishes arranged.");
        }
    }

    private void ResetPosition()
    {
        transform.position = startPosition;
    }

    // Start the helper hand delay timer
    public void StartHelperHandTimer()
    {
        if (helperHandCoroutine != null)
        {
            StopCoroutine(helperHandCoroutine);
        }
        helperHandCoroutine = StartCoroutine(HelperHandDelayTimer());
    }

    private IEnumerator HelperHandDelayTimer()
    {
        yield return new WaitForSeconds(helperHandDelay);

        if (!isDroppedCorrectly)
        {
            helperHandManager.SpawnHelperHand(transform.position, dropTarget.position);
        }
    }

    public static void StartHelperHandCheckForAll()
    {
        foreach (var dishController in allDishControllers)
        {
            if (!dishController.isDroppedCorrectly)
            {
                dishController.StartHelperHandTimer();
                break;
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

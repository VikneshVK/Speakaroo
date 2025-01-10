using TMPro;
using UnityEngine;
using System.Collections;

public class DressDragDrop : MonoBehaviour
{
    private Vector3 initialPosition;
    private bool isDragging = false;
    public string dressType; // "School", "Summer", "Winter"
    public GameObject boyCharacter;
    public GameObject summerDress;
    public GameObject winterDress;
    public GameObject schoolDress;
    public Animator boyAnimator;
    public Animator birdAnimator;
    /*public TextMeshProUGUI subtitleText;*/
    public SubtitleManager subtitleManager;
    public Lvl1Sc1AudioManager audiomanager;
    public Lvl1Sc3HelperController helperController;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    private bool hasboyTalkStarted;
    private AudioSource SfxAudioSource;
    public AudioClip sfxAudio1;
    public AudioClip sfxAudio2;
    public AudioClip sfxAudio3;
    private bool isGlowSequenceComplete;
    
    public static int incorrectDrops = 2;
    void Start()
    {
        initialPosition = transform.position;
        hasboyTalkStarted = false;
        isGlowSequenceComplete = false;
        
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the z position is 0 for 2D games
            transform.position = mousePosition;

            // Set the layer order of all children to 10
            SetChildrenSortingOrder(10);
        }
        else
        {
            // Reset the layer order of all children to default when not dragging
            SetChildrenSortingOrder(6);
        }


        if (!isGlowSequenceComplete &&
        boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle 0") &&
        boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f)
        {
            isGlowSequenceComplete = true; // Prevent repeated glow sequence
            StartCoroutine(HandleGlowSequence());
        }

        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk sample") &&
            boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f && !hasboyTalkStarted)
        {
            hasboyTalkStarted = true;
            audiomanager.PlayAudio(audio3);            
            subtitleManager.DisplaySubtitle("Lets get Dressed for School.", "JoJo", audio3);
        }
    }

    void OnMouseDown()
    {
        isDragging = true;

        SetChildrenSortingOrder(10);

        if (dressType == "School" && helperController != null)
        {
            helperController.DestroyHelperHand(); // Destroy helper hand when dragging the school dress
        }

        if (SfxAudioSource != null)
        {
            SfxAudioSource.PlayOneShot(sfxAudio1);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;

        SetChildrenSortingOrder(6);

        if (IsDroppedOnBoy())
        {
            HandleDressChange();
        }
        else
        {
            transform.position = initialPosition; // Reset to initial position if not dropped on boy
        }
    }

    private void SetChildrenSortingOrder(int order)
    {
        foreach (Transform child in transform)
        {
            SpriteRenderer spriteRenderer = child.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = order;
            }
        }
    }

    private bool IsDroppedOnBoy()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            Debug.Log("Hit object: " + hit.collider.gameObject.name);
            if (hit.collider.gameObject == boyCharacter)
            {
                Debug.Log("Collider check is good");                
                return true;
            }
        }
        Debug.Log("Collider check is not good");
        return false;
    }

    private void HandleDressChange()
    {
        if (boyAnimator != null)
        {
            switch (dressType)
            {
                case "School":
                    DisableAllDresses(); // Disable dragging and colliders on all dresses
                    boyAnimator.Play("SchoolDress");
                    EnableDisableDresses(schoolDress, summerDress, winterDress);
                    SfxAudioSource.PlayOneShot(sfxAudio2);
                    StartCoroutine(JojoDialoge());
                    break;

                case "Summer":
                    DisableAllDresses(); // Disable dragging and colliders on all dresses
                    boyAnimator.Play("red dress Sad Face Hand Movements");
                    SfxAudioSource.PlayOneShot(sfxAudio3);                                       
                    EnableDisableDresses(summerDress, schoolDress, winterDress);
                    incorrectDrops--;
                    StartCoroutine(HandleBirdInteraction());
                    break;

                case "Winter":
                    DisableAllDresses(); // Disable dragging and colliders on all dresses
                    boyAnimator.Play("Blue dress Sad Face Hand Movements");
                    SfxAudioSource.PlayOneShot(sfxAudio3);
                    EnableDisableDresses(winterDress, summerDress, schoolDress);
                    incorrectDrops--;
                    StartCoroutine(HandleBirdInteraction());
                    break;
            }
        }
        transform.position = initialPosition; // Reset position after handling the dress change
    }

    private IEnumerator JojoDialoge()
    {
        yield return new WaitForSeconds(0.1f);
        audiomanager.PlayAudio(audio2);
        subtitleManager.DisplaySubtitle("Woo Hoo! Let's go to school now.", "JoJo", audio2);        
    }

    private IEnumerator HandleBirdInteraction()
    {
        yield return new WaitForSeconds(1f); // Add 1-second delay

        birdAnimator.SetTrigger("talk"); // Trigger bird animation
        audiomanager.PlayAudio(audio1); // Play bird's audio
       

        yield return new WaitForSeconds(0.5f); // Add 2-second delay before handling glow or helper hand

        if (incorrectDrops > 0)
        {
            if (helperController != null)
            {
                // Logic for spawning glow based on dress type
                if (dressType == "Summer")
                {
                    if (winterDress != null)
                        yield return helperController.SpawnGlow(winterDress.transform.position);

                    yield return new WaitForSeconds(0.5f);

                    if (schoolDress != null)
                        yield return helperController.SpawnGlow(schoolDress.transform.position);

                    EnableAllDressColliders();
                }
                else if (dressType == "Winter")
                {
                    if (summerDress != null)
                        yield return helperController.SpawnGlow(summerDress.transform.position);

                    yield return new WaitForSeconds(0.5f);

                    if (schoolDress != null)
                        yield return helperController.SpawnGlow(schoolDress.transform.position);

                    EnableAllDressColliders();
                }
            }
        }
        else
        {
            EnableAllDressColliders();
            // Spawn helper hand if no incorrect drops remain
            if (helperController != null && schoolDress != null)
            {
                helperController.SpawnHelperHand(schoolDress.transform.position);
            }
        }
    }


    public void EnableDisableDresses(GameObject activeDress, GameObject firstInactiveDress, GameObject secondInactiveDress)
    {
        SetChildSpriteRenderers(activeDress, false);

        SetChildSpriteRenderers(firstInactiveDress, true);

        SetChildSpriteRenderers(secondInactiveDress, true);
    }

    private void SetChildSpriteRenderers(GameObject parent, bool enable)
    {
        SpriteRenderer[] renderers = parent.GetComponentsInChildren<SpriteRenderer>();

        foreach (var renderer in renderers)
        {
            renderer.enabled = enable;
        }
    }



    private void DisableAllDresses()
    {
        DressDragDrop[] allDresses = FindObjectsOfType<DressDragDrop>();
        foreach (var dress in allDresses)
        {
            Collider2D collider = dress.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false; // Disable collider
            }
        }
    }

    private IEnumerator HandleGlowSequence()
    {
        if (helperController != null && winterDress != null)
        {
            yield return helperController.SpawnGlow(winterDress.transform.position);
        }

        if (helperController != null && summerDress != null)
        {
            yield return helperController.SpawnGlow(summerDress.transform.position);
        }

        if (helperController != null && schoolDress != null)
        {
            yield return helperController.SpawnGlow(schoolDress.transform.position);
        }

        EnableAllDressColliders();
    }

    private void EnableAllDressColliders()
    {
       
            DressDragDrop[] allDresses = FindObjectsOfType<DressDragDrop>();
            foreach (var dress in allDresses)
            {
                Collider2D collider = dress.GetComponent<Collider2D>();
                if (collider != null)
                {
                    collider.enabled = true;
                }
            }
        
        
    }


    
}

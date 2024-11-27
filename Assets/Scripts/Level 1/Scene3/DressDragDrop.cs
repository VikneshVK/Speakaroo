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
    public TextMeshProUGUI subtitleText;
    public Lvl1Sc1AudioManager audiomanager;
    public AudioClip audio1;
    public AudioClip audio2;
    public AudioClip audio3;
    private bool hasboyTalkStarted;
    void Start()
    {
        initialPosition = transform.position;
        hasboyTalkStarted = false;
    }

    void Update()
    {
        if (isDragging)
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0f; // Ensure the z position is 0 for 2D games
            transform.position = mousePosition;
        }
        Collider2D collider = GetComponent<Collider2D>();
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle 0") &&
           boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1.0f)
        {
            collider.enabled = true;
        }
        if (boyAnimator.GetCurrentAnimatorStateInfo(0).IsName("Talk sample") &&
           boyAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 0.1f && !hasboyTalkStarted)
        {
            hasboyTalkStarted = true;
            audiomanager.PlayAudio(audio3);
            StartCoroutine(RevealTextWordByWord("Lets get Dressed for School", 0.5f));
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
    }

    void OnMouseUp()
    {
        isDragging = false;

        if (IsDroppedOnBoy())
        {
            HandleDressChange();
        }
        else
        {
            transform.position = initialPosition; // Reset to initial position if not dropped on boy
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
                    boyAnimator.Play("SchoolDress");
                    EnableDisableDresses(schoolDress, summerDress, winterDress);
                    audiomanager.PlayAudio(audio2);
                    StartCoroutine(RevealTextWordByWord("Woo Hoo..! Let's go to School Now", 0.5f));
                    DisableAllDresses(); // Disable dragging and colliders on all dresses
                    break;

                case "Summer":
                    boyAnimator.Play("red dress Sad Face Hand Movements");
                    EnableDisableDresses(summerDress, schoolDress, winterDress);
                    birdAnimator.SetTrigger("talk");
                    audiomanager.PlayAudio(audio1);
                    StartCoroutine(RevealTextWordByWord("That's not the School Outfit, Jojo", 0.5f));
                    break;

                case "Winter":
                    boyAnimator.Play("Blue dress Sad Face Hand Movements");
                    EnableDisableDresses(winterDress, summerDress, schoolDress);
                    birdAnimator.SetTrigger("talk");
                    audiomanager.PlayAudio(audio1);
                    StartCoroutine(RevealTextWordByWord("That's not the School Outfit, Jojo", 0.5f));
                    break;
            }
        }
        transform.position = initialPosition; // Reset position after handling the dress change
    }

    private void EnableDisableDresses(GameObject activeDress, GameObject firstInactiveDress, GameObject secondInactiveDress)
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
            dress.enabled = false; // Disable dragging
            Collider2D collider = dress.GetComponent<Collider2D>();
            if (collider != null)
            {
                collider.enabled = false; // Disable collider
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

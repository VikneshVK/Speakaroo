using UnityEngine;

public class dragManager : MonoBehaviour
{
    public static int totalCorrectDrops = 0;
    public Animator birdAnimator;
    public GameObject[] gameObjects;
    public AudioClip[] audioClips;
    public AudioSource audioSource;
    public string[] triggerNames = { "bagTalk", "sheetTalk", "rugTalk", "pillowTalk", "shoeTalk", "teddyTalk" };
    public string allDoneBool = "allDone";

    private GameObject parrot;

    void Start()
    {
        parrot = GameObject.FindGameObjectWithTag("Bird");
        InitializeGameObjects();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectParrotClick();
        }
    }

    void InitializeGameObjects()
    {
        foreach (GameObject obj in gameObjects)
        {
            obj.GetComponent<Collider2D>().enabled = false;
            obj.GetComponent<DragHandler>().dragManager = this;
        }
        if (gameObjects.Length > 0)
        {
            gameObjects[0].GetComponent<Collider2D>().enabled = true;
        }

        // Ensure no triggers are active at the start
        birdAnimator.ResetTrigger("bagTalk");
        birdAnimator.ResetTrigger("sheetTalk");
        birdAnimator.ResetTrigger("rugTalk");
        birdAnimator.ResetTrigger("pillowTalk");
        birdAnimator.ResetTrigger("shoeTalk");
        birdAnimator.ResetTrigger("teddyTalk");
        birdAnimator.SetBool(allDoneBool, false);
    }

    public void OnItemDropped()
    {
        totalCorrectDrops++;
        if (totalCorrectDrops < gameObjects.Length)
        {
            AnimateGameObject(gameObjects[totalCorrectDrops]);
            birdAnimator.SetTrigger(triggerNames[totalCorrectDrops]);
            PlayAudio(totalCorrectDrops);
        }
        else
        {
            birdAnimator.SetBool(allDoneBool, true);
        }
    }

    public void OnParrotClicked()
    {
        if (birdAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle0"))
        {
            if (totalCorrectDrops < triggerNames.Length)
            {
                AnimateGameObject(gameObjects[totalCorrectDrops]);
                birdAnimator.SetTrigger(triggerNames[totalCorrectDrops]);
                PlayAudio(totalCorrectDrops);
            }
        }
    }

    void DetectParrotClick()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D collider = Physics2D.OverlapPoint(mousePosition);

        if (collider != null && collider.gameObject == parrot)
        {
            OnParrotClicked();
        }
    }

    public void OnTriggerActivated(string triggerName)
    {
        int index = System.Array.IndexOf(triggerNames, triggerName);
        if (index >= 0 && index < gameObjects.Length)
        {
            AnimateGameObject(gameObjects[index]);
            PlayAudio(index);
        }
    }

    void AnimateGameObject(GameObject obj)
    {
        Vector3 originalScale = obj.transform.localScale;
        Vector3 targetScale = originalScale + new Vector3(0.35f, 0.35f, 0.35f);
        LeanTween.scale(obj, targetScale, 0.5f).setEaseInOutQuad().setOnComplete(() =>
        {
            LeanTween.scale(obj, originalScale, 0.5f).setEaseInOutQuad();
        });
    }

    void PlayAudio(int index)
    {
        if (index < audioClips.Length)
        {
            audioSource.clip = audioClips[index];
            audioSource.Play();
        }
    }
}

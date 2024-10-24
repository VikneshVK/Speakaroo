using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class DraggableTextHandler : MonoBehaviour
{
    public Transform dropPosition; // Set this in the inspector for the drop target
    public AudioSource audioSource; // AudioSource attached to this text
    public Image ringImage; // The ring to show progress
    public GameObject panel; // Reference to the panel containing buttons and texts
    public GameObject prefabToSpawn; // Reference to the prefab to spawn after panel scales down

    public AudioSource buttonAudioSource; // AudioSource attached to the button
    public GameObject childTextObject; // The TextMeshPro child object to enable and scale
    public Button retryButton;
    public Canvas canvas; // Reference to the canvas
    public TextMeshProUGUI tmpText; // Reference to the TMP component
    public TextMeshProUGUI textComponent;
    public Image imageComponent; // Reference to the Image component to change sprite
    public string spriteName;

    private Vector3 initialPosition;
    private bool isDropped = false;
    private bool isBeingDragged = false;
   /* private bool isScaled = false; */// Set to true only when scaling is complete

    private string recordedClipName = "RecordedAudio";


    private void Start()
    {
        // Initialize positions and set the child text object
        initialPosition = transform.position;

        /*if (childTextObject != null)
        {
            childTextObject.SetActive(false);
            childTextObject.transform.localScale = Vector3.zero; // Set initial scale to zero
        }*/

        // Set the ring's fill amount to zero initially
        if (ringImage != null) ringImage.fillAmount = 0;
    }

    private void Update()
    {
        if (!isDropped && isBeingDragged)
        {
            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                Input.mousePosition,
                canvas.worldCamera,
                out Vector3 worldPoint
            );
            transform.position = worldPoint;
        }
    }

    public void OnMouseDown()
    {
        // If the text is not dropped and has not been scaled yet, trigger scaling
        if (!isDropped)
        {
            
            buttonAudioSource.Play();
            isBeingDragged = true;
            tmpText.enabled = false;

        }
    }



    public void OnMouseUp()
    {        
        isBeingDragged = false;

        Vector2 screenDropPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, dropPosition.position);
        Vector2 screenObjectPosition = RectTransformUtility.WorldToScreenPoint(canvas.worldCamera, transform.position);

        float distance = Vector2.Distance(screenObjectPosition, screenDropPosition);

        if (distance < 100f) // Adjust threshold as needed
        {
            isDropped = true;

            RectTransformUtility.ScreenPointToWorldPointInRectangle(
                canvas.transform as RectTransform,
                screenDropPosition,
                canvas.worldCamera,
                out Vector3 worldPoint
            );

            transform.position = worldPoint;            
            Sprite newSprite = Resources.Load<Sprite>($"Images/LVL6 Sc1/{spriteName}");
            imageComponent.sprite = newSprite;
            textComponent.text = "Let's Play";
            audioSource.Play();           
            StartCoroutine(HandleAudioAndRecording());
        }
        else
        {
            // Return to initial position if not dropped correctly
            transform.position = initialPosition;
            tmpText.enabled = true;
        }
    }


    private IEnumerator HandleAudioAndRecording()
    {
        
        retryButton.gameObject.SetActive(true);
        retryButton.interactable = false;

        
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/DefaultSprite");

        yield return new WaitForSeconds(audioSource.clip.length);

        yield return StartCoroutine(StartRecording());

        yield return StartCoroutine(PlayRecordedAudio());

        retryButton.interactable = true;

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        yield return new WaitForSeconds(3f);

        ScaleDownAndSpawnPrefab();
    }

    private IEnumerator StartRecording()
    {
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

        int recordingDuration = 5; 
        int frequency = 44100; 

        audioSource.clip = Microphone.Start(null, false, recordingDuration, frequency);
        Debug.Log("Recording started...");

        float timeElapsed = 0f;
        while (timeElapsed < recordingDuration)
        {
            ringImage.fillAmount = timeElapsed / recordingDuration;
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        ringImage.fillAmount = 1;

        while (Microphone.IsRecording(null))
        {
            yield return null;
        }

        Debug.Log("Recording completed.");
    }

    private IEnumerator PlayRecordedAudio()
    {
        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/PlaybackSprite");

        audioSource.Play();
        Debug.Log("Playing back recorded audio...");

        float playbackDuration = audioSource.clip.length;
        float timeElapsed = 0f;

        while (timeElapsed < playbackDuration)
        {
            ringImage.fillAmount = 1 - (timeElapsed / playbackDuration);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        ringImage.fillAmount = 0;

        Debug.Log("Playback completed.");

        yield return new WaitForSeconds(1f);

        retryButton.GetComponent<Image>().sprite = Resources.Load<Sprite>("Images/STMechanics/RetrySprite");

    }

    private void ScaleDownAndSpawnPrefab()
    {
        if (panel == null || prefabToSpawn == null)
        {
            Debug.LogWarning("Panel or Prefab to spawn is not assigned.");
            return;
        }

        LeanTween.scale(panel, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInBack)
            .setOnComplete(() =>
            {
                GameObject spawnedPrefab = Instantiate(prefabToSpawn, Vector3.zero, Quaternion.identity);

                spawnedPrefab.transform.localScale = Vector3.zero;

                LeanTween.scale(spawnedPrefab, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBack);

                Debug.Log("Prefab Instantiated and tweened to scale (1, 1, 1)");
            });
        textComponent.text = "What do we play Next?";
        gameObject.SetActive(false);
    }

}

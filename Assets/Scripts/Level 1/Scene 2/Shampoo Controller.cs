using TMPro;
using UnityEngine;
using System.Collections;

public class ShampooController : MonoBehaviour
{
    public GameObject foamPrefab; // Reference to the foam prefab
    public Transform foamSpawnLocation; // Reference to the spawn location
    public Transform shampooFinalPosition; // Position to reset the shampoo to
    public Vector3 initialPosition; // Initial position of the shampoo
    public int maxFoamCount = 6; // Maximum number of foams to spawn
    public float spawnCooldown = 0.5f; // Cooldown time in seconds between spawns
    public GameObject boyGameObject; // Reference to the boy GameObject
    public GameObject showerGameObject; // Reference to the shower GameObject
    public Lvl1Sc2HelperFunction helperFunctionScript;
    public GameObject HotTap;
    private Animator hotTapAnimator;
    public ParticleSystem showerParticles;
    public Animator birdAnimator;
    public AudioClip audio1;
    public AudioClip audio2;
    public Lvl1Sc1AudioManager audiomanager;
    public TextMeshProUGUI subtitleText;
    private bool isDragging = false;
    private Vector3 offset;
    private Camera mainCamera;
    private int foamCount = 0; // Current count of spawned foams
    public bool isDraggable = false;
    private bool shampooApplied = false;
    private bool tapClickedAfterShampoo = false;
    private bool colliderEnabled = false;
    private bool PositionReseted = false;
    private float nextSpawnTime = 0f; // Time when the next foam can be spawned
    private Collider2D shampooCollider; // Collider of the shampoo

    private AudioSource SfxAudioSource;
    public AudioClip SfxAudio1;

    private ShowerMechanics showerMechanics; // ShowerMechanics script reference
    private ShowerController showerController; // ShowerController script reference

    private void Start()
    {
        mainCamera = Camera.main;
        hotTapAnimator = HotTap.GetComponent<Animator>();
        initialPosition = transform.position;
        shampooCollider = GetComponent<Collider2D>();
        SfxAudioSource = GameObject.FindWithTag("SFXAudioSource").GetComponent<AudioSource>();
        // Ensure the shower GameObject is active before fetching components
        if (showerGameObject.activeSelf)
        {
            showerMechanics = boyGameObject.GetComponent<ShowerMechanics>();
            showerController = showerGameObject.GetComponent<ShowerController>();
        }

        if (showerMechanics == null)
            Debug.LogError("ShowerMechanics script not found on " + showerGameObject.name);
        if (showerController == null)
            Debug.LogError("ShowerController script not found on " + showerGameObject.name);

    }

    private void OnMouseDown()
    {
        if (isDraggable)
        {
            isDragging = true;
            offset = transform.position - GetMouseWorldPos(); // Calculate offset
            helperFunctionScript.ResetTimer();
        }

        // Check if shampoo has been applied and tap is clicked after shampoo application
        if (shampooApplied && !tapClickedAfterShampoo)
        {
            // If shampoo has been applied, we need to handle tap click
            tapClickedAfterShampoo = true; // Set the flag to prevent multiple clicks            
            TriggerTapInteraction();
        }
    }

    private void OnMouseUp()
    {
        isDragging = false;
    }

    private void Update()
    {
        if (isDragging && isDraggable)
        {
            Vector3 mousePos = GetMouseWorldPos() + offset;
            transform.position = new Vector3(mousePos.x, mousePos.y, initialPosition.z); // Dragging movement

            if (Time.time >= nextSpawnTime)
            {
                RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);
                foreach (var hit in hits)
                {
                    if (hit.collider.CompareTag("Hair"))
                    {
                        if (foamCount < maxFoamCount)
                        {
                            SpawnFoam();                            
                        }
                        else
                        {
                            if (!PositionReseted)
                            {
                                PositionReseted = true;
                                ResetPosition();
                            }                             
                            return;
                        }
                        break;
                    }
                }
            }
        }
    }

    private void SpawnFoam()
    {
        if (SfxAudioSource != null)
        {
            SfxAudioSource.loop = false;
            SfxAudioSource.PlayOneShot(SfxAudio1);
        }
        Vector3 randomPosition = foamSpawnLocation.position + new Vector3(
            Random.Range(-foamSpawnLocation.localScale.x / 2, foamSpawnLocation.localScale.x / 2),
            Random.Range(-foamSpawnLocation.localScale.y / 2, foamSpawnLocation.localScale.y / 2),
            0
        );
        GameObject foamInstance = Instantiate(foamPrefab, randomPosition, Quaternion.identity);
        foamInstance.transform.localScale = Vector3.zero;
        LeanTween.scale(foamInstance, new Vector3(1, 1, 1), 0.5f); // Animate foam appearance
        foamCount++;
        showerController.AddFoamObject(foamInstance); // Add foam to shower controller
        nextSpawnTime = Time.time + spawnCooldown; // Update next spawn time
    }

    private Vector3 GetMouseWorldPos()
    {
        Vector3 mousePoint = Input.mousePosition;
        mousePoint.z = mainCamera.nearClipPlane;
        return mainCamera.ScreenToWorldPoint(mousePoint);
    }

    private void ResetPosition()
    {
        if (!colliderEnabled)
        {
            colliderEnabled = true;
            HotTap.GetComponent<Collider2D>().enabled = true;
        }
        
        transform.position = shampooFinalPosition.position; // Reset the position
        transform.rotation = Quaternion.identity; // Reset the rotation
        birdAnimator.SetTrigger("open tap");
        audiomanager.PlayAudio(audio1);
        StartCoroutine(RevealTextWordByWord("Open the Tap", 0.5f));
        helperFunctionScript.StartTimer(true);
        isDragging = false; // Stop dragging
        isDraggable = false; // Make non-draggable
        shampooCollider.enabled = false; // Deactivate the collider
        shampooApplied = true;

        if (showerMechanics == null)
        {
            Debug.LogError("showerMechanics is null!");
        }
        else
        {
            Debug.Log($"showerMechanics.hotTapOn: {showerMechanics.hotTapOn}");
        }

        if (showerMechanics != null && showerMechanics.hotTapOn)
        {
            Debug.Log("Tap is on, closing the tap.");
            StartCoroutine(TriggerCloseTapAfterDelay());
        }
    }

    private void TriggerTapInteraction()
    {
        showerMechanics.hotTapOn = !showerMechanics.hotTapOn;

        // Play the "Tap On" or "Tap Off" animation, based on the state
        hotTapAnimator.SetTrigger(showerMechanics.hotTapOn ? "TapOn" : "TapOff");

        // Play shower particles and audio
        if (showerMechanics.hotTapOn)
        {
            showerParticles.Play(); // Turn on shower
            if (SfxAudioSource != null)
            {
                SfxAudioSource.clip = audio1; // Set to appropriate sound
                SfxAudioSource.loop = true;
                SfxAudioSource.Play();
            }

            // Wait for the "TapOn" animation to complete before disabling collider
            StartCoroutine(WaitForTapAnimationAndDisableCollider());
        }
        else
        {
            // Disable collider immediately when "Tap Off" animation starts playing
            HotTap.GetComponent<Collider2D>().enabled = false;

            showerParticles.Stop(); // Turn off shower
            if (SfxAudioSource != null)
            {
                SfxAudioSource.loop = false;
                SfxAudioSource.Stop();
            }
        }
    }

    private IEnumerator WaitForTapAnimationAndDisableCollider()
    {
        // Wait for the animation duration (adjust to the actual duration of the "TapOn" animation)
        float animationDuration = 1f; // Set this to the length of the "TapOn" animation in seconds
        yield return new WaitForSeconds(animationDuration);

        // After animation completes, disable the collider
        HotTap.GetComponent<Collider2D>().enabled = false;

        // Proceed with further actions
        StartCoroutine(TriggerCloseTapAfterDelay());
    }


    private IEnumerator TriggerCloseTapAfterDelay()
    {
        yield return new WaitForSeconds(2f); // Wait for 2 seconds after tap is turned on

        // Trigger "close tap" animation
        if (birdAnimator != null)
        {
            birdAnimator.SetTrigger("close tap");
            audiomanager.PlayAudio(audio2);
            StartCoroutine(RevealTextWordByWord("Close the Tap", 0.5f));
        }

        // Wait until bird animation is done
        yield return new WaitForSeconds(1.5f);

        
        
            colliderEnabled = true;
            HotTap.GetComponent<Collider2D>().enabled = true;
        
        
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

using UnityEngine;

public class ShowerMechanics : MonoBehaviour
{
    public Animator boyAnimator;                // Reference to the boy's Animator
    public GameObject prefabToSpawn;            // Prefab to spawn
    public Transform spawnLocation;             // Location to spawn the prefab

    public ParticleSystem showerParticles;
    public Animator hotTapAnimator, coldTapAnimator;

    private bool hotTapOn = false;
    private bool coldTapOn = false;
    private bool hasSpawned = false;            // To ensure prefab spawns only once per animation completion

    void Update()
    {
        HandleTapInput();
        CheckAnimationAndSpawnPrefab();
    }

    void HandleTapInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                if (hit.collider.CompareTag("Hot Tap") || hit.collider.CompareTag("Cold Tap"))
                {
                    if (hotTapOn && coldTapOn)
                    {
                        hotTapOn = false;
                        coldTapOn = false;
                        hotTapAnimator.SetTrigger("TapOff");
                        coldTapAnimator.SetTrigger("TapOff");
                        boyAnimator.SetBool("isReaching", true);
                        UpdateShowerState();
                    }
                    else
                    {
                        ToggleTap(hit.collider);
                    }
                }
            }
        }
    }

    void ToggleTap(Collider2D tappedTap)
    {
        bool isHotTap = tappedTap.CompareTag("Hot Tap");
        if (isHotTap)
        {
            hotTapOn = !hotTapOn;
            hotTapAnimator.SetTrigger(hotTapOn ? "TapOn" : "TapOff");
        }
        else
        {
            coldTapOn = !coldTapOn;
            coldTapAnimator.SetTrigger(coldTapOn ? "TapOn" : "TapOff");
        }
        UpdateShowerState();
    }

    void UpdateShowerState()
    {
        boyAnimator.SetBool("IsNormal", hotTapOn && coldTapOn);
        boyAnimator.SetBool("IsTooHot", hotTapOn);
        boyAnimator.SetBool("IsTooCold", coldTapOn);

        if (hotTapOn || coldTapOn)
        {
            showerParticles.Play();
        }
        else
        {
            showerParticles.Stop();
        }
    }

    void CheckAnimationAndSpawnPrefab()
    {
        AnimatorStateInfo stateInfo = boyAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Talk Animation") && stateInfo.normalizedTime >= 1.0f && !hasSpawned)
        {
            SpawnPrefab();
            hasSpawned = true;
        }
    }

    void SpawnPrefab()
    {
        Instantiate(prefabToSpawn, spawnLocation.position, Quaternion.identity);
    }

    void OnDisable()
    {
        hasSpawned = false;  // Reset spawn flag when disabled
    }
}

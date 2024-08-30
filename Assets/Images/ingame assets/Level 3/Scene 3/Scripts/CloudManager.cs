using UnityEngine;

public class CloudManager : MonoBehaviour
{
    public GameObject[] initialClouds; 
    public GameObject[] cloudPrefabs; 
    public Transform startPoint; 
    public Transform endPoint; 
    public float minSpeed = 0.5f; 
    public float maxSpeed = 1f; 

    private void Start()
    {
        
        foreach (GameObject cloud in initialClouds)
        {
            InitializeCloud(cloud);
        }
    }

    private void InitializeCloud(GameObject cloud)
    {
        
        float randomSpeed = Random.Range(minSpeed, maxSpeed);
        cloud.AddComponent<CloudMovement>().Initialize(endPoint, randomSpeed, this);
    }

    public void OnCloudDestroyed(GameObject destroyedCloud)
    {
       
        int randomIndex = Random.Range(0, cloudPrefabs.Length);
        GameObject selectedPrefab = cloudPrefabs[randomIndex];

        
        SpawnCloud(selectedPrefab);
    }

    private void SpawnCloud(GameObject cloudPrefab)
    {

        GameObject cloud = Instantiate(cloudPrefab, startPoint.position, Quaternion.identity);
        InitializeCloud(cloud);
    }
    public void UpdateCloudSpeeds()
    {
        CloudMovement[] activeClouds = FindObjectsOfType<CloudMovement>();

        foreach (CloudMovement cloud in activeClouds)
        {
            cloud.speed = Random.Range(minSpeed, maxSpeed);
            Debug.Log($"Updated cloud speed to: {cloud.speed}");
        }
    }


}

public class CloudMovement : MonoBehaviour
{
    private Transform endPoint;
    public float speed;
    private CloudManager cloudManager;

    public void Initialize(Transform endPoint, float speed, CloudManager cloudManager)
    {
        this.endPoint = endPoint;
        this.speed = speed;
        this.cloudManager = cloudManager;
    }

    private void Update()
    {

        transform.position = Vector3.MoveTowards(transform.position, endPoint.position, speed * Time.deltaTime);


        if (Vector3.Distance(transform.position, endPoint.position) < 0.1f)
        {

            cloudManager.OnCloudDestroyed(gameObject);
            Destroy(gameObject);
        }
    }

}

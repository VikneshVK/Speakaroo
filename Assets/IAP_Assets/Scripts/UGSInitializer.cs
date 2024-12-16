using Unity.Services.Core;
using Unity.Services.Core.Environments;
using UnityEngine;

public class UGSInitializer : MonoBehaviour
{
    async void Start()
    {
        try
        {
            // Optionally, set the environment (e.g., "production" or "development")
            var options = new InitializationOptions();
            options.SetEnvironmentName("production"); // Change as needed
            await UnityServices.InitializeAsync(options);

            Debug.Log("Unity Gaming Services initialized successfully.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to initialize Unity Gaming Services: {e.Message}");
        }
    }
}

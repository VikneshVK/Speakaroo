using UnityEngine;
using UnityEngine.UI;

public class ButtonClickListener : MonoBehaviour
{
    public Scene_Manager_Final sceneManager;

    // This will be assigned from the Inspector
    public string sceneName;
    public int sceneCategory;

    public void OnButtonClick()
    {
        // Call the LoadLevel function when the button is clicked
        sceneManager.LoadLevel(sceneName, sceneCategory);
    }
}

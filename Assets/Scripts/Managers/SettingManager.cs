using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public GameObject generalSettingsPanel;
    public GameObject socialMediaPanel;
    public GameObject parentalControlPanel;

    private void Awake()
    {
        settingsPanel.SetActive(false); 
    }

    public void openSettingsPanel()
    {
        settingsPanel.SetActive(true);
        ShowGeneralSettings();
    }

    public void ShowGeneralSettings()
    {
        CloseAllPanels();
        generalSettingsPanel.SetActive(true); // Activate the General Settings panel
    }

    public void ShowSocialMediaPanel()
    {
        CloseAllPanels();
        socialMediaPanel.SetActive(true); // Activate the Social Media panel
    }

    public void ShowParentalControlPanel()
    {
        CloseAllPanels();
        parentalControlPanel.SetActive(true); // Activate the Parental Control panel
    }

    public void CloseSettingsPanel()
    {
        settingsPanel.SetActive(false); // Deactivate the main settings canvas
    }

    private void CloseAllPanels()
    {
        generalSettingsPanel.SetActive(false); // Deactivate the General Settings panel
        socialMediaPanel.SetActive(false); // Deactivate the Social Media panel
        parentalControlPanel.SetActive(false); // Deactivate the Parental Control panel
    }
}

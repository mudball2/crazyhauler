using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionsController : MonoBehaviour
{
    [SerializeField] Slider volumeSlider;
    [SerializeField] float defaultVolume = 0.8f;

    // Start is called before the first frame update
    void Start()
    {
        volumeSlider.value = PlayerPrefsController.GetMasterVolume();
    }

    // Update is called once per frame
    void Update()
    {
        var musicPlayer = FindObjectOfType<MusicPlayer>();
        if (musicPlayer)
        {
            musicPlayer.SetVolume(volumeSlider.value);
        }
        else
        {
            Debug.LogWarning("No music player found.");
        }
    }

    public void SaveAndExit()
    {
        PlayerPrefsController.SetMasterVolume(volumeSlider.value);
        // Insert code to navigate back to main menu.
    }

    public void SetDefaults()
    {
        volumeSlider.value = defaultVolume;
    }
}
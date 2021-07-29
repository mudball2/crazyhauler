using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    AudioSource audioSource;
    private static MusicPlayer currentMusic;
     
    void Awake()
    {
        if (!currentMusic)
        {
            currentMusic = this;
        }
        else
        {
            Destroy(this.gameObject) ;
        } 
        DontDestroyOnLoad(this.gameObject) ;
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = PlayerPrefsController.GetMasterVolume();
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = volume;
    }
}

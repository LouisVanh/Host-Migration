using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(this.gameObject);
        else
            Instance = this;

        DontDestroyOnLoad(this.gameObject);
    }

    [Header("Audio Sources")]
    public AudioSource VoiceAudioSource;
    public AudioSource CupAudioSource;

    [Header("Clips")]
    public AudioClip DiceRollingClip1;


    public float TimeSinceVoiceLine;

    private void Update()
    {
        TimeSinceVoiceLine += Time.deltaTime;
    }

    public void PlayRandomDiceSound()
    {
        var chance = Random.Range(0, 300);
        if (chance < 6)
        {
            TimeSinceVoiceLine = 0;
            switch (chance)
            {
                case 1:
                    CupAudioSource.PlayOneShot(DiceRollingClip1);
                    break;
                case 2:
                    CupAudioSource.PlayOneShot(DiceRollingClip1);
                    break;
                case 3:
                    CupAudioSource.PlayOneShot(DiceRollingClip1);
                    break;
                case 4:
                    CupAudioSource.PlayOneShot(DiceRollingClip1);
                    break;
                case 5:
                    CupAudioSource.PlayOneShot(DiceRollingClip1);
                    break;
            }
        }
    }
}



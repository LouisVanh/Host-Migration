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
    public AudioClip DiceInCupSound;
    public AudioClip DiceOnTableSound;
    public AudioClip EnemyHitSound1;
    public AudioClip EnemyHitSound2;
    public AudioClip EnemyHitSound3;
    public AudioClip EnemyHitSound4;
    public AudioClip EnemyHitSound5;


    public float TimeSinceVoiceLine;

    private void Update()
    {
        TimeSinceVoiceLine += Time.deltaTime;
    }

    public void PlayRandomEnemyHitSound() //TODO CHANGE SOUNDS LOL
    {
        var chance = Random.Range(0, 300);
        if (chance < 6)
        {
            TimeSinceVoiceLine = 0;
            switch (chance)
            {
                case 1:
                    CupAudioSource.pitch = Random.Range(0.5f, 0.8f);
                    CupAudioSource.PlayOneShot(EnemyHitSound1);
                    break;
                case 2:
                    CupAudioSource.PlayOneShot(EnemyHitSound2);
                    break;
                case 3:
                    CupAudioSource.PlayOneShot(EnemyHitSound3);
                    break;
                case 4:
                    CupAudioSource.PlayOneShot(EnemyHitSound4);
                    break;
                case 5:
                    CupAudioSource.PlayOneShot(EnemyHitSound5);
                    break;
            }
        }
    }
    public void PlayDiceInCupSound()
    {
        var pitch = Random.Range(0.8f, 1.2f);
        CupAudioSource.pitch = pitch;
        CupAudioSource.PlayOneShot(DiceInCupSound);
    }
}



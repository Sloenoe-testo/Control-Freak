using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioClip defaultAmbience;
    [SerializeField] private AudioClip storageAmbience;
    [SerializeField] private AudioClip spaceportAmbience;
    [SerializeField] private float defaultVolume;
    [SerializeField] private float storageVolume;
    [SerializeField] private float spaceportVolume;

    private void Awake()
    {
        ambienceSource.clip = defaultAmbience;
        ambienceSource.Play();
    }

    private void OnEnable()
    {
        CharacterSwitch.OnEnteredCapsule += PlayStorageAmbience;
        CharacterSwitch.OnStartedExitingCapsule += PlayDefaultAmbience;
        AmbienceTrigger.OnAmbienceActivated += PlaySpaceportAmbience;
    }

    private void OnDisable()
    {
        CharacterSwitch.OnEnteredCapsule -= PlayStorageAmbience;
        CharacterSwitch.OnStartedExitingCapsule -= PlayDefaultAmbience;
        AmbienceTrigger.OnAmbienceActivated -= PlaySpaceportAmbience;
    }

    private void PlayDefaultAmbience()
    {
        if (CharacterSwitch.usedCapsules <= 1)
        {
            Debug.Log("PlayDefaultAmbience");
            ambienceSource.Stop();
            ambienceSource.volume = defaultVolume;
            ambienceSource.clip = defaultAmbience;
            ambienceSource.Play();
        }
    }

    private void PlayStorageAmbience()
    {
        if (CharacterSwitch.usedCapsules <= 0)
        {
            Debug.Log("PlayStorageAmbience");
            ambienceSource.Stop();
            ambienceSource.volume = storageVolume;
            ambienceSource.clip = storageAmbience;
            ambienceSource.Play();
        }
    }

    private void PlaySpaceportAmbience(bool ambienceTriggerActivated)
    {
        if (ambienceTriggerActivated)
        {
            Debug.Log("PlaySpaceportAmbience");
            ambienceSource.Stop();
            ambienceSource.volume = spaceportVolume;
            ambienceSource.clip = spaceportAmbience;
            ambienceSource.Play();
        }
        else
        {
            Debug.Log("PlayDefaultAmbience");
            ambienceSource.Stop();
            ambienceSource.volume = defaultVolume;
            ambienceSource.clip = defaultAmbience;
            ambienceSource.Play();
        }
    }
}

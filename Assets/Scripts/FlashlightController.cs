using UnityEngine;

public class FlashlightController : MonoBehaviour
{
    private Light flashlight;
    public static bool isActive = false;

    private AudioSource flashlightSource;
    [SerializeField] private AudioClip flashlightOn;
    [SerializeField] private AudioClip flashlightOff;

    private void Awake()
    {
        flashlight = GetComponent<Light>();
        flashlightSource = GetComponent<AudioSource>();
        flashlight.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isActive)
            {
                flashlight.enabled = false;
                isActive = false;
                flashlightSource.PlayOneShot(flashlightOn);
            }
            else
            {
                flashlight.enabled = true;
                isActive = true;
                flashlightSource.PlayOneShot(flashlightOff);
            }
        }
    }
}

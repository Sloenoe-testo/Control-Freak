using UnityEngine;
using System;

public class AmbienceTrigger : MonoBehaviour
{
    private bool ambienceTriggerActivated = false;
    public static Action<bool> OnAmbienceActivated;
 
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterController>().enabled)
        {
            ambienceTriggerActivated = !ambienceTriggerActivated;

            OnAmbienceActivated?.Invoke(ambienceTriggerActivated);
        }
    }
}

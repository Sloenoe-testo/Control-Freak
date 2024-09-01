using System;
using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public static Action OnDoorTriggerEntered;

    private bool triggerActivated = false;

    public void OnTriggerEnter(Collider other)
    {
        if (!triggerActivated)
        {
            Debug.Log("Door trigger activated");

            OnDoorTriggerEntered?.Invoke();

            triggerActivated = true;
        }
    }
}

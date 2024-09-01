using UnityEngine;

public class CaphTrigger : MonoBehaviour
{
    [SerializeField] GameObject caph;
    public static bool triggerActivated = false;

    public void OnTriggerEnter(Collider other)
    {
        if (!triggerActivated || Scenes.autosaveLoaded)
        {
            Debug.Log("Caph trigger activated");

            caph.SetActive(true);

            triggerActivated = true;
        }
    }
}

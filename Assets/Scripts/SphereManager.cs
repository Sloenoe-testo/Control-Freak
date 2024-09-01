using System;
using UnityEngine;

public class SphereManager : MonoBehaviour
{
    [SerializeField] private GameObject[] sphereArray;

    private int currentSphereIndex = 0;

    [SerializeField] private int spheresInRoom = 3;
    private int sphereRoomIndex = 0;

    public static bool allSpheresActivated = false;

    public static Action OnAllSpheresActivated;

    private void Awake()
    {
        sphereArray[0].SetActive(true);
        for (int i = 1; i < sphereArray.Length; i++)
        {
            sphereArray[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        PuzzlePoint.OnFinished += EnableNextSphere;
    }

    private void OnDisable()
    {
        PuzzlePoint.OnFinished -= EnableNextSphere;
    }

    private void EnableNextSphere(bool isFinished)
    {
        if (isFinished)
        {
            sphereArray[currentSphereIndex].GetComponentInChildren<PuzzleSphere>().enabled = false;
            sphereArray[currentSphereIndex].GetComponentInChildren<Collider>().enabled = false;

            if (sphereRoomIndex < spheresInRoom - 1)
            {
                allSpheresActivated = false;
                sphereRoomIndex += 1;

                currentSphereIndex += 1;
                sphereArray[currentSphereIndex].SetActive(true);
            }
            else
            {
                sphereRoomIndex = 0;
                allSpheresActivated = true;
                OnAllSpheresActivated?.Invoke();

                if (currentSphereIndex < sphereArray.Length - 1)
                {
                    currentSphereIndex += 1;
                    sphereArray[currentSphereIndex].SetActive(true);
                }
            }
        }
    }
}

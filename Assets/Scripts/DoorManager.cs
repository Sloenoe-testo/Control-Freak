using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{
    [SerializeField] List<GameObject> doorList;
    private int i = 0; // ������ ����� � ������ doorList

    [SerializeField] Color openedColor;
    [SerializeField] Material openedMaterial;

    [SerializeField] float moveDuration = 1f;
    [SerializeField] float doorOffset;
    [SerializeField] private AnimationCurve doorAnimationCurve;

    public static Action OnDoorClosed;

    private AudioSource doorSource;
    [SerializeField] AudioClip doorOpen;
    [SerializeField] AudioClip doorClose;


    private void OnEnable()
    {
        SphereManager.OnAllSpheresActivated += OpenDoor;
        DoorTrigger.OnDoorTriggerEntered += CloseDoor;
    }

    private void OnDisable()
    {
        SphereManager.OnAllSpheresActivated -= OpenDoor;
        DoorTrigger.OnDoorTriggerEntered -= CloseDoor;
    }

    //private void Update()
    //{
    //    // ��� ������������
    //    if (Input.GetKeyDown(KeyCode.O))
    //        OpenDoor();
    //    if (Input.GetKeyDown(KeyCode.C))
    //        CloseDoor();
    //}

    private void OpenDoor()
    {
        // ������ ���� � �������� �������� �� �������
        doorList[i].GetComponentInChildren<Light>().color = openedColor;
        GameObject lightBulb = doorList[i].transform.Find("Wall Light/Wall Light Bulb").gameObject;
        lightBulb.GetComponent<MeshRenderer>().material = openedMaterial;

        StartCoroutine(CoOpenDoor(doorList[i].transform.Find("LP_Bay_Door_snap_l").gameObject, doorList[i].transform.Find("LP_Bay_Door_snap_r").gameObject)); // �������� � �������� ����� � ������ ������� ������� � ������ �����
    }

    private void CloseDoor()
    {
        StartCoroutine(CoCloseDoor(doorList[i].transform.Find("LP_Bay_Door_snap_l").gameObject, doorList[i].transform.Find("LP_Bay_Door_snap_r").gameObject));
        if (i < doorList.Count - 1) // ���� ������ �� ������� �� ������� ���������� �������� � ������ ������
            i += 1; // ����������� ������ �� 1, ����� ����� ������� �������� ������� �����, ������� ����������� ��������� �����
    }

    IEnumerator CoOpenDoor(GameObject leftDoor, GameObject rightDoor)
    {
        yield return new WaitForSeconds(1f); // ����� ����������� � ���������, ����� ������ ����������� �������� ����������� ������ ��� ������������ ����� �����������
        Debug.Log("DoorOpening");

        // ���� �������� �����
        doorSource = doorList[i].GetComponent<AudioSource>();
        doorSource.PlayOneShot(doorOpen);

        float journey = 0f;
        Vector3 leftDoorTargetPosition = new Vector3(leftDoor.transform.position.x + doorOffset, leftDoor.transform.position.y, leftDoor.transform.position.z);
        Vector3 rightDoorTargetPosition = new Vector3(rightDoor.transform.position.x - doorOffset, rightDoor.transform.position.y, rightDoor.transform.position.z);
        while (journey <= moveDuration)
        {   // ���� �����������
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / moveDuration);
            leftDoor.transform.position = Vector3.Lerp(leftDoor.transform.position, leftDoorTargetPosition, percent);
            rightDoor.transform.position = Vector3.Lerp(rightDoor.transform.position, rightDoorTargetPosition, percent);

            yield return null;
        }
    }

    IEnumerator CoCloseDoor(GameObject leftDoor, GameObject rightDoor)
    {
        Debug.Log("DoorClosing");

        // ���� �������� �����
        doorSource = doorList[i].GetComponent<AudioSource>();
        doorSource.PlayOneShot(doorClose);

        float journey = 0f;
        Vector3 leftDoorTargetPosition = new Vector3(leftDoor.transform.position.x - doorOffset, leftDoor.transform.position.y, leftDoor.transform.position.z);
        Vector3 rightDoorTargetPosition = new Vector3(rightDoor.transform.position.x + doorOffset, rightDoor.transform.position.y, rightDoor.transform.position.z);
        while (journey <= moveDuration)
        {   // ���� �����������
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / moveDuration);
            leftDoor.transform.position = Vector3.Lerp(leftDoor.transform.position, leftDoorTargetPosition, percent);
            rightDoor.transform.position = Vector3.Lerp(rightDoor.transform.position, rightDoorTargetPosition, percent);

            yield return null;
        }

        OnDoorClosed?.Invoke();
    }
}

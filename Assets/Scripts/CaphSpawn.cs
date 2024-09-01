using System.Collections.Generic;
using UnityEngine;

public class CaphSpawn : MonoBehaviour
{
    [SerializeField] GameObject caph;
    [SerializeField] List<Transform> spawnList;
    private int i = 0; // ������ ����� ������ � ������ spawnList

    private void OnEnable()
    {
        CharacterSwitch.OnStartedExitingCapsule += SpawnCaph;
    }

    private void OnDisable()
    {
        CharacterSwitch.OnStartedExitingCapsule -= SpawnCaph;
    }

    private void Start()
    {
        // � ������
        caph.transform.position = spawnList[i].position;
        if (i < spawnList.Count - 1)
            i += 1;
    }

    private void SpawnCaph()
    {
        if (!CaphTrigger.triggerActivated)
        {
            caph.transform.position = spawnList[i].position;
            caph.SetActive(false);

            if (i < spawnList.Count - 1) // ���� ������ �� ������� �� ������� ���������� �������� � ������ ����� ������
                i += 1; // ����������� ������ �� 1, ����� ����� ������� ������, ������� ����������� ��������� ����� ������
        }
    }
}

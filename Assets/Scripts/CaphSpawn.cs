using System.Collections.Generic;
using UnityEngine;

public class CaphSpawn : MonoBehaviour
{
    [SerializeField] GameObject caph;
    [SerializeField] List<Transform> spawnList;
    private int i = 0; // Индекс точки спавна в списке spawnList

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
        // В начале
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

            if (i < spawnList.Count - 1) // Если индекс не выходит за пределы количества объектов в списке точек спавна
                i += 1; // Увеличиваем индекс на 1, чтобы после каждого спавна, текущей становилась следующая точка спавна
        }
    }
}

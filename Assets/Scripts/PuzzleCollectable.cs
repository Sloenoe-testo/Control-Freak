using System;
using UnityEngine;

public class PuzzleCollectable : MonoBehaviour
{
    public static Action<bool, bool> OnCollected;
    private bool collected = false;
    private bool wrongCollectable = false;
    private int collectableType = 1;

    private void Awake()
    {
        if (gameObject.tag == "CollectableDouble")
            collectableType = 2;
        else if (gameObject.tag == "CollectableTriple")
            collectableType = 3;
    }

    private void OnEnable()
    {
        PuzzlePoint.OnLineReseted += ResetCollected;
        PuzzleSphere.OnPuzzleExited += ResetCollected;
    }

    private void OnDisable()
    {
        PuzzlePoint.OnLineReseted -= ResetCollected;
        PuzzleSphere.OnPuzzleExited -= ResetCollected;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PuzzleManager.puzzleType == 2 && collision.tag == "PuzzlePointer") // Для второго типа головоломки
        {
            if (collected) // Если стираем линию и она больше не проведена по очку, то не засчитываем сбор
            {
                collected = false;
            }
            else // Если еще не проведена линия, то засчитываем сбор очка
            {
                collected = true;
            }

            OnCollected?.Invoke(collected, false); // Вызываем событие в PuzzleManager
        }

        else if (PuzzleManager.puzzleType == 3 && collision.tag == "PuzzlePointer") // Для третьего типа головоломки
        {
            if (collected) // Если стираем линию и она больше не проведена по очку, то не засчитываем сбор
            {
                collected = false;
            }
            else // Если еще не проведена линия
            {
                if ((collectableType == 1 && PuzzleManager.collectableCounter == 0) || (collectableType == 2 && PuzzleManager.collectableCounter == 1) || (collectableType == 3 && PuzzleManager.collectableCounter == 2)) // Если линия проведена по очкам в нужном порядке, то засчитываем сбор очка
                {
                    wrongCollectable = false;
                    collected = true;
                }
                else // Если линия проведена в неправильном порядке, то не засчитываем
                    wrongCollectable = true;
            }

            OnCollected?.Invoke(collected, wrongCollectable); // Вызываем событие в PuzzleManager
        }
    }

    private void ResetCollected(bool finished) // Если линия сброшена, то засчитываем все очки за НЕсобранные
    {
        if (!finished)
            collected = false;
    }
}

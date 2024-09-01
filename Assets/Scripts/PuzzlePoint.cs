using Radishmouse;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PuzzlePoint : MonoBehaviour
{
    public static List<GameObject> pointsList;
    [SerializeField] Color defaultColor;
    [SerializeField] Color greenColor;

    [SerializeField] EdgeCollider2D lineCollider;
    public static Action OnLineChanged;
    public static Action<bool> OnLineReseted;
    public static Action<bool> OnFinished;
    private bool finished = false;

    private void Awake()
    {
        pointsList = UILineRenderer.pointsList;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "PuzzlePointer")
        {
            // Если текущая точка равна предпоследней точке из списка, то есть если мы повели указатель назад
            if ((pointsList.Count >= 2) && (pointsList[pointsList.Count - 2] == gameObject))
            {
                EraseLine(); // Стираем линию
            }

            else // Если не стираем линию
            {
                if (pointsList.Count >= 4 && pointsList.Contains(gameObject)) // При пересечении линии
                {
                    ResetLine(); // Сбрасываем линию
                }
                else // Если ведем указатель вперед и не пересекаем линию
                {
                    DrawLine(); // Рисуем линию

                    if (gameObject.tag == "PuzzleFinish") // Если навели на точку финиша
                    {
                        Finish();
                    }
                }
            }
        }
    }

    // Рисование линии
    private void DrawLine()
    {
        gameObject.GetComponent<SpriteRenderer>().color = greenColor; // Меняем цвет точки на зеленый
        pointsList.Add(gameObject); // Добавляем точку в список
        //OnLineChanged?.Invoke(); // Вызываем событие для изменения коллайдера линии
    }

    // Стирание линии
    private void EraseLine()
    {
        pointsList[pointsList.Count - 1].GetComponent<SpriteRenderer>().color = defaultColor; // Меняем цвет точки на дефолтный
        pointsList.RemoveAt(pointsList.Count - 1); // Удаляем точку из списка
    }

    // Сбрасывание линии
    private void ResetLine()
    {
        // Сначала сбрасываем цвет у всех точек в списке
        foreach (var point in pointsList)
        {
            point.GetComponent<SpriteRenderer>().color = defaultColor;
        }

        pointsList.Clear(); // Очищаем список точек

        OnLineReseted?.Invoke(false);
        PuzzleStart.isStarted = false;

        PuzzleManager.allCollected = false;
        PuzzleManager.collectableCounter = 0;
    }

    private void Finish()
    {
        if ((PuzzleManager.puzzleType == 2 || PuzzleManager.puzzleType == 3) && PuzzleManager.allCollected) // Если головоломка 2 или 3 типа и собраны все очки
        {
            Debug.Log("Finish");
            finished = true;
            OnFinished?.Invoke(finished);

            PuzzleManager.allCollected = false;
            PuzzleManager.collectableCounter = 0;
            PuzzleStart.isStarted = false;
        }

        else if (PuzzleManager.puzzleType == 1) // Если головоломка 1 типа
        {
            Debug.Log("Finish");
            finished = true;
            OnFinished?.Invoke(finished);

            PuzzleStart.isStarted = false;
        }
    }
}

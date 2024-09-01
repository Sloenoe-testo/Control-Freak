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
            // ���� ������� ����� ����� ������������� ����� �� ������, �� ���� ���� �� ������ ��������� �����
            if ((pointsList.Count >= 2) && (pointsList[pointsList.Count - 2] == gameObject))
            {
                EraseLine(); // ������� �����
            }

            else // ���� �� ������� �����
            {
                if (pointsList.Count >= 4 && pointsList.Contains(gameObject)) // ��� ����������� �����
                {
                    ResetLine(); // ���������� �����
                }
                else // ���� ����� ��������� ������ � �� ���������� �����
                {
                    DrawLine(); // ������ �����

                    if (gameObject.tag == "PuzzleFinish") // ���� ������ �� ����� ������
                    {
                        Finish();
                    }
                }
            }
        }
    }

    // ��������� �����
    private void DrawLine()
    {
        gameObject.GetComponent<SpriteRenderer>().color = greenColor; // ������ ���� ����� �� �������
        pointsList.Add(gameObject); // ��������� ����� � ������
        //OnLineChanged?.Invoke(); // �������� ������� ��� ��������� ���������� �����
    }

    // �������� �����
    private void EraseLine()
    {
        pointsList[pointsList.Count - 1].GetComponent<SpriteRenderer>().color = defaultColor; // ������ ���� ����� �� ���������
        pointsList.RemoveAt(pointsList.Count - 1); // ������� ����� �� ������
    }

    // ����������� �����
    private void ResetLine()
    {
        // ������� ���������� ���� � ���� ����� � ������
        foreach (var point in pointsList)
        {
            point.GetComponent<SpriteRenderer>().color = defaultColor;
        }

        pointsList.Clear(); // ������� ������ �����

        OnLineReseted?.Invoke(false);
        PuzzleStart.isStarted = false;

        PuzzleManager.allCollected = false;
        PuzzleManager.collectableCounter = 0;
    }

    private void Finish()
    {
        if ((PuzzleManager.puzzleType == 2 || PuzzleManager.puzzleType == 3) && PuzzleManager.allCollected) // ���� ����������� 2 ��� 3 ���� � ������� ��� ����
        {
            Debug.Log("Finish");
            finished = true;
            OnFinished?.Invoke(finished);

            PuzzleManager.allCollected = false;
            PuzzleManager.collectableCounter = 0;
            PuzzleStart.isStarted = false;
        }

        else if (PuzzleManager.puzzleType == 1) // ���� ����������� 1 ����
        {
            Debug.Log("Finish");
            finished = true;
            OnFinished?.Invoke(finished);

            PuzzleStart.isStarted = false;
        }
    }
}

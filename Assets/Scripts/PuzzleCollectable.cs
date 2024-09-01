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
        if (PuzzleManager.puzzleType == 2 && collision.tag == "PuzzlePointer") // ��� ������� ���� �����������
        {
            if (collected) // ���� ������� ����� � ��� ������ �� ��������� �� ����, �� �� ����������� ����
            {
                collected = false;
            }
            else // ���� ��� �� ��������� �����, �� ����������� ���� ����
            {
                collected = true;
            }

            OnCollected?.Invoke(collected, false); // �������� ������� � PuzzleManager
        }

        else if (PuzzleManager.puzzleType == 3 && collision.tag == "PuzzlePointer") // ��� �������� ���� �����������
        {
            if (collected) // ���� ������� ����� � ��� ������ �� ��������� �� ����, �� �� ����������� ����
            {
                collected = false;
            }
            else // ���� ��� �� ��������� �����
            {
                if ((collectableType == 1 && PuzzleManager.collectableCounter == 0) || (collectableType == 2 && PuzzleManager.collectableCounter == 1) || (collectableType == 3 && PuzzleManager.collectableCounter == 2)) // ���� ����� ��������� �� ����� � ������ �������, �� ����������� ���� ����
                {
                    wrongCollectable = false;
                    collected = true;
                }
                else // ���� ����� ��������� � ������������ �������, �� �� �����������
                    wrongCollectable = true;
            }

            OnCollected?.Invoke(collected, wrongCollectable); // �������� ������� � PuzzleManager
        }
    }

    private void ResetCollected(bool finished) // ���� ����� ��������, �� ����������� ��� ���� �� �����������
    {
        if (!finished)
            collected = false;
    }
}

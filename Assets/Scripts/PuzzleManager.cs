using UnityEngine;

public class PuzzleManager : MonoBehaviour
{
    public static int collectableCounter = 0;
    public static bool allCollected = false;
    [SerializeField] private int numberOfCollectables = 3;

    [SerializeField] private GameObject[] puzzleArray;

    public static int puzzleType = 1;

    private void OnEnable()
    {
        PuzzleCollectable.OnCollected += CollectableTriggered;
        PuzzleSphere.OnPuzzleEnabled += GetPuzzleType;
    }

    private void OnDisable()
    {
        PuzzleCollectable.OnCollected -= CollectableTriggered;
        PuzzleSphere.OnPuzzleEnabled -= GetPuzzleType;
    }

    private void GetPuzzleType() // ����������� ���� ����������
    {
        // ������� �������� � �������� �����������

        GameObject currentPuzzle = null;

        foreach (var puzzle in puzzleArray)
        {
            if (puzzle.activeInHierarchy)
                currentPuzzle = puzzle.gameObject;
        }

        // ���������� ���

        if (currentPuzzle.tag == "PuzzleFirstType")
            puzzleType = 1;
        else if (currentPuzzle.tag == "PuzzleSecondType")
            puzzleType = 2;
        else if (currentPuzzle.tag == "PuzzleThirdType")
            puzzleType = 3;

        GetCollectablesNumber();
    }

    private void CollectableTriggered(bool collected, bool wrongCollectable)
    {
        if (collectableCounter <= numberOfCollectables)
        {
            if (collected) // ���� �������� ���� ����, �� ����������� ������� �����
            {
                collectableCounter++;
                Debug.Log("Counter: " + collectableCounter);

                if (collectableCounter == numberOfCollectables)
                {
                    Debug.Log("All Collected");
                    allCollected = true; // ������������ � PuzzlePoint
                }
            }
            else if (!collected && !wrongCollectable) // ���� ����� ������ �� ��������� �� ����, �� ��������� �������
            {
                collectableCounter--;
                allCollected = false;
                Debug.Log("Counter: " + collectableCounter);
            }
        }
    }

    private void GetCollectablesNumber() // ����� ��� ��������� numberOfCollectables �� ���������� ����� � ������� ����������� (��� �� ���������)
    {
        // ������� ������� � �������� �������� ����������� (�� �����)

        GameObject currentPuzzle = null;

        foreach (var puzzle in puzzleArray)
        {
            if (puzzle.activeInHierarchy && (puzzle.tag == "PuzzleSecondType" || puzzle.tag == "PuzzleThirdType"))
                currentPuzzle = puzzle.gameObject;
        }

        // ����� ������� � ��� ���������� ����� (�������� �������� � ������������� ������)

        int len = 0;
        if (currentPuzzle != null)
            len = currentPuzzle.transform.childCount;
        GameObject currentChild = null;
        int numberOfChildren = 0;

        for (int i = 0; i < len; i++)
        {
            currentChild = currentPuzzle.transform.GetChild(i).gameObject;
            if (currentChild.tag == "CollectableSingular" || currentChild.tag == "CollectableDouble" || currentChild.tag == "CollectableTriple")
                numberOfChildren++;
        }

        // ����������� ���������� �����
        numberOfCollectables = numberOfChildren;
        numberOfChildren = 0;
    }
}

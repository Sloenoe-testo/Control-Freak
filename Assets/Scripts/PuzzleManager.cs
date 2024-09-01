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

    private void GetPuzzleType() // Определение типа головломки
    {
        // Находим активную в иерархии головоломку

        GameObject currentPuzzle = null;

        foreach (var puzzle in puzzleArray)
        {
            if (puzzle.activeInHierarchy)
                currentPuzzle = puzzle.gameObject;
        }

        // Определяем тип

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
            if (collected) // Если засчитан сбор очка, то увеличиваем счетчик очков
            {
                collectableCounter++;
                Debug.Log("Counter: " + collectableCounter);

                if (collectableCounter == numberOfCollectables)
                {
                    Debug.Log("All Collected");
                    allCollected = true; // Используется в PuzzlePoint
                }
            }
            else if (!collected && !wrongCollectable) // Если линия больше не проведена по очку, то уменьшаем счетчик
            {
                collectableCounter--;
                allCollected = false;
                Debug.Log("Counter: " + collectableCounter);
            }
        }
    }

    private void GetCollectablesNumber() // Метод для изменения numberOfCollectables на количество очков в текущей головоломке (при ее активации)
    {
        // Сначала находим в иерархии активную головоломку (по тегам)

        GameObject currentPuzzle = null;

        foreach (var puzzle in puzzleArray)
        {
            if (puzzle.activeInHierarchy && (puzzle.tag == "PuzzleSecondType" || puzzle.tag == "PuzzleThirdType"))
                currentPuzzle = puzzle.gameObject;
        }

        // Затем считаем в ней количество очков (дочерних объектов с определенными тегами)

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

        // Присваиваем количество очков
        numberOfCollectables = numberOfChildren;
        numberOfChildren = 0;
    }
}

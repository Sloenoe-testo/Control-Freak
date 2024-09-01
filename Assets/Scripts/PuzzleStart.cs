using UnityEngine;

public class PuzzleStart : MonoBehaviour
{
    public static bool isStarted = false;

    [SerializeField] Color defaultColor;
    [SerializeField] Color hoverColor;
    [SerializeField] Color greenColor;

    [SerializeField] GameObject pointer;

    private void Awake()
    {
        DisablePointer(false);
    }

    private void OnEnable()
    {
        PuzzlePoint.OnLineReseted += DisablePointer;
        PuzzlePoint.OnFinished += DisablePointer;
        PuzzleSphere.OnPuzzleExited += DisablePointer;
    }

    private void OnDisable()
    {
        PuzzlePoint.OnLineReseted -= DisablePointer;
        PuzzlePoint.OnFinished -= DisablePointer;
        PuzzleSphere.OnPuzzleExited -= DisablePointer;
    }

    private void DisablePointer(bool finished)
    {
        pointer.SetActive(false); // Отключаем указатель

        if (!finished)
            gameObject.GetComponent<SpriteRenderer>().color = defaultColor; // Делаем точку старта дефолтного красного цвета
    }

    private void OnMouseEnter() // При наведении на точку старта она становится темнее
    {
        if (!isStarted)
        {
            gameObject.GetComponent<SpriteRenderer>().color = hoverColor;
        }
    }

    private void OnMouseExit() // Когда убираем мышку с точки старта, она снова становится дефолтного цвета
    {
        if (!isStarted)
        {
            gameObject.GetComponent<SpriteRenderer>().color = defaultColor;
        }
    }

    private void OnMouseDown() // При нажатии на кнопку старта
    {
        Debug.Log("Start");

        gameObject.GetComponent<SpriteRenderer>().color = greenColor; // Точка старта меняет цвет на зеленый

        // Перемещаем указатель на точку старта и включаем его
        pointer.transform.position = FindObjectOfType<PuzzleStart>().transform.position; // FindObjectOfType находит именно активный объект на сцене 
        pointer.SetActive(true);

        isStarted = true;
    }
}

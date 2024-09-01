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
        pointer.SetActive(false); // ��������� ���������

        if (!finished)
            gameObject.GetComponent<SpriteRenderer>().color = defaultColor; // ������ ����� ������ ���������� �������� �����
    }

    private void OnMouseEnter() // ��� ��������� �� ����� ������ ��� ���������� ������
    {
        if (!isStarted)
        {
            gameObject.GetComponent<SpriteRenderer>().color = hoverColor;
        }
    }

    private void OnMouseExit() // ����� ������� ����� � ����� ������, ��� ����� ���������� ���������� �����
    {
        if (!isStarted)
        {
            gameObject.GetComponent<SpriteRenderer>().color = defaultColor;
        }
    }

    private void OnMouseDown() // ��� ������� �� ������ ������
    {
        Debug.Log("Start");

        gameObject.GetComponent<SpriteRenderer>().color = greenColor; // ����� ������ ������ ���� �� �������

        // ���������� ��������� �� ����� ������ � �������� ���
        pointer.transform.position = FindObjectOfType<PuzzleStart>().transform.position; // FindObjectOfType ������� ������ �������� ������ �� ����� 
        pointer.SetActive(true);

        isStarted = true;
    }
}

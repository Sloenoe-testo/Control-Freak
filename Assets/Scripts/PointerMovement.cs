using System.Collections.Generic;
using UnityEngine;

public class PointerMovement : MonoBehaviour
{
    [SerializeField] private Camera puzzleCamera;
    public static Rigidbody2D pointerRigidbody;
    [SerializeField] private float speed = 0.1f;
    private Vector3 mousePostion;
    private Vector2 position = new Vector2 (0f, 0f);

    [SerializeField] EdgeCollider2D lineCollider;
    private List<GameObject> pointsList;

    public static bool isStarted = false;

    private void Awake()
    {
        pointerRigidbody = GetComponent<Rigidbody2D>();
        pointsList = PuzzlePoint.pointsList;
    }

    private void OnEnable()
    {
        PuzzlePoint.OnLineChanged += ChangeLineCollider;
    }

    private void OnDisable()
    {
        PuzzlePoint.OnLineChanged -= ChangeLineCollider;
    }

    private void Update()
    {
        // –ассчет движени€ указател€ за мышью
        mousePostion = puzzleCamera.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 222));
        position = Vector2.Lerp(transform.position, mousePostion, speed);
    }

    private void FixedUpdate()
    {
        // ѕрисваивание позиции указателю (с использованием физики, чтобы сталкивалс€ с коллайдерами)
        pointerRigidbody.MovePosition(position);
    }

    // »зменение EdgeCollider линии во врем€ добавлени€ сегментов
    private void ChangeLineCollider()
    {
        if (pointsList.Count >= 2) //  оллайдер добавл€етс€, начина€ с предпоследней точки (чтобы он не вытесн€л указатель)
        {
            Vector2[] points = lineCollider.points;

            points.SetValue(pointsList[pointsList.Count - 2], pointsList.Count - 2); // ѕрисваиваем точку коллайдеру в соответствии с текущей точкой в pointsList

            // ќстальным точкам в массиве присваиваем значение текущей точки
            for (int i = pointsList.Count; i < points.Length; i++)
            {
                points.SetValue(points[pointsList.Count - 2], i);
            }

            lineCollider.points = points;
        }
    }
}

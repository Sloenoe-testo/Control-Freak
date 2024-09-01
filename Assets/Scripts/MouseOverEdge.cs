using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverEdge : MonoBehaviour, IPointerEnterHandler
{
    [SerializeField] private GameObject pointer;
    public static GameObject mouseEdge = null;
    private List<GameObject> edges;

    public float x;
    public float y;

    public static Vector3[] v; // Массив с координатами углов UI-элемента

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEdge = eventData.pointerCurrentRaycast.gameObject;
        Debug.Log("mouseEdge:" + mouseEdge);

        v = new Vector3[4]; // Присваиваем массиву 4 вектора для 4 углов
        mouseEdge.GetComponent<RectTransform>().GetWorldCorners(v); // Присваиваем векторам значения
    }
}
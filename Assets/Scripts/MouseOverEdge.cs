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

    public static Vector3[] v; // ������ � ������������ ����� UI-��������

    public void OnPointerEnter(PointerEventData eventData)
    {
        mouseEdge = eventData.pointerCurrentRaycast.gameObject;
        Debug.Log("mouseEdge:" + mouseEdge);

        v = new Vector3[4]; // ����������� ������� 4 ������� ��� 4 �����
        mouseEdge.GetComponent<RectTransform>().GetWorldCorners(v); // ����������� �������� ��������
    }
}
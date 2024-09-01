using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class MenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{

    public TextMeshProUGUI text;
    [SerializeField] private Color defaultColor;
    [SerializeField] private Color hoverColor;
    [SerializeField] private Color clickColor;

    public void OnPointerEnter(PointerEventData eventData)
    {
        text.color = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        text.color = defaultColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        text.color = clickColor;
    }
}
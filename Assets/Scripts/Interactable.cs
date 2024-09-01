using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public virtual void Awake()
    {
        gameObject.layer = 6; // Устанавливаем объекту слой "Interactable"
    }

    public abstract void OnFocus(); // Когда луч попадает на интерактивный объект
    public abstract void OnInteract(); // Когда луч попадает на интерактивный объект и мы нажимаем клавишу взаимодействия
    public abstract void OnLoseFocus(); // Когда луч перестал попадать на интерактивный объект
}

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [Header("Crosshair Parameters")]
    [SerializeField] private Image crosshair;
    [SerializeField] private Sprite defaultCrosshairSprite;
    [SerializeField] private Sprite interactiveCrosshairSprite;

    [Header("Stamina Parameters")]
    [SerializeField] private TextMeshProUGUI staminaText = default;
    [SerializeField] private Image staminaBG;
    [SerializeField] private Image staminaSliderBar;
    [SerializeField] private Color defaultStaminaColor = default;
    [SerializeField] private Color recoverStaminaColor = default;
    [SerializeField] Animator staminaBGAnimator = null;
    [SerializeField] Animator staminaCanvasGPAnimator = null;
    private Coroutine closingStamina;

    private void OnEnable()
    {
        FirstPersonController.OnFocused += ChangeCrosshair;
        FirstPersonController.OnStaminaChange += UpdateStamina; // Обращаемся к событию и подписываем метод UpdateStamina
        CharacterSwitch.OnStartedEnteringCapsule += ForceCloseStamina;
        CharacterSwitch.OnStartedEnteringCapsule += ForceChangeCrosshair;
    }

    private void OnDisable()
    {
        FirstPersonController.OnFocused -= ChangeCrosshair;
        FirstPersonController.OnStaminaChange -= UpdateStamina; // Обращаемся к событию и отписываем метод UpdateStamina
        CharacterSwitch.OnStartedEnteringCapsule -= ForceCloseStamina;
        CharacterSwitch.OnStartedEnteringCapsule -= ForceChangeCrosshair;
    }

    void Start()
    {
        UpdateStamina(100, false);
    }

    // Метод, меняющий прицел при наведении на интерактивный объект
    void ChangeCrosshair(bool onFocused)
    {
        if (onFocused)
            crosshair.enabled = true;
        else crosshair.enabled = false;
    }

    void UpdateStamina(float currentStamina, bool staminaRecovering)
    {
        staminaText.text = currentStamina.ToString("00"); // Удалить позже

        //staminaSliderBar.fillAmount = currentStamina / 100; // Обычное уменьшение шкалы стамины влево

        staminaSliderBar.transform.localScale = new Vector3(currentStamina / 100, 1f, 1f); // Симметричное уменьшение шкалы стамины к центру

        //Изменение цвета шкалы стамины во время рекаверинга
        if (staminaRecovering)
            staminaSliderBar.color = recoverStaminaColor;
        else staminaSliderBar.color = defaultStaminaColor;

        // Останавливаем анимацию закрытия стамины (корутину), если в этот момент снова побежали (снова нужно проиграть анимацию открытья)
        if (closingStamina != null)
        {
            StopCoroutine(closingStamina);
            closingStamina = null;
        }

        // Анимация закрытия стамины
        if (currentStamina >= 100)
            closingStamina = StartCoroutine(CloseStamina());
        else staminaBGAnimator.SetBool("Open", true);

        // Анимация мигания стамины
        if (staminaRecovering)
            staminaCanvasGPAnimator.SetBool("Blink", true);
        else staminaCanvasGPAnimator.SetBool("Blink", false);
    }

    // Корутина нужна, чтобы анимация закрытия стамины проигрывалась не сразу по достижении 100%, а через 1 секунду + чтобы можно было прервать эту анимацию
    private IEnumerator CloseStamina()
    {
        yield return new WaitForSeconds(1f);
        staminaBGAnimator.SetBool("Open", false);

        closingStamina = null;
    }

    private void ForceCloseStamina() // Чтобы стамина сразу закрывалась при нажатии на капсулу
    {
        StartCoroutine(CloseStamina());
    }

    private void ForceChangeCrosshair()
    {
        ChangeCrosshair(false);
    }
}

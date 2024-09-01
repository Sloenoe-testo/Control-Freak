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
        FirstPersonController.OnStaminaChange += UpdateStamina; // ���������� � ������� � ����������� ����� UpdateStamina
        CharacterSwitch.OnStartedEnteringCapsule += ForceCloseStamina;
        CharacterSwitch.OnStartedEnteringCapsule += ForceChangeCrosshair;
    }

    private void OnDisable()
    {
        FirstPersonController.OnFocused -= ChangeCrosshair;
        FirstPersonController.OnStaminaChange -= UpdateStamina; // ���������� � ������� � ���������� ����� UpdateStamina
        CharacterSwitch.OnStartedEnteringCapsule -= ForceCloseStamina;
        CharacterSwitch.OnStartedEnteringCapsule -= ForceChangeCrosshair;
    }

    void Start()
    {
        UpdateStamina(100, false);
    }

    // �����, �������� ������ ��� ��������� �� ������������� ������
    void ChangeCrosshair(bool onFocused)
    {
        if (onFocused)
            crosshair.enabled = true;
        else crosshair.enabled = false;
    }

    void UpdateStamina(float currentStamina, bool staminaRecovering)
    {
        staminaText.text = currentStamina.ToString("00"); // ������� �����

        //staminaSliderBar.fillAmount = currentStamina / 100; // ������� ���������� ����� ������� �����

        staminaSliderBar.transform.localScale = new Vector3(currentStamina / 100, 1f, 1f); // ������������ ���������� ����� ������� � ������

        //��������� ����� ����� ������� �� ����� �����������
        if (staminaRecovering)
            staminaSliderBar.color = recoverStaminaColor;
        else staminaSliderBar.color = defaultStaminaColor;

        // ������������� �������� �������� ������� (��������), ���� � ���� ������ ����� �������� (����� ����� ��������� �������� ��������)
        if (closingStamina != null)
        {
            StopCoroutine(closingStamina);
            closingStamina = null;
        }

        // �������� �������� �������
        if (currentStamina >= 100)
            closingStamina = StartCoroutine(CloseStamina());
        else staminaBGAnimator.SetBool("Open", true);

        // �������� ������� �������
        if (staminaRecovering)
            staminaCanvasGPAnimator.SetBool("Blink", true);
        else staminaCanvasGPAnimator.SetBool("Blink", false);
    }

    // �������� �����, ����� �������� �������� ������� ������������� �� ����� �� ���������� 100%, � ����� 1 ������� + ����� ����� ���� �������� ��� ��������
    private IEnumerator CloseStamina()
    {
        yield return new WaitForSeconds(1f);
        staminaBGAnimator.SetBool("Open", false);

        closingStamina = null;
    }

    private void ForceCloseStamina() // ����� ������� ����� ����������� ��� ������� �� �������
    {
        StartCoroutine(CloseStamina());
    }

    private void ForceChangeCrosshair()
    {
        ChangeCrosshair(false);
    }
}

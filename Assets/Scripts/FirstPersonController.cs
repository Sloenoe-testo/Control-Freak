using System;
using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey); // Читаем переменную, только если canSprint и Input.GetKey(sprintKey) - true

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool useFootsteps = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool canInteract = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode interactKey = KeyCode.Mouse0;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4.0f;
    [SerializeField] private float sprintSpeed = 7.0f;
    [SerializeField] private float gravity = 30.0f;

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 100)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 100)] private float lowerLookLimit = 80.0f;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.1f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float sprintStepMultiplier = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] floorClips = default;
    [SerializeField] private AudioClip[] iceClips = default;
    [SerializeField] private float lowerPitchLimit = 0.9f;
    [SerializeField] private float upperPitchLimit = 1.1f;
    private float footstepTimer = 0; // Время между шагами
    private float GetCurrentOffset => IsSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed; // Свойство, которое определяет скорость шагов в зависимости от того, бежим мы или нет
    [SerializeField] private float walkVolume = 0.15f;
    [SerializeField] private float runVolume = 0.3f;
    private float footStepVolume => IsSprinting ? runVolume : walkVolume;

    [Header("Interaction Parameters")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentIntereactable; // Референс интерактивного объекта, который наследуется от абстрактного класса Interactable
    public static Action<bool> OnFocused; // Событие, нужное для изменения прицела на интерактивных объектах

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5; // Количество единиц стамины, которое мы теряем за секунду во время бега
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaValueIncrement = 2; // Количество единиц стамины, которое мы прибавляем за секунду во время регенерации
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private bool staminaRecovering = false; // Регенерация стамины после ее полного израсходования
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float, bool> OnStaminaChange; // Событие, которое передает currentStamina и staminaRecovering

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; // Финальное значение движения, которое мы передаем CharacterController'у
    private Vector2 currentInput; // Значение, которое мы передаем CharacterController'у через клавиатуру (перемещение)

    private float rotationX = 0; // Мы будем ограничивать эту переменную с помощью upperLookLimit и lowerLookLimit

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y; // Дефолтная позиция камеры, которая нужна для headbob'а
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnEnable()
    {
        CharacterSwitch.OnExitedCapsule += HandleStamina;
    }

    private void OnDisable()
    {
        CharacterSwitch.OnExitedCapsule -= HandleStamina;
    }

    void Update()
    {
        if (CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();

            if (canUseHeadbob)
                HandleHeadbob();

            if (useFootsteps)
                HandleFootsteps();

            if (canInteract)
            {
                HandleInteractionCheck();
                HandleInteractionInput();
            }

            if (useStamina)
                HandleStamina(false);

            ApplyFinalMovements();
        }
    }
    // Контролирует ввод с клавиатуры
    private void HandleMovementInput()
    {
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); // Получаем ввод с клавиатуры и умножаем на скорость (если спринтуем, то sprintSpeed, иначе walkSpeed)

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y); // Рассчитываем направление движения
        moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, (IsSprinting ? sprintSpeed : walkSpeed)); // Чтобы скорость не увеличивалась при движении по диагонали
        moveDirection.y = moveDirectionY;
    }
    // Контролирует управление мышью
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); // Ограничиваем поворот по X (вверх и вниз)
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // Применяем поворот по X к камере игрока
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0); // Для поворота по Y (вправо и влево) мы поворачиваем не камеру, а сам объект
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1 || Mathf.Abs(moveDirection.z) > 0.1f) // Проверяем, двигаемся ли мы
        {
            timer += Time.deltaTime * (IsSprinting ? sprintBobSpeed : walkBobSpeed); // Рассчитываем интервал движения камеры во время покачивания
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z); // Задаем движение камеры игрока вверх и вниз так, чтобы оно совпадало с рассчетами
        }
    }

    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepAudioSource.volume = footStepVolume;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            footstepAudioSource.pitch = UnityEngine.Random.Range(lowerPitchLimit, upperPitchLimit); // Изменение высоты звука для добавления вариативности

            if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag) // Проверяем теги у объекта, на который попадает луч
                {
                    case "Footsteps/FLOOR":
                        footstepAudioSource.PlayOneShot(floorClips[UnityEngine.Random.Range(0, floorClips.Length - 1)]);
                        break;
                    case "Footsteps/ICE":
                        footstepAudioSource.PlayOneShot(iceClips[UnityEngine.Random.Range(0, floorClips.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(floorClips[UnityEngine.Random.Range(0, floorClips.Length - 1)]);
                        break;
                }
            }

            footstepTimer = GetCurrentOffset;
        }
    }
    // Метод, который постоянно ищет интерактивные объекты с помощью рейкаста
    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 6 && (currentIntereactable == null || hit.collider.gameObject.GetInstanceID() != currentIntereactable.gameObject.GetInstanceID())) // Если объект на слое "Interactable", и currentIntereactable null или это другой интерактивный объект (не тот, что у нас сохранен в currentIntereactable)
            {
                hit.collider.TryGetComponent(out currentIntereactable); // Получаем компонент <Interactable> и, если он есть у объекта, делаем этот объект новым currentIntereactable

                if (currentIntereactable) // Если currentIntereactable не null
                {
                    currentIntereactable.OnFocus();
                    OnFocused?.Invoke(true); // Запускаем изменение прицела в скрипте UI
                }
            }
        }
        else if (currentIntereactable) // Если рейкаст ничего не находит, но currentIntereactable не null
        {
            currentIntereactable.OnLoseFocus();
            currentIntereactable = null;
            OnFocused?.Invoke(false);
        }
    }
    // Метод, который срабатывает, когда мы нажимаем клавишу взаимодействия
    private void HandleInteractionInput()
    {
        if(Input.GetKeyDown(interactKey) && currentIntereactable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer)) // Если мы нажали клавишу взаимодействия и если луч попадает на интерактивный объект
        {
            currentIntereactable.OnInteract();
        }
    }

    private void HandleStamina(bool exitedCapsule)
    {
        if (exitedCapsule) // При выходе из капсулы стамина сбрасывается
        {
            currentStamina = 100;
        }

        if (IsSprinting && currentInput != Vector2.zero) // Стамина расходуется, только если мы нажимаем shift И НЕ стоим на месте
        {   // Останавливаем регенерацию (корутину), если началась новая регенерация (корутина), но старая не закончилась
            if (regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }

            currentStamina -= staminaUseMultiplier * Time.deltaTime;

            if (currentStamina < 0)
                currentStamina = 0;

            if (currentStamina <= 0)
            {
                staminaRecovering = true;
                canSprint = false;
            }

            OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // Если у события есть подписанные на него методы, то запускаем событие (запускаем изменение отображения стамины на UI)
        }

        if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null) // Стамина восстанавливается, даже если мы стоим на месте
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void ApplyFinalMovements()
    {   // Гравитация
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        
        if (characterController.enabled)
            characterController.Move(moveDirection * Time.deltaTime); // Присваеваем направление движения, которое мы рассчитали в HandleMovementInput()
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts); // Ждем несколько секунд, затем продолжаем выполнение корутины
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while ((currentStamina < maxStamina) && !CharacterSwitch.isCaph)
        {
            if (currentStamina > 0 && !staminaRecovering)
            {
                canSprint = true;
            }

            currentStamina += staminaValueIncrement;

            if (currentStamina > maxStamina)
                currentStamina = maxStamina;

            OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // Если у события есть подписанные на него методы, то запускаем событие (запускаем изменение отображения стамины на UI)

            yield return timeToWait;
        }
        // Когда стамина полностью восстановилась, мы можем бежать
        staminaRecovering = false;
        canSprint = true;

        OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // Чтобы стамина изменяла цвет с красного на дефолтный, как только она полностью восстановится

        regeneratingStamina = null; // Регенерация закончилась (корутина закончилась)
    }
}

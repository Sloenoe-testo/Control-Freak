using System;
using System.Collections;
using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove { get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey); // ������ ����������, ������ ���� canSprint � Input.GetKey(sprintKey) - true

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
    private float footstepTimer = 0; // ����� ����� ������
    private float GetCurrentOffset => IsSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed; // ��������, ������� ���������� �������� ����� � ����������� �� ����, ����� �� ��� ���
    [SerializeField] private float walkVolume = 0.15f;
    [SerializeField] private float runVolume = 0.3f;
    private float footStepVolume => IsSprinting ? runVolume : walkVolume;

    [Header("Interaction Parameters")]
    [SerializeField] private Vector3 interactionRayPoint = default;
    [SerializeField] private float interactionDistance = default;
    [SerializeField] private LayerMask interactionLayer = default;
    private Interactable currentIntereactable; // �������� �������������� �������, ������� ����������� �� ������������ ������ Interactable
    public static Action<bool> OnFocused; // �������, ������ ��� ��������� ������� �� ������������� ��������

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5; // ���������� ������ �������, ������� �� ������ �� ������� �� ����� ����
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaValueIncrement = 2; // ���������� ������ �������, ������� �� ���������� �� ������� �� ����� �����������
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private bool staminaRecovering = false; // ����������� ������� ����� �� ������� ��������������
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float, bool> OnStaminaChange; // �������, ������� �������� currentStamina � staminaRecovering

    private Camera playerCamera;
    private CharacterController characterController;

    private Vector3 moveDirection; // ��������� �������� ��������, ������� �� �������� CharacterController'�
    private Vector2 currentInput; // ��������, ������� �� �������� CharacterController'� ����� ���������� (�����������)

    private float rotationX = 0; // �� ����� ������������ ��� ���������� � ������� upperLookLimit � lowerLookLimit

    void Awake()
    {
        playerCamera = GetComponentInChildren<Camera>();
        characterController = GetComponent<CharacterController>();
        defaultYPos = playerCamera.transform.localPosition.y; // ��������� ������� ������, ������� ����� ��� headbob'�
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
    // ������������ ���� � ����������
    private void HandleMovementInput()
    {
        currentInput = new Vector2((IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal")); // �������� ���� � ���������� � �������� �� �������� (���� ���������, �� sprintSpeed, ����� walkSpeed)

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y); // ������������ ����������� ��������
        moveDirection = moveDirection.normalized * Mathf.Clamp(moveDirection.magnitude, 0, (IsSprinting ? sprintSpeed : walkSpeed)); // ����� �������� �� ������������� ��� �������� �� ���������
        moveDirection.y = moveDirectionY;
    }
    // ������������ ���������� �����
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit); // ������������ ������� �� X (����� � ����)
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0); // ��������� ������� �� X � ������ ������
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0); // ��� �������� �� Y (������ � �����) �� ������������ �� ������, � ��� ������
    }

    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if (Mathf.Abs(moveDirection.x) > 0.1 || Mathf.Abs(moveDirection.z) > 0.1f) // ���������, ��������� �� ��
        {
            timer += Time.deltaTime * (IsSprinting ? sprintBobSpeed : walkBobSpeed); // ������������ �������� �������� ������ �� ����� �����������
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z); // ������ �������� ������ ������ ����� � ���� ���, ����� ��� ��������� � ����������
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
            footstepAudioSource.pitch = UnityEngine.Random.Range(lowerPitchLimit, upperPitchLimit); // ��������� ������ ����� ��� ���������� �������������

            if (Physics.Raycast(characterController.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag) // ��������� ���� � �������, �� ������� �������� ���
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
    // �����, ������� ��������� ���� ������������� ������� � ������� ��������
    private void HandleInteractionCheck()
    {
        if (Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance))
        {
            if (hit.collider.gameObject.layer == 6 && (currentIntereactable == null || hit.collider.gameObject.GetInstanceID() != currentIntereactable.gameObject.GetInstanceID())) // ���� ������ �� ���� "Interactable", � currentIntereactable null ��� ��� ������ ������������� ������ (�� ���, ��� � ��� �������� � currentIntereactable)
            {
                hit.collider.TryGetComponent(out currentIntereactable); // �������� ��������� <Interactable> �, ���� �� ���� � �������, ������ ���� ������ ����� currentIntereactable

                if (currentIntereactable) // ���� currentIntereactable �� null
                {
                    currentIntereactable.OnFocus();
                    OnFocused?.Invoke(true); // ��������� ��������� ������� � ������� UI
                }
            }
        }
        else if (currentIntereactable) // ���� ������� ������ �� �������, �� currentIntereactable �� null
        {
            currentIntereactable.OnLoseFocus();
            currentIntereactable = null;
            OnFocused?.Invoke(false);
        }
    }
    // �����, ������� �����������, ����� �� �������� ������� ��������������
    private void HandleInteractionInput()
    {
        if(Input.GetKeyDown(interactKey) && currentIntereactable != null && Physics.Raycast(playerCamera.ViewportPointToRay(interactionRayPoint), out RaycastHit hit, interactionDistance, interactionLayer)) // ���� �� ������ ������� �������������� � ���� ��� �������� �� ������������� ������
        {
            currentIntereactable.OnInteract();
        }
    }

    private void HandleStamina(bool exitedCapsule)
    {
        if (exitedCapsule) // ��� ������ �� ������� ������� ������������
        {
            currentStamina = 100;
        }

        if (IsSprinting && currentInput != Vector2.zero) // ������� �����������, ������ ���� �� �������� shift � �� ����� �� �����
        {   // ������������� ����������� (��������), ���� �������� ����� ����������� (��������), �� ������ �� �����������
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

            OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // ���� � ������� ���� ����������� �� ���� ������, �� ��������� ������� (��������� ��������� ����������� ������� �� UI)
        }

        if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null) // ������� �����������������, ���� ���� �� ����� �� �����
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private void ApplyFinalMovements()
    {   // ����������
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;
        
        if (characterController.enabled)
            characterController.Move(moveDirection * Time.deltaTime); // ����������� ����������� ��������, ������� �� ���������� � HandleMovementInput()
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts); // ���� ��������� ������, ����� ���������� ���������� ��������
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

            OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // ���� � ������� ���� ����������� �� ���� ������, �� ��������� ������� (��������� ��������� ����������� ������� �� UI)

            yield return timeToWait;
        }
        // ����� ������� ��������� ��������������, �� ����� ������
        staminaRecovering = false;
        canSprint = true;

        OnStaminaChange?.Invoke(currentStamina, staminaRecovering); // ����� ������� �������� ���� � �������� �� ���������, ��� ������ ��� ��������� �������������

        regeneratingStamina = null; // ����������� ����������� (�������� �����������)
    }
}

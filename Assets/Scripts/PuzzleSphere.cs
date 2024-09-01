using Radishmouse;
using System;
using System.Collections;
using UnityEngine;

public class PuzzleSphere : Interactable
{
    [SerializeField] private KeyCode StartExitingPuzzleKey = KeyCode.Escape;
    public static bool puzzleEnabled = false;

    [SerializeField] private CharacterController caphCharacterController;
    [SerializeField] private FirstPersonController caphFirstPersonController;

    [SerializeField] private GameObject puzzleCamera;
    [SerializeField] private GameObject puzzle;
    private Animator cameraAC;
    [SerializeField] private Canvas lineCanvas;
    private UILineRenderer uiLineRenderer;

    [SerializeField] private Material sphereMaterial;
    [SerializeField] private Material starsMaterial;
    [SerializeField] private Material raysMaterial;
    [SerializeField] private Color redColor;
    [SerializeField] private Color greenColor;
    [SerializeField] private Color raysRedColor;
    [SerializeField] private Color raysGreenColor;
    [SerializeField] private float redColorMultiplier = 12f;
    [SerializeField] private float greenColorMultiplier = 5f;
    [SerializeField] private float colorChangeTime;

    [SerializeField] GameObject sphere;
    [SerializeField] GameObject stars;
    [SerializeField] GameObject rays;

    public static Action OnPuzzleEnabled;
    public static Action<bool> OnPuzzleExited;

    [SerializeField] private GameObject crosshair;

    private AudioSource puzzleSource;
    [SerializeField] private AudioClip puzzleOpen;
    [SerializeField] private AudioClip puzzleClose;
    [SerializeField] private float puzzleOpenVolume = 0.5f;
    [SerializeField] private float puzzleCloseVolume = 0.3f;

    private new void Awake()
    {
        uiLineRenderer = lineCanvas.GetComponentInChildren<UILineRenderer>();
        cameraAC = puzzleCamera.GetComponent<Animator>();
        puzzleSource = GetComponent<AudioSource>();

        // � ������ ������������� ������� ���� � ������ ��� ���� ���� ���������
        redColor *= redColorMultiplier; // �������� ���� � ����� ������� �������� �������� "Intensity" � ����� Emission
        sphereMaterial.SetColor("_EmissionColor", redColor);
        starsMaterial.SetColor("_EmissionColor", redColor);
        raysMaterial.SetColor("_EmissionColor", raysRedColor);

        // ������� ����� ���������� � ��������� �� � �������� (����� � ������ ����� ��� ���� ��������)
        sphereMaterial = new Material(sphereMaterial);
        sphere.GetComponent<Renderer>().material = sphereMaterial;
        starsMaterial = new Material(starsMaterial);
        stars.GetComponent<Renderer>().material = starsMaterial;
        raysMaterial = new Material(raysMaterial);
        rays.GetComponent<Renderer>().material = raysMaterial;

        lineCanvas.enabled = false; // �������� �� false
    }

    private void OnEnable()
    {
        PuzzlePoint.OnFinished += StartExitingPuzzle;
    }

    private void OnDisable()
    {
        PuzzlePoint.OnFinished -= StartExitingPuzzle;
    }

    public override void OnFocus()
    {

    }

    public override void OnInteract()
    {
        if (CharacterSwitch.isCaph)
        {
            caphCharacterController.enabled = false;
            caphFirstPersonController.enabled = false;
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;

            cameraAC.Play("Open"); // �������� �������� ������, ���������� �����������

            puzzleSource.volume = puzzleOpenVolume;
            puzzleSource.PlayOneShot(puzzleOpen);

            puzzle.SetActive(true);
            lineCanvas.enabled = true;
            crosshair.SetActive(false);

            puzzleEnabled = true;
            OnPuzzleEnabled?.Invoke();
        }
    }

    public override void OnLoseFocus()
    {

    }

    private void Update()
    {
        // ���� �� ����� ����� �� �����������, �� ����� �� (������ Escape)
        if (puzzleEnabled && Input.GetKeyDown(StartExitingPuzzleKey))
            StartExitingPuzzle(false);
    }

    private void StartExitingPuzzle(bool isFinished)
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        cameraAC.Play("Close"); // �������� �������� ������, ���������� �����������

        puzzleSource.volume = puzzleCloseVolume;
        puzzleSource.PlayOneShot(puzzleClose);

        StartCoroutine(EndExitingPuzzle(isFinished)); // �������� �����, ����� ����� ������� ���������� ������ ����� �������� �����������, � �� �����������
    }

    private IEnumerator EndExitingPuzzle(bool isFinished)
    {
        yield return new WaitForSeconds(0.3f);

        // ���� ����������� ������
        if (isFinished)
        {
            // ������� �����
            UILineRenderer.pointsList.Clear();
            uiLineRenderer.SetAllDirty();

            PuzzleStart.isStarted = false;

            StartCoroutine(ChangeSphereColor());
        }

        // �������� ���������� �����, ���� �������� ���������������� �����
        if (!SphereManager.allSpheresActivated)
        {
            caphCharacterController.enabled = true;
            caphFirstPersonController.enabled = true;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lineCanvas.enabled = false;
        puzzle.SetActive(false);
        puzzleEnabled = false;
        crosshair.SetActive(true);
        OnPuzzleExited?.Invoke(false);
    }

    // �������� ��� ��������� ����� ����� � �������� �� �������
    private IEnumerator ChangeSphereColor()
    {
        Color lerpedColor;
        float currentTime = 0;

        greenColor *= greenColorMultiplier; // �������� ������������� �������� emission

        // ����������� ��������� �����
        while (currentTime < colorChangeTime)
        {
            lerpedColor = Color.Lerp(redColor, greenColor, currentTime += Time.deltaTime);
            sphereMaterial.SetColor("_EmissionColor", lerpedColor);
            starsMaterial.SetColor("_EmissionColor", lerpedColor);

            lerpedColor = Color.Lerp(raysRedColor, raysGreenColor, currentTime += Time.deltaTime);
            raysMaterial.SetColor("_EmissionColor", lerpedColor);

            yield return null;
        }
    }
}

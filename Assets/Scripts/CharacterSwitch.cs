using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CharacterSwitch : MonoBehaviour
{
    [SerializeField] List<GameObject> capsuleList;
    public static int i = 0; // Индекс капсулы в списке capsuleList

    [SerializeField] private GameObject mc;
    [SerializeField] private GameObject robot;

    [Header("MC Parameters")]
    private CharacterController mcCharacterController;
    private FirstPersonController mcFirstPersonController;
    private Camera mcCamera;
    private GameObject mcMesh;
    private GameObject mcFlashlight;

    [Header("Caph Parameters")]
    private CharacterController robotCharacterController;
    private FirstPersonController robotFirstPersonController;
    private Camera robotCamera;
    private Animator robotAnimator;
    private SkinnedMeshRenderer robotMesh;
    private NavMeshAgent robotAgent;
    private CaphChase caphChase;
    private GameObject robotFlashlight;

    [SerializeField] private KeyCode switchToMCKey = KeyCode.E;

    [Header("Capsule Entering/Exeting Movement")]
    [SerializeField] float moveDuration = 3f;
    private GameObject enterTarget;
    private GameObject exitTarget;
    private Vector3 enterTargetPos;
    private Quaternion enterTargetRot;
    private Vector3 exitTargetPos;
    [SerializeField] private AnimationCurve posAnimationCurve;
    [SerializeField] private AnimationCurve rotAnimationCurve;

    private bool isInCapsule = false;

    [SerializeField] private Image caphHUD;
    [SerializeField] Animator blackScreenAC = null;

    public static bool isCaph = false;
    public static bool exitedCapsule = false;
    public static Action OnStartedExitingCapsule;
    public static Action OnStartedEnteringCapsule;
    public static Action<bool> OnExitedCapsule;
    public static Action OnEnteredCapsule;
    public static int usedCapsules = 0;

    private AudioSource caphSource;
    [SerializeField] private AudioClip switchSound;

    void Awake()
    {
        mcCharacterController = mc.GetComponent<CharacterController>();
        mcFirstPersonController = mc.GetComponent<FirstPersonController>();
        mcCamera = mc.GetComponentInChildren<Camera>();
        mcMesh = mc.transform.Find("MC mesh").gameObject;
        mcFlashlight = mc.transform.Find("Camera Holder/MC Camera/Flashlight").gameObject;

        robotCharacterController = robot.GetComponent<CharacterController>();
        robotFirstPersonController = robot.GetComponent<FirstPersonController>();
        robotCamera = robot.GetComponentInChildren<Camera>();
        robotAnimator = robot.GetComponent<Animator>();
        robotMesh = robot.GetComponentInChildren<SkinnedMeshRenderer>();
        robotAgent = robot.GetComponent<NavMeshAgent>();
        caphChase = robot.GetComponent<CaphChase>();
        robotFlashlight = robot.transform.Find("Robot Camera/Flashlight").gameObject;
        caphSource = robot.GetComponent<AudioSource>();

        mcCharacterController.enabled = true;
        mcFirstPersonController.enabled = true;
        mcCamera.enabled = true;
        mcMesh.SetActive(false);
        mcFlashlight.SetActive(true);
    }

    private void OnEnable()
    {
        ControlCapsule.OnInteracted += SwitchToRobot;
        SphereManager.OnAllSpheresActivated += SwitchToMC;
    }

    private void OnDisable()
    {
        ControlCapsule.OnInteracted -= SwitchToRobot;
        SphereManager.OnAllSpheresActivated -= SwitchToMC;
    }

    //void Update()
    //{
    //    // Для тестирования
    //    if (Input.GetKeyDown(switchToMCKey))
    //        SwitchToMC();
    //}

    private void ManageCapsule()
    {
        // Получаем точки входа и выхода у текущей активной капсулы
        enterTarget = capsuleList[i].transform.Find("Enter target").gameObject;
        exitTarget = capsuleList[i].transform.Find("Exit target").gameObject;
        enterTargetPos = enterTarget.transform.position;
        enterTargetRot = enterTarget.transform.rotation;
        exitTargetPos = exitTarget.transform.position;

        // Отключаем коллайдер у использованной капсулы, чтобы нельзя было снова ее использовать
        GameObject capsuleInteractive = capsuleList[i].transform.Find("Capsule interactive").gameObject;
        capsuleInteractive.GetComponent<Collider>().enabled = false;

        if (i < capsuleList.Count - 1) // Если индекс не выходит за пределы количества объектов в списке капсул
            i += 1; // Увеличиваем индекс на 1, чтобы после каждой использованной капсулы, активной становилась следующая капсула
    }

    private void SwitchToRobot()
    {
        ManageCapsule();

        StartCoroutine(CapsuleEnter(mc.transform.position, mc.transform.rotation, enterTargetPos, moveDuration));

        OnStartedEnteringCapsule?.Invoke();
        isCaph = true;
    }

    private void SwitchToMC()
    {
        if (isInCapsule)
            StartCoroutine(CapsuleExit(mc.transform.position, exitTargetPos, moveDuration));

        OnStartedExitingCapsule?.Invoke();
        isCaph = false;
        exitedCapsule = true;
    }

    // Корутина для анимации входа в капсулу
    IEnumerator CapsuleEnter(Vector3 startPosition, Quaternion StartRotation, Vector3 targetPosition, float moveDuration)
    {
        mcFirstPersonController.enabled = false;

        // Выключаем фонарик Эстер перед входом в капсулу (как сам свет, так и компонент, чтобы нельзя было управлять фонариком Эстер, играя за Кафа)
        mcFlashlight.GetComponent<Light>().enabled = false;
        FlashlightController.isActive = false;
        mcFlashlight.SetActive(false);

        // Перемещение в капсулу
        float journey = 0f;
        while (journey <= moveDuration)
        {
            if (Death.collidedCaph) // Если Каф догнал, то перемещение в капсулу прерывается
                yield break;
            else
            {
                // Само перемещение и вращение
                journey += Time.deltaTime;
                float percent = Mathf.Clamp01(journey / moveDuration);
                mc.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, targetPosition, percent), Quaternion.Lerp(StartRotation, enterTargetRot, percent));

                // Добавление реалистичности перемещению и вращению с помощью animation curves
                float posCurvePercent = posAnimationCurve.Evaluate(percent);
                float rotCurvePercent = rotAnimationCurve.Evaluate(percent);
                mc.transform.SetPositionAndRotation(Vector3.LerpUnclamped(startPosition, targetPosition, posCurvePercent), Quaternion.LerpUnclamped(StartRotation, enterTargetRot, rotCurvePercent));

                yield return null;
            }
        }

        mcCharacterController.enabled = false;
        //Затухание экрана между переключением
        blackScreenAC.Play("Fade in");
        yield return new WaitForSeconds(0.1f);
        mcCamera.enabled = false;
        robotMesh.enabled = false;
        mcMesh.SetActive(true);
        robotCamera.enabled = true;
        caphHUD.enabled = true;
        blackScreenAC.Play("Fade out");

        OnEnteredCapsule?.Invoke(); // Должно быть перед отключением скрипта caphChase, чтобы событие вызвалось в этом скрипте

        caphChase.enabled = false;
        robotAgent.enabled = false;
        robotAnimator.enabled = false;
        robotCharacterController.enabled = true;
        robotFirstPersonController.enabled = true;
        robotFlashlight.SetActive(true);

        caphSource.PlayOneShot(switchSound);

        isInCapsule = true;
        usedCapsules += 1;
    }

    // Корутина для анимации выхода из капсулы
    IEnumerator CapsuleExit(Vector3 startPosition, Vector3 exitTargetPos, float moveDuration)
    {
        robotFirstPersonController.enabled = false;
        robotCharacterController.enabled = false;

        // Выключаем фонарик Кафа перед выходом из капсулы (как сам свет, так и компонент, чтобы нельзя было управлять фонариком Кафа, играя за Эстер)
        robotFlashlight.GetComponent<Light>().enabled = false;
        FlashlightController.isActive = false;
        robotFlashlight.SetActive(false);

        blackScreenAC.Play("Fade in");
        yield return new WaitForSeconds(0.1f);
        robotCamera.enabled = false;
        caphHUD.enabled = false;
        //robotMesh.shadowCastingMode = ShadowCastingMode.On;
        robotMesh.enabled = true;
        robotAnimator.enabled = true;
        robotAgent.enabled = true;
        caphChase.enabled = true;
        mcMesh.SetActive(false);
        mcCamera.enabled = true;
        blackScreenAC.Play("Fade out");

        // Перемещение из капсулы (без вращения)
        float journey = 0f;
        while (journey <= moveDuration)
        {   // Само перемещение
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / moveDuration);
            mc.transform.position = Vector3.Lerp(startPosition, exitTargetPos, percent);

            // Добавление реалистичности перемещению с помощью animation curves
            float posCurvePercent = posAnimationCurve.Evaluate(percent);
            mc.transform.position = Vector3.LerpUnclamped(startPosition, exitTargetPos, posCurvePercent);

            yield return null;
        }

        mcCharacterController.enabled = true;
        mcFirstPersonController.enabled = true;
        mcFlashlight.SetActive(true);

        isInCapsule = false;
        OnExitedCapsule?.Invoke(true);
    }
}

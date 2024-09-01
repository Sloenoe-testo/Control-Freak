using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class CaphChase : MonoBehaviour
{
    [SerializeField] private GameObject caph;
    private NavMeshAgent caphAgent;
    private Animator caphAC;
    [SerializeField] private Transform mc;

    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float sprintSpeed = 8f;

    [SerializeField] private float walkTimer = 6f;
    [SerializeField] private float runTimer = 10f;

    [SerializeField] private float walkTimer_2 = 6f;
    [SerializeField] private float runTimer_2 = 10f;

    private bool walkingFinished;
    private bool runningFinished;
    private bool walkingFinished_2;
    private bool runningFinished_2;
    private bool isRunning = false;
    private bool isSprinting = false;

    public static Action OnCollidedCaph;

    [SerializeField] private GameObject mcCameraHolder;
    private float shakeIntensity = 0;
    [SerializeField] private float shakeConstant = 0.2f;
    [SerializeField] private float minDistance;
    [SerializeField] private float maxDistance;
    private bool inShakeRange = false;
    private float currentDistance = 0;

    [SerializeField] private AudioSource footstepSource = default;
    [SerializeField] private AudioClip[] floorClips = default;
    private float footstepTimer = 0; // Время между шагами
    [SerializeField] private float baseStepSpeed = 0.75f;
    [SerializeField] private float runStepMultiplier = 0.45f;
    [SerializeField] private float sprintStepMultiplier = 0.3f;
    private float GetCurrentOffset => isRunning ? baseStepSpeed * runStepMultiplier : isSprinting ? baseStepSpeed * sprintStepMultiplier : baseStepSpeed;
    [SerializeField] private float lowerPitchLimit = 0.9f;
    [SerializeField] private float upperPitchLimit = 1.1f;
    [SerializeField] private float walkVolume = 0.5f;
    [SerializeField] private float runVolume = 0.75f;
    private float footStepVolume => isRunning || isSprinting ? runVolume : walkVolume;
    private bool useFootsteps = true;

    [SerializeField] private AudioSource chaseMusicSource = default;
    [SerializeField] private AudioClip chaseMusic_1 = default;
    [SerializeField] private AudioClip chaseMusic_2 = default;

    private void Awake()
    {
        caphAgent = caph.GetComponent<NavMeshAgent>();
        caphAC = caph.GetComponent<Animator>();


        // Устанавливаем скорость ходьбы в начале
        caphAgent.speed = walkSpeed;
        walkingFinished = false;
        runningFinished = false;
        walkingFinished_2 = false;
        runningFinished_2 = false;
    }

    private void OnEnable()
    {
        //CharacterSwitch.OnExitedCapsule += StartSecondChase;
        CharacterSwitch.OnEnteredCapsule += StopChaseMusic;
        DoorManager.OnDoorClosed += StopCaph;
    }

    private void OnDisable()
    {
        //CharacterSwitch.OnExitedCapsule -= StartSecondChase;
        CharacterSwitch.OnEnteredCapsule -= StopChaseMusic;
        DoorManager.OnDoorClosed -= StopCaph;
    }

    private void Update()
    {
        if (!inShakeRange && (walkingFinished || (walkingFinished && walkingFinished_2))) // Когда Каф начал бежать (для тряски камеры)
        {
            currentDistance = Vector3.Distance(transform.position, mc.position); // Рассчитываем дистанцию

            if ((currentDistance <= maxDistance) && (currentDistance >= minDistance)) // Если игрок находится в радиусе тряски
            {
                inShakeRange = true;
                StartCoroutine(CameraShake()); // Вызываем корутину, и пока больше не используем Update (используем снова, когда игрок вышел из радиуса тряски, и корутина закончилась)
            }
        }

        if (useFootsteps)
            HandleFootsteps();

        if (!chaseMusicSource.isPlaying)
            StartChaseMusic(true);
    }

    private void FixedUpdate()
    {
        if (caphAgent.enabled)
        {
            if ((CharacterSwitch.usedCapsules == 1 && !Scenes.autosaveLoaded) || (Scenes.autosaveLoaded && CharacterSwitch.usedCapsules == 0)) // Если использована 1 капсула (при первой игре) или если использовано 0 капсул (при автосохранении)
            {
                FirstChase();
                Debug.Log("First chase");
            }
            else if (CharacterSwitch.usedCapsules == 2 || (Scenes.autosaveLoaded && CharacterSwitch.usedCapsules == 1)) // Если использовано 2 капсулы (при первой игре) или если использована 1 капсула (при автосохранении)
            {
                SecondChase();
                Debug.Log("Second chase");
            }
            caphAgent.SetDestination(mc.position);
        }
    }

    private void StartSecondChase()
    {
        if (CharacterSwitch.usedCapsules == 2)
        {
            caphAgent.speed = walkSpeed;
            caphAC.Play("Walk");
            Debug.Log("Walking");
        }
    }

    private void FirstChase() // Перемещение, переключение скорости и анимаций Кафа во время первого преследования
    {
        if (!walkingFinished && (walkTimer >= 0)) // Если ходьба еще не закончилась, уменьшаем таймер
        {
            Debug.Log("Walking");
            walkTimer -= Time.deltaTime;
        }
        else if (!walkingFinished) // Если ходьба закончилась, включаем бег
        {
            if (!walkingFinished_2)
                Debug.Log("Running");
            caphAgent.speed = runSpeed;

            // Переключение на анимацию бега (с переходом)
            if (!caphAC.GetAnimatorTransitionInfo(0).IsName("Walk -> Run"))
                caphAC.CrossFade("Run", 0.5f);

            walkingFinished = true;
            isRunning = true;
        }

        if (walkingFinished && !runningFinished && (runTimer >= 0)) // Если бег еще не закончился (но ходьба закончилась), уменьшаем таймер
        {
            runTimer -= Time.deltaTime;
        }
        else if (runTimer < 0 && !runningFinished) // Если ходьба закончилась, включаем спринт
        {
            Debug.Log("Sprinting");
            caphAgent.speed = sprintSpeed;

            if (!caphAC.GetAnimatorTransitionInfo(0).IsName("Run -> Sprint"))
                caphAC.CrossFade("Sprint", 1f);

            runningFinished = true;
            isRunning = false;
            isSprinting = true;
        }
    }

    private void SecondChase() // Перемещение, переключение скорости и анимаций Кафа во время второго преследования
    {

        if (!walkingFinished_2 && (walkTimer_2 >= 0))
        {
            walkTimer_2 -= Time.deltaTime;
        }
        else if (!walkingFinished_2)
        {
            Debug.Log("Running");
            caphAgent.speed = runSpeed;

            // Переключение на анимацию бега (с переходом)
            if (!caphAC.GetAnimatorTransitionInfo(0).IsName("Walk -> Run"))
                caphAC.CrossFade("Run", 0.5f);

            walkingFinished_2 = true;
            isRunning = true;
            isSprinting = false;
        }

        if (walkingFinished_2 && !runningFinished_2 && (runTimer_2 >= 0))
        {
            runTimer_2 -= Time.deltaTime;
        }
        else if (runTimer_2 < 0 && !runningFinished_2)
        {
            Debug.Log("Sprinting");
            caphAgent.speed = sprintSpeed;

            if (!caphAC.GetAnimatorTransitionInfo(0).IsName("Run -> Sprint"))
                caphAC.CrossFade("Sprint", 1f);

            runningFinished_2 = true;
            isRunning = false;
            isSprinting = true;
        }
    }

    private void StopCaph() // Остановка Кафа и музыки после закрытия двери
    {
        Debug.Log("Stop Caph");
        caphAgent.enabled = false;
        caph.SetActive(false);

        StopChaseMusic();
    }

    public void OnTriggerEnter(Collider other) // Вызов смерти
    {
        if (other.tag == "Player")
        {
            OnCollidedCaph?.Invoke();
            inShakeRange = false;
            useFootsteps = false;
            StopChaseMusic();
        }
    }

    IEnumerator CameraShake() // Корутина для тряски камеры игрока во время погони
    {
        Vector3 startPosition = mcCameraHolder.transform.localPosition;

        while ((currentDistance <= maxDistance) && (currentDistance >= minDistance) && inShakeRange) // Пока игрок в радиусе тряски
        {
            currentDistance = Vector3.Distance(transform.position, mc.position);
            shakeIntensity = shakeConstant / currentDistance;

            mcCameraHolder.transform.localPosition = startPosition + Random.insideUnitSphere * shakeIntensity;

            yield return null;
        }

        mcCameraHolder.transform.localPosition = startPosition;

        inShakeRange = false;
    }

    private void HandleFootsteps() // Шаги, издаваемые Кафом во время погони
    {
        footstepSource.volume = footStepVolume;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            footstepSource.pitch = Random.Range(lowerPitchLimit, upperPitchLimit); // Изменение высоты звука для добавления вариативности

            footstepSource.PlayOneShot(floorClips[Random.Range(0, floorClips.Length - 1)]);

            footstepTimer = GetCurrentOffset;
        }
    }

    private void StartChaseMusic(bool exitedCapsule) // Включение музыки погони
    {
        if (CharacterSwitch.usedCapsules == 1) // Во время первой погони - первый вариант музыки (т.к. Каф сначала просто идет)
        {
            chaseMusicSource.clip = chaseMusic_1;
            chaseMusicSource.Play();
        }
        else // Во время второй погони - второй вариант
        {
            chaseMusicSource.clip = chaseMusic_2;
            chaseMusicSource.Play();
        }
    }

    private void StopChaseMusic() // Выключение музыки погони
    {
        chaseMusicSource.Stop();
    }
}

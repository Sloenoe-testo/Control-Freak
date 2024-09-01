using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

public class ChaseController : MonoBehaviour
{
    [SerializeField] private GameObject caph;
    private NavMeshAgent caphAgent;
    [SerializeField] private Transform mc;

    [SerializeField] private float walkSpeed = 1.5f;
    [SerializeField] private float runSpeed = 6f;
    [SerializeField] private float sprintSpeed = 8f;

    [SerializeField] private float walkTimer = 6f;
    [SerializeField] private float runTimer = 10f;

    [SerializeField] private float walkTimer_2 = 6f;
    [SerializeField] private float runTimer_2 = 10f;

    private bool walkingFinished;
    private bool walkingFinished_2;
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
    private float footstepTimer = 0; // ¬рем€ между шагами
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
    [SerializeField] private AudioClip chaseMusic = default;

    private void OnEnable()
    {
        CharacterSwitch.OnEnteredCapsule += StopChaseMusic;
        DoorManager.OnDoorClosed += StopCaph;
    }

    private void OnDisable()
    {
        CharacterSwitch.OnEnteredCapsule -= StopChaseMusic;
        DoorManager.OnDoorClosed -= StopCaph;
    }

    private void Update()
    {
        if (!inShakeRange && (walkingFinished || (walkingFinished && walkingFinished_2)))
        {
            currentDistance = Vector3.Distance(transform.position, mc.position);

            if ((currentDistance <= maxDistance) && (currentDistance >= minDistance))
            {
                inShakeRange = true;
                StartCoroutine(CameraShake());
            }
        }

        if (useFootsteps)
            HandleFootsteps();

        if (!chaseMusicSource.isPlaying)
            chaseMusicSource.PlayOneShot(chaseMusic);
    }

    private void StopCaph()
    {
        Debug.Log("Stop Caph");
        caphAgent.enabled = false;
        caph.SetActive(false);

        StopChaseMusic();
    }

    //  орутина дл€ тр€ски камеры игрока во врем€ погони
    IEnumerator CameraShake()
    {
        Vector3 startPosition = mcCameraHolder.transform.localPosition;

        while ((currentDistance <= maxDistance) && (currentDistance >= minDistance) && inShakeRange)
        {
            currentDistance = Vector3.Distance(transform.position, mc.position);
            shakeIntensity = shakeConstant / currentDistance;

            mcCameraHolder.transform.localPosition = startPosition + Random.insideUnitSphere * shakeIntensity;

            yield return null;
        }

        mcCameraHolder.transform.localPosition = startPosition;

        inShakeRange = false;
    }

    private void HandleFootsteps()
    {
        footstepSource.volume = footStepVolume;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            footstepSource.pitch = Random.Range(lowerPitchLimit, upperPitchLimit); // »зменение высоты звука дл€ добавлени€ вариативности

            footstepSource.PlayOneShot(floorClips[Random.Range(0, floorClips.Length - 1)]);

            footstepTimer = GetCurrentOffset;
        }
    }

    private void StopChaseMusic()
    {
        Debug.Log("Stop music");
        chaseMusicSource.Stop();
    }
}

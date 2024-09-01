using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class Death : MonoBehaviour
{
    [SerializeField] private GameObject mc;
    [SerializeField] private GameObject caph;

    [SerializeField] private VideoPlayer jumpscarePlayer;
    [SerializeField] private Transform jumpscarePoint;
    [SerializeField] private float moveDuration = 1f;

    private float shakeDuration;
    [SerializeField] private float shakeIntensity = 0.3f;

    public static bool collidedCaph = false;

    private void Awake()
    {
        shakeDuration = (float)jumpscarePlayer.clip.length;
    }

    private void Start()
    {
        jumpscarePlayer.Prepare(); // Загрузка видео в память (иначе оно не проигрывается первый раз)
    }

    private void OnEnable()
    {
        CaphChase.OnCollidedCaph += CollideCaph;
    }

    private void OnDisable()
    {
        CaphChase.OnCollidedCaph -= CollideCaph;
    }

    // При столкновении с Кафом
    private void CollideCaph()
    {
        mc.GetComponent<CharacterController>().enabled = false;
        mc.GetComponent<FirstPersonController>().enabled = false;

        collidedCaph = true;

        StartCoroutine(GetJumpscare(mc.transform.position, mc.transform.rotation)); // Запускаем падение и поворот
        StartCoroutine(CameraShake()); // Запускаем тряску камеры
        jumpscarePlayer.Play(); // Запускаем сам скример
        caph.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false; // Выключаем меш Кафа
    }

    // Корутина для падения, поворота к Кафу и включения экрана смерти
    IEnumerator GetJumpscare(Vector3 startPosition, Quaternion StartRotation)
    {
        // Поворот и перемещение к "точке скримера" (опускаем Эстер и поворачиваем ее вверх для создания эффекта того, что она упала и Каф смотрит сверху)
        float journey = 0f;
        while (journey <= moveDuration)
        {
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / moveDuration);
            mc.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, jumpscarePoint.position, percent), Quaternion.Lerp(StartRotation, jumpscarePoint.rotation, percent));

            yield return null;
        }

        // Дожидаемся, пока видео закончит проигрываться
        while (jumpscarePlayer.isPlaying)
        {
            yield return null;
        }

        // Загружаем экран смерти
        SceneManager.LoadSceneAsync("Death Screen");
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    // Корутина для тряски камеры во время скримера
    IEnumerator CameraShake()
    {
        Vector3 startPosition = mc.transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < shakeDuration)
        {
            elapsedTime += Time.deltaTime;
            mc.transform.position = startPosition + Random.insideUnitSphere * shakeIntensity;

            yield return null;
        }

        mc.transform.position = startPosition;
    }
}

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
        jumpscarePlayer.Prepare(); // �������� ����� � ������ (����� ��� �� ������������� ������ ���)
    }

    private void OnEnable()
    {
        CaphChase.OnCollidedCaph += CollideCaph;
    }

    private void OnDisable()
    {
        CaphChase.OnCollidedCaph -= CollideCaph;
    }

    // ��� ������������ � �����
    private void CollideCaph()
    {
        mc.GetComponent<CharacterController>().enabled = false;
        mc.GetComponent<FirstPersonController>().enabled = false;

        collidedCaph = true;

        StartCoroutine(GetJumpscare(mc.transform.position, mc.transform.rotation)); // ��������� ������� � �������
        StartCoroutine(CameraShake()); // ��������� ������ ������
        jumpscarePlayer.Play(); // ��������� ��� �������
        caph.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false; // ��������� ��� ����
    }

    // �������� ��� �������, �������� � ���� � ��������� ������ ������
    IEnumerator GetJumpscare(Vector3 startPosition, Quaternion StartRotation)
    {
        // ������� � ����������� � "����� ��������" (�������� ����� � ������������ �� ����� ��� �������� ������� ����, ��� ��� ����� � ��� ������� ������)
        float journey = 0f;
        while (journey <= moveDuration)
        {
            journey += Time.deltaTime;
            float percent = Mathf.Clamp01(journey / moveDuration);
            mc.transform.SetPositionAndRotation(Vector3.Lerp(startPosition, jumpscarePoint.position, percent), Quaternion.Lerp(StartRotation, jumpscarePoint.rotation, percent));

            yield return null;
        }

        // ����������, ���� ����� �������� �������������
        while (jumpscarePlayer.isPlaying)
        {
            yield return null;
        }

        // ��������� ����� ������
        SceneManager.LoadSceneAsync("Death Screen");
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    // �������� ��� ������ ������ �� ����� ��������
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

using UnityEngine;
using UnityEngine.SceneManagement;

public class Scenes : MonoBehaviour
{
    public static bool autosaveLoaded = false;
    [SerializeField] GameObject pauseWindow;
    private bool isPaused = false;

    [SerializeField] private FirstPersonController estherFPC;
    [SerializeField] private FirstPersonController caphFPC;
    private bool estherPaused = false;
    private bool caphPaused = false;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) // При загрузке сцены сбрасываем значения до изначальных у всех статических переменных
    {
        Debug.Log("OnSceneLoaded: " + scene.name);

        CharacterSwitch.i = 0;
        CharacterSwitch.isCaph = false;
        CharacterSwitch.exitedCapsule = false;
        CharacterSwitch.usedCapsules = 0;

        PuzzleSphere.puzzleEnabled = false;

        SphereManager.allSpheresActivated = false;

        Death.collidedCaph = false;

        CaphTrigger.triggerActivated = false;

        FlashlightController.isActive = false;

        PointerMovement.isStarted = false;

        PuzzleManager.collectableCounter = 0;
        PuzzleManager.allCollected = false;
        PuzzleManager.puzzleType = 1;

        PuzzleStart.isStarted = false;

        Time.timeScale = 1;
        if (estherPaused)
            estherFPC.enabled = true;
        else if (caphPaused)
            caphFPC.enabled = true;
    }

    private void Update()
    {
        if (!PuzzleSphere.puzzleEnabled && pauseWindow != null)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!isPaused)
                    Pause();
                else
                    Continue();
            }
        }
    }

    public void Play()
    {
        SceneManager.LoadSceneAsync("Game");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ToMenu()
    {
        SceneManager.LoadSceneAsync("Main Menu");
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    public void Exit()
    {
        Application.Quit();
    }

    private void Pause()
    {

        if (estherFPC.enabled)
        {
            estherFPC.enabled = false;
            estherPaused = true;
        }
        else if (caphFPC.enabled)
        {
            caphFPC.enabled = false;
            caphPaused = true;
        }


        Time.timeScale = 0;
        pauseWindow.SetActive(true);
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        isPaused = true;
    }

    public void Continue()
    {
        Time.timeScale = 1;
        if (pauseWindow != null)
            pauseWindow.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (estherPaused)
            estherFPC.enabled = true;
        else if (caphPaused)
            caphFPC.enabled = true;

        estherPaused = false;
        caphPaused = false;

        isPaused = false;
    }

    public void Restart()
    {
        SceneManager.LoadSceneAsync("Autosave");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        autosaveLoaded = true;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            SceneManager.LoadSceneAsync("Finish screen");
            Cursor.lockState = CursorLockMode.Confined;
            Cursor.visible = true;
        }
    }
}

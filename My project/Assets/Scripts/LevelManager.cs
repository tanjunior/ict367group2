using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using TMPro;
using WebXR;

[System.Serializable]
public class OnParked : UnityEvent<float> {}

public class LevelManager : MonoBehaviour
{
    private CursorLockMode lockmode;
    private Vector2 headRotation = Vector2.zero;
    private WebXRManager xrManager;
    private XRGeneralSettings xrSettings;
    private float elapsedTime = 0.0f;
    private bool levelCompleted = false;
    [SerializeField] private GameObject pauseMenu, toolTips, NameSelector, startButton, quitButton, nameSelectorTooltip, highscoreMenu;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private WebXRController leftController, rightController;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private AudioSource engineSound, engineStartSound;
    [SerializeField] private Animator rigAnimator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private HighscoreManager highscoreManager;
    [System.NonSerialized] public bool isPaused = false, showHologram = true, firstStart = true, showPointer = true;
    [System.NonSerialized] public string playerName;
    public UnityEvent onRestart;
    public OnParked onParked;
    public bool isVR;
    public int currentLevelIndex;

    private void Awake() {
        GameObject[] objs = GameObject.FindGameObjectsWithTag("VR");

        if (objs.Length > 1)
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this.gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {

        xrSettings = XRGeneralSettings.Instance;
        xrManager = WebXRManager.Instance;
        isVR = CheckVR();
        StartCoroutine(ExampleCoroutine(0.1f, false));
        StartCoroutine(ExampleCoroutine(0.2f, true));
    }

    IEnumerator ExampleCoroutine(float seconds, bool pointerState)
    {
        yield return new WaitForSeconds(seconds);
        showPointer = pointerState;
    }

    // Update is called once per frame
    void Update()
    {
        // public enum WebXRState { VR, AR, NORMAL }
        isVR = CheckVR();
        string levelName = SceneManager.GetActiveScene().name;
        if (levelName == "Main" || levelName.Contains("Test")) {
            if (!firstStart) MouseLook();
        } else {
            if (leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB) || Input.GetKeyDown(KeyCode.Escape) && !levelCompleted) {
                isPaused = !isPaused;
                Time.timeScale = isPaused? 0 : 1;
            }
            if (rightController.GetButtonDown(WebXRController.ButtonTypes.ButtonB) || Input.GetKeyDown(KeyCode.Tab)) {
                showHologram = !showHologram;
            }
            if (isPaused) {
                pauseMenu.SetActive(true);
                showPointer = true;
                lockmode = CursorLockMode.None;
                Cursor.lockState = lockmode;
            } else if (levelCompleted) {
                showPointer = true;
                lockmode = CursorLockMode.None;
                Cursor.lockState = lockmode;
            } else {
                TimeCounter();
                showPointer = false;
                pauseMenu.SetActive(false);
                if (!isVR) {
                    lockmode = CursorLockMode.Locked;
                    Cursor.lockState = lockmode;
                    MouseLook();
                } else if (isVR) {
                    lockmode = CursorLockMode.None;
                    Cursor.lockState = lockmode;
                }
            }
        }
    }

    private bool CheckVR() {
        if (Application.isEditor) {
            if (xrSettings.Manager.activeLoader != null) return true;
            else return false;
        }
        return xrManager.XRState == WebXRState.VR;
    }

    private void MouseLook() {
        headRotation.y += Input.GetAxis ("Mouse X");
		headRotation.x += -Input.GetAxis ("Mouse Y");

        headRotation.x = Mathf.Clamp(headRotation.x, -30, 30);

        mainCamera.localEulerAngles = headRotation * 3;
    }

    private void TimeCounter() {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timer.text = string.Format("{0:N0}:{1:N0}", minutes, seconds);
    }

    public void Park(bool fl, bool fr, bool rl, bool rr) {
        if (!fl || !fr || !rl || !rr) return;
        levelCompleted = true;
        if (currentLevelIndex == 3) {
            highscoreMenu.transform.GetChild(2).gameObject.SetActive(false); //disable next level button
        } else if (PlayerPrefs.GetInt("levelCompleted") < currentLevelIndex) {
            PlayerPrefs.SetInt("levelCompleted", currentLevelIndex);
        }
        rigAnimator.Play("TopView");
        float completeTime = elapsedTime;
        onParked.Invoke(completeTime);
        //highscoreManager.SaveHighScore();
        highscoreMenu.SetActive(true);   
    }

    public void ConfirmName() {
        IEnumerator PlayEngineSound(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            engineSound.Play();
            rb.constraints = RigidbodyConstraints.None;
            collider.enabled = true;
            firstStart = false;
            NameSelector.SetActive(false);
        }
        showPointer = false;
        playerName = NameSelector.GetComponent<NameSelector>().GetName();
        engineStartSound.Play();
        StartCoroutine(PlayEngineSound(1.8f));
        rigAnimator.Play("MoveToCar");
        quitButton.SetActive(false);
        startButton.SetActive(false);
        nameSelectorTooltip.SetActive(false);
    }


    public void LoadScene(int index) {
        onRestart.Invoke();
        Time.timeScale = 1;
        SceneManager.LoadScene(index, LoadSceneMode.Single);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        mainCamera.localEulerAngles = Vector3.zero;
        rigAnimator.Play("Idle");
        levelCompleted = false;
        showPointer = false;
        isPaused = false;
        elapsedTime = 0;
        currentLevelIndex = index;
        highscoreMenu.SetActive(false);
        pauseMenu.SetActive(false);
        toolTips.SetActive(false);
        lockmode = CursorLockMode.Locked;
        Cursor.lockState = lockmode;
    }

    public void RestartLevel() {
        onRestart.Invoke();
        LoadScene(currentLevelIndex);
    }

    public void NextLevel() {
        onRestart.Invoke();
        LoadScene(currentLevelIndex+1);
    }

    public void LoadSceneMain() {
        StartCoroutine(Delay(0.2f));
        LoadScene(0);
    }

    
    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pauseMenu.SetActive(false);
        lockmode = CursorLockMode.None;
        Cursor.lockState = lockmode;
        if (isVR) showPointer = true;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        mainCamera.localEulerAngles = Vector3.zero;
    }

    public void QuitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
            if (isVR) xrManager.ToggleVR();
        #else
            Application.Quit();
        #endif
    }
}

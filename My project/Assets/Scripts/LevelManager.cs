using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.XR.Management;
using TMPro;
using WebXR;
using Newtonsoft.Json;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject pauseMenu, highscoreMenu, toolTips, NameSelector, highscoreRowPrefab, startButton, quitButton, nameSelectorTooltip;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private WebXRController leftController, rightController;
    [SerializeField] private Transform mainCamera, highscore;
    [SerializeField] private ColliderCheck colliderCheck;
    [SerializeField] private ParkingAccuracy parkingAccuracy;
    [SerializeField] private AudioSource engineSound, engineStartSound;
    [SerializeField] private Animator rigAnimator;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private List<Dictionary<string, string>> currentHighscores;
    public UnityEvent onRestart;
    public CursorLockMode lockmode;
    public bool firstStart = true;
    public bool showPointer = true;
    public bool isVR;
    public int currentLevelIndex;
    public string levelName;
    [System.NonSerialized] public bool isPaused = false, showHologram = true;
    private Vector2 headRotation = Vector2.zero;
    private WebXRManager xrManager;
    private XRGeneralSettings xrSettings;
    private string newSerializedString;
    private string playerName = "ICT";
    private float elapsedTime = 0.0f;
    private bool levelCompleted = false;

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
        levelName = SceneManager.GetActiveScene().name;
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
        if (currentLevelIndex == 1) {
            //unlock level 2
            PlayerPrefs.SetInt("levelCompleted", 1);

        } else if (currentLevelIndex == 2) {
            //unlock level 3
            PlayerPrefs.SetInt("levelCompleted", 2);
        } else if (currentLevelIndex == 3) {
            highscoreMenu.transform.GetChild(2).gameObject.SetActive(false); //disable next level button
        }
        rigAnimator.Play("TopView");
        float completeTime = elapsedTime;
        SaveHighScore(completeTime);

        /*
        if (SceneManager.GetActiveScene().buildIndex+1 >= PlayerPrefs.GetInt("levelCompleted") && SceneManager.GetActiveScene().buildIndex != 3)
        {
            PlayerPrefs.SetInt("levelCompleted", SceneManager.GetActiveScene().buildIndex);
            Debug.Log("Enabling up to level"+ SceneManager.GetActiveScene().buildIndex );
        }
        */    

           
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
        playerName = NameSelector.GetComponent<NameSelector>().GetName();
        engineStartSound.Play();
        StartCoroutine(PlayEngineSound(1.8f));
        rigAnimator.Play("MoveToCar");
        quitButton.SetActive(false);
        startButton.SetActive(false);
        nameSelectorTooltip.SetActive(false);
    }

    public void SaveHighScore(float time) {
        float score,timeScore,collisionScore,parkingScore;

        //get values
        int numberOfCollisions = colliderCheck.getNumberOfCollisions();
        float parkingAccuracyPercentage = parkingAccuracy.getAccuracyPercentage();

        //reset values
        colliderCheck.resetNumberOfCollisions();
        parkingAccuracy.resetAccuracyPercentage();
      

        if(time<60)
        {
            timeScore = 1000;
        }
        else if(time<120)
        {
            timeScore = 800;
        }
        else if(time<250)
        {
            timeScore = 400;
        }
        else if(time<500)
        {
            timeScore = 200;
        }
        else
        {
            timeScore = 0;
        }

        if(numberOfCollisions<1)
        {
            collisionScore = 1500;
        }
        else if(numberOfCollisions<4)
        {
            collisionScore = 1100;
        }
        else if(numberOfCollisions <8)
        {
            collisionScore = 700;
        }
        else
        {
            collisionScore = 0;
        }
      
        
    
       parkingScore =  parkingAccuracyPercentage*20;

        score = parkingScore + collisionScore + timeScore;

        Debug.Log("Parking score:" + parkingScore + " Collision score:" + collisionScore + " time score:" + timeScore);


        //main focus on parking

        Dictionary<string, string> currentSession = new Dictionary<string, string>(); // dictionary kv pair to store level highscore)
        currentSession.Add("name", playerName);
        currentSession.Add("score", score.ToString());
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        if (PlayerPrefs.HasKey(currentLevelIndex.ToString())) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(currentLevelIndex.ToString());
            //Debug.Log(serializedString);
            list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string
            //Debug.Log(highscore.Count);
            //check if player already have a highscore
            // if (highscore.ContainsKey(name)){
            //     if (time < highscore[name]) highscore[name] = score; // replace the score if the current score is higher
            // }
        }

        list.Add(currentSession); 

        // sort the list
        var sortedList = list.OrderByDescending(d => float.Parse(d["score"])).ToList();

        // // sort the dictionary
        // Dictionary<string, float> sorted = new Dictionary<string, float>();
        // foreach (KeyValuePair<string, float> pair in highscore.OrderByDescending(key => key.Value)) {
        //     sorted.Add(pair.Key, pair.Value);
        // }

        currentHighscores = sortedList;

        // serialize the dictionary and save it back to playerprefs.
        newSerializedString = JsonConvert.SerializeObject(sortedList);
        PlayerPrefs.SetString(currentLevelIndex.ToString(), newSerializedString);

        Debug.Log("Loading high score");

        
        DisplayHighScore();
    }

    private void DisplayHighScore() {
        highscoreMenu.SetActive(true);
        float height = 0;
        for (int i = 0; i < currentHighscores.Count; i++) {
            if (i == 9) return;
            var d = currentHighscores.ElementAt(i);
            GameObject rowObject = Instantiate(highscoreRowPrefab, highscore);
            rowObject.transform.localPosition = new Vector3(0, height, 0);
            HighscoreRow script = rowObject.GetComponent<HighscoreRow>();
            script.setRowValues(d["name"], d["score"]);
            height -= 0.2f;
        }
    }

    public void LoadScene(int index) {
        foreach (Transform child in highscore) {
            GameObject.Destroy(child.gameObject);
        }
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

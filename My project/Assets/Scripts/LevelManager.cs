﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using TMPro;
using WebXR;
using Newtonsoft.Json;

public class LevelManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenu, pauseMenu, highscoreMenu;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private GameObject highscoreRowPrefab, civic;
    [SerializeField] private WebXRController leftController;
    [SerializeField] private Camera mainCamera;
    public UnityEvent onRestart;
    public bool showPointer = true;
    public bool highscoreDisplayed = false;
    [System.NonSerialized] public bool isPaused = false;
    private float elapsedTime = 0.0f;
    public string levelName;
    private Vector2 headRotation = Vector2.zero;
    private WebXRManager xrManager;
    private bool isVR;
    public int currentLevelIndex;
    private string newSerializedString;
    [SerializeField] private Dictionary<string, float> currentHighscores;
    public WebXRState state = WebXRState.NORMAL;
    public CursorLockMode lockmode;

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
        xrManager = WebXRManager.Instance;
        isVR = xrManager.isSupportedVR;
        StartCoroutine(ExampleCoroutine(0.1f, false));
        StartCoroutine(ExampleCoroutine(0.2f, true));
    }

    IEnumerator ExampleCoroutine(float seconds, bool state)
    {
        yield return new WaitForSeconds(seconds);
        showPointer = state;
    }

    // Update is called once per frame
    void Update()
    {
        // public enum WebXRState { VR, AR, NORMAL }
        if (isVR) state = xrManager.XRState;
        levelName = SceneManager.GetActiveScene().name;
        if (levelName == "Highscore") {
            if (!highscoreDisplayed) DisplayHighScore();
        } else if (levelName == "Main") {}
        else {
            if (leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB) || Input.GetKeyDown(KeyCode.Escape)) {
                isPaused = !isPaused;
                //Time.timeScale = isPaused? 0 : 1;
            }
            if (isPaused) {
                pauseMenu.SetActive(true);
                showPointer = true;
                lockmode = CursorLockMode.None;
                Cursor.lockState = lockmode;
            } else {
                TimeCounter();
                showPointer = false;
                pauseMenu.SetActive(false);
                if (state == WebXRState.NORMAL) {
                    
                    lockmode = CursorLockMode.Locked;
                    Cursor.lockState = lockmode;
                    MouseLook();
                } else if (state == WebXRState.VR) {
                    
                    lockmode = CursorLockMode.None;
                    Cursor.lockState = lockmode;
                }
            }
        }
    }

    private void MouseLook() {
        headRotation.y += Input.GetAxis ("Mouse X");
		headRotation.x += -Input.GetAxis ("Mouse Y");

        headRotation.x = Mathf.Clamp(headRotation.x, -30, 30);

        mainCamera.transform.localEulerAngles = headRotation * 3;
    }

    private void TimeCounter() {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timer.text = string.Format("{0:N0}:{1:N0}", minutes, seconds);
    }

    public void Park(bool fl, bool fr, bool rl, bool rr) {
        if (!fl || !fr || !rl || !rr) return;
        float completeTime = elapsedTime;
        SaveHighScore(completeTime);
    }

    public void SaveHighScore(float time) {
        string name = Environment.UserName; //get player name from PC name.
        Dictionary<string, float> highscore; // dictionary kv pair to store name and score

        int score,baseScore;

        int numberOfCollisions = GetComponent<ColliderCheck>().getNumberOfCollisions();
        float parkingAccuracy = GetComponent<ParkingAccuracy>().getAccuracyPercentage();
        //get values

        GetComponent<ColliderCheck>().resetNumberOfCollisions();
        GetComponent<ParkingAccuracy>().resetAccuracyPercentage();
        //reset values


        baseScore = 1500;

        if(time<60)
        {
            score = 1000;
        }
        else if(time<120)
        {
            score = 800;
        }
        else if(time<250)
        {
            score = 400;
        }
        else if(time<500)
        {
            score = 200;
        }
        else
        {
            score = 0;
        }

        if(numberOfCollisions<3)
        {
            score -= 0;
        }
        else if(numberOfCollisions<5)
        {
            score -= 400;
        }
        else if(numberOfCollisions <10)
        {
            score -= 800;
        }
        else
        {
            score -= 1500;
        }
        

        


        if (PlayerPrefs.HasKey(currentLevelIndex.ToString())) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(currentLevelIndex.ToString());
            Debug.Log(serializedString);
            highscore = JsonConvert.DeserializeObject<Dictionary<string, float>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string
            Debug.Log(highscore.Count);
            if (highscore.ContainsKey(name)){ // check if player already have a highscore
                if (time < highscore[name]) highscore[name] = time; // replace the score if the current score is higher



            }
        } else {
            highscore = new Dictionary<string, float>(); // create a new dictionary if there is no record
            highscore.Add(name, time); 
        }

        // sort the dictionary
        Dictionary<string, float> sorted = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> pair in highscore.OrderByDescending(key => key.Value)) {
            sorted.Add(pair.Key, pair.Value);
        }

        currentHighscores = sorted;

        // serialize the dictionary and save it back to playerprefs.
        newSerializedString = JsonConvert.SerializeObject(sorted);
        PlayerPrefs.SetString(currentLevelIndex.ToString(), newSerializedString);
             
        LoadSceneHighscore();
    }

    private void DisplayHighScore() {
        float height = 3;
        foreach(var row in currentHighscores) {
            Debug.Log(row.Key);
            Debug.Log(row.Value);
            GameObject rowObject = Instantiate(highscoreRowPrefab, new Vector3(0.408f, height, 4), Quaternion.identity);
            HighscoreRow script = rowObject.GetComponent<HighscoreRow>();
            script.setRowValues(row.Key, row.Value.ToString());
            height -= 0.2f;
        }
        highscoreDisplayed = true;
    }

    public void LoadScene(int index) {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        mainCamera.transform.rotation = Quaternion.identity;
        showPointer = false;
        elapsedTime = 0;
        currentLevelIndex = index;
        mainMenu.SetActive(false);
        highscoreMenu.SetActive(false);
        pauseMenu.SetActive(false);
        civic.SetActive(true);
    }

    public void RestartLevel() {
        onRestart.Invoke();
        isPaused = false;
        LoadScene(currentLevelIndex);
    }

    public void NextLevel() {
        LoadScene(currentLevelIndex+1);
    }

    public void LoadSceneHighscore() {
        StartCoroutine(Delay(1));
        isPaused = false;
        highscoreDisplayed = false;
        civic.SetActive(false);
        SceneManager.LoadScene("Highscore", LoadSceneMode.Single);
        highscoreMenu.SetActive(true);
    }

    public void LoadSceneMain() {
        StartCoroutine(Delay(1));
        isPaused = false;
        civic.SetActive(false);
        SceneManager.LoadScene("Main", LoadSceneMode.Single);
        mainMenu.SetActive(true);
        highscoreMenu.SetActive(false);
    }

    
    IEnumerator Delay(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        pauseMenu.SetActive(false);
        lockmode = CursorLockMode.None;
        Cursor.lockState = lockmode;
        showPointer = true;
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
        mainCamera.transform.rotation = Quaternion.identity;
    }

    public void QuitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #elif UNITY_WEBGL
            if (state == WebXRState.VR) xrManager.ToggleVR();
        #else
            Application.Quit();
        #endif
    }
}
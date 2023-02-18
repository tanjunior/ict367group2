using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using WebXR;
using UnityEngine.SceneManagement;
using Newtonsoft.Json;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private GameObject highscoreRowPrefab;
    [SerializeField] private WebXRController leftController;
    [SerializeField] private Camera mainCamera;
    [System.NonSerialized] public bool isPaused = false;
    public bool showPointer = true;
    private float elapsedTime = 0.0f;
    private string level;
    private Vector2 headRotation = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        level = SceneManager.GetActiveScene().name;
        if (level == "Highscore") {
            DisplayHighScore();
            showPointer = true;
        } else if (level == "Main") showPointer = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (level != "Highscore" && level != "Main") {
            if (leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB) || Input.GetKeyDown(KeyCode.Escape)) {
                isPaused = !isPaused;
                Debug.Log("paused: "+isPaused);
            }
            if (isPaused) {
                menu.SetActive(true);
                Cursor.lockState = CursorLockMode.None;
                showPointer = true;
            }
            else {
                showPointer = false;
                TimeCounter();
                Cursor.lockState = CursorLockMode.Locked;
                MouseLook();
                menu.SetActive(false);
            } 
        } else {
            showPointer = true;
            MouseLook();
        }
    }
        
    private void TimeCounter() {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timer.text = string.Format("{0:N0}:{1:N0}", minutes, seconds);
    }

    public void Park(bool fl, bool fr, bool rl, bool rr) {
        if (!fl || !fr || !rl || !rr) return;
        isPaused = true;
        float completeTime = elapsedTime;
        SaveHighScore(completeTime);
    }

    private void DisplayHighScore() {
        float height = 3;
        string highscorePrefs = PlayerPrefs.GetString("SampleScene");
        Dictionary<string, float> highscore = JsonConvert.DeserializeObject<Dictionary<string, float>>(highscorePrefs);
        
        foreach(var row in highscore) {
            GameObject rowObject = Instantiate(highscoreRowPrefab, new Vector3(0, height, 0), Quaternion.identity);
            HighscoreRow script = rowObject.GetComponent<HighscoreRow>();
            script.setRowValues(row.Key, row.Value.ToString());
            height -= 0.2f;
        }        
    }
    
    private void MouseLook() {
        headRotation.y += Input.GetAxis ("Mouse X");
		headRotation.x += -Input.GetAxis ("Mouse Y");
		mainCamera.transform.eulerAngles = headRotation * 3;
    }

    private void SaveHighScore(float time) {
        string name = "Player"; //temp player name var
        Dictionary<string, float> highscore; // dictionary kv pair to store name and score
        if (PlayerPrefs.HasKey(level)) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(level);
            Debug.Log(serializedString);
            highscore = JsonConvert.DeserializeObject<Dictionary<string, float>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string
        } else {
            highscore = new Dictionary<string, float>(); // create a new dictionary if there is no record
        }

        if (highscore.ContainsKey(name)){ // check if player already have a highscore
            if (time < highscore[name]) highscore[name] = time; // replace the score if the current score is higher
        } else { // else add a new record for the player
            highscore.Add(name, time); 
        }

        // sort the dictionary
        Dictionary<string, float> sorted = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> pair in highscore.OrderByDescending(key => key.Value)) {
            sorted.Add(pair.Key, pair.Value);
        }

        
        
        string newSerializedString = JsonConvert.SerializeObject(sorted);

        // serialize the dictionary and save it back to playerprefs.
        PlayerPrefs.SetString(level, newSerializedString);
        SceneManager.LoadScene("Highscore", LoadSceneMode.Single);
    }

    public void LoadScene(string name) {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void RestartLevel() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    public void QuitGame() {
        Application.Quit();
    }
}

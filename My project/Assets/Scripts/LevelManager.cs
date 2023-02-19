using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using WebXR;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private GameObject highscoreRowPrefab;
    [SerializeField] private WebXRController leftController;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Data data;
    [System.NonSerialized] public bool showPointer = true;
    [System.NonSerialized] public bool isPaused = false;
    private float elapsedTime = 0.0f;
    private string level;
    private Vector2 headRotation = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        level = SceneManager.GetActiveScene().name;

        
        
        switch (level)
        {
            case "Highscore":
                DisplayHighScore();
                break;
            case "Main":
                break;
            default:
                data.prevLevel = data.currentLevel;
                data.currentLevel = SceneManager.GetActiveScene().buildIndex;
                break;
        }
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
            MouseLook();
        }
    }
        
    private void TimeCounter() {
        elapsedTime += Time.deltaTime;
        int minutes = Mathf.FloorToInt(elapsedTime / 60);
        int seconds = Mathf.FloorToInt(elapsedTime % 60);
        timer.text = string.Format("{0:N0}:{1:N0}", minutes, seconds);
    }

    public async void Park(bool fl, bool fr, bool rl, bool rr) {
        if (!fl || !fr || !rl || !rr) return;
        isPaused = true;
        float completeTime = elapsedTime;
        await data.SaveHighScore(completeTime);
        SceneManager.LoadScene("Highscore", LoadSceneMode.Single);
    }

    private void DisplayHighScore() {
        float height = 3;
        Dictionary<string, float> highscore = data.currentHighscores;
        
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



    public void LoadScene(string name) {
        SceneManager.LoadScene(name, LoadSceneMode.Single);
    }

    public void LoadScene(int index) {
        SceneManager.LoadScene(index, LoadSceneMode.Single);
    }

    public void RestartLevel() {
        SceneManager.LoadScene(data.currentLevel, LoadSceneMode.Single);
    }

    public void NextLevel() {
        SceneManager.LoadScene(data.currentLevel+1, LoadSceneMode.Single);
    }

    public void QuitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}

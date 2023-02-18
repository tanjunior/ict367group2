using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using WebXR;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    [SerializeField] GameObject menu;
    [SerializeField] private TextMeshPro timer;
    [SerializeField] private WebXRController leftController;
    [SerializeField] private bool isMainMenu = false;
    [SerializeField] private string level = "1";
    [SerializeField] private GameObject highscoreRowPrefab;
    [System.NonSerialized] public bool isPaused = false;
    private float elapsedTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isMainMenu) menu.SetActive(true);
        else { // only count time and listen to pause button during levels
            if (leftController.GetButtonDown(WebXRController.ButtonTypes.ButtonB)) {
                Debug.Log("paused");
                isPaused = !isPaused;
            }
            if (isPaused) {
                menu.SetActive(true);
            }
            else {
                TimeCounter();
                menu.SetActive(false);
            }
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

    private void DisplayHighScore(Dictionary<string, float> highscore) {
        HighscoreRow row = Instantiate(highscoreRowPrefab, Vector3.zero, Quaternion.identity).GetComponent<HighscoreRow>();
        row.setRowValues("Player", highscore["Player"].ToString());
    }

    private void SaveHighScore(float time) {
        string name = "Player"; //temp player name var
        Dictionary<string, float> highscore; // dictionary kv pair to store name and score
        if (PlayerPrefs.HasKey(level)) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(level);
            highscore = JsonUtility.FromJson<Dictionary<string, float>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string
        } else {
            highscore = new Dictionary<string, float>(); // create a new dictionary if there is no record
        }

        if (highscore.ContainsKey(name)){ // check if player already have a highscore
            if (time > highscore[name]) highscore[name] = time; // replace the score if the current score is higher
        } else { // else add a new record for the player
            highscore.Add(name, time); 
        }

        // sort the dictionary
        Dictionary<string, float> sorted = new Dictionary<string, float>();
        foreach (KeyValuePair<string, float> pair in highscore.OrderByDescending(key => key.Value)) {
            sorted.Add(pair.Key, pair.Value);
        }

        
        
        string newSerializedString = JsonUtility.ToJson(sorted);
        Debug.Log(newSerializedString);
        // serialize the dictionary and save it back to playerprefs.
        PlayerPrefs.SetString(level, newSerializedString);
        SceneManager.LoadScene("Highscore", LoadSceneMode.Single);
        // display the highscores on screen
        DisplayHighScore(sorted);

    }
}

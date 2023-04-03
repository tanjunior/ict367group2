using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using TMPro;

public class HighscoreManager : MonoBehaviour
{
    [SerializeField] private GameObject highscoreRowPrefab;
    [SerializeField] private Transform highscore;
    [SerializeField] private List<Dictionary<string, string>> currentHighscores;
    [SerializeField] private ColliderCheck colliderCheck;
    [SerializeField] private ParkingAccuracy parkingAccuracy;
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private TextMeshPro timeValueText, collisionValueText, accuracyValueText, timeScoreText, collisionScoreText, accuracyScoreText, totalScoreText;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    
    public void SaveHighScore(float time) {
        float timeScore,collisionScore;

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
      
        
    
        float parkingScore =  parkingAccuracyPercentage*20;

        float score = parkingScore + collisionScore + timeScore;

        //Debug.Log("Parking score:" + parkingScore + " Collision score:" + collisionScore + " time score:" + timeScore);


        //main focus on parking

        Dictionary<string, string> currentSession = new Dictionary<string, string>(); // dictionary kv pair to store level highscore)
        currentSession.Add("name", levelManager.playerName);
        currentSession.Add("score", score.ToString());
        List<Dictionary<string, string>> list = new List<Dictionary<string, string>>();
        if (PlayerPrefs.HasKey(levelManager.currentLevelIndex.ToString())) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(levelManager.currentLevelIndex.ToString());
            //Debug.Log(serializedString);
            list = JsonConvert.DeserializeObject<List<Dictionary<string, string>>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string

        }

        list.Add(currentSession); 

        // sort the list
        var sortedList = list.OrderByDescending(d => float.Parse(d["score"])).ToList();

        currentHighscores = sortedList;

        // serialize the dictionary and save it back to playerprefs.
        string newSerializedString = JsonConvert.SerializeObject(sortedList);
        PlayerPrefs.SetString(levelManager.currentLevelIndex.ToString(), newSerializedString);

        Debug.Log("Loading high score");
        
        
        gameObject.SetActive(true);
        timeValueText.text = time.ToString();
        timeScoreText.text = timeScore.ToString();
        collisionValueText.text = numberOfCollisions.ToString();
        collisionScoreText.text = collisionScore.ToString();
        accuracyValueText.text = parkingAccuracyPercentage.ToString();
        accuracyScoreText.text = parkingScore.ToString();
        totalScoreText.text = score.ToString();
        DisplayHighScore();
    }

    private void DisplayHighScore() {
        
        foreach (Transform child in highscore.transform) {
            GameObject.Destroy(child.gameObject);
        }

        float height = -1;
        for (int i = 0; i < currentHighscores.Count; i++) {
            if (i == 9) return;
            var d = currentHighscores.ElementAt(i);
            GameObject rowObject = Instantiate(highscoreRowPrefab, highscore.transform);
            rowObject.transform.localPosition = new Vector3(0, height, 0);
            HighscoreRow script = rowObject.GetComponent<HighscoreRow>();
            script.setRowValues(d["name"], d["score"]);
            height -= 0.2f;
        }
    }
}

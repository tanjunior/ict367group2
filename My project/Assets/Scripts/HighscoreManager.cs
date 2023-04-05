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
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private BoxCollider carCollider;
    [SerializeField] private TextMeshPro timeValueText, collisionValueText, accuracyValueText, timeScoreText, collisionScoreText, accuracyScoreText, totalScoreText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnParked(float time) {
        SaveHighScore(time);
    }

    // calculate the overlap volume of two box colliders
    private float CalculateOverlapVolume(BoxCollider box1, BoxCollider box2)
    {
        //Debug.Log(box1.name);
        //Debug.Log(box2.name);
        Vector3 overlapSize = Vector3.Min(box1.bounds.max, box2.bounds.max) - Vector3.Max(box1.bounds.min, box2.bounds.min);
        overlapSize = Vector3.Max(overlapSize, Vector3.zero);
        return overlapSize.x * overlapSize.y * overlapSize.z;
    }

    private float getAccuracyPercentage()
    {
        BoxCollider parkingTrigger = GameObject.FindGameObjectWithTag("ParkingSpot").GetComponent<BoxCollider>();
;
        // if (parkingTrigger == null)
        //     parkingTrigger = GameObject.FindGameObjectWithTag("ParkingSpot").GetComponent<BoxCollider>();

        float overlapVolume = CalculateOverlapVolume(carCollider, parkingTrigger);
        float carVolume = carCollider.bounds.size.x * carCollider.bounds.size.z * carCollider.bounds.size.y;
        return overlapVolume / carVolume * 100f;
        //Debug.Log("Parking accuracy: " + accuracyPercentage.ToString("F2") + "%");
        //return accuracyPercentage;
    }

    private float calculateScore(float time) {
        float timeScore,collisionScore;

        if(time<60) timeScore = 1000;
        else if(time<120) timeScore = 800;
        else if(time<250) timeScore = 400;
        else if(time<500) timeScore = 200;
        else timeScore = 0;

        int numberOfCollisions = colliderCheck.getNumberOfCollisions();
        colliderCheck.resetNumberOfCollisions();

        if(numberOfCollisions<1) collisionScore = 1500;
        else if(numberOfCollisions<4) collisionScore = 1100;
        else if(numberOfCollisions <8) collisionScore = 700;
        else collisionScore = 0;

        float parkingAccuracyPercentage = getAccuracyPercentage();
        float parkingScore =  parkingAccuracyPercentage*20;
        float total = parkingScore + collisionScore + timeScore;

        timeValueText.text = time.ToString("F2");
        timeScoreText.text = timeScore.ToString();
        collisionValueText.text = numberOfCollisions.ToString();
        collisionScoreText.text = collisionScore.ToString();
        accuracyValueText.text = parkingAccuracyPercentage.ToString("F2");
        accuracyScoreText.text = parkingScore.ToString("F0");
        totalScoreText.text = total.ToString("F0");

        return total;
    }

    private void SaveHighScore(float time) {
        float score = calculateScore(time);

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

        //Debug.Log("Loading high score");
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
            script.setRowValues((i+1)+". "+d["name"], d["score"]);
            height -= 0.2f;
        }
    }
}

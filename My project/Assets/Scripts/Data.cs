using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;


[CreateAssetMenu(fileName = "New Data", menuName = "Data", order = 0)]
public class Data : ScriptableObject {
    public int currentLevel;
    public int prevLevel;
    public string newSerializedString;
    public Dictionary<string, float> currentHighscores;

    public async Task SaveHighScore(float time) {
        string name = Environment.UserName; //temp player name var
        Dictionary<string, float> highscore; // dictionary kv pair to store name and score
        if (PlayerPrefs.HasKey(currentLevel.ToString())) { // check if highscore for this level exists in the playerprefs
            string serializedString = PlayerPrefs.GetString(currentLevel.ToString());
            Debug.Log(serializedString);
            highscore = JsonUtility.FromJson<Dictionary<string, float>>(serializedString); // the dictionary was serialized before saving so we need to deserialize the string
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

        currentHighscores = sorted;
        
        newSerializedString = JsonUtility.ToJson(sorted);

        // serialize the dictionary and save it back to playerprefs.
        PlayerPrefs.SetString(currentLevel.ToString(), newSerializedString);
        //SceneManager.LoadScene("Highscore", LoadSceneMode.Single);
    }
}
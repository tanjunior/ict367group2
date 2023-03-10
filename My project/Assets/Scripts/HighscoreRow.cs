using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HighscoreRow : MonoBehaviour
{
    [SerializeField] private TextMeshPro nameTextMesh;
    [SerializeField] private TextMeshPro scoreTextMesh;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void setRowValues(string name, string score) {
        nameTextMesh.text = name;
        scoreTextMesh.text = score;
    }
}

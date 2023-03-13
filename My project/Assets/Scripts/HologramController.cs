using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HologramController : MonoBehaviour
{
    private LevelManager levelManager;
    [SerializeField] private GameObject hologram;
    // Start is called before the first frame update
    void Start()
    {
        levelManager = GameObject.Find("VR").GetComponent<LevelManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (levelManager.showHologram) hologram.SetActive(true);
        else hologram.SetActive(false);
    }
}

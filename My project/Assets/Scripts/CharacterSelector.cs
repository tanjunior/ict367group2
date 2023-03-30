using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [SerializeField] private TextMeshPro charText;
    private int charIndex = 0;
    private char[] alphabet = new char[] {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
    public string text = "A";
    public UnityEvent onCharacterChanged;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }    

    public void CharacterUp() {
        if (charIndex < alphabet.Length-1) charIndex += 1;
        else charIndex = 0;
        text = alphabet[charIndex].ToString();
        charText.text = text;
        onCharacterChanged.Invoke();
    }

    public void CharacterDown() {
        if (charIndex > 0) charIndex -= 1;
        else charIndex = alphabet.Length-1;
        text = alphabet[charIndex].ToString();
        charText.text = text;
        onCharacterChanged.Invoke();
    }
}

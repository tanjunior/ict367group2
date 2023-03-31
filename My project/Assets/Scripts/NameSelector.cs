using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameSelector : MonoBehaviour
{
    [SerializeField] private CharacterSelector[] chars = new CharacterSelector[3];
    private string name = "ICT";

    public string GetName() {
        return name;
    }

    public void NameChanged() {
        name = chars[0].text + chars[1].text + chars[2].text;
    }
}

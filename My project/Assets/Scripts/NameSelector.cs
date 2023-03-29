using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class OnNameChanged : UnityEvent<string>
{
}


public class NameSelector : MonoBehaviour
{
    [SerializeField] private CharacterSelector[] chars = new CharacterSelector[3];
    public OnNameChanged onNameChanged;

    public void NameChanged() {
        onNameChanged.Invoke(chars[0].text + chars[1].text + chars[2].text);
    }
}

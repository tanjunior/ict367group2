using UnityEngine;
using UnityEngine.Events;

public class MouseInteractSpatialTarget : MonoBehaviour
{
  public UnityEvent onClicked;

  void OnMouseUp()
  {
    onClicked.Invoke();
    
  }
}

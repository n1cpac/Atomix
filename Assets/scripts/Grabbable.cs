using UnityEngine;

public class Grabbable : MonoBehaviour
{
    [HideInInspector] public bool IsGrabbed = false;

    // Llamar estos métodos desde tu sistema de agarre
    public void GrabStart() => IsGrabbed = true;
    public void GrabEnd() => IsGrabbed = false;
}
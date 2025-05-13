using UnityEngine;


public class TriggerTester : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"TRIGGER DETECTADO CON: {other.name}, TAG: {other.tag}");
    }
    
}

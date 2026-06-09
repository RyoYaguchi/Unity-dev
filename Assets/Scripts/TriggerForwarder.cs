using UnityEngine;

public class TriggerForwarder : MonoBehaviour
{
    public GameObject target;

    private void OnTriggerEnter(Collider other)
    {
        if (target != null)
        {
            target.SendMessage("OnTriggerEnter", other, SendMessageOptions.DontRequireReceiver);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (target != null)
        {
            target.SendMessage("OnTriggerExit", other, SendMessageOptions.DontRequireReceiver);
        }
    }
}

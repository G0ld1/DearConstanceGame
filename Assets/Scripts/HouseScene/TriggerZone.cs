using UnityEngine;

// TriggerZone.cs - Para detectar quando Frank chega a certos locais
public class TriggerZone : MonoBehaviour
{
    [SerializeField] private TriggerType triggerType;
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (triggerType)
            {
                case TriggerType.ConstanceRoom:
                    GameManager.Instance.OnReachedConstance();
                    break;
                case TriggerType.StudyRoom:
                    GameManager.Instance.OnReachedStudy();
                    break;
            }
        }
    }
}

public enum TriggerType
{
    ConstanceRoom,
    StudyRoom
}

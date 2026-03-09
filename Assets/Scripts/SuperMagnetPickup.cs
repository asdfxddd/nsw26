using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SuperMagnetPickup : MonoBehaviour
{
    [SerializeField, Tooltip("플레이어 태그")]
    private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        MagnetBoostController boostController = other.GetComponent<MagnetBoostController>();
        if (boostController == null)
        {
            boostController = other.GetComponentInParent<MagnetBoostController>();
        }

        if (boostController != null)
        {
            boostController.MagnetBoost();
            Destroy(gameObject);
        }
    }
}

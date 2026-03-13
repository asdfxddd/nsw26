using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SuperMagnetPickup : MagnetCollectible
{
    public override bool CanUseBoostedPickupRadius => false;

    protected override void OnCollected()
    {
        if (AttractionTarget == null)
        {
            return;
        }

        MagnetBoostController boostController = AttractionTarget.GetComponent<MagnetBoostController>();
        if (boostController == null)
        {
            boostController = AttractionTarget.GetComponentInParent<MagnetBoostController>();
        }

        if (boostController != null)
        {
            boostController.MagnetBoost();
        }
    }
}

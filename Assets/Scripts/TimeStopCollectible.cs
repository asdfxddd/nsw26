using UnityEngine;

public class TimeStopCollectible : MagnetCollectible
{
    public override bool CanUseBoostedPickupRadius => false;

    protected override void OnCollected()
    {
        TimeStopController.TriggerFromPickup();
    }
}

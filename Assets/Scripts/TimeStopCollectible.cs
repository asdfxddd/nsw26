using UnityEngine;

public class TimeStopCollectible : MagnetCollectible
{
    protected override void OnCollected()
    {
        TimeStopController.TriggerFromPickup();
    }
}

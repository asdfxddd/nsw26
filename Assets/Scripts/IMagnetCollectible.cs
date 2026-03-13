public interface IMagnetCollectible
{
    bool IsCollected { get; }
    bool CanUseBoostedPickupRadius { get; }

    void BeginMagnetAttraction(UnityEngine.Transform target, float moveSpeed, float collectDistance);
}

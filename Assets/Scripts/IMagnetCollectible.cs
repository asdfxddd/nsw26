public interface IMagnetCollectible
{
    bool IsCollected { get; }

    void BeginMagnetAttraction(UnityEngine.Transform target, float moveSpeed, float collectDistance);
}

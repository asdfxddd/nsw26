using UnityEngine;

[DisallowMultipleComponent]
public class CollectibleController : MonoBehaviour
{
    [SerializeField, Tooltip("자석 흡수 대상 태그")]
    private string collectibleTag = "MagnetCollectible";

    [SerializeField, Tooltip("자석 흡수 이동 속도")]
    private float magnetMoveSpeed = 10f;

    [SerializeField, Tooltip("플레이어 도달 판정 거리")]
    private float collectDistance = 0.05f;

    public bool TryGetSettings(string targetTag, out float moveSpeed, out float distance)
    {
        moveSpeed = 0f;
        distance = 0f;

        if (string.IsNullOrEmpty(collectibleTag) || !string.Equals(collectibleTag, targetTag))
        {
            return false;
        }

        moveSpeed = magnetMoveSpeed;
        distance = collectDistance;
        return true;
    }

    private void OnValidate()
    {
        magnetMoveSpeed = Mathf.Max(0.01f, magnetMoveSpeed);
        collectDistance = Mathf.Max(0.01f, collectDistance);
    }
}

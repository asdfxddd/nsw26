using UnityEngine;

public class ExpOrbController : MagnetCollectible
{
    [SerializeField]
    private int expValue = 1;

    public int ExpValue => expValue;

    protected override void OnCollected()
    {
        ExpDropManager.Instance.AddExp(expValue);
    }

    protected override void OnValidate()
    {
        base.OnValidate();
        expValue = Mathf.Max(1, expValue);
    }
}

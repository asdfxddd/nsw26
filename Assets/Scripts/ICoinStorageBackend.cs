public interface ICoinStorageBackend
{
    bool TryLoad(out CoinSaveData data);
    bool TrySave(CoinSaveData data);
}

namespace PS1Ubr
{
    public interface ILoadable
    {
        bool Load(ref CDynMemoryReader r, ref CUberData data);
    }
}

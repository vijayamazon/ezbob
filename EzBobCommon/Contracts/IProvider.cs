namespace EzBobCommon.Contracts
{
    public interface IProvider<out T> 
    {
        T GetNew();
    }
}

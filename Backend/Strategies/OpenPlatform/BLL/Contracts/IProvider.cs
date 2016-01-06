namespace Ezbob.Backend.Strategies.OpenPlatform.BLL.Contracts
{
    public interface IProvider<out T> 
    {
        T GetNew();
    }
}

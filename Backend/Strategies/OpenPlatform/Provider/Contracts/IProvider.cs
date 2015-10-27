namespace Ezbob.Backend.Strategies.OpenPlatform.Provider.Contracts
{
    public interface IProvider<out T> 
    {
        T GetNew();
    }
}

using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.PostCode
{
    public interface IPostCodeFacade
    {
        IPostCodeResponse GetAddressFromPostCode(Customer customer, string postCode);
        IPostCodeResponse GetFullAddressFromPostCode(Customer customer, string id);
    }
}
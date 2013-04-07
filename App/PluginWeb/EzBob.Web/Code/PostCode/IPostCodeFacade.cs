using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Code.PostCode
{
    public interface IPostCodeFacade
    {
        IPostCodeResponse GetAddressFromPostCode(Customer customer, string postCode);
        IPostCodeResponse GetFullAddressFromPostCode(Customer customer, string id);
    }
}
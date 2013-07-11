using ApplicationMng.Model;

namespace EzBob.Web.Areas.Customer.Models
{
    public class DirectorAddressModel : CustomerAddress
    {
        public virtual int DirectorId { get; set; }
    }
}
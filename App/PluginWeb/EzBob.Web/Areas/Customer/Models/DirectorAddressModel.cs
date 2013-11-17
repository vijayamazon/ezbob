namespace EzBob.Web.Areas.Customer.Models
{
	using EZBob.DatabaseLib.Model.Database;

	public class DirectorAddressModel : CustomerAddress
    {
        public virtual int DirectorId { get; set; }
    }
}
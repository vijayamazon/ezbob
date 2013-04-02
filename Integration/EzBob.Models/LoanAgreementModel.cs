using System.Globalization;

namespace EzBob.Web.Areas.Customer.Models
{
    public class LoanAgreementModel
    {
        public LoanAgreementModel()
        {
        }

        public LoanAgreementModel(int id, string name)
        {
            Id = id;
            Name = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(name);
        }

        public int Id { get; set; }
        public string Name { get; set; }
    }
}
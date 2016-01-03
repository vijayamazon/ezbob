using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels
{
    public class AlibabaBuyer
    {
        public int Id { get; set; }
        public long AliId { get; set; }
        public decimal? Freeze { get; set; }
        public int CustomerId { get; set; }
        public int? ContractId { get; set; }
        public string BussinessName { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string AuthRepFname { get; set; }
        public string AuthRepLname { get; set; }
        public string Phone { get; set; }
        public string Fax { get; set; }
        public string Email { get; set; }
        public int? OrderRequestCountLastYear { get; set; }
        public bool? ConfirmShippingDocAndAmount { get; set; }
        public string FinancingType { get; set; }
        public bool? ConfirmReleaseFunds { get; set; }
    }
}

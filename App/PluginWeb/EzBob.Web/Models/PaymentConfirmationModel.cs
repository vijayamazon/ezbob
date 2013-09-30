namespace EzBob.Web.Models
{
	using System.Web.Mvc;
	using Newtonsoft.Json;

    public class PaymentConfirmationModel
    {
        public string amount { get; set; }

        public decimal saved { get; set; }

        public decimal savedPounds { get; set; }

        public string card_no { get; set; }

        public string email { get; set; }

        public string surname { get; set; }

        public string name { get; set; }

        public string refnum { get; set; }

        public string transRefnums { get; set; }

        public bool hasLateLoans { get; set; }

        public bool isRolloverPaid { get; set; }

		public bool IsEarly { get; set; }

        public MvcHtmlString ToJSON()
        {
            return new MvcHtmlString(JsonConvert.SerializeObject(this));
        }
    }
}
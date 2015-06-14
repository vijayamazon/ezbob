namespace EzBob.Models
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Converters;

	public class EditLoanDetailsModel
    {
        private readonly List<string> _errors = new List<string>();

        public int Id { get; set; }

        public List<SchedultItemModel> Items { get; set; }

        public decimal InterestRate { get; set; }

        public decimal SetupFee { get; set; }

        public decimal Amount { get; set; }

        public long CashRequestId { get; set; }

        public DateTime Date { get; set; }

        public int WithinWeek { get; set; }

        public int WithinMonth { get; set; }

        public int OutsideWeek { get; set; }

        public int OutsideMonth { get; set; }

        public decimal? ReschedulingBalance { get; set; }

        public static EditLoanDetailsModel Parse(string json)
        {
            return JsonConvert.DeserializeObject<EditLoanDetailsModel>(json);
        }

        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, new IsoDateTimeConverter());
        }

        public List<string> Errors
        {
            get { return _errors; }
        }

        public string LoanType { get; set; }

        public bool HasErrors { get { return Errors.Any(); } }

		public List<string> InterestFreeze { get; set; } 

        public void Validate()
        {
            Errors.Clear();
            
            var installments = Items.Where(i => i.Type == "Installment").OrderBy(i => i.Date).ToList();
            var fees = Items.Where(i => i.Type == "Fee").OrderBy(i => i.Date).ToList();

            CheckZeroBalance(installments);
            CheckMaxBalance(installments);
            CheckInstallmentEarlyDate(installments);
            CheckOnlyOneInstallmentIsZero(installments);
            CheckNoZeroFees(fees);
        }

        private void CheckInstallmentEarlyDate(IEnumerable<SchedultItemModel> installments)
        {
            var tooEarlyInstallment = installments.Any(i => i.Date.Date < Date.AddDays(1).Date);

            if (tooEarlyInstallment)
            {
                Errors.Add("The installment date should be greater than loan date.");
            }
        }

        private void CheckZeroBalance(IEnumerable<SchedultItemModel> installments)
        {
            var noZeroBalance = installments.Last().Balance != 0;
            if (noZeroBalance)
            {
                Errors.Add("Last installment should have zero balance.");
            }
        }

        private void CheckMaxBalance(IEnumerable<SchedultItemModel> installments)
        {
            var balanceTooBig = installments.Any(i => i.Balance > Amount);

            if (balanceTooBig)
            {
                Errors.Add("Installment Balance is too big");
            }
        }

        private void CheckOnlyOneInstallmentIsZero(IEnumerable<SchedultItemModel> installments)
        {
            var zeroInstallments = installments.Count(i => i.Balance == 0);
            if (zeroInstallments > 1)
            {
                Errors.Add("Only one installment with zero balance is allowed");
            }
        }

        private void CheckNoZeroFees(IEnumerable<SchedultItemModel> fees)
        {
            if (fees.Count(i => i.Fees == 0) > 1)
            {
                Errors.Add("Zero fees are not allowed");
            }
        }

	   
	    public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(EditLoanDetailsModel);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append("\t\t" + prop.Name).Append(": ").Append(prop.GetValue(this)).Append(";");
			}
			return sb.ToString();
	    }
    }
}
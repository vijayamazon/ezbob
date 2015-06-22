namespace EzBob.Models
{
	using System;
	using System.Text;

	public class SchedultItemModel
    {
        public int Id { get; set; }

        public DateTime Date { get; set; }
        public decimal Principal { get; set; }
        public decimal Balance { get; set; }
        public decimal BalanceBeforeRepayment { get; set; }
        public decimal Interest { get; set; }
        public decimal InterestRate { get; set; }
        public decimal Fees { get; set; }
        public decimal Total { get; set; }

        public string Status { get; set; }
        public string Description { get; set; }

        public string Type { get; set; }

        public string Editor { get; set; }
        public bool Editable { get; set; }
        public bool Deletable { get; set; }


		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(SchedultItemModel);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t\t");
			}
			return sb.ToString();
		}
    }
}
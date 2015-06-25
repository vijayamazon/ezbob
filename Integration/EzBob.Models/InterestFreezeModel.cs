namespace EzBob.Models {
	using System;
	using System.Text;

	public class InterestFreezeModel {

		public int Id { get; set; }
		public DateTime StartDate { get; set; }
		public DateTime? EndDate { get; set; }
		public decimal InterestRate { get; set; }
		public DateTime ActivationDate { get; set; }
		public DateTime? DeactivationDate { get; set; }


		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(InterestFreezeModel);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t\t");
			}
			return sb.ToString();
		}
	}
}
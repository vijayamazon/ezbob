namespace Ezbob.Backend.Models {
	using System;
	using System.Text;

	public class CustomerStatusTransition {

		public string OldStatus { get; set; }
		public string NewStatus { get; set; }
		public DateTime ChangeDate { get; set; }
		public bool OldIsDefault { get; set; }
		public bool NewIsDefault { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(CustomerStatusTransition);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}
	}
}

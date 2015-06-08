namespace Ezbob.Backend.CalculateLoan.Models.Helpers {
	using System;
	using System.Text;

	public class Rollovers {

		public Rollovers() { }

		public Rollovers(DateTime created, DateTime? expired, decimal fee) {
			CreationTime = created;
			if (expired != null)
				ExpirationTime = (DateTime)expired;
			Fee = fee;
		}

		public decimal Fee { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime ExpirationTime { get; set; }


		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": ");
			Type t = typeof(Rollovers);
			foreach (var prop in t.GetProperties()) {
				if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \t");
			}
			return sb.ToString();
		}

	}
}
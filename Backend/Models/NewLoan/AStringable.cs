namespace Ezbob.Backend.Models.NewLoan {
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils;

	[DataContract(IsReference = true)]
	public abstract class AStringable {
		public override string ToString() {
			StringBuilder sb = new StringBuilder(GetType().Name + ": ");

			this.Traverse((instance, pi) => {
				object val = pi.GetValue(this);

				if ((val != null) && DisplayFieldInToString(pi.Name))
					sb.Append(pi.Name).Append(": ").Append(val).Append("; \n");
			});

			return sb.ToString();
		} // ToString

		protected virtual bool DisplayFieldInToString(string fieldName) {
			return true;
		} // DisplayFieldInToString
	} // class AStringable
} // namespace

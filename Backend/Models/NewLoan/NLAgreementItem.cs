namespace Ezbob.Backend.Models.NewLoan
{
	using System;
	using System.Runtime.Serialization;
    using System.Text;
    using Ezbob.Backend.ModelsWithDB.NewLoan;

    [DataContract]
	public class NLAgreementItem
    {

        [DataMember]
		public NL_LoanAgreements Agreement { get; set; }

        [DataMember]
		public TemplateModel TemplateModel { get; set; }


		
		public string Path1 { get; set; }

		public string Path2 { get; set; }

		public override string ToString() {
			StringBuilder sb = new StringBuilder(this.GetType().Name + ": \n");
			Type t = typeof(NLAgreementItem);
			foreach (var prop in t.GetProperties()) {

				if(prop.PropertyType == typeof(NL_LoanAgreements)) {
					prop.GetValue(this).ToString();
				} 
				else if (prop.GetValue(this) != null)
					sb.Append(prop.Name).Append(": ").Append(prop.GetValue(this)).Append("; \n");
			}
			return sb.ToString();
		}
    }
}

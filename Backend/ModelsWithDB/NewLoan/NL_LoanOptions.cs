namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Extensions;

	[DataContract(IsReference = true)]
	public class NL_LoanOptions : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanOptionsID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		public long LoanID { get; set; }

		[DataMember]
		public bool AutoCharge { get; set; }

		[DataMember]
		public DateTime? StopAutoChargeDate { get; set; }

		[DataMember]
		public bool AutoLateFees { get; set; }

		[DataMember]
		public DateTime? StopAutoLateFeesDate { get; set; }

		[DataMember]
		public bool AutoInterest { get; set; }

		[DataMember]
		public DateTime? StopAutoInterestDate { get; set; }

		[DataMember]
		public bool ReductionFee { get; set; }

		[DataMember]
		public bool LatePaymentNotification { get; set; }

		[Length(50)]
		[DataMember]
		public string CaisAccountStatus { get; set; }

		[Length(20)]
		[DataMember]
		public string ManualCaisFlag { get; set; }

		[DataMember]
		public bool EmailSendingAllowed { get; set; }

		[DataMember]
		public bool SmsSendingAllowed { get; set; }

		[DataMember]
		public bool MailSendingAllowed { get; set; }

		[DataMember]
		public int? UserID { get; set; }

		[DataMember]
		public DateTime InsertDate { get; set; }

		[DataMember]
		public bool IsActive { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		/// <summary>
		/// prints data only
		/// to print headers line call base static GetHeadersLine 
		/// </summary>
		/// <returns></returns>
		public override string ToString() {
			try {
				return ToStringTable();
			} catch (InvalidCastException invalidCastException) {
				Console.WriteLine(invalidCastException);
			}
			return string.Empty;
		}

		/*public override string ToString() {
			Type t = typeof(NL_LoanOptions);
			var props = FilterPrintable(t);

			string lineSeparator = lineSeparatorChar.PadRight(ColumnTotalWidth * props.Count, '-') + Environment.NewLine;
			StringBuilder sb = new StringBuilder(propertyDelimiter);

			foreach (var x in ForeachExt.WithIndex(props)) {
				PropertyInfo prop = x.Value;
				var val = prop.GetValue(this);
				string strVal = "--";
				if (val != null) {

					var formatattr = prop.GetCustomAttribute(typeof(DecimalFormatAttribute)) as DecimalFormatAttribute;
					if (formatattr != null)
						strVal = formatattr.Formatted((decimal)val);

					var enumattr = prop.GetCustomAttribute(typeof(EnumNameAttribute)) as EnumNameAttribute;
					if (enumattr != null)
						strVal = enumattr.GetName((int)val);
				}

				// ReSharper disable once PossibleNullReferenceException
				sb.Append(strVal.PadRight(ColumnTotalWidth)).Append(propertyDelimiter);

				if (x.IsLast)
					sb.Append(lineSeparator);
			}

			return sb.ToString();
		}*/

	} // class NL_LoanOptions
} // ns

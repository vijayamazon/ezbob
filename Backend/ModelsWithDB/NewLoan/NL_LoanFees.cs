namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Extensions;

	[DataContract(IsReference = true)]
	public class NL_LoanFees : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanFeeID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		[ExcludeFromToString]
		public long LoanID { get; set; }

		[FK("NL_LoanFeeTypes", "LoanFeeTypeID")]
		[DataMember]
		[EnumName(typeof(NLFeeTypes))]
		public int LoanFeeTypeID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int AssignedByUserID { get; set; } // Use 1 for automatically assigned fees. 

		[DataMember]
		[DecimalFormat("C2")]
		public decimal Amount { get; set; }

		[DataMember]
		public DateTime CreatedTime { get; set; }

		[DataMember]
		public DateTime AssignTime { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }

		[DataMember]
		public DateTime? DisabledTime { get; set; }

		[Length(LengthType.MAX)]
		[DataMember]
		public string Notes { get; set; }

		public new static int ColumnTotalWidth = 26;

		public override string ToString() {
			Type t = typeof(NL_LoanFees);
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
		}

	} // class NL_LoanFees
} // ns

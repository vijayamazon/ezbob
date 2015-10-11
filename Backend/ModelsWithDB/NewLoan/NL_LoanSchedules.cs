namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using DbConstants;
	using Ezbob.Utils;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Extensions;

	[DataContract(IsReference = true)]
	public class NL_LoanSchedules : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanScheduleID { get; set; }

		[FK("NL_LoanHistory", "LoanHistoryID")]
		[DataMember]
		public long LoanHistoryID { get; set; }

		[FK("NL_LoanScheduleStatuses", "LoanScheduleStatusID")]
		[DataMember]
		[EnumName(typeof(NLScheduleStatuses))]
		public int LoanScheduleStatusID { get; set; }

		[DataMember]
		public int Position { get; set; }

		[DataMember]
		public DateTime PlannedDate { get; set; }

		[DataMember]
		public DateTime? ClosedTime { get; set; }

		[DataMember]
		[DecimalFormat("C2")]
		public decimal Principal { get; set; }

		[DataMember]
		[DecimalFormat("P4")]
		public decimal InterestRate { get; set; }


		// additions
		private decimal _balance;
		private decimal _interest;
		private decimal _feesAmount;
		private decimal _amountDue;
		private decimal _interestPaid;
		private decimal _feesPaid;

		[DataMember] // p*r
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal Interest {
			get { return this._interest; }
			set { this._interest = value; }
		}

		[DataMember]
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal FeesAmount {
			get { return this._feesAmount; }
			set { this._feesAmount = value; }
		}

		[DataMember]
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal AmountDue {
			get { return this._amountDue; }
			set { this._amountDue = value; }
		}

		[DataMember]
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal InterestPaid {
			get { return this._interestPaid; }
			set { this._interestPaid = value; }
		}

		[DataMember]
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal FeesPaid {
			get { return this._feesPaid; }
			set { this._feesPaid = value; }
		}

		[DataMember]
		[NonTraversable]
		[DecimalFormat("C2")]
		public decimal Balance {
			get { return this._balance; }
			set { this._balance = value; }
		}

		public new static int ColumnTotalWidth = 20;

		public override string ToString() {
			Type t = typeof(NL_LoanSchedules);
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

	} // class NL_LoanSchedules
} // ns

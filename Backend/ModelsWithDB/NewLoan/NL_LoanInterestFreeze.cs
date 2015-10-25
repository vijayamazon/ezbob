namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Reflection;
	using System.Runtime.Serialization;
	using System.Text;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;
	using Ezbob.Utils.Extensions;

	[DataContract(IsReference = true)]
	public class NL_LoanInterestFreeze : AStringable {
		[PK(true)]
		[DataMember]
		public long LoanInterestFreezeID { get; set; }

		[FK("NL_Loans", "LoanID")]
		[DataMember]
		[ExcludeFromToString]
		public long LoanID { get; set; }

		[DataMember]
		public DateTime? StartDate { get; set; }

		[DataMember]
		public DateTime? EndDate { get; set; }

		[DataMember]
		[DecimalFormat("C2")]
		public decimal InterestRate { get; set; }

		[DataMember]
		public DateTime? ActivationDate { get; set; }

		[DataMember]
		public DateTime? DeactivationDate { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int AssignedByUserID { get; set; }

		[FK("Security_User", "UserId")]
		[DataMember]
		public int? DeletedByUserID { get; set; }


		public new static int ColumnTotalWidth = 26;

		/// <summary>
		/// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
		/// </summary>
		/// <returns>
		/// true if the specified object  is equal to the current object; otherwise, false.
		/// </returns>
		/// <param name="obj">The object to compare with the current object. </param>
		public override bool Equals(object obj) {
			NL_LoanInterestFreeze p = obj as NL_LoanInterestFreeze;
			if (p == null)
				return false;

			if (ActivationDate != null && p.ActivationDate == null)
				return false;
			if (ActivationDate == null && p.ActivationDate != null)
				return false;
			if (DeactivationDate != null && p.DeactivationDate == null)
				return false;
			if (DeactivationDate == null && p.DeactivationDate != null)
				return false;

			return (LoanID == p.LoanID) && (StartDate == p.StartDate) && (EndDate == p.EndDate);
		}

		/// <summary>
		/// Serves as a hash function for a particular type. 
		/// </summary>
		/// <returns>
		/// A hash code for the current <see cref="T:System.Object"/>.
		/// </returns>
		public override int GetHashCode() {
			return LoanID.GetHashCode() ^ StartDate.GetHashCode() ^ EndDate.GetHashCode() ^ ActivationDate.GetHashCode() ^ DeactivationDate.GetHashCode();
		}

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
			Type t = typeof(NL_LoanInterestFreeze);
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
		
	} // class NL_LoanInterestFreeze
} // ns
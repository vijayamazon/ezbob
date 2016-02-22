namespace Ezbob.Backend.ModelsWithDB.NewLoan {
	using System;
	using System.Runtime.Serialization;
	using Ezbob.Utils.Attributes;
	using Ezbob.Utils.dbutils;

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

		[DataMember]
		public int? OldID { get; set; }
	
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

			if (StartDate == null && p.StartDate != null)
				return false;
			if (StartDate != null && p.StartDate == null)
				return false;
			if (StartDate.HasValue && p.StartDate.HasValue && !StartDate.Value.Date.Equals(p.StartDate.Value.Date))
				return false;

			if (EndDate == null && p.EndDate != null)
				return false;
			if (EndDate != null && p.EndDate == null)
				return false;
			if (EndDate.HasValue && p.EndDate.HasValue && !EndDate.Value.Date.Equals(p.EndDate.Value.Date))
				return false;

			return (LoanID == p.LoanID);// && (StartDate == p.StartDate) && (EndDate == p.EndDate);
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
		
	} // class NL_LoanInterestFreeze
} // ns
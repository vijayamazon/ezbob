namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using EZBob.DatabaseLib.Model.Database;

	[Serializable]
	public class DecisionModel {
		/// <summary>
		/// Customer ID.
		/// </summary>
		public int id { get; set; }

		/// <summary>
		/// New customer status (i.e. customer's status should be changed from whatever it is to this).
		/// </summary>
		public CreditResultStatus status { get; set; }

		/// <summary>
		/// Reason to change status (underwriter comment).
		/// </summary>
		public string reason { get; set; }

		/// <summary>
		/// List of standard rejection reasons in case of rejection.
		/// </summary>
		public int[] rejectionReasons { get; set; }

		/// <summary>
		/// 1: customer is in "waiting for signature" status.
		/// otherwise: customer is not in "waiting for signature" status.
		/// </summary>
		public int signature { get; set; }
	} // class DecisionModel
} // namespace

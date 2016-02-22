namespace Ezbob.Backend.Models {
	using System;
	using System.Collections.Generic;
	using System.Diagnostics.CodeAnalysis;
	using System.Runtime.Serialization;
	using Ezbob.Utils;
	using EZBob.DatabaseLib.Model.Database;

	[DataContract]
	[Serializable]
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public class DecisionModel {
		/// <summary>
		/// Customer ID. Decision is being made for this customer.
		/// </summary>
		[DataMember]
		public int customerID { get; set; }

		/// <summary>
		/// New customer status (i.e. customer's status should be changed from whatever it is to this).
		/// </summary>
		[DataMember]
		public CreditResultStatus status { get; set; }

		/// <summary>
		/// Reason to change status (underwriter comment).
		/// </summary>
		[DataMember]
		public string reason { get; set; }

		/// <summary>
		/// List of standard rejection reasons in case of rejection.
		/// </summary>
		[DataMember]
		public int[] rejectionReasons { get; set; }

		/// <summary>
		/// 1: customer is in "waiting for signature" status.
		/// otherwise: customer is not in "waiting for signature" status.
		/// </summary>
		[DataMember]
		public int signature { get; set; }

		/// <summary>
		/// Cash request ID. Decision is being made for this cash request.
		/// No decision can be made if there is no cash request.
		/// </summary>
		[DataMember]
		public long cashRequestID { get; set; }

		/// <summary>
		/// Cash request row version (timestamp counter) when UW page was loaded.
		/// No decision can be made if cash request row version doesn't match this value.
		/// </summary>
		[DataMember]
		public string cashRequestRowVersion { get; set; }

		/// <summary>
		/// Underwriter ID. Decision is being made by this underwriter.
		/// </summary>
		[DataMember]
		public int underwriterID { get; set; }

		/// <summary>
		/// Internal id, used to track decision process in logs.
		/// </summary>
		[DataMember]
		public string attemptID { get; set; }

		/// <summary>
		/// Used in PendingInvestor flow to assign investor manually
		/// </summary>
		[DataMember]
		public bool ForceInvestor { get; set; }

		/// <summary>
		/// Used in PendingInvestor flow to assign investor manually
		/// </summary>
		[DataMember]
		public int? InvestorID { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			var lst = new List<string>();

			this.Traverse((instance, pi) => {
				object v = pi.GetValue(instance);

				string val = (v == null) ? "-- null --" : v.ToString();

				if ((v != null) && TypeUtils.IsEnumerable(v.GetType()))
					val = string.Join(", ", v);

				lst.Add(string.Format(
					"{0}({1}): '{2}'",
					pi.Name,
					pi.PropertyType,
					val
				));
			});

			return string.Join("; ", lst);
		} // ToString
	} // class DecisionModel

	public static class DecisionModelExt {
		public static string Stringify(this DecisionModel model) {
			return model == null ? "UNDEFINED MODEL" : model.ToString();
		} // Stringify
	} // class DecisionModelExt
} // namespace

namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using EZBob.DatabaseLib.Model.Database;

	[Serializable]
	public class DecisionModel
	{
		public int id { get; set; }
		public CreditResultStatus status{ get; set; }
		public string reason{ get; set; }
		public int[] rejectionReasons { get; set; }
		public int signature { get; set; }
	}
}
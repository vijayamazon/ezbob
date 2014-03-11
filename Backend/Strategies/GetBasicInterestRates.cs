namespace EzBob.Backend.Strategies {
	using System.Collections.Generic;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class OneBasicInterestRate
	{
		public int Id { get; set; }
		public int FromScore { get; set; }
		public int ToScore { get; set; }
		public int LoanInterestBase { get; set; }
	}

	public class GetBasicInterestRates : AStrategy
	{
		public List<OneBasicInterestRate> BasicInterestRates { get; set; }
		#region constructor

		public GetBasicInterestRates(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
			BasicInterestRates = new List<OneBasicInterestRate>();
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Get basic interest rates"; }
		} // Name

		#endregion property Name

		#region property Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetBasicInterestRates", CommandSpecies.StoredProcedure);
			foreach (DataRow row in dt.Rows)
			{
				var sr = new SafeReader(row);

				var tmp = new OneBasicInterestRate();
				tmp.Id = sr["Id"];
				tmp.FromScore = sr["FromScore"];
				tmp.ToScore = sr["ToScore"];
				tmp.LoanInterestBase = sr["LoanInterestBase"];
				BasicInterestRates.Add(tmp);
			}
		} // Execute

		#endregion property Execute
	} // class GetBasicInterestRates
} // namespace

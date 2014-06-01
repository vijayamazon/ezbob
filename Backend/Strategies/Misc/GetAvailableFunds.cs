namespace EzBob.Backend.Strategies.Misc {
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;

	public class GetAvailableFunds : AStrategy
	{
		#region constructor

		public GetAvailableFunds(AConnection oDb, ASafeLog oLog)
			: base(oDb, oLog)
		{
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Get Available Funds"; }
		} // Name

		#endregion property Name

		public decimal AvailableFunds { get; private set; }

		#region property Execute

		public override void Execute() {
			DataTable dt = DB.ExecuteReader("GetAvailableFunds", CommandSpecies.StoredProcedure);
			var sr = new SafeReader(dt.Rows[0]);
			AvailableFunds = sr["AvailableFunds"];
		} // Execute

		#endregion property Execute
	} // class GetAvailableFunds
} // namespace

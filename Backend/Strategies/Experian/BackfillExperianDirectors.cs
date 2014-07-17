namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Database;
	using Ezbob.Logger;

	public class BackfillExperianDirectors : AStrategy {
		#region constructor

		public BackfillExperianDirectors(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nCustomerID = nCustomerID.HasValue ? (nCustomerID > 0 ? nCustomerID : null) : null;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "BackfillExperianDirectors"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (m_nCustomerID.HasValue)
				DB.ExecuteNonQuery("DELETE FROM ExperianDirectors WHERE CustomerID = " + m_nCustomerID.Value, CommandSpecies.Text);
			else
				DB.ExecuteNonQuery("DELETE FROM ExperianDirectors", CommandSpecies.Text);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					int nCustomerID = sr["CustomerID"];
					string sServiceType = sr["ServiceType"];

					bool bIsLimited = sServiceType == "E-SeriesLimitedData";

					new UpdateExperianDirectors(
						nCustomerID,
						sr["Id"],
						bIsLimited ? string.Empty : sr["ResponseData"],
						bIsLimited,
						DB,
						Log
					).Execute();

					return ActionResult.Continue;
				},
				"LoadExperianDataForDirectorsBackfill",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID)
			);
		} // Execute

		#endregion method Execute

		private int? m_nCustomerID;
	} // class BackfillExperianDirectors
} // namespace

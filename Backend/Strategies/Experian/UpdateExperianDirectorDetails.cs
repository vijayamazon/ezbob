namespace EzBob.Backend.Strategies.Experian {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using JetBrains.Annotations;

	public class UpdateExperianDirectorDetails : AStrategy {
		#region public

		#region constructor

		public UpdateExperianDirectorDetails(Esigner oDetails, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_oSp = new SpUpdateExperianDirectorDetails(DB, Log) {
				DirectorID = oDetails.DirectorID,
				Email = oDetails.Email,
				MobilePhone = oDetails.MobilePhone,
				Line1 = oDetails.Line1,
				Line2 = oDetails.Line2,
				Line3 = oDetails.Line3,
				Town = oDetails.Town,
				County = oDetails.County,
				Postcode = oDetails.Postcode,
			};
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "Update Experian director details"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			m_oSp.ExecuteNonQuery();
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly SpUpdateExperianDirectorDetails m_oSp;

		#region class SpUpdateExperianDirectorDetails

		private class SpUpdateExperianDirectorDetails : AStoredProc {
			public SpUpdateExperianDirectorDetails(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {} // constructor

			public override bool HasValidParameters() {
				return
					(DirectorID > 0) &&
					!string.IsNullOrWhiteSpace(Email) &&
					!string.IsNullOrWhiteSpace(MobilePhone) &&
					!string.IsNullOrWhiteSpace(Line1) &&
					!string.IsNullOrWhiteSpace(Town) &&
					!string.IsNullOrWhiteSpace(Postcode);
			} // HasValidParameters

			[UsedImplicitly]
			public int DirectorID { get; set; }

			[UsedImplicitly]
			public string Email { get; set; }

			[UsedImplicitly]
			public string MobilePhone { get; set; }

			[UsedImplicitly]
			public string Line1 { get; set; }

			[UsedImplicitly]
			public string Line2 { get; set; }

			[UsedImplicitly]
			public string Line3 { get; set; }

			[UsedImplicitly]
			public string Town { get; set; }

			[UsedImplicitly]
			public string County { get; set; }

			[UsedImplicitly]
			public string Postcode { get; set; }
		} // class SpUpdateExperianDirectorDetails

		#endregion class SpUpdateExperianDirectorDetails

		#endregion private
	} // class UpdateExperianDirectorDetails
} // namespace

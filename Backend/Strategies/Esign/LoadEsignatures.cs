namespace EzBob.Backend.Strategies.Esign {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class LoadEsignatures : AStrategy {
		#region public

		#region constructor

		public LoadEsignatures(int? nCustomerID, bool bPollStatus, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_bPollStatus = bPollStatus;
			Result = new SortedTable<int, long, Esignature>();
			m_oSp = new LoadCustomerEsignatures(DB, Log) { CustomerID = nCustomerID, };
			m_nCustomerID = nCustomerID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadEsignatures"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			if (m_bPollStatus)
				new EsignProcessPending(m_nCustomerID, DB, Log).Execute();

			Result = m_oSp.Load();

			PotentialEsigners = DB.Fill<Esigner>(
				"LoadPotentialEsigners",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID)
			);
		} // Execute

		#endregion method Execute

		#region property Result

		public SortedTable<int, long, Esignature> Result { get; private set; }

		#endregion property Result

		#region property PotentialEsigners

		public List<Esigner> PotentialEsigners { get; private set; }

		#endregion property PotentialEsigners

		#endregion public

		#region private

		private readonly LoadCustomerEsignatures m_oSp;
		private readonly int? m_nCustomerID;
		private readonly bool m_bPollStatus;

		#endregion private
	} // class LoadEsignatures
} // namespace

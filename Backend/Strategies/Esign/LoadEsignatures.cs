namespace Ezbob.Backend.Strategies.Esign {
	using System.Collections.Generic;
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Utils;

	public class LoadEsignatures : AStrategy {
		public LoadEsignatures(int? nCustomerID, bool bPollStatus) {
			m_bPollStatus = bPollStatus;
			Result = new SortedTable<int, long, Esignature>();
			m_oSp = new LoadCustomerEsignatures(DB, Log) { CustomerID = nCustomerID, };
			m_nCustomerID = nCustomerID;
		} // constructor

		public override string Name {
			get { return "LoadEsignatures"; }
		} // Name

		public override void Execute() {
			if (m_bPollStatus)
				new EsignProcessPending(m_nCustomerID).Execute();

			Result = m_oSp.Load();

			PotentialEsigners = DB.Fill<Esigner>(
				"LoadPotentialEsigners",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", m_nCustomerID)
			);
		} // Execute

		public SortedTable<int, long, Esignature> Result { get; private set; }

		public List<Esigner> PotentialEsigners { get; private set; }

		private readonly LoadCustomerEsignatures m_oSp;
		private readonly int? m_nCustomerID;
		private readonly bool m_bPollStatus;

	} // class LoadEsignatures
} // namespace

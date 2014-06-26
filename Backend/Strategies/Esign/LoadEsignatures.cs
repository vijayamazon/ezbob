namespace EzBob.Backend.Strategies.Esign {
	using Ezbob.Backend.Models;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class LoadEsignatures : AStrategy {
		#region public

		#region constructor

		public LoadEsignatures(int? nCustomerID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			Result = new SortedTable<int, long, Esignature>();
			m_oSp = new LoadCustomerEsignatures(DB, Log) { CustomerID = nCustomerID, };
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "LoadEsignatures"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			Result = m_oSp.Load();
		} // Execute

		#endregion method Execute

		#region property Result

		public SortedTable<int, long, Esignature> Result { get; private set; }

		#endregion property Result

		#endregion public

		#region private

		private readonly LoadCustomerEsignatures m_oSp;

		#endregion private
	} // class LoadEsignatures
} // namespace

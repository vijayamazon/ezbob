namespace Ezbob.Matrices {
	using Ezbob.Database;

	public class CapOfferByCustomerScoreMatrix : DBMatrix {
		public CapOfferByCustomerScoreMatrix(int customerID, AConnection db) : base(null, db) {
			this.customerID = customerID;
		} // constructor

		public override bool Load() {
			DB.ForEachRowSafe(
				ProcessRow,
				"LoadCapOfferByCustomerScoreMatrix",
				CommandSpecies.StoredProcedure,
				new QueryParameter("CustomerID", this.customerID)
			);

			return (MatrixID > 0) && Init();
		} // Load

		private readonly int customerID;
	} // class CapOfferByCustomerScoreMatrix
} // namespace

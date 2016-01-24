namespace Ezbob.Backend.Strategies.LegalDocs {
    using System.Collections.Generic;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;

    public class GetLegalDocs : AStrategy {

        public GetLegalDocs(int originID, bool isRegulated, int productSubTypeID) {
            OriginID = originID;
            IsRegulated = isRegulated;
            ProductSubTypeID = productSubTypeID;

        } // constructor

		public override string Name {
            get { return "GetLegalDocs"; }
		} // Name

		public override void Execute() {
              LoanAgreementTemplate = DB.Fill<LoanAgreementTemplate>("I_GetLegalDocs", CommandSpecies.StoredProcedure,
                  new QueryParameter("OriginID", OriginID),
                  new QueryParameter("IsRegulated", IsRegulated),
                  new QueryParameter("ProductSubTypeID", ProductSubTypeID)
                  
                  ); 
		} // Execute

        int OriginID { get; set; }
        bool IsRegulated { get; set; }
        int ProductSubTypeID { get; set; }
        public List<LoanAgreementTemplate> LoanAgreementTemplate { get;set; }
    } // class GetLegalDocs
} // namespace

namespace Ezbob.Backend.Strategies.LegalDocs {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ezbob.Backend.Models.LegalDocs;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;
    using EZBob.DatabaseLib.Model.Database;

	public class ManualLegalDocsSyncTemplatesFiles : AStrategy {
		private readonly string agreementsPath;

		public ManualLegalDocsSyncTemplatesFiles(string agreementsPath) {
			this.agreementsPath = agreementsPath;
		}

		public override string Name {
            get { return "ManualLegalDocsSyncTemplatesFiles"; }
		} // Name

        public bool Result { get; set; }

        public override void Execute() {
		    try {
                var docs = GetDocs();
			    Log.Info("ManualLegalDocsSyncTemplatesFiles Execute num of docs to populate : {0}", docs.Count);
				foreach (var doc in docs) {
					Log.Info("ManualLegalDocsSyncTemplatesFiles Inserting template: {0} {1} {2} {3}", doc.TemplateTypeName, doc.TemplateTypeID, doc.OriginID, doc.IsRegulated);
                    DB.ExecuteNonQuery("I_InsertLegalDocTemplate", CommandSpecies.StoredProcedure,
                      new QueryParameter("Template", doc.Template),
                      new QueryParameter("TemplateTypeID", doc.TemplateTypeID),
                      new QueryParameter("OriginID", doc.OriginID),
                      new QueryParameter("IsRegulated", doc.IsRegulated),
                      new QueryParameter("IsApproved", doc.IsApproved),
                      new QueryParameter("IsReviewed", doc.IsReviewed),
                      new QueryParameter("ProductID", doc.ProductID),
                      new QueryParameter("ReleaseDate", doc.ReleaseDate),
                      new QueryParameter("IsUpdate", doc.IsUpdate)
                    );
                }
		        Result = true;
		    } catch (Exception ex) {
			    Log.Error(ex, "Failed to populate docs");
		        Result = false;
		    }


		} // Execute

        private List<LoanAgreementTemplate> GetDocs() {
            List<LoanAgreementTemplate> legalDocs = new List<LoanAgreementTemplate>();
			var path = this.agreementsPath;
            var files = Directory.GetFiles(path, "*.cshtml", SearchOption.AllDirectories).ToList();
            int i = 1;
            foreach (string fileName in files) {
                if (!fileName.Contains("EzbobAlibabaCreditFacility")) {
                    if (fileName.Contains("Alibaba") || fileName.Contains(@"\NA\") || fileName.Contains(@"Boardresolution"))
                        continue;
                }


                legalDocs.Add(
                    new LoanAgreementTemplate() {
                        Template = System.IO.File.ReadAllText(fileName),
                        TemplateTypeID = GetTemplateTypeID(fileName),
                        OriginID = GetBrandID(fileName),
                        IsRegulated = IsRegulated(fileName),
                        IsApproved = true,
                        IsReviewed = true,
                        ProductID = 1,
                        ReleaseDate = DateTime.UtcNow,
                        IsUpdate = true
                    });
                i++;
            }
            return legalDocs;
        }

        private int GetTemplateTypeID(string name) {
            if (name.Contains("GuarantyAgreement"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.GuarantyAgreement;
            if (name.Contains("PreContract"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.PreContract;
            if (name.Contains("RegulatedLoanAgreement"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.RegulatedLoanAgreement;
            if (name.Contains("PrivateCompanyLoanAgreement"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.PrivateCompanyLoanAgreement;
            if (name.Contains("CreditFacility"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.CreditFacility;
            if (name.Contains("BoardResolution"))
				return (int)LegalDocsEnums.LoanAgreementTemplateType.BoardResolution;
            return 0;
        }

        private int GetBrandID(string name) {
            if (name.Contains("Alibaba"))
                return (int)CustomerOriginEnum.alibaba;
            if (name.Contains("Ezbob"))
				return (int)CustomerOriginEnum.ezbob;
            if (name.Contains("EVL"))
				return (int)CustomerOriginEnum.everline;
            return 0;
        }

        private bool IsRegulated(string name) {
            var templateTypeID = GetTemplateTypeID(name);
            switch (templateTypeID) 
            {
                case 2: case 3 : return true;
                case 1: case 4: case 5: case 6:  return false;
            }
            return false;
        }

    } // class Alibaba
} // namespace

namespace Ezbob.Backend.Strategies.LegalDocs {
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using Ezbob.Backend.ModelsWithDB.LegalDocs;
    using Ezbob.Database;

    public class ManualLegalDocsSyncTemplatesFiles : AStrategy {
       
		public override string Name {
            get { return "ManualLegalDocsSyncTemplatesFiles"; }
		} // Name

        public bool Result { get; set; }

        public override void Execute() {
		    try {
                var docs = GetDocs();
                foreach (var doc in docs) {
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
		    } catch (Exception) {

		        Result = false;
		    }


		} // Execute

        private List<LoanAgreementTemplate> GetDocs() {
            List<LoanAgreementTemplate> legalDocs = new List<LoanAgreementTemplate>();
            var path = System.AppDomain.CurrentDomain.BaseDirectory.Substring(0, System.AppDomain.CurrentDomain.BaseDirectory.IndexOf("ezbob", StringComparison.Ordinal) +5) + @"\App\PluginWeb\EzBob.Web\Areas\Customer\Views\Agreement";
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
                return 1;
            if (name.Contains("PreContract"))
                return 2;
            if (name.Contains("RegulatedLoanAgreement"))
                return 3;
            if (name.Contains("PrivateCompanyLoanAgreement"))
                return 4;
            if (name.Contains("CreditFacility"))
                return 5;
            if (name.Contains("BoardResolution"))
                return 6;
            return 0;
        }

        private int GetBrandID(string name) {
            if (name.Contains("Alibaba"))
                return 3;
            if (name.Contains("Ezbob"))
                return 1;
            if (name.Contains("EVL"))
                return 2;
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

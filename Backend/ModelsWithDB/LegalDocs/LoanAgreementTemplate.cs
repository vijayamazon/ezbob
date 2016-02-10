namespace Ezbob.Backend.ModelsWithDB.LegalDocs {
    using System;
    using System.Runtime.Serialization;
    using Ezbob.Backend.Models.LegalDocs;
    using Ezbob.Utils.dbutils;
    using Ezbob.Utils.Extensions;

    [DataContract(IsReference = true)]
    public class LoanAgreementTemplate {
        [PK(true)]
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Template { get; set; }

        [DataMember]
        public bool IsUpdate { get; set; }

        [DataMember]
        public int OriginID { get; set; }

        [DataMember]
        public bool IsRegulated { get; set; }

        [DataMember]
        public int ProductID { get; set; }

        [DataMember]
        public bool IsApproved { get; set; }

        [DataMember]
        public bool IsReviewed { get; set; }

        [DataMember]
        public DateTime ReleaseDate { get; set; }

        [DataMember]
        public int TemplateTypeID { get; set; }

        public String Name
        {
            get { return ((LegalDocsEnums.LoanAgreementTemplateType)TemplateTypeID).DescriptionAttr(); }
        }

        public String TemplateTypeName {
            get
            {
                return  Enum.GetName(typeof(LegalDocsEnums.LoanAgreementTemplateType), TemplateTypeID);
            }
        }
    }
}

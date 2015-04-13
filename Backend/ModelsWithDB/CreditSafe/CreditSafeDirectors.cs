using System;
using System.Collections.Generic;
using Ezbob.Utils;
using Ezbob.Utils.dbutils;

namespace Ezbob.Backend.ModelsWithDB.CreditSafe
{
    public class CreditSafeDirectors
    {
        public CreditSafeDirectors()
        {
            Directorships=new List<CreditSafeDirectors_Directorships>();
        }

        [PK(true)]
        [NonTraversable]
        public long CreditSafeDirectorsID { get; set; }
        [FK("CreditSafeBaseData", "CreditSafeBaseDataID")]
        public long? CreditSafeBaseDataID { get; set; }
        [Length(10)]
        public string Title { get; set; }
        [Length(100)]
        public string Name { get; set; }
        [Length(100)]
        public string Address1 { get; set; }
        [Length(100)]
        public string Address2 { get; set; }
        [Length(100)]
        public string Address3 { get; set; }
        [Length(100)]
        public string Address4 { get; set; }
        [Length(100)]
        public string Address5 { get; set; }
        [Length(100)]
        public string Address6 { get; set; }
        [Length(10)]
        public string PostCode { get; set; }
        public DateTime? BirthDate { get; set; }
        [Length(100)]
        public string Nationality { get; set; }
        [Length(100)]
        public string Honours { get; set; }
        [NonTraversable]
        public List<CreditSafeDirectors_Directorships> Directorships { get; set; } 

    }
}

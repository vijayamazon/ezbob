using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataAddressLinks {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataID { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime LastConfDate { get; set; }
		public int From { get; set; }
		public int To { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }

	}
}

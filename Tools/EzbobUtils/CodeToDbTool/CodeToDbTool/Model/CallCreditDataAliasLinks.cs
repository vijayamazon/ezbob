using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Utils.dbutils;

namespace CodeToDbTool.Model {
	class CallCreditDataAliasLinks {
		[PK]
		public long Id { get; set; }
		[FK("CallCreditData", "Id")]
		public long DataId { get; set; }
		public bool Declared { get; set; }
		[Length(164)]
		public string NameBefore { get; set; }
		[Length(164)]
		public string Alias { get; set; }
		public DateTime CreationDate { get; set; }
		public DateTime LastConfDate { get; set; }
		[Length(60)]
		public string SupplierName { get; set; }
		[Length(10)]
		public string SupplierTypeCode { get; set; }

	}
}

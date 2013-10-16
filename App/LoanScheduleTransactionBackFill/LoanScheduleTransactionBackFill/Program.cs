using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ezbob.Context;
using Ezbob.Database;
using Ezbob.Logger;

namespace LoanScheduleTransactionBackFill {
	class Program {
		static void Main(string[] args) {
			using (var oLog = new FileLog(oLog: new ConsoleLog())) {
				var oEnv = new Ezbob.Context.Environment(Name.Production, oLog: oLog);

				var oDB = new SqlConnection(oEnv, oLog);
			} // using log file
		} // Main
	} // class Program
} // namespace

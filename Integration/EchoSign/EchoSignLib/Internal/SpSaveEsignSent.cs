﻿namespace EchoSignLib {
	using System;
	using System.Collections.Generic;
	using Ezbob.Database;
	using Ezbob.Logger;

	internal class SpSaveEsignSent : AStoredProc {
		#region constructor

		public SpSaveEsignSent(AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
		} // constructor

		#endregion constructor

		#region method HasValidParameters

		public override bool HasValidParameters() {
			return
				(CustomerID > 0) &&
				(TemplateID > 0) &&
				!string.IsNullOrWhiteSpace(DocumentKey);
		} // HasValidParameters

		#endregion method HasValidParameters

		public int CustomerID { get; set; }

		public int TemplateID { get; set; }

		public string DocumentKey { get; set; }

		public bool SentToCustomer { get; set; }

		public DateTime Now {
			get { return DateTime.UtcNow; }
			set { }
		} // Now

		public List<int> Directors { get; set; }
	} // class SpSaveEsignSent
} // namespace

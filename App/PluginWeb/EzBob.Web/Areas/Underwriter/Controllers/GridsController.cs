namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Text;
	using System.Web.Mvc;
	using System.Web.Script.Serialization;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;
	using Ezbob.Utils.Html.Attributes;
	using EzBob.Web.Areas.Underwriter.Models;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;
	using EZBob.DatabaseLib.Model.Database;

	public class GridsController : Controller {
		public GridsController() {
			this.db = DbConnectionGenerator.Get();
		} // constructor

		[HttpGet]
		[Ajax]
		public JsonResult GetCounters(bool isTest) {
			int nWaiting = 0;
			int nPending = 0;
			int nRegistered = 0;
			int nEscalated = 0;
			int nSignature = 0;
			int nPendingInvestor = 0;
			this.db.ForEachRowSafe(
				sr => {
					string sCustomerType = sr["CustomerType"];

					if (sCustomerType == "Signature")
						nSignature = sr["CustomerCount"];
					else if (sCustomerType == "Registered")
						nRegistered = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.Escalated.ToString())
						nEscalated = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.ApprovedPending.ToString())
						nPending = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.WaitingForDecision.ToString())
						nWaiting = sr["CustomerCount"];
					else if (sCustomerType == CreditResultStatus.PendingInvestor.ToString())
						nPendingInvestor = sr["CustomerCount"];
				},
				"UwGetCounters",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@isTest", isTest)
			);

			return Json(new List<CustomersCountersModel> {
				new CustomersCountersModel { Count = nWaiting,    Name = "waiting" },
				new CustomersCountersModel { Count = nPending,    Name = "pending" },
				new CustomersCountersModel { Count = nRegistered, Name = "RegisteredCustomers" },
				new CustomersCountersModel { Count = nEscalated,  Name = "escalated" },
				new CustomersCountersModel { Count = nSignature,  Name = "signature" },
				new CustomersCountersModel { Count = nPendingInvestor,  Name = "pendinginvestor" },
			}, JsonRequestBehavior.AllowGet);
		} // GetCounters

		
		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public ContentResult Load(string grid, bool includeTestCustomers, bool includeAllCustomers) {
			log.Debug("Started: GetGrid('{0}', {1}, {2}...)", grid, includeTestCustomers, includeAllCustomers);

			GridActions nAction;

			if (!Enum.TryParse(grid, true, out nAction)) {
				string sMsg = string.Format("Cannot load underwriter grid because '{0}' is not known grid name.", grid);
				throw new Exception(sMsg);
			} // if

			switch (nAction) {
			case GridActions.UwGridWaiting:
				return LoadGrid(nAction, includeTestCustomers, () => new GridWaitingRow());

			case GridActions.UwGridPending:
				return LoadGrid(nAction, includeTestCustomers, () => new GridPendingRow());

			case GridActions.UwGridRegistered:
				return LoadGrid(
					nAction,
					includeTestCustomers,
					() => new GridRegisteredRow(),
					includeAllCustomers,
					oMoreSpArgs: new[] { new QueryParameter("@Now", DateTime.UtcNow), }
				);

			case GridActions.UwGridRejected:
				return LoadGrid(
					nAction,
					includeTestCustomers,
					() => new GridRejectedRow(),
					oMoreSpArgs: new[] { new QueryParameter("@Now", DateTime.UtcNow), }
				);

			case GridActions.UwGridSignature:
				return LoadGrid(nAction, includeTestCustomers, () => new GridPendingRow());

			case GridActions.UwGridAll:
				return LoadGrid(nAction, includeTestCustomers, () => new GridAllRow());

			case GridActions.UwGridApproved:
				return LoadGrid(nAction, includeTestCustomers, () => new GridApprovedRow());

			case GridActions.UwGridPendingInvestor:

				List<PendingInvestorModel> allInvestorData = this.db.Fill<PendingInvestorModel>("GetInvestorData", CommandSpecies.StoredProcedure);

				return LoadGrid(nAction, includeTestCustomers, () => new GridPendingInvestorRow(allInvestorData));

			case GridActions.UwGridCollection:
				return LoadGrid(nAction, includeTestCustomers, () => new GridCollectionRow());

			case GridActions.UwGridEscalated:
				return LoadGrid(nAction, includeTestCustomers, () => new GridEscalatedRow());

			case GridActions.UwGridLate:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLateRow(), null, DateTime.UtcNow);

			case GridActions.UwGridLoans:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLoansRow());

			case GridActions.UwGridLogbook:
				return LoadGrid(nAction, includeTestCustomers, () => new GridLogbookRow());

			case GridActions.UwGridSales:
				return LoadGrid(nAction, includeTestCustomers, () => new GridSalesRow(), null, DateTime.UtcNow);

			case GridActions.UwGridBrokers:
				return LoadGrid(nAction, includeTestCustomers, () => new GridBroker());
			case GridActions.UwGridInvestors:
				return LoadGrid(nAction, includeTestCustomers, () => new GridInvestor());

			default:
				string sMsg = string.Format("Cannot load underwriter grid because '{0}' is not implemented.", nAction);
				throw new ArgumentOutOfRangeException(sMsg);
			} // switch
		} // Load

		private ContentResult LoadGrid(
			GridActions nSpName,
			bool bIncludeTestCustomers,
			Func<AGridRow> oFactory,
			IEnumerable<QueryParameter> oMoreSpArgs = null
		) {
			return LoadGrid(
				nSpName,
				bIncludeTestCustomers,
				oFactory,
				bIncludeAllCustomers: null,
				oMoreSpArgs: oMoreSpArgs
			);
		} // LoadGrid

		private ContentResult LoadGrid(
			GridActions nSpName,
			bool bIncludeTestCustomers,
			Func<AGridRow> oFactory,
			bool? bIncludeAllCustomers,
			DateTime? now = null,
			IEnumerable<QueryParameter> oMoreSpArgs = null
		) {
			TimeCounter tc = new TimeCounter("LoadGrid building time for grid " + nSpName);
			var oRes = new SortedDictionary<long, AGridRow>();
			

			var args = new List<QueryParameter> {
				new QueryParameter("@WithTest", bIncludeTestCustomers),
			};

			if (bIncludeAllCustomers.HasValue)
				args.Add(new QueryParameter("@WithAll", bIncludeAllCustomers));

			if (now.HasValue)
				args.Add(new QueryParameter("@Now", now.Value));

			if (oMoreSpArgs != null)
				args.AddRange(oMoreSpArgs);



			using (tc.AddStep("retrieving from db and processing")) {
				this.db.ForEachRowSafe(
					sr => {
						AGridRow r = oFactory();

						long nRowID = sr[r.RowIDFieldName()];

						r.Init(nRowID, sr);

						if (r.IsValid())
							oRes[nRowID] = r;
					},
					nSpName.ToString(),
					CommandSpecies.StoredProcedure,
					args.ToArray()
				); // foreach
			} // using

			
			log.Debug("{0}: traversing done.", nSpName);

			var sb = new StringBuilder();

			sb.AppendLine(tc.Title);

			foreach (var time in tc.Steps)
				sb.AppendFormat("\t{0}: {1}ms\n", time.Name, time.Length);

			log.Info("{0}", sb);

			var serializer = new JavaScriptSerializer {
				MaxJsonLength = Int32.MaxValue,
			};

			return new ContentResult {
				Content = serializer.Serialize(new { aaData = oRes.Values }),
				ContentType = "application/json",
			};
		} // LoadGrid

		private enum GridActions {
			UwGridWaiting,
			UwGridPending,
			UwGridRegistered,
			UwGridRejected,
			UwGridSignature,
			UwGridAll,
			UwGridApproved,
			UwGridPendingInvestor,
			UwGridCollection,
			UwGridEscalated,
			UwGridLate,
			UwGridLoans,
			UwGridLogbook,
			UwGridSales,
			UwGridBrokers,
			UwGridInvestors,
		} // enum GridActions

		private readonly AConnection db;
		private static readonly ASafeLog log = new SafeILog(typeof(LogBookController));
	} // class GridsController
} // namespace

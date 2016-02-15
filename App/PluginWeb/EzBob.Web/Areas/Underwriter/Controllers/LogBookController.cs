namespace EzBob.Web.Areas.Underwriter.Controllers {
	using System;
	using System.Collections.Generic;
	using System.Web.Mvc;
	using Ezbob.Database;
	using Ezbob.Logger;
	using EzBob.Web.Infrastructure;
	using EzBob.Web.Infrastructure.Attributes;
	using EzBob.Web.Infrastructure.csrf;

	public class LogBookController : Controller {
		public LogBookController(IWorkplaceContext context) {
			this.db = DbConnectionGenerator.Get();
			this.context = context;
		} // constructor

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpGet]
		public JsonResult Index() {
			var oRes = new List<object>();

			const string sSpName = "LogbookEntryTypeList";

			this.db.ForEachRowSafe(
				sr => {
					oRes.Add(new {
						ID = (int)sr["LogbookEntryTypeID"],
						Name = (string)sr["LogbookEntryType"],
						Description = (string)sr["LogbookEntryTypeDescription"],
					});
				},
				sSpName,
				CommandSpecies.StoredProcedure
			); // foreach

			log.Debug("{0}: traversing done.", sSpName);

			var j = Json(oRes, JsonRequestBehavior.AllowGet);

			log.Debug("{0}: converted to json.", sSpName);

			return j;
		} // Index

		[ValidateJsonAntiForgeryToken]
		[Ajax]
		[HttpPost]
		[Permission(Name = "AddLogbookEntry")]
		public JsonResult Add(int type, string content) {
			bool bSuccess;
			string sMsg = string.Empty;

			try {
				if (string.IsNullOrWhiteSpace(content))
					throw new Exception("Content is empty.");

				this.db.ExecuteNonQuery(
					"LogbookAdd",
					CommandSpecies.StoredProcedure,
					new QueryParameter("@LogbookEntryTypeID", type),
					new QueryParameter("@UserID", this.context.User.Id),
					new QueryParameter("@EntryContent", content)
				);

				bSuccess = true;
			} catch (Exception e) {
				bSuccess = false;
				sMsg = e.Message;
			} // try

			return Json(new { success = bSuccess, msg = sMsg });
		} // Add

		private readonly IWorkplaceContext context;
		private readonly AConnection db;
		private static readonly ASafeLog log = new SafeILog(typeof(LogBookController));
	} // class LogBookController
} // namespace

namespace EzBob.Web.Areas.Customer.Controllers {
	using System;
	using System.Web.Mvc;
	using EzBob.Web.Code;

	public class ConsentController : Controller {
		public ConsentController() {
			this.concentAgreementHelper = new ConcentAgreementHelper();
		} // constructor

		public FileResult Download(int id, string firstName, string middleInitial, string surname) {
			DateTime now = DateTime.UtcNow;

			string fullName = string.Format("{0} {1} {2}", firstName, middleInitial, surname);

			byte[] pdf = this.concentAgreementHelper.Generate(fullName, now);

			string fileName = this.concentAgreementHelper.GetFileName(id, firstName, surname, now);

			return File(pdf, "application/pdf", fileName);
		} // Download

		private readonly IConcentAgreementHelper concentAgreementHelper;
	} // class ConsentController
} // namespace
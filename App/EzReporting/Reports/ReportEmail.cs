namespace Reports {
	using Ezbob.Utils.Html;
	using Ezbob.Utils.Html.Attributes;
	using Ezbob.Utils.Html.Tags;

	public class ReportEmail {

		public ReportEmail() {
			HtmlBody = new Body().Add<Class>("Body");

			var oTbl = new Table().Add<Class>("Header");
			HtmlBody.Append(oTbl);

			var oImgLogo = new Img()
				.Add<Class>("Logo")
				.Add<Src>("http://www.ezbob.com/wp-content/themes/ezbob-desktop/images/sign/logo_sig2.png");

			var oLogoLink = new A()
				.Add<Href>("http://www.ezbob.com/")
				.Add<Class>("logo_ezbob")
				.Add<Class>("indent_text")
				.Add<ID>("ezbob_logo")
				.Add<Title>("Fast business loans for Ebay and Amazon merchants")
				.Add<Alt>("Fast business loans for Ebay and Amazon merchants")
				.Append(oImgLogo);

			var oTr = new Tr();
			oTbl.Append(oTr);

			oTr.Append(new Td().Append(oLogoLink));

			Title = new H1();

			oTr.Append(new Td().Append(Title));

			ReportBody = new P().Add<Class>("Body");

			HtmlBody.Append(ReportBody);
		} // constructor

		public ATag HtmlBody { get; private set; }
		public ATag Title { get; private set; }
		public ATag ReportBody { get; private set; }

	} // class ReportEmail

} // namespace Reports

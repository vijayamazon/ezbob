namespace Reports.TraficReport {
	public static class SourceRefMapper {

		public static Source GetSourceBySourceRef(string sourceRef, string googleCookie = null) {
			sourceRef = (sourceRef ?? string.Empty).ToLowerInvariant().Trim();

			if (sourceRef.StartsWith("bros-hs"))
				return Source.PpcPromotion;

			if (sourceRef.StartsWith("adroll"))
				return Source.Adroll;

			if (sourceRef.StartsWith("amazon"))
				return Source.Amazon;

			if (sourceRef.StartsWith("bing"))
				return Source.Bing;

			if (sourceRef.StartsWith("brk") || sourceRef.Equals("liqcen"))
				return Source.Broker;

			if (sourceRef.StartsWith("bros_gmail"))
				return Source.Gmail;

			if (sourceRef.Contains("adwords") || sourceRef.StartsWith("bros") || sourceRef.StartsWith("google"))
				return Source.GooglePpc;

			if (sourceRef.StartsWith("ebay"))
				return Source.Ebay;

			if (sourceRef.StartsWith("ekm"))
				return Source.Ekm;

			if (sourceRef.StartsWith("fb") || sourceRef.StartsWith("face"))
				return Source.FaceBook;

			if (sourceRef.StartsWith("growthebusiness"))
				return Source.GrowTheBusiness;

			if (sourceRef.StartsWith("gumtree"))
				return Source.Gumtree;

			if (sourceRef.Equals("m.co.uk") || sourceRef.Equals("m_co_uk") || sourceRef.Equals("money.co.uk") || sourceRef.Equals("money_co_uk"))
				return Source.MoneyCoUk;

			if (sourceRef.Contains("remarketing"))
				return Source.GoogleRemarketing;

			if (sourceRef.StartsWith("presta"))
				return Source.Prestashop;

			if (sourceRef.StartsWith("sb") || sourceRef.StartsWith("small_business"))
				return Source.SmallBusinessCoUk;

			if (sourceRef.StartsWith("tamebay"))
				return Source.Tamebay;

			if (sourceRef.StartsWith("uk_business_forum"))
				return Source.UkBusinessForum;

			if (sourceRef.StartsWith("twitter"))
				return Source.Twitter;

			if (sourceRef.StartsWith("msm"))
				return Source.MoneySupermarket;

			if (string.IsNullOrEmpty(sourceRef)) {
				if (googleCookie != null && googleCookie.Contains("direct"))
					return Source.Direct;

				return Source.Seo;
			} // if

			return Source.Other;
		} // GetSourceBySourceRef

		public static Source GetSourceByAnalytics(string analytics) {
			analytics = analytics ?? string.Empty;

			if (analytics.StartsWith("bros-hs"))
				return Source.PpcPromotion;

			if (analytics.Equals("(direct) / (none)"))
				return Source.Direct;

			if (analytics.Equals("money.co.uk / referral"))
				return Source.MoneyCoUk;

			if (analytics.Equals("google / organic") || analytics.Equals("google.co.uk / referral"))
				return Source.Seo;

			if (analytics.Equals("google / cpc") || analytics.Equals("adwords.google.com / referral"))
				return Source.GooglePpc;

			if (analytics.Equals("ad-emea.doubleclick.net / referral"))
				return Source.Ebay;

			if (analytics.Equals("smallbusiness.co.uk / referral"))
				return Source.SmallBusinessCoUk;

			if (analytics.Equals("bing / organic"))
				return Source.Bing;

			if (analytics.Equals("moneysupermarket.com / referral"))
				return Source.MoneySupermarket;

			if (analytics.Equals("yahoo / organic"))
				return Source.Bing;

			if (analytics.Contains("facebook.com / referral"))
				return Source.FaceBook;

			if (analytics.Equals("mail.google.com / referral"))
				return Source.Gmail;

			if (analytics.Equals("t.co / referral"))
				return Source.Twitter;

			if (analytics.Equals("gumtree.com / referral"))
				return Source.Gumtree;

			if (analytics.Equals("growthbusiness.co.uk / referral"))
				return Source.GrowTheBusiness;

			if (analytics.Equals("ekmpowershop.com / referral"))
				return Source.Ekm;

			if (analytics.Equals("googleads.g.doubleclick.net / referral"))
				return Source.GooglePpc;

			if (analytics.Equals("tamebay.com / referral"))
				return Source.Tamebay;

			if (analytics.Contains("amazon"))
				return Source.Amazon;

			if (analytics.Contains("ezbob"))
				return Source.Ezbob;

			return Source.Other;
		} // GetSourceByAnalytics

	} // class SourceRefMapper
} // namespace

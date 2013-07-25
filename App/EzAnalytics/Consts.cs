namespace EzAnalyticsConsoleClient {
	#region class DbConsts

	static class DbConsts {
		public const string UkVisits = "UKVisits";
		public const string UkVisitors = "UKVisitors";
		public const string WorldWideVisits = "WorldWideVisits";
		public const string WorldWideVisitors = "WorldWideVisitors";
	} // class DbConsts

	#endregion class DbConsts

	#region class GoogleConsts

	static class GoogleConsts {
		public const string ProfileID = "ga:60953365";
		public const string KeyFileName = "08a190d7e7b61e5cdfa63301e528134d3699f096-privatekey.p12";
		public const string ServiceAccountUser = "660066936754-gcs16ckt4dhklhcqrktjispn0l3ddfhl@developer.gserviceaccount.com";
		public const string Scope = "analytics";
		public const string Metrics = "ga:visits,ga:visitors";
		public const string Dimensions = "ga:date,ga:country";

		public const string OAuthDateFormat = "yyyy-MM-dd";

		public const string IL = "Israel";
		public const string UA = "Ukraine";
		public const string UK = "United Kingdom";
	} // class GoogleConsts

	#endregion class GoogleConsts
} // namespace

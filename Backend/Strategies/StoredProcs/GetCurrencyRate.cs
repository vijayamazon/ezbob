namespace Ezbob.Backend.Strategies.StoredProcs {
	using System;
	using System.Data;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public enum CurrencyCode {
		GBP = 0, GBX,
		AED, AFN, ALL, AMD, ANG, AOA, ARS, AUD, AWG, AZN, BAM, BBD, BDT, BGN, BHD, BIF, BMD, BND, BOB, BRL, BSD, BTN,
		BWP, BYR, BZD, CAD, CDF, CHF, CLF, CLP, CNH, CNY, COP, CRC, CUP, CVE, CYP, CZK, DEM, DJF, DKK, DOP, DZD, ECS,
		EGP, ERN, ETB, EUR, FJD, FKP, FRF, GEL, GHS, GIP, GMD, GNF, GTQ, GYD, HKD, HNL, HRK, HTG, HUF, IDR, IEP, ILS,
		INR, IQD, IRR, ISK, ITL, JMD, JOD, JPY, KES, KGS, KHR, KMF, KPW, KRW, KWD, KYD, KZT, LAK, LBP, LKR, LRD, LSL,
		LTL, LVL, LYD, MAD, MDL, MGA, MKD, MMK, MNT, MOP, MRO, MUR, MVR, MWK, MXN, MXV, MYR, MZN, NAD, NGN, NIO, NOK,
		NPR, NZD, OMR, PAB, PEN, PGK, PHP, PKR, PLN, PYG, QAR, RON, RSD, RUB, RWF, SAR, SBD, SCR, SDG, SEK, SGD, SHP,
		SIT, SLL, SOS, SRD, STD, SVC, SYP, SZL, THB, TJS, TMT, TND, TOP, TRY, TTD, TWD, TZS, UAH, UGX, USD, UYU, UZS,
		VEF, VND, VUV, WST, XAF, XAG, XAU, XCD, XCP, XDR, XOF, XPD, XPF, XPT, YER, ZAR, ZMK, ZMW, ZWL,
	} // enum CurrencyCode

	/// <summary>
	/// Rate returned by this stored procedure should be used as follows:
	/// var amount_in_GBP = known_amount_in_USD * new CurrencyRate(CurrencyCode.USD, some_date).Load();
	/// </summary>
	public class CurrencyRate : AStoredProcedure {
		public CurrencyRate(AConnection db, ASafeLog log) : base(db, log) {
			CurrencyCode = CurrencyCode.GBP;
		} // constructor

		public CurrencyRate(CurrencyCode currencyCode, DateTime date, AConnection db, ASafeLog log) : this(db, log) {
			CurrencyCode = currencyCode;
			TheDate = date;
		} // constructor

		public override bool HasValidParameters() {
			return TheDate >= minDate;
		} // HasValidParameters

		public string CurrencyName {
			get { return CurrencyCode.ToString(); }
			// ReSharper disable once ValueParameterNotUsed
			set { }
		} // CurrencyName

		[NonTraversable]
		public CurrencyCode CurrencyCode { get; set; }

		public DateTime TheDate { get; set; }

		[Direction(ParameterDirection.Output)]
		public decimal Rate { get; set; }

		public decimal Load() {
			ExecuteNonQuery();
			return Rate;
		} // Load

		/// <summary>
		/// Returns the name of the stored procedure.
		/// Stored procedure name is current class name.
		/// </summary>
		/// <returns>SP name.</returns>
		protected override string GetName() {
			return "GetCurrencyRate";
		} // GetName

		private static readonly DateTime minDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
	} // class CurrencyRate
} // namespace

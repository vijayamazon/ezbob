namespace EZBob.DatabaseLib.Model.Database {
	public enum TypeOfBusinessReduced {
		Personal = 0,
		Limited = 1,
		NonLimited = 2,
	} // enum TypeOfBusinessReduced

	public enum TypeOfBusinessAgreementReduced {
		Personal,
		Business,
	} // enum TypeOfBusinessAgreementReduced

	public static class TypeOfBusinessExt {
		public static TypeOfBusinessReduced Reduce(this TypeOfBusiness business) {
			switch (business) {
			case TypeOfBusiness.Entrepreneur:
				return TypeOfBusinessReduced.Personal;

			case TypeOfBusiness.LLP:
			case TypeOfBusiness.Limited:
				return TypeOfBusinessReduced.Limited;

			case TypeOfBusiness.PShip:
			case TypeOfBusiness.PShip3P:
			case TypeOfBusiness.SoleTrader:
				return TypeOfBusinessReduced.NonLimited;
			} // switch

			return TypeOfBusinessReduced.Personal;
		} // Reduce

		public static TypeOfBusinessAgreementReduced AgreementReduce(this TypeOfBusiness typeOfBusiness) {
			switch (typeOfBusiness) {
			case TypeOfBusiness.Entrepreneur:
			case TypeOfBusiness.PShip3P:
			case TypeOfBusiness.SoleTrader:
				return TypeOfBusinessAgreementReduced.Personal;

			case TypeOfBusiness.PShip:
			case TypeOfBusiness.LLP:
			case TypeOfBusiness.Limited:
				return TypeOfBusinessAgreementReduced.Business;
			} // switch

			return TypeOfBusinessAgreementReduced.Personal;
		} // AgreementReduce

		public static bool IsRegulated(this TypeOfBusiness typeOfBusiness) {
			return typeOfBusiness.AgreementReduce() == TypeOfBusinessAgreementReduced.Personal;
		} // IsRegulated

		public static string TypeOfBussinessForWeb(this TypeOfBusiness businessReduced) {
			switch (businessReduced) {
			case TypeOfBusiness.Limited:
				return "Limited Company";

			case TypeOfBusiness.Entrepreneur:
				return "Sole Trader (not Inc.)";

			case TypeOfBusiness.LLP:
				return "Limited liability partnership";

			case TypeOfBusiness.PShip:
				return "Partnership (More than 3)";

			case TypeOfBusiness.PShip3P:
				return "Partnership (Up to 3)";

			case TypeOfBusiness.SoleTrader:
				return "Sole Trader (Inc.)";
			} // switch

			return string.Empty;
		} // TypeOfBussinessForWeb
	} // class TypeOfBusinessExt
} // namespace

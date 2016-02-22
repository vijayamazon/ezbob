namespace DbConstants {
	public static class FeeTypesExt {
		public static bool In(this NLFeeTypes ft, params NLFeeTypes[] args) {
			foreach (NLFeeTypes a in args)
				if (a == ft)
					return true;

			return false;
		} // In
	} // class FeeTypesExt
} // namespace

namespace DbConstants {
	public static class FeeTypesExt {
		public static bool In(this FeeTypes ft, params FeeTypes[] args) {
			foreach (FeeTypes a in args)
				if (a == ft)
					return true;

			return false;
		} // In
	} // class FeeTypesExt
} // namespace

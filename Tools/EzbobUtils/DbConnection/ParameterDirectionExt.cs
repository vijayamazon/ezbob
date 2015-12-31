namespace Ezbob.Database {
	using System.Data;

	public static class ParameterDirectionExt {
		public static bool In(this ParameterDirection prm, params ParameterDirection[] args) {
			if (args == null)
				return false;

			foreach (ParameterDirection p in args)
				if (p == prm)
					return true;

			return false;
		} // In
	} // class ParameterDirectionExt
} // namespace

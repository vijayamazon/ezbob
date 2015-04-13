namespace Ezbob.Logger {
	public static class LogExt {
		public static ASafeLog Safe(this ASafeLog log) {
			return log ?? new SafeLog();
		} // Safe
	} // class LogExt
} // namespace

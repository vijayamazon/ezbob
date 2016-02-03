namespace Ezbob.Backend.Strategies.MedalCalculations {
	public enum MedalType {
		NoMedal,
		Limited,
		NonLimited,
		OnlineLimited,
		OnlineNonLimitedNoBusinessScore,
		OnlineNonLimitedWithBusinessScore,
		SoleTrader,
	} // enum MedalType

	public static class MedalTypeExtension {
		public static bool IsOnline(this MedalType variable) {
			return variable.In(
				MedalType.OnlineLimited,
				MedalType.OnlineNonLimitedNoBusinessScore,
				MedalType.OnlineNonLimitedWithBusinessScore
			);
		} // class MedalTypeExtension

		public static bool In(this MedalType variable, params MedalType[] args) {
			foreach (MedalType mt in args)
				if (variable == mt)
					return true;

			return false;
		} // class MedalTypeExtension
	} // class MedalTypeExtension
} // namespace

namespace Ezbob.Backend.Strategies.MedalCalculations {
	public enum MedalType {
		NoMedal,
		Limited,
		NonLimited,
		OnlineLimited,
		OnlineNonLimitedNoBusinessScore,
		OnlineNonLimitedWithBusinessScore,
		SoleTrader
	}

	public static class MedalTypeExtension {
		public static bool IsOnline(this MedalType variable) {
			return variable == MedalType.OnlineLimited ||
				   variable == MedalType.OnlineNonLimitedNoBusinessScore ||
				   variable == MedalType.OnlineNonLimitedWithBusinessScore;
		}
	}
}

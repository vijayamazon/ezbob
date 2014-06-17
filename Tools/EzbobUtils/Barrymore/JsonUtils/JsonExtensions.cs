﻿namespace Ezbob.Utils.JsonUtils {
	using Newtonsoft.Json.Linq;

	public static class JsonExtensions {
		#region method Offspring

		public static JToken Offspring(this JToken oRoot, NameList oChildNames) {
			if (ReferenceEquals(oRoot, null))
				return null;

			if (ReferenceEquals(oChildNames, null))
				return oRoot;

			if (oChildNames.Count < 1)
				return oRoot;

			return oRoot.SelectToken(oChildNames.ToString("."));
		} // Offspring

		public static JToken Offspring(this JToken oRoot, params string[] sChildNames) {
			return Offspring(oRoot, new NameList(sChildNames));
		} // Offspring

		#endregion method Offspring
	} // class JsonExtensions
} // namespace Ezbob.Utils.JsonUtils

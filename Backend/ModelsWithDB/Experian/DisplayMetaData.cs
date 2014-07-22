namespace Ezbob.Backend.ModelsWithDB.Experian {
	using System.Collections.Generic;

	public class DisplayMetaData {
		#region enum DisplayDirections

		public enum DisplayDirections {
			Vertical,
			Horizontal,
		} // enum DisplayDirections

		#endregion enum DisplayDirections

		#region constructor

		public DisplayMetaData() {
			ID = null;
			UnlimitedWidth = null;
			DisplayDirection = DisplayDirections.Vertical;
		} // constructor

		#endregion constructor

		public string ID { get; set; }

		public bool? UnlimitedWidth { get; set; }

		public DisplayDirections DisplayDirection { get; set; }

		public string Sorting { get; set; }
	} // class DisplayMetaData

	public static class DisplayMetaDataExt {
		public static SortedDictionary<string, string> ToDictionary(this DisplayMetaData oMetaData) {
			var oResult = new SortedDictionary<string, string>();

			if (oMetaData == null)
				return oResult;

			if (!string.IsNullOrWhiteSpace(oMetaData.ID))
				oResult["ID"] = oMetaData.ID;

			if (oMetaData.UnlimitedWidth.HasValue)
				oResult["UnlimitedWidth"] = oMetaData.UnlimitedWidth.Value.ToString().ToLowerInvariant();

			oResult["DisplayDirection"] = oMetaData.DisplayDirection.ToString().ToLowerInvariant();

			if (!string.IsNullOrWhiteSpace(oMetaData.Sorting))
				oResult["Sorting"] = oMetaData.Sorting;

			return oResult;
		} // ToDictionary
	} // class DisplayMetaDataExt
} // namespace

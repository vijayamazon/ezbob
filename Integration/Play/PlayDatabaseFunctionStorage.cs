using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using System;

namespace Integration.Play {
	public enum PlayDatabaseFunctionType {
		NumOfOrders,
		TotalSumOfOrders,
		AverageSumOfOrder,
	} // enum PlayDatabaseFunctionType

	internal class PlayDatabaseFunctionStorage : DatabaseFunctionStorage<PlayDatabaseFunctionType> {
		public static PlayDatabaseFunctionStorage Instance { get {
			return _Instance ?? (_Instance = new PlayDatabaseFunctionStorage());
		}} // Instance

		private static PlayDatabaseFunctionStorage _Instance;

		private PlayDatabaseFunctionStorage() : base(new PlayDatabaseFunctionTypeConverter()) {
			CreateFunctionAndAddToCollection(PlayDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{B85A8CCB-4CC0-4D93-A216-C2B4D06326D1}");
			CreateFunctionAndAddToCollection(PlayDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{BEA6E606-EA64-4903-BF10-82CF16D6220A}");
			CreateFunctionAndAddToCollection(PlayDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{B59DEC6F-38FC-4304-AB7C-6DF60A580A08}");
		} // constructor
	} // class PlayDatabaseFunctionStorage

	internal class PlayDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<PlayDatabaseFunctionType> {
		public ConvertedTypeInfo Convert(PlayDatabaseFunctionType type) {
			string displayName = string.Empty;
			string description = string.Empty;

			string name = type.ToString();

			switch (type) {
			case PlayDatabaseFunctionType.NumOfOrders:
				displayName = "Num of Orders";
				break;

			case PlayDatabaseFunctionType.AverageSumOfOrder:
				displayName = "Average Sum of Order";
				break;

			case PlayDatabaseFunctionType.TotalSumOfOrders:
				displayName = "Total Sum of Orders";
				break;

			default:
				throw new NotImplementedException();
			} // switch

			return new ConvertedTypeInfo(name, displayName, description);
		} // Convert
	} // class PlayDatabaseFunctionTypeConverter
} // namespace
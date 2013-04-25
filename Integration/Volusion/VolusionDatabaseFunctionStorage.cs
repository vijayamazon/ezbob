using EZBob.DatabaseLib.Common;
using EzBob.CommonLib;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using System;

namespace Integration.Volusion {
	public enum VolusionDatabaseFunctionType {
		NumOfOrders,
		TotalSumOfOrders,
		AverageSumOfOrder,
	} // enum VolusionDatabaseFunctionType

	internal class VolusionDatabaseFunctionStorage : DatabaseFunctionStorage<VolusionDatabaseFunctionType> {
		public static VolusionDatabaseFunctionStorage Instance { get {
			return _Instance ?? (_Instance = new VolusionDatabaseFunctionStorage());
		}} // Instance

		private static VolusionDatabaseFunctionStorage _Instance;

		private VolusionDatabaseFunctionStorage() : base(new VolusionDatabaseFunctionTypeConverter()) {
			CreateFunctionAndAddToCollection(VolusionDatabaseFunctionType.NumOfOrders, DatabaseValueTypeEnum.Integer, "{e512f12d-207c-46f8-8d61-b1199bc34555}");
			CreateFunctionAndAddToCollection(VolusionDatabaseFunctionType.TotalSumOfOrders, DatabaseValueTypeEnum.Double, "{b28f5acb-7d54-48f9-a7ff-bb2861ad1900}");
			CreateFunctionAndAddToCollection(VolusionDatabaseFunctionType.AverageSumOfOrder, DatabaseValueTypeEnum.Double, "{b3c96690-c061-42da-bfba-8176137e149d}");
		} // constructor
	} // class VolusionDatabaseFunctionStorage

	internal class VolusionDatabaseFunctionTypeConverter : IDatabaseEnumTypeConverter<VolusionDatabaseFunctionType> {
		public ConvertedTypeInfo Convert(VolusionDatabaseFunctionType type) {
			string displayName = string.Empty;
			string description = string.Empty;

			string name = type.ToString();

			switch (type) {
			case VolusionDatabaseFunctionType.NumOfOrders:
				displayName = "Num of Orders";
				break;

			case VolusionDatabaseFunctionType.AverageSumOfOrder:
				displayName = "Average Sum of Order";
				break;

			case VolusionDatabaseFunctionType.TotalSumOfOrders:
				displayName = "Total Sum of Orders";
				break;

			default:
				throw new NotImplementedException();
			} // switch

			return new ConvertedTypeInfo(name, displayName, description);
		} // Convert
	} // class VolusionDatabaseFunctionTypeConverter
} // namespace
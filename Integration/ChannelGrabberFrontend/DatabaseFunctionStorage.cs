using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using ValueType = Integration.ChannelGrabberConfig.ValueType;

namespace Integration.ChannelGrabberFrontend {
	internal class FunctionStorage : DatabaseFunctionStorage<FunctionType> {
		static FunctionStorage() {
			ms_oInstances = new Dictionary<string, FunctionStorage>();
		} // static constructor

		public static FunctionStorage GetInstance(VendorInfo oVendorInfo) {
			lock (typeof(FunctionStorage)) {
				if (!ms_oInstances.ContainsKey(oVendorInfo.Name))
					ms_oInstances[oVendorInfo.Name] = new FunctionStorage(oVendorInfo);

				return ms_oInstances[oVendorInfo.Name];
			} // lock
		} // GetInstance

		private static readonly Dictionary<string, FunctionStorage> ms_oInstances;

		private FunctionStorage(VendorInfo oVendorInfo) : base(new FunctionTypeConverter()) {
			oVendorInfo.Aggregators.ForEach(agg => {
				DatabaseValueTypeEnum dbvt;

				switch (agg.ValueType()) {
				case ValueType.Boolean:
					dbvt = DatabaseValueTypeEnum.Boolean;
					break;

				case ValueType.DateTime:
					dbvt = DatabaseValueTypeEnum.DateTime;
					break;

				case ValueType.Double:
					dbvt = DatabaseValueTypeEnum.Double;
					break;

				case ValueType.Integer:
					dbvt = DatabaseValueTypeEnum.Integer;
					break;

				case ValueType.String:
					dbvt = DatabaseValueTypeEnum.String;
					break;

				case ValueType.Xml:
					dbvt = DatabaseValueTypeEnum.Xml;
					break;

				default:
					throw new ApiException("Unsupported function type: " + agg.ValueType().ToString());
				} // switch

				CreateFunctionAndAddToCollection(agg.FunctionType(), dbvt, agg.Guid().ToString());
			});
		} // constructor
	} // class FunctionStorage

	internal class FunctionTypeConverter : IDatabaseEnumTypeConverter<FunctionType> {
		public ConvertedTypeInfo Convert(FunctionType type) {
			string displayName = string.Empty;

			string name = type.ToString();

			switch (type) {
			case FunctionType.NumOfOrders:
				displayName = "Num of Orders";
				break;

			case FunctionType.AverageSumOfOrders:
				displayName = "Average Sum of Orders";
				break;

			case FunctionType.TotalSumOfOrders:
				displayName = "Total Sum of Orders";
				break;

			case FunctionType.NumOfExpenses:
				displayName = "Num of Expenses";
				break;

			case FunctionType.AverageSumOfExpenses:
				displayName = "Average Sum of Expenses";
				break;

			case FunctionType.TotalSumOfExpenses:
				displayName = "Total Sum of Expenses";
				break;

			default:
				throw new NotImplementedException();
			} // switch

			return new ConvertedTypeInfo(name, displayName, string.Empty);
		} // Convert
	} // class FunctionTypeConverter
} // namespace
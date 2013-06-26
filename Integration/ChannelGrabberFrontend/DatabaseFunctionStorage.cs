using System;
using System.Collections.Generic;
using EZBob.DatabaseLib.Common;
using EZBob.DatabaseLib.DatabaseWrapper.ValueType;
using EzBob.CommonLib;
using Integration.ChannelGrabberAPI;
using Integration.ChannelGrabberConfig;
using log4net;
using ValueType = Integration.ChannelGrabberConfig.ValueType;

namespace Integration.ChannelGrabberFrontend {
	internal class FunctionStorage : DatabaseFunctionStorage<FunctionType> {
		static FunctionStorage() {
			ms_oLog.Debug("Static FunctionStorage start");

			ms_oInstances = new Dictionary<string, FunctionStorage>();

			ms_oLog.Debug("Static FunctionStorage end");
		} // static constructor

		public static FunctionStorage GetInstance(VendorInfo oVendorInfo) {
			ms_oLog.Debug("start");

			lock (typeof(FunctionStorage)) {
				if (!ms_oInstances.ContainsKey(oVendorInfo.Name))
					ms_oInstances[oVendorInfo.Name] = new FunctionStorage(oVendorInfo);

				ms_oLog.Debug("end");

				return ms_oInstances[oVendorInfo.Name];
			} // lock
		} // GetInstance

		private static readonly Dictionary<string, FunctionStorage> ms_oInstances;

		private FunctionStorage(VendorInfo oVendorInfo) : base(new FunctionTypeConverter()) {
			ms_oLog.Debug("start");

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

			ms_oLog.Debug("end");
		} // constructor

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(FunctionStorage));
	} // class FunctionStorage

	internal class FunctionTypeConverter : IDatabaseEnumTypeConverter<FunctionType> {
		public ConvertedTypeInfo Convert(FunctionType type) {
			ms_oLog.DebugFormat("start {0}", type.ToString());

			string displayName = string.Empty;

			string name = type.ToString();

			switch (type) {
			case FunctionType.NumOfOrders:
				displayName = "Num of Orders";
				break;

			case FunctionType.AverageSumOfOrders:
				displayName = "Average Sum of Order";
				break;

			case FunctionType.TotalSumOfOrders:
				displayName = "Total Sum of Orders";
				break;

			default:
				ms_oLog.DebugFormat("exception {0}", type.ToString());
				throw new NotImplementedException();
			} // switch

			var c = new ConvertedTypeInfo(name, displayName, string.Empty);

			ms_oLog.DebugFormat("end {0} -> {1}", type.ToString(), displayName);

			return c;
		} // Convert

		private static readonly ILog ms_oLog = LogManager.GetLogger(typeof(FunctionTypeConverter));
	} // class FunctionTypeConverter
} // namespace
namespace Ezbob.Backend.Strategies.Misc {
	using System;
	using System.Linq;
	using System.Text.RegularExpressions;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using NHibernate;
	using StructureMap;
	using ZooplaLib;

	public class ZooplaStub : AStrategy {
		public override string Name {
			get {
				return "ZooplaStub";
			}
		} // Name

		public ZooplaStub(int customerId, bool reCheck = false) {
			this.customerId = customerId;
			this.reCheck = reCheck;
			_session = ObjectFactory.GetInstance<ISession>();
			customerAddressRepository = ObjectFactory.GetInstance<CustomerAddressRepository>();
		} // constructor

		public override void Execute() {
			GetZooplaData();
		} // Execute

		private void GetZooplaData() {
			// TODO add flag customer address - isOwned for simple query and
			// in case customer changes his personal address to have the owned address list correct
			var customerAddress = customerAddressRepository.GetAll()
				.Where(a =>
					a.Customer.Id == customerId &&
					(a.AddressType == CustomerAddressType.OtherPropertyAddress || (
						a.AddressType == CustomerAddressType.PersonalAddress &&
						a.Customer.PropertyStatus.IsOwnerOfMainAddress
						))
				);

			Log.Info("Fetching zoopla data for {0} addresses", customerAddress.Count());

			if (customerAddress.Any()) {
				foreach (var address in customerAddress) {
					if (!address.Zoopla.Any() || reCheck) {
						var zooplaApi = new ZooplaApi();
						try {
							var areaValueGraphs = zooplaApi.GetAreaValueGraphs(address.Postcode);
							var averageSoldPrices = zooplaApi.GetAverageSoldPrices(address.Postcode);
							var zooplaEstimate = zooplaApi.GetZooplaEstimate(address.ZooplaAddress);
							var regexObj = new Regex(@"[^\d]");
							var stringVal = string.IsNullOrEmpty(zooplaEstimate)
								? string.Empty
								: regexObj.Replace(zooplaEstimate.Trim(), "");
							int intVal;
							if (!int.TryParse(stringVal, out intVal))
								intVal = 0;
							address.Zoopla.Add(new Zoopla {
								AreaName = averageSoldPrices.AreaName,
								AverageSoldPrice1Year = averageSoldPrices.AverageSoldPrice1Year,
								AverageSoldPrice3Year = averageSoldPrices.AverageSoldPrice3Year,
								AverageSoldPrice5Year = averageSoldPrices.AverageSoldPrice5Year,
								AverageSoldPrice7Year = averageSoldPrices.AverageSoldPrice7Year,
								NumerOfSales1Year = averageSoldPrices.NumerOfSales1Year,
								NumerOfSales3Year = averageSoldPrices.NumerOfSales3Year,
								NumerOfSales5Year = averageSoldPrices.NumerOfSales5Year,
								NumerOfSales7Year = averageSoldPrices.NumerOfSales7Year,
								TurnOver = averageSoldPrices.TurnOver,
								PricesUrl = averageSoldPrices.PricesUrl,
								AverageValuesGraphUrl = areaValueGraphs.AverageValuesGraphUrl,
								HomeValuesGraphUrl = areaValueGraphs.HomeValuesGraphUrl,
								ValueRangesGraphUrl = areaValueGraphs.ValueRangesGraphUrl,
								ValueTrendGraphUrl = areaValueGraphs.ValueTrendGraphUrl,
								CustomerAddress = address,
								ZooplaEstimate = zooplaEstimate,
								ZooplaEstimateValue = intVal,
								UpdateDate = DateTime.UtcNow
							});

							_session.Flush();
						} catch (Exception arg) {
							Log.Error(arg, "Zoopla error");
						}
					}
				}
			}
		}

		private readonly ISession _session;
		private readonly CustomerAddressRepository customerAddressRepository;
		private readonly int customerId;
		private readonly bool reCheck;
	} // class ZooplaStub
} // namespace

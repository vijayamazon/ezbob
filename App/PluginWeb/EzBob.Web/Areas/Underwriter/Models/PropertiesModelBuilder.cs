namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Linq;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure;
	using LandRegistryLib;

	public class PropertiesModelBuilder {
	    private readonly CustomerAddressRepository _customerAddressRepository;
	    private readonly LandRegistryRepository _landRegistryRepository;
		private readonly IWorkplaceContext _context;

		public PropertiesModelBuilder(
			CustomerAddressRepository customerAddressRepository,
			LandRegistryRepository landRegistryRepository, 
			IWorkplaceContext context) {
			_customerAddressRepository = customerAddressRepository;
			_landRegistryRepository = landRegistryRepository;
			_context = context;
		}
		
		public PropertiesModel Create(Customer customer) {
			int experianMortgage;
			int experianMortgageCount;
			int propertyCounter = 0;
			var data = new PropertiesModel();

			if (customer.PropertyStatus != null && customer.PropertyStatus.IsOwnerOfMainAddress) {
				var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
				if (currentAddress != null) {
					var property = GetPropertyModel(customer, currentAddress);
					if (property != null) {
						propertyCounter++;
						property.SerialNumberForCustomer = propertyCounter;
						data.Properties.Add(property);
					}
				}
			}

			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses.Where(x => x.IsOwnerAccordingToLandRegistry)) {
				var property = GetPropertyModel(customer, ownedProperty);
				if (property != null) {
					propertyCounter++;
					property.SerialNumberForCustomer = propertyCounter;
					data.Properties.Add(property);
				}
			}

			CrossCheckModel.GetMortgagesData(_context.UserId, customer, out experianMortgage, out experianMortgageCount);
			data.Init(
				data.Properties.Count(),
				experianMortgageCount,
				data.Properties.Sum(x => x.MarketValue),
				experianMortgage,
				data.Properties.Where(x => x.Zoopla != null).Sum(x => x.Zoopla.AverageSoldPrice1Year));

			return data;
		}

		private PropertyModel GetPropertyModel(Customer customer, CustomerAddress address) {
			int zooplaValue = 0;
			var lrBuilder = new LandRegistryModelBuilder();

			if (address != null) {

				if (address.IsOwnerAccordingToLandRegistry) {
					Zoopla zoopla = address.Zoopla.LastOrDefault();

					if (zoopla != null) {
						CrossCheckModel.GetZooplaData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue);
					}

					var personalAddressPropertyModel = new PropertyModel {
						AddressId = address.AddressId,
						Address = address.FormattedAddress,
						FormattedAddress = address.FormattedAddress,
						MarketValue = zooplaValue,
						Zoopla = zoopla
					};

					var lr = address.LandRegistry
										   .OrderByDescending(x => x.InsertDate)
										   .FirstOrDefault(
											   x =>
											   x.RequestType == LandRegistryRequestType.Res &&
											   x.ResponseType == LandRegistryResponseType.Success);

					if (lr != null) {
						personalAddressPropertyModel.LandRegistry = lrBuilder.BuildResModel(lr.Response);
						if (personalAddressPropertyModel.LandRegistry.Proprietorship.CurrentProprietorshipDate.HasValue) {
							personalAddressPropertyModel.YearOfOwnership = DateTime.SpecifyKind(personalAddressPropertyModel.LandRegistry.Proprietorship.CurrentProprietorshipDate.Value, DateTimeKind.Utc);
						}
						personalAddressPropertyModel.NumberOfOwners = lr.Owners.Count();
					}

					return personalAddressPropertyModel;
				}
			}

			return null;
		}
    }
}
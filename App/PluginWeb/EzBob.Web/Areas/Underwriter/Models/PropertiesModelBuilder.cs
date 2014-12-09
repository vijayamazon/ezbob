namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Repository;
	using EZBob.DatabaseLib.Repository;
	using Infrastructure;
	using LandRegistryLib;
	using StructureMap;

	public class PropertiesModelBuilder {
		private readonly CustomerAddressRepository _customerAddressRepository;
		private readonly LandRegistryRepository _landRegistryRepository;
		private readonly IWorkplaceContext _context;
		private readonly LandRegistryModelBuilder _landRegistryModelBuilder;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> _fileRepo;

		public PropertiesModelBuilder(
			CustomerAddressRepository customerAddressRepository,
			LandRegistryRepository landRegistryRepository,
			IWorkplaceContext context) {
			_customerAddressRepository = customerAddressRepository;
			_landRegistryRepository = landRegistryRepository;
			_context = context;
			_landRegistryModelBuilder = new LandRegistryModelBuilder();
			_fileRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_AlertDocument>>();
		}

		public PropertiesModel Create(Customer customer) {
			int experianMortgage;
			int experianMortgageCount;
			var data = new PropertiesModel();

			if (customer.PropertyStatus != null && customer.PropertyStatus.IsOwnerOfMainAddress) {
				var currentAddresses = customer.AddressInfo.PersonalAddress.Where(x => x.AddressType == CustomerAddressType.PersonalAddress);
				foreach (var customerAddress in currentAddresses) {
					var property = GetPropertyModel(customer, customerAddress);
					if (property != null) {
						data.Properties.Add(property);
					}
				}
			}

			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses) {
				var property = GetPropertyModel(customer, ownedProperty);
				if (property != null) {
					data.Properties.Add(property);
				}
			}

			var unmappedLrs = _landRegistryRepository.GetByCustomer(customer).Where(x => x.RequestType == LandRegistryRequestType.Res && x.CustomerAddress == null);

			foreach (var unmappedLr in unmappedLrs) {
				var lrModel = _landRegistryModelBuilder.BuildResModel(unmappedLr.Response, unmappedLr.TitleNumber);

				lrModel.AttachmentId = _fileRepo.GetAll()
							 .Where(
								 x =>
								 x.Customer == customer && x.Description == "LandRegistry" && x.DocName.StartsWith(lrModel.TitleNumber))
							 .Select(x => x.Id)
							 .FirstOrDefault();

				var unMappedProperty = new PropertyModel {
					AddressType = "Unmapped LR",
					VerifyStatus = PropertyVerifyStatus.NotVerified,
					LandRegistries = new List<LandRegistryResModel> { lrModel }
				};

				if (lrModel.PropertyAddresses != null && lrModel.PropertyAddresses.Any()) {
					unMappedProperty.Postcode = lrModel.PropertyAddresses.First().PostCode;
					unMappedProperty.NumberOfOwners = unmappedLr.Owners.Count();
					unMappedProperty.FormattedAddress = lrModel.PropertyAddresses.First().Lines;
				}

				data.Properties.Add(unMappedProperty);
			}

			CrossCheckModel.GetMortgagesData(_context.UserId, customer, out experianMortgage, out experianMortgageCount);
			data.Init(
				numberOfProperties: data.Properties
										.Count(x => x.VerifyStatus == PropertyVerifyStatus.VerifiedOwned),
				numberOfMortgages: experianMortgageCount,
				assetsValue: data.Properties
								 .Where(x => x.VerifyStatus == PropertyVerifyStatus.VerifiedOwned)
								 .Sum(x => x.MarketValue),
				totalMortgages: experianMortgage,
				zooplaAverage: data.Properties
								   .Where(x => x.Zoopla != null && x.VerifyStatus == PropertyVerifyStatus.VerifiedOwned)
								   .Sum(x => x.Zoopla.AverageSoldPrice1Year)
				);

			return data;
		}

		private PropertyModel GetPropertyModel(Customer customer, CustomerAddress address) {
			int zooplaValue = 0;

			if (address != null) {
				Zoopla zoopla = address.Zoopla.LastOrDefault();
				if (zoopla != null) {
					CrossCheckModel.GetZooplaData(customer, zoopla.ZooplaEstimateValue, zoopla.AverageSoldPrice1Year, out zooplaValue);
				}

				var propertyModel = new PropertyModel {
					AddressId = address.AddressId,
					Address = address.FormattedAddress,
					FormattedAddress = address.FormattedAddress,
					MarketValue = zooplaValue,
					Zoopla = zoopla,
					LandRegistries = new List<LandRegistryResModel>(),
					LandRegistryEnquiries = new List<LandRegistryEnquiryModel>(),
					VerifyStatus = PropertyVerifyStatus.NotVerified,
					Postcode = address.Postcode,
					AddressType = address.AddressType.DescriptionAttr()
				};

				var lrs = address.LandRegistry
								 .OrderByDescending(x => x.InsertDate)
								 .Where(x => x.RequestType == LandRegistryRequestType.Res)
								 .ToList();

				if (lrs.Any()) {
					propertyModel.VerifyStatus = address.IsOwnerAccordingToLandRegistry
																	? PropertyVerifyStatus.VerifiedOwned
																	: PropertyVerifyStatus.VerifiedNotOwned;
				}

				if (propertyModel.VerifyStatus != PropertyVerifyStatus.VerifiedOwned) {
					var enquiries = _landRegistryRepository.GetEnquiry(customer.Id, address.Postcode);
					foreach (var enq in enquiries) {
						var enqModel = _landRegistryModelBuilder.BuildEnquiryModel(enq.Response);
						propertyModel.LandRegistryEnquiries.Add(enqModel);
					}
				}

				foreach (var lr in lrs) {
					var lrModel = _landRegistryModelBuilder.BuildResModel(lr.Response, lr.TitleNumber);

					if (lrModel.Proprietorship != null && lrModel.Proprietorship.CurrentProprietorshipDate.HasValue) {
						propertyModel.YearOfOwnership = DateTime.SpecifyKind(lrModel.Proprietorship.CurrentProprietorshipDate.Value, DateTimeKind.Utc);
					}
					propertyModel.NumberOfOwners += lr.Owners.Count();

					lrModel.AttachmentId = _fileRepo.GetAll()
							 .Where(
								 x =>
								 x.Customer == customer && x.Description == "LandRegistry" && x.DocName.StartsWith(lrModel.TitleNumber))
							 .Select(x => x.Id)
							 .FirstOrDefault();

					propertyModel.LandRegistries.Add(lrModel);
				}

				return propertyModel;

			}

			return null;
		}
	}
}

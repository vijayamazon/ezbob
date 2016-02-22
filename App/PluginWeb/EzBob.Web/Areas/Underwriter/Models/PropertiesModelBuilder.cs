namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using ApplicationMng.Repository;
	using EZBob.DatabaseLib.Model.Database;
	using Infrastructure;
	using LandRegistryLib;
	using ServiceClientProxy;
	using ServiceClientProxy.EzServiceReference;
	using StructureMap;
	
	public class PropertiesModelBuilder {
		private readonly IWorkplaceContext context;
		private readonly LandRegistryModelBuilder landRegistryModelBuilder;
		private readonly NHibernateRepositoryBase<MP_AlertDocument> fileRepo;
		private readonly ServiceClient serviceClient;
		public PropertiesModelBuilder(IWorkplaceContext context) {
			this.context = context;
			this.landRegistryModelBuilder = new LandRegistryModelBuilder();
			this.fileRepo = ObjectFactory.GetInstance<NHibernateRepositoryBase<MP_AlertDocument>>();
			this.serviceClient = new ServiceClient();
		}

		public PropertiesModel Create(Customer customer) {
			int experianMortgage;
			int experianMortgageCount;
			var data = new PropertiesModel();

			var customersLrs = this.serviceClient.Instance.LandRegistryLoad(customer.Id, this.context.UserId).Value;
			if (customer.PropertyStatus != null && customer.PropertyStatus.IsOwnerOfMainAddress) {
				var currentAddresses = customer.AddressInfo.PersonalAddress.Where(x => x.AddressType == CustomerAddressType.PersonalAddress);
				foreach (var customerAddress in currentAddresses) {
					var property = GetPropertyModel(customer, customerAddress, customersLrs);
					if (property != null) {
						data.Properties.Add(property);
					}
				}
			}

			foreach (CustomerAddress ownedProperty in customer.AddressInfo.OtherPropertiesAddresses) {
				var property = GetPropertyModel(customer, ownedProperty, customersLrs);
				if (property != null) {
					data.Properties.Add(property);
				}
			}



			var unmappedLrs = customersLrs.Where(x => x.RequestType == LandRegistryRequestType.Res.ToString() && !x.AddressID.HasValue);

			foreach (var unmappedLr in unmappedLrs) {
				LandRegistryResModel lrModel = this.landRegistryModelBuilder.BuildResModel(unmappedLr.Response, unmappedLr.TitleNumber);

				lrModel.AttachmentId = this.fileRepo.GetAll()
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

			CrossCheckModel.GetMortgagesData(this.context.UserId, customer, out experianMortgage, out experianMortgageCount);
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

		private PropertyModel GetPropertyModel(Customer customer, CustomerAddress address, IList<LandRegistryDB> customerLrs) {
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

				var lrs = customerLrs
								 .Where(x => x.AddressID == address.AddressId)
								 .OrderByDescending(x => x.InsertDate)
								 .Where(x => x.RequestType == LandRegistryRequestType.Res.ToString())
								 .ToList();

				if (lrs.Any()) {
					propertyModel.VerifyStatus = address.IsOwnerAccordingToLandRegistry
																	? PropertyVerifyStatus.VerifiedOwned
																	: PropertyVerifyStatus.VerifiedNotOwned;
				}

				if (propertyModel.VerifyStatus != PropertyVerifyStatus.VerifiedOwned) {
					var enquiries = customerLrs
						.Where(x =>
							(x.RequestType == LandRegistryRequestType.Enquiry.ToString() || x.RequestType == LandRegistryRequestType.EnquiryPoll.ToString()) &&
							(x.ResponseType == LandRegistryResponseType.Success.ToString()) &&
							(x.Postcode == address.Postcode))
						.OrderByDescending(x => x.InsertDate)
						.ToList();

					foreach (var enq in enquiries) {
						var enqModel = this.landRegistryModelBuilder.BuildEnquiryModel(enq.Response);
						propertyModel.LandRegistryEnquiries.Add(enqModel);
					}
				}

				foreach (var lr in lrs) {
					var lrModel = this.landRegistryModelBuilder.BuildResModel(lr.Response, lr.TitleNumber);

					if (lrModel.Proprietorship != null && lrModel.Proprietorship.CurrentProprietorshipDate.HasValue) {
						propertyModel.YearOfOwnership = DateTime.SpecifyKind(lrModel.Proprietorship.CurrentProprietorshipDate.Value, DateTimeKind.Utc);
					}
					propertyModel.NumberOfOwners += lr.Owners.Count();

					lrModel.AttachmentId = this.fileRepo.GetAll()
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

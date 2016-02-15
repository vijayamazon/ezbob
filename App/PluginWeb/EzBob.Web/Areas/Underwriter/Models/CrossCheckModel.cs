namespace EzBob.Web.Areas.Underwriter.Models {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Text;
	using AutoMapper;
	using Ezbob.Backend.Models;
	using EzBob.Models;
	using EZBob.DatabaseLib.Model.Database;
	using EZBob.DatabaseLib.Model.Database.Mapping;
	using LandRegistryLib;
	using ServiceClientProxy;
	using StructureMap;

	public class CrossCheckModel {
		public enum TriState {
			NoData,
			No,
			Yes
		} // enum TriState

		public PersonalInfo Application { get; set; }
		public PersonalInfo EBay { get; set; }
		public PersonalInfo PayPal { get; set; }
		public CustomerAddress CurrentAddress { get; set; }
		public CustomerAddress PrevAddress { get; set; }
		public CustomerAddress EBayAddress { get; set; }
		public CustomerAddress PayPalAddress { get; set; }
		public CustomerAddress SellerAddress { get; set; }
		public CrossCheckStatus CrossCheckStatus { get; set; }
		public PropertyStatusModel PropertyStatus { get; set; }

		public List<Director> Directors { get; set; }
		public Customer Customer { get; set; }
		public List<string> ExperianDirectors { get; set; }

		public int ExperianMortgage { get; set; }
		public int ExperianMortgageCount { get; set; }
		public int AssetWorth { get; set; }
		

		static CrossCheckModel() {
			Mapper.CreateMap<EZBob.DatabaseLib.Model.Database.PersonalInfo, PersonalInfo>();
			Mapper.CreateMap<MP_PayPalPersonalInfo, PersonalInfo>()
				.ForMember(x => x.Fullname, y => y.MapFrom(z => z.FullName))
				.ForMember(x => x.Surname, y => y.MapFrom(z => z.LastName))
				.ForMember(x => x.DaytimePhone, y => y.MapFrom(z => z.Phone))
				.ForMember(x => x.Email, y => y.MapFrom(z => z.EMail));
			Mapper.CreateMap<MP_EbayUserData, PersonalInfo>()
				.ForMember(x => x.Email, y => y.MapFrom(z => z.EMail));
			Mapper.CreateMap<MP_EbayUserAddressData, CustomerAddress>()
				.ForMember(x => x.Line1, y => y.MapFrom(z => z.Street))
				.ForMember(x => x.Line2, y => y.MapFrom(z => z.Street1))
				.ForMember(x => x.Line3, y => y.MapFrom(z => z.Street2))
				.ForMember(x => x.Town, y => y.MapFrom(z => z.CityName))
				.ForMember(x => x.Postcode, y => y.MapFrom(z => z.PostalCode))
				.ForMember(x => x.Country, y => y.MapFrom(z => z.CountryName));
			Mapper.CreateMap<MP_PayPalPersonalInfo, CustomerAddress>()
				.ForMember(x => x.Line1, y => y.MapFrom(z => z.Street1))
				.ForMember(x => x.Line2, y => y.MapFrom(z => z.Street2))
				.ForMember(x => x.Town, y => y.MapFrom(z => z.City));

			serviceClient = new ServiceClient();
		} // static constructor

		public CrossCheckModel(int userId, Customer customer) {
			Customer = customer;

			Application = new PersonalInfo();
			EBay = new PersonalInfo();
			PayPal = new PersonalInfo();
			EBayAddress = new CustomerAddress();
			PayPalAddress = new CustomerAddress();
			SellerAddress = new CustomerAddress();
			CurrentAddress = new CustomerAddress();
			PrevAddress = new CustomerAddress();
			Directors = new List<Director>();
			CrossCheckStatus = new CrossCheckStatus();

			ExperianDirectors = GetExperianDirectors(customer)
				.DirectorNames;

		    var current = customer
		        .AddressInfo
                .PersonalAddress
                .FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);

			Zoopla zoopla = null;
			if (current != null)
				zoopla = current.Zoopla.LastOrDefault();

			if (zoopla != null) {
				current.ZooplaEstimate = zoopla.ZooplaEstimate;
				current.ZooplaEstimateValue = zoopla.ZooplaEstimateValue;

				int zooplaValue, experianMortgage, experianMortgageCount;
				GetZooplaData(customer, zoopla.ZooplaEstimateValue, zoopla.AverageSoldPrice1Year, out zooplaValue);
				GetMortgagesData(userId, customer, out experianMortgage, out experianMortgageCount);
				current.ZooplaValue = zooplaValue;
				ExperianMortgage = experianMortgage;
				ExperianMortgageCount = experianMortgageCount;

				current.ZooplaUpdateDate = zoopla.UpdateDate;
				current.ZooplaAverage = zoopla.AverageSoldPrice1Year.ToString("N0");

				AssetWorth = current.ZooplaValue - ExperianMortgage;
			}
            var prev = customer
                .AddressInfo
                .PrevPersonAddresses
                .OrderByDescending(x => x.AddressId)
                .FirstOrDefault(x => x.AddressType == CustomerAddressType.PrevPersonAddresses);

			CurrentAddress = current;
			if (prev != null)
				PrevAddress = prev;
			if (customer.PersonalInfo != null) {
				Application = Mapper.Map<EZBob.DatabaseLib.Model.Database.PersonalInfo, PersonalInfo>(customer.PersonalInfo);
				Application.Email = customer.Name;
			}

			if (customer.PropertyStatus != null) {
				PropertyStatus = new PropertyStatusModel {
					Id = customer.PropertyStatus.Id,
					Description = customer.PropertyStatus.Description,
					IsOwnerOfMainAddress = customer.PropertyStatus.IsOwnerOfMainAddress,
					IsOwnerOfOtherProperties = customer.PropertyStatus.IsOwnerOfOtherProperties
				};
			}

			var ebay = customer.GetEbayCustomerMarketPlaces()
				.FirstOrDefault();
			if (ebay != null) {
				var eBayUserData = ebay.EbayUserData.LastOrDefault();
				if (eBayUserData != null) {
					EBay = Mapper.Map<MP_EbayUserData, PersonalInfo>(eBayUserData);
					if (EBay.SellerInfo.SellerPaymentAddress != null)
						SellerAddress = Mapper.Map<MP_EbayUserAddressData, CustomerAddress>(EBay.SellerInfo.SellerPaymentAddress);
					if (eBayUserData.RegistrationAddress != null)
						EBayAddress = Mapper.Map<MP_EbayUserAddressData, CustomerAddress>(eBayUserData.RegistrationAddress);
				}
				if (eBayUserData != null && eBayUserData.RegistrationAddress != null) {
					EBay.Fullname = eBayUserData.RegistrationAddress.Name;
					EBay.DaytimePhone = eBayUserData.RegistrationAddress.Phone;
					EBay.MobilePhone = eBayUserData.RegistrationAddress.Phone2;
				}
			}
			var paypal = customer.GetPayPalCustomerMarketPlaces()
				.FirstOrDefault();
			if (paypal != null) {
				PayPal = Mapper.Map<MP_PayPalPersonalInfo, PersonalInfo>(paypal.PersonalInfo);
				PayPalAddress = Mapper.Map<MP_PayPalPersonalInfo, CustomerAddress>(paypal.PersonalInfo);
			}

			if (customer.Company != null)
				Directors.AddRange(customer.Company.Directors.Where(x => !x.IsDeleted));

			CrossCheckStatus.BuildMarkerStatusForPersonalInfo(Application, PayPal, EBay);
			CrossCheckStatus.BuildMarkerStatusForCustomerAddress(CurrentAddress, EBayAddress, PayPalAddress);
		} // constructor

		public static void GetZooplaData(Customer customer, int zooplaEstimate, int zoopla1YearAvg, out int zooplaValue) {
			zooplaValue = zooplaEstimate != 0 ? zooplaEstimate : zoopla1YearAvg;
		} // GetZooplaData

		public static void GetMortgagesData(int userId, Customer customer, out int mortgageBalance, out int mortgageCount) {
			var data = serviceClient.Instance.LoadExperianConsumerMortgageData(userId, customer.Id);
			mortgageBalance = data.Value.MortgageBalance;
			mortgageCount = data.Value.NumMortgages;
		} // GetMortgagesData

		public static ExperianDirectorsModel GetExperianDirectors(Customer customer) {
			var expDirRepo = ObjectFactory.GetInstance<ExperianDirectorRepository>();
			var dirs = expDirRepo.Find(customer.Id);
			var directorsModel = new ExperianDirectorsModel();

			if (dirs.Any()) {
				directorsModel.NumOfDirectors = dirs.Count(x => x.IsDirector);
				directorsModel.NumOfShareHolders = dirs.Count(x => x.IsShareholder);
				directorsModel.DirectorNames = new List<string>();
				foreach (var expDir in dirs)
					directorsModel.DirectorNames.Add(DetailsToName(expDir.FirstName, expDir.MiddleName, expDir.LastName));
			}
			return directorsModel;
		} // GetExperianDirectors

		public TriState IsExperianDirector(PersonalInfo oInfo) {
			if (ExperianDirectors == null)
				return TriState.NoData;

			return ExperianDirectors.Contains(DetailsToName(oInfo)) ? TriState.Yes : TriState.No;
		} // IsExperianDirector

		public TriState IsExperianDirector(Director oInfo) {
			if (ExperianDirectors == null)
				return TriState.NoData;

			return ExperianDirectors.Contains(DetailsToName(oInfo)) ? TriState.Yes : TriState.No;
		} // IsExperianDirector

		private static string DetailsToName(params string[] aryNames) {
			var os = new StringBuilder();

			foreach (string sName in aryNames) {
				string s = (sName ?? string.Empty).Trim()
					.ToLower();

				if (s != string.Empty)
					os.AppendFormat(" {0}", s);
			} // for each

			return os.ToString()
				.Trim();
		} // DetailsToName

		private string DetailsToName(PersonalInfo oInfo) {
			return oInfo == null ? "" : DetailsToName(oInfo.FirstName, oInfo.MiddleInitial, oInfo.Surname);
		} // DetailsToName

		private string DetailsToName(Director oInfo) {
			return oInfo == null ? "" : DetailsToName(oInfo.Name, oInfo.Middle, oInfo.Surname);
		} // DetailsToName

		private static readonly ServiceClient serviceClient;
	} // class CrossCheckModel

	public class PersonalInfo : EZBob.DatabaseLib.Model.Database.PersonalInfo {
		public string Email { get; set; }
		public string BusinessName { get; set; }
		public string PlayerId { get; set; }
		public string BillingEmail { get; set; }
		public string ResidentialStatus { get; set; }
		public DateTime? RegistrationDate { get; set; }
		public EbaySellerInfo SellerInfo { get; set; }
		public string Site { get; set; }
		public string SkypeID { get; set; }

		public PersonalInfo() {
			SellerInfo = new EbaySellerInfo();
		} // constructor
	} // class PersonalInfo

	public class ExperianDirectorsModel {
		public List<string> DirectorNames { get; set; }
		public int NumOfDirectors { get; set; }
		public int NumOfShareHolders { get; set; }
	} // class ExperianDirectorsModel
} // namespace

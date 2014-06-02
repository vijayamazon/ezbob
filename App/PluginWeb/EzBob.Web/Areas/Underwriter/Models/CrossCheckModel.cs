﻿namespace EzBob.Web.Areas.Underwriter.Models
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using AutoMapper;
	using EZBob.DatabaseLib.Model.Database;
	using System.Linq;
	using EZBob.DatabaseLib;
	using EzBob.Models;
	using Ezbob.ExperianParser;
	using System.Text.RegularExpressions;
	using ExperianLib;
	using LandRegistryLib;
	using StructureMap;

	public class CrossCheckModel
	{
		public PersonalInfo Application { get; set; }
		public PersonalInfo EBay { get; set; }
		public PersonalInfo PayPal { get; set; }
		public CustomerAddress CurrentAddress { get; set; }
		public CustomerAddress PrevAddress { get; set; }
		public CustomerAddress OtherPropertyAddress { get; set; }
		public CustomerAddress EBayAddress { get; set; }
		public CustomerAddress PayPalAddress { get; set; }
		public CustomerAddress SellerAddress { get; set; }
		public CrossCheckStatus CrossCheckStatus { get; set; }

		public List<Director> Directors { get; set; }
		public Customer Customer { get; set; }
		public SortedSet<string> ExperianDirectors { get; set; }

		public int ExperianMortgage { get; set; }
		public int ExperianMortgageCount { get; set; }
		public int AssetWorth { get; set; }
		public List<LandRegistryResModel> LandRegistries { get; set; }

		static CrossCheckModel()
		{
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
		}

		public static void GetZooplaAndMortgagesData(Customer customer, string zooplaEstimateStr, int zoopla1YearAvg, out int zooplaValue, out int mortgageBalance, out int mortgageCount)
		{
			var currentAddress = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
			mortgageBalance = 0;
			mortgageCount = 0;
			var regexObj = new Regex(@"[^\d]");
			var stringVal = string.IsNullOrEmpty(zooplaEstimateStr) ? "" : regexObj.Replace(zooplaEstimateStr.Trim(), "");
			int intVal;
			if (!int.TryParse(stringVal, out intVal))
			{
				intVal = zoopla1YearAvg;
			}
			zooplaValue = intVal;
			try
			{
				ConsumerServiceResult result;
				CreditBureauModelBuilder creditBureauModelBuilder = ObjectFactory.GetInstance<CreditBureauModelBuilder>();
				creditBureauModelBuilder.GetConsumerInfo(customer, false, null, currentAddress, out result);
				var experian = creditBureauModelBuilder.GenerateConsumerModel(customer.Id, result);
				if (experian != null && experian.ConsumerAccountsOverview != null)
				{
					int mtg;
					int.TryParse(experian.ConsumerAccountsOverview.Balance_Mtg, out mtg);
					mortgageBalance = mtg;

					int mtgCount;
					int.TryParse(experian.ConsumerAccountsOverview.OpenAccounts_Mtg, out mtgCount);
					mortgageCount = mtgCount;
				}
			}
			catch { }
		}

		public CrossCheckModel(Customer customer, CreditBureauModelBuilder creditBureauModelBuilder)
		{
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

			ExperianDirectors = GetExperianDirectors(customer);
			
			OtherPropertyAddress = customer.AddressInfo.OtherPropertyAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.OtherPropertyAddress);

			var current = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == CustomerAddressType.PersonalAddress);
			Zoopla zoopla = null;
			if(current != null) zoopla = current.Zoopla.LastOrDefault();

			if (zoopla != null)
			{
				current.ZooplaEstimate = zoopla.ZooplaEstimate;

				int zooplaValue, experianMortgage, experianMortgageCount;
				GetZooplaAndMortgagesData(customer, zoopla.ZooplaEstimate, zoopla.AverageSoldPrice1Year, out zooplaValue, out experianMortgage, out experianMortgageCount);
				current.ZooplaValue = zooplaValue;
				ExperianMortgage = experianMortgage;
				ExperianMortgageCount = experianMortgageCount;

				current.ZooplaUpdateDate = zoopla.UpdateDate;
				current.ZooplaAverage = zoopla.AverageSoldPrice1Year.ToString("N0");

				AssetWorth = current.ZooplaValue - ExperianMortgage;
			}
			var prev = customer.AddressInfo.PrevPersonAddresses.FirstOrDefault(x => x.AddressType == CustomerAddressType.PrevPersonAddresses);
			CurrentAddress = current;
			if (prev != null) PrevAddress = prev;
			if (customer.PersonalInfo != null)
			{
				Application = Mapper.Map<EZBob.DatabaseLib.Model.Database.PersonalInfo, PersonalInfo>(customer.PersonalInfo);
				Application.Email = customer.Name;
			}

			var ebay = customer.GetEbayCustomerMarketPlaces().FirstOrDefault();
			if (ebay != null)
			{
				var eBayUserData = ebay.EbayUserData.LastOrDefault();
				if (eBayUserData != null)
				{
					EBay = Mapper.Map<MP_EbayUserData, PersonalInfo>(eBayUserData);
					if (EBay.SellerInfo.SellerPaymentAddress != null)
					{
						SellerAddress = Mapper.Map<MP_EbayUserAddressData, CustomerAddress>(EBay.SellerInfo.SellerPaymentAddress);
					}
					if (eBayUserData.RegistrationAddress != null)
					{
						EBayAddress = Mapper.Map<MP_EbayUserAddressData, CustomerAddress>(eBayUserData.RegistrationAddress);
					}
				}
				if (eBayUserData != null && eBayUserData.RegistrationAddress != null)
				{
					EBay.Fullname = eBayUserData.RegistrationAddress.Name;
					EBay.DaytimePhone = eBayUserData.RegistrationAddress.Phone;
					EBay.MobilePhone = eBayUserData.RegistrationAddress.Phone2;
				}
			}
			var paypal = customer.GetPayPalCustomerMarketPlaces().FirstOrDefault();
			if (paypal != null)
			{
				PayPal = Mapper.Map<MP_PayPalPersonalInfo, PersonalInfo>(paypal.PersonalInfo);
				PayPalAddress = Mapper.Map<MP_PayPalPersonalInfo, CustomerAddress>(paypal.PersonalInfo);
			}

			if (customer.Company != null)
			{
				Directors.AddRange(customer.Company.Directors);
			}

			CrossCheckStatus.BuildMarkerStatusForPersonalInfo(Application, PayPal, EBay);
			CrossCheckStatus.BuildMarkerStatusForCustomerAddress(CurrentAddress, EBayAddress, PayPalAddress);

			var lrs = customer.LandRegistries.Where(x => x.RequestType == LandRegistryRequestType.Res).Select(x => x.Response);
			var b = new LandRegistryModelBuilder();
			LandRegistries = new List<LandRegistryResModel>();
			foreach (var lr in lrs)
			{
				LandRegistries.Add(b.BuildResModel(lr));
			}
		}

		public static SortedSet<string> GetExperianDirectors(Customer customer)
		{
			SortedSet<string> experianDirectors = null;
			ExperianParserOutput oParseResult = customer.ParseExperian(ExperianParserFacade.Target.Director);

			if (oParseResult.ParsingResult == ParsingResult.Ok)
			{
				foreach (var pair in oParseResult.Dataset)
				{
					foreach (ParsedDataItem di in pair.Value.Data)
					{
						string sFullName = DetailsToName(di["FirstName"], di["MidName1"], di["MidName2"], di["LastName"]);

						if (sFullName != string.Empty)
						{
							if (experianDirectors == null)
								experianDirectors = new SortedSet<string>();

							experianDirectors.Add(sFullName);
						} // if
					} // for each director
				} // for each group
			} // if

			return experianDirectors;
		}

		public enum TriState { NoData, No, Yes }

		public TriState IsExperianDirector(PersonalInfo oInfo)
		{
			if (ExperianDirectors == null)
				return TriState.NoData;

			return ExperianDirectors.Contains(DetailsToName(oInfo)) ? TriState.Yes : TriState.No;
		} // IsExperianDirector

		public TriState IsExperianDirector(Director oInfo)
		{
			if (ExperianDirectors == null)
				return TriState.NoData;

			return ExperianDirectors.Contains(DetailsToName(oInfo)) ? TriState.Yes : TriState.No;
		} // IsExperianDirector

		private string DetailsToName(PersonalInfo oInfo)
		{
			return oInfo == null ? "" : DetailsToName(oInfo.FirstName, oInfo.MiddleInitial, oInfo.Surname);
		} // DetailsToName

		private string DetailsToName(Director oInfo)
		{
			return oInfo == null ? "" : DetailsToName(oInfo.Name, oInfo.Middle, oInfo.Surname);
		} // DetailsToName

		private static string DetailsToName(params string[] aryNames)
		{
			var os = new StringBuilder();

			foreach (string sName in aryNames)
			{
				string s = (sName ?? string.Empty).Trim().ToLower();

				if (s != string.Empty)
					os.AppendFormat(" {0}", s);
			} // for each

			return os.ToString().Trim();
		} // DetailsToName
	}

	public class PersonalInfo : EZBob.DatabaseLib.Model.Database.PersonalInfo
	{
		public string Email { get; set; }
		public string BusinessName { get; set; }
		public string PlayerId { get; set; }
		public string BillingEmail { get; set; }
		public DateTime? RegistrationDate { get; set; }
		public EbaySellerInfo SellerInfo { get; set; }
		public string Site { get; set; }
		public string SkypeID { get; set; }

		public PersonalInfo()
		{
			SellerInfo = new EbaySellerInfo();
		}

	}
}
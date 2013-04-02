using System;
using System.Collections.Generic;
using ApplicationMng.Model;
using AutoMapper;
using EZBob.DatabaseLib.Model.Database;
using System.Linq;
using EzBob.Web.Areas.Customer.Models;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class CrossCheckModel
    {
        public PersonalInfo Application { get; set; }
        public PersonalInfo EBay { get; set; }
        public PersonalInfo PayPal { get; set; }
        public CustomerAddress CurrentAddress { get; set; }
        public CustomerAddress PrevAddress { get; set; }
        public CustomerAddress EBayAddress { get; set; }
        public CustomerAddress PayPalAddress { get; set; }
        public CustomerAddress SellerAddress { get; set; }
        public CrossCheckStatus CrossCheckStatus { get; set; }

        public List<Director> Directors { get; set; }
        public EZBob.DatabaseLib.Model.Database.Customer Customer { get; set; }


        static CrossCheckModel()
        {
            Mapper.CreateMap<EZBob.DatabaseLib.Model.Database.PersonalInfo, PersonalInfo>();
            Mapper.CreateMap<MP_PayPalPersonalInfo, PersonalInfo>()
                .ForMember(x=>x.Fullname, y=>y.MapFrom(z=>z.FullName))
                .ForMember(x=>x.Surname, y=>y.MapFrom(z=>z.LastName))
                .ForMember(x=>x.DaytimePhone, y=>y.MapFrom(z=>z.Phone))
                .ForMember(x=>x.Email, y=>y.MapFrom(z=>z.EMail));
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

        public CrossCheckModel(EZBob.DatabaseLib.Model.Database.Customer customer)
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
            CrossCheckStatus =new CrossCheckStatus();

            var current = customer.AddressInfo.PersonalAddress.FirstOrDefault(x => x.AddressType == AddressType.PersonalAddress);
            var prev = customer.AddressInfo.PrevPersonAddresses.FirstOrDefault(x => x.AddressType == AddressType.PrevPersonAddresses);
            if (current != null)CurrentAddress = current;
            if (prev != null)PrevAddress = prev;
            if (customer.PersonalInfo != null)
            {
                Application = Mapper.Map<EZBob.DatabaseLib.Model.Database.PersonalInfo, PersonalInfo>(customer.PersonalInfo);
                Application.Email = customer.Name;
            }

            var ebay = customer.GetEbayCustomerMarketPlaces().FirstOrDefault();
            if (ebay != null)
            {
                var eBayUserData = ebay.EbayUserData.FirstOrDefault();
                if (eBayUserData != null)
                {
                    EBay = Mapper.Map<MP_EbayUserData, PersonalInfo>(eBayUserData);
                    if(EBay.SellerInfo.SellerPaymentAddress != null)
                    {
                        SellerAddress = Mapper.Map<MP_EbayUserAddressData, CustomerAddress>(EBay.SellerInfo.SellerPaymentAddress);
                    }
                    if(eBayUserData.RegistrationAddress != null)
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

            if(customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.NonLimited)
            {
                Directors.AddRange(customer.NonLimitedInfo.Directors);
            }
            if (customer.PersonalInfo.TypeOfBusiness.Reduce() == TypeOfBusinessReduced.Limited)
            {
                Directors.AddRange(customer.LimitedInfo.Directors);
            }

            CrossCheckStatus.BuildMarkerStatusForPersonalInfo(Application,  PayPal, EBay);
            CrossCheckStatus.BuildMarkerStatusForCustomerAddress(CurrentAddress, EBayAddress, PayPalAddress);
        }
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
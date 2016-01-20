namespace EzBob3dParties.PayPalService.Soap.Handlers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using eBay.Service.Call;
    using EzBob3dParties.PayPalService.Soap;
    using EzBob3dPartiesApi.PayPal.Soap;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;
    using NServiceBus;
    using PayPal.OpenIdConnect;
    using PayPal.Permissions.Model;

    /// <summary>
    /// Handles PayPalGetCustomerPersonalData3dPartyCommand
    /// </summary>
    public class PayPalGetUserDataCommandHandler : HandlerBase<PayPalGetCustomerPersonalData3dPartyCommandResponse>, IHandleMessages<PayPalGetCustomerPersonalData3dPartyCommand>
    {
        [Injected]
        public PayPalSoapService PayPalService { get; set; }
        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(PayPalGetCustomerPersonalData3dPartyCommand command)
        {
            GetAdvancedPersonalDataResponse personalData = await PayPalService.GetPersonalData(command.Token, command.TokenSecret);

            var personalInfo = GetUserPersonalInfo(personalData.response);

            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => resp.UserPersonalInfo = personalInfo);
        }

        /// <summary>
        /// Gets the user personal information.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <returns></returns>
        private PayPalUserPersonalInfo GetUserPersonalInfo(PersonalDataList data) {
            var personalData = data.personalData.Where(o => o.personalDataKey.HasValue)
                .ToDictionary(o => o.personalDataKey.Value, o => o.personalDataValue);

            Func<PersonalAttribute, string> getAttributeValue = attr => {
                string res = null;
                personalData.TryGetValue(attr, out res);
                return res;
            };

            PayPalUserPersonalInfo personalInfo = new PayPalUserPersonalInfo {
                FirstName = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGNAMEPERSONFIRST),
                LastName = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGNAMEPERSONLAST),
                FullName = getAttributeValue(PersonalAttribute.HTTPSCHEMAOPENIDNETCONTACTFULLNAME),
                Phone = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTPHONEDEFAULT),
                BusinessName = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCOMPANYNAME),
                EMail = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTEMAIL),
                State = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTSTATEHOME),
                City = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTCITYHOME),
                Country = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTCOUNTRYHOME),
                Street1 = getAttributeValue(PersonalAttribute.HTTPSCHEMAOPENIDNETCONTACTSTREET1),
                Street2 = getAttributeValue(PersonalAttribute.HTTPSCHEMAOPENIDNETCONTACTSTREET),
                Postcode = getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGCONTACTPOSTALCODEHOME),
                DateOfBirth = ConvertBirthdayStringToDateTime(getAttributeValue(PersonalAttribute.HTTPAXSCHEMAORGBIRTHDATE)),
                PlayerId = getAttributeValue(PersonalAttribute.HTTPSWWWPAYPALCOMWEBAPPSAUTHSCHEMAPAYERID2),
                Created = DateTime.UtcNow
            };
            return personalInfo;
        }

        /// <summary>
        /// Converts the birthday string to date time.
        /// </summary>
        /// <param name="birthDay">The birth day.</param>
        /// <returns></returns>
        private DateTime? ConvertBirthdayStringToDateTime(string birthDay)
        {
            DateTime date;
            if (DateTime.TryParse(birthDay, out date))
            {
                return date;
            }

            return null;
        }
    }
}
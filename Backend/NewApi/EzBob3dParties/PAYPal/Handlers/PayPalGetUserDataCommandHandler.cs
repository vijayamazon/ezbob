using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties.PAYPal.Handlers
{
    using EzBob3dPartiesApi.PayPal;
    using EzBobCommon;
    using EzBobCommon.NSB;
    using EzBobModels.PayPal;
    using NServiceBus;
    using PayPal.Api.OpenIdConnect;

    /// <summary>
    /// Handles PayPalGetUserData3dPartyCommand
    /// </summary>
    public class PayPalGetUserDataCommandHandler : HandlerBase<PayPalGetUserData3dPartyCommandResponse>, IHandleMessages<PayPalGetUserData3dPartyCommand>
    {
        [Injected]
        public PayPalService PayPalService { get; set; }
        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(PayPalGetUserData3dPartyCommand command) {
            Userinfo userinfo = await PayPalService.GetUserInfo(command.Token);

            var personalInfo = GetUserPersonalInfo(userinfo);

            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => resp.UserPersonalInfo = personalInfo);
        }

        /// <summary>
        /// Gets the user personal information.
        /// </summary>
        /// <param name="userinfo">The user info.</param>
        /// <returns></returns>
        private PayPalUserPersonalInfo GetUserPersonalInfo(Userinfo userinfo) {
            PayPalUserPersonalInfo personalInfo = new PayPalUserPersonalInfo {
                FirstName = userinfo.given_name,
                LastName = userinfo.family_name,
                FullName = userinfo.name,
                Phone = userinfo.phone_number,
                BusinessName = null, //TODO: check it out where business name could be found
                EMail = userinfo.email,
                State = userinfo.address.region,
                City = userinfo.address.locality,
                Country = userinfo.address.country,
                Street1 = userinfo.address.street_address,
                Postcode = userinfo.address.postal_code,
                DateOfBirth = ConvertBirthdayStringToDateTime(userinfo.birthdate),
                PlayerId = userinfo.payer_id,
                Created = DateTime.UtcNow
            };
            return personalInfo;
        }

        /// <summary>
        /// Converts the birthday string to date time.
        /// </summary>
        /// <param name="birthDay">The birth day.</param>
        /// <returns></returns>
        private DateTime? ConvertBirthdayStringToDateTime(string birthDay) {
            DateTime date;
            if (DateTime.TryParse(birthDay, out date)) {
                return date;
            }

            return null;
        }
    }
}

namespace EzBob3dParties.Hmrc.Handlers {
    using System.Collections.Generic;
    using System.Linq;
    using EzBob3dPartiesApi.Hmrc;
    using EzBobCommon;
    using EzBobCommon.Currencies;
    using EzBobCommon.NSB;
    using EzBobCommon.Utils;
    using EzBobModels.Hmrc;
    using NServiceBus;

    public class HmrcGetVatReturnsHandler : HandlerBase<HmrcGetVatReturns3dPartyCommandResponse>, IHandleMessages<HmrcGetVatReturns3dPartyCommand> {

        [Injected]
        public IHmrcService HmrcService { get; set; }

        /// <summary>
        /// Handles the specified command.
        /// </summary>
        /// <param name="command">The command.</param>
        public async void Handle(HmrcGetVatReturns3dPartyCommand command) {
            HmrcVatReturnsInfo vatReturns = await HmrcService.GetVatReturns(command.UserName, command.Password);
            if (vatReturns.Info.HasErrors) {
                SendReply(vatReturns.Info, command);
                return;
            }
            InfoAccumulator info = new InfoAccumulator();
            SendReply(info, command, resp => {
                resp.Password = command.Password;
                resp.UserName = command.UserName;
                resp.CustomerId = command.CustomerId;
                PrepareResponse(resp, vatReturns);
            });
        }

        /// <summary>
        /// Prepares the response.
        /// </summary>
        /// <param name="response">The response.</param>
        /// <param name="vatInfo">The vat information.</param>
        private void PrepareResponse(HmrcGetVatReturns3dPartyCommandResponse response, HmrcVatReturnsInfo vatInfo) {
            response.VatReturnsPerBusiness = ExtractVatReturnsPerBusiness(vatInfo);
            response.RtiMonthEntries = ExtractMonthEntries(vatInfo);
        }

        private IEnumerable<RtiTaxMonthEntry> ExtractMonthEntries(HmrcVatReturnsInfo vatInfo) {
            return vatInfo.RtiTaxYearInfo.Months.Select(ConvertToRtiMonthEntry);
        }

        /// <summary>
        /// Converts to rti month entry.
        /// </summary>
        /// <param name="taxMonthInfo">The tax month information.</param>
        /// <returns></returns>
        private RtiTaxMonthEntry ConvertToRtiMonthEntry(RtiTaxMonthInfo taxMonthInfo) {
            return new RtiTaxMonthEntry {
                CurrencyCode = taxMonthInfo.AmountDue.ISOCurrencySymbol,
                AmountDue = taxMonthInfo.AmountDue.Amount,
                AmountPaid = taxMonthInfo.AmountPaid.Amount,
                DateStart = taxMonthInfo.DateStart,
                DateEnd = taxMonthInfo.DateEnd
            };
        }

        /// <summary>
        /// Extracts the vat returns per business.
        /// </summary>
        /// <param name="vatInfo">The vat information.</param>
        /// <returns></returns>
        private IEnumerable<VatReturnsPerBusiness> ExtractVatReturnsPerBusiness(HmrcVatReturnsInfo vatInfo) {
            return vatInfo.VatReturnInfos.Select(ConvertToVatReturnRecord);
        }

        /// <summary>
        /// Converts to vat return record.
        /// </summary>
        /// <param name="vatInfo">The vat information.</param>
        /// <returns></returns>
        private VatReturnsPerBusiness ConvertToVatReturnRecord(VatReturnInfo vatInfo) {
            VatReturnRecord v = new VatReturnRecord {
                DateDue = vatInfo.DueDate,
                DateFrom = vatInfo.FromDate,
                DateTo = vatInfo.ToDate,
                Period = vatInfo.Period,
                RegistrationNo = vatInfo.RegistrationNumber
            };

            HmrcBusiness business = new HmrcBusiness {
                RegistrationNo = vatInfo.RegistrationNumber,
                Name = vatInfo.BusinessName,
                Address = CreateAddressString(vatInfo.BusinessAddress)
            };

            IEnumerable<VatReturnEntry> entries = CreateVatReturnEntries(vatInfo);

            return new VatReturnsPerBusiness {
                VatReturnRecord = v,
                Entries = entries,
                Business = business
            };
        }

        /// <summary>
        /// Creates the vat return entries.
        /// </summary>
        /// <param name="vatInfo">The vat information.</param>
        /// <returns></returns>
        private IEnumerable<VatReturnEntry> CreateVatReturnEntries(VatReturnInfo vatInfo) {
            return vatInfo.ReturnDetails.Select(ConvertToVatReturnEntry);
        }

        /// <summary>
        /// Converts to vat return entry.
        /// </summary>
        /// <param name="entry">The entry.</param>
        /// <returns></returns>
        private VatReturnEntry ConvertToVatReturnEntry(KeyValuePair<string, Money> entry) {
            return new VatReturnEntry {
                Amount = entry.Value.Amount,
                CurrencyCode = entry.Value.ISOCurrencySymbol
            };
        }

        /// <summary>
        /// Creates the address string.
        /// </summary>
        /// <param name="addressParts">The address parts.</param>
        /// <returns></returns>
        private string CreateAddressString(string[] addressParts) {
            if (CollectionUtils.IsEmpty(addressParts)) {
                return null;
            }

            return addressParts.Aggregate((s1, s2) => s1 + " " + s2);
        }
    }
}

namespace EzBobService.ThirdParties.Hmrc.Upload {
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using Common.Logging;
    using EzBobCommon;
    using EzBobModels.Hmrc;
    using iTextSharp.text.pdf;

    public class HmrcUploadedFileParser : IHmrcUploadedFileParser {
        private static readonly CultureInfo Culture = new CultureInfo("en-GB", false);
        private static readonly DateTime LongTimeAgo = new DateTime(1976, 7, 1, 9, 30, 0, DateTimeKind.Utc);

        private static readonly string Period = "returnDetailsDto.vatPeriod.formattedAvailablePeriod";
        private static readonly string DateFrom = "returnDetailsDto.vatPeriod.formattedDateFrom";
        private static readonly string DateTo = "returnDetailsDto.vatPeriod.formattedDateTo";
        private static readonly string DateDue = "returnDetailsDto.vatPeriod.formattedDueDate";

        private static readonly string RegistrationNo = "returnDetailsDto.traderDetails.vrn";
        private static readonly string BusinessName = "returnDetailsDto.traderDetails.tradersName";

        private static readonly string[] AddressFields; //filled by static constructor
        private static readonly string[] VatReturnFields;

        private static readonly char PoundsChar = '£';
        private static readonly string PoundsString = "£";

        static HmrcUploadedFileParser() {

            AddressFields = new string[6];
            for (int i = 1; i <= 6; ++i) {
                AddressFields[i - 1] = "returnDetailsDto.traderDetails." + (i == 6 ? "postcode" : "address" + i);
            }

            VatReturnFields = new string[9];
            for (int i = 1; i <= 9; i++) {
                VatReturnFields[i - 1] = string.Format("returnDetailsDto.vatReturnForm.box{0}.display{1}", i, i <= 5 ? "Amount" : "Pounds");
            }
        }


        [Injected]
        public ILog Log { get; set; }

        /// <summary>
        /// Parses the HMRC vat returns PDF.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        public Optional<VatReturnsPerBusiness> ParseHmrcVatReturnsPdf(string fileName, InfoAccumulator info) {
            PdfReader reader;
            try {
                reader = new FdfReader(fileName);
            } catch (Exception ex) {
                info.AddException(ex);
                return null;
            }

            VatReturnsPerBusiness vatReturnsPerBusiness = new VatReturnsPerBusiness();

            HmrcBusiness hmrcBusiness = new HmrcBusiness();
            VatReturnRecord vatReturnRecord = new VatReturnRecord();

            vatReturnsPerBusiness.VatReturnRecord = vatReturnRecord;
            vatReturnsPerBusiness.Business = hmrcBusiness;
            vatReturnsPerBusiness.Entries = ParseVatReturnDetails(reader, info).Value;
            FillPeriodData(reader, vatReturnRecord, info);
            FillBusinessData(reader, vatReturnRecord, hmrcBusiness, info);

            if (!info.HasErrors) {
                return vatReturnsPerBusiness;
            }

            return null;
        }

        /// <summary>
        /// Parses the vat return details.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private Optional<IEnumerable<VatReturnEntry>> ParseVatReturnDetails(PdfReader reader, InfoAccumulator info) {
            return VatReturnFields
                .SelectMany(v => ReadPoundsField(reader, v)
                    .IfEmpty(() => RegisterWarning(info, "Invalid amount.")))
                .Select(n => new VatReturnEntry {
                    Amount = n,
                    CurrencyCode = "GBP"
                })
                .DefaultIfEmpty(null)
                .AsOptional()
                .IfEmpty(() => RegisterError(info, "could nof find vat return ammounts"));
        }

        /// <summary>
        /// Fills the business data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="vatReturnRecord">The vat return record.</param>
        /// <param name="business">The business.</param>
        /// <param name="info">The information.</param>
        private void FillBusinessData(PdfReader reader, VatReturnRecord vatReturnRecord, HmrcBusiness business, InfoAccumulator info) {
            ReadLongField(reader, RegistrationNo)
                .IfNotEmpty(reg => {
                    vatReturnRecord.RegistrationNo = reg;
                    business.RegistrationNo = reg;
                })
                .IfEmpty(() => RegisterError(info, "Invalid 'registration number'"));

            ReadStringField(reader, BusinessName)
                .IfNotEmpty(name => business.Name = name)
                .IfEmpty(() => RegisterError(info, "Invalid 'business name'"));

            AddressFields
                .SelectMany(addr => ReadStringField(reader, addr))
                .DefaultIfEmpty(null) //otherwise, if collection is empty, 'Aggregate' below will throw an exception
                .Aggregate((s1, s2) => s1 + " " + s2)
                .AsOptional()
                .IfNotEmpty(addr => business.Address = addr)
                .IfEmpty(() => RegisterError(info, "Invalid 'business address.'"));
        }

        /// <summary>
        /// Fills the period data.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="vatReturnRecord">The vat return record.</param>
        /// <param name="info">The information.</param>
        /// <returns></returns>
        private void FillPeriodData(PdfReader reader, VatReturnRecord vatReturnRecord, InfoAccumulator info) {
            ReadStringField(reader, Period)
                .IfNotEmpty(val => vatReturnRecord.Period = val)
                .IfEmpty(() => RegisterError(info, "Invalid 'period'."));

            ReadDateTimeField(reader, DateFrom)
                .IfNotEmpty(from => vatReturnRecord.DateFrom = from)
                .IfEmpty(() => RegisterError(info, "Invalid 'date from'."));

            ReadDateTimeField(reader, DateTo)
                .IfNotEmpty(to => vatReturnRecord.DateTo = to)
                .IfEmpty(() => RegisterError(info, "Invalid 'date to'."));

            ReadDateTimeField(reader, DateDue)
                .IfNotEmpty(due => vatReturnRecord.DateDue = due)
                .IfEmpty(() => RegisterError(info, "Invalid 'date due'."));
        }

        /// <summary>
        /// Registers the error.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="errorMessage">The error message.</param>
        private void RegisterError(InfoAccumulator info, string errorMessage) {
            info.AddError(errorMessage);
            Log.Error(errorMessage);
        }

        /// <summary>
        /// Registers the warning.
        /// </summary>
        /// <param name="info">The information.</param>
        /// <param name="warningMessage">The warning message.</param>
        private void RegisterWarning(InfoAccumulator info, string warningMessage) {
            info.AddWarning(warningMessage);
            Log.Warn(warningMessage);
        }

        /// <summary>
        /// Reads the string field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private Optional<string> ReadStringField(PdfReader reader, string fieldName) {
            string res = reader.AcroFields.GetField(fieldName);
            if (EzBobCommon.Utils.StringUtils.IsEmpty(res)) {
                return Optional<string>.Empty();
            }

            return res;
        }

        /// <summary>
        /// Reads the long field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private Optional<long> ReadLongField(PdfReader reader, string fieldName) {
            var val = ReadStringField(reader, fieldName);
            if (!val.HasValue) {
                return Optional<long>.Empty();
            }

            long n;
            if (long.TryParse((string)val, out n)) {
                if (n > 0) {
                    return n;
                }
            }

            return Optional<long>.Empty();
        }

        /// <summary>
        /// Reads the pounds field.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private Optional<decimal> ReadPoundsField(PdfReader reader, string fieldName) {
            var val = ReadStringField(reader, fieldName);
            if (!val.HasValue) {
                return Optional<decimal>.Empty();
            }

            //TODO: do we really need to parse pounds in this way
            var pounds = ((string)val)
                .Replace(Convert.ToChar(65533), PoundsChar)
                .Replace("GBP", PoundsString);

            decimal n;
            if (decimal.TryParse(pounds, NumberStyles.Currency, Culture, out n)) {
                return n;
            }

            return Optional<decimal>.Empty();
        }

        /// <summary>
        /// Reads the date time.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="fieldName">Name of the field.</param>
        /// <returns></returns>
        private Optional<DateTime> ReadDateTimeField(PdfReader reader, string fieldName) {
            var date = ReadStringField(reader, fieldName);
            if (!date.HasValue) {
                return Optional<DateTime>.Empty();
            }

            DateTime res;
            if (DateTime.TryParseExact((string)date, "dd MMM yyyy", Culture, DateTimeStyles.None, out res)) {
                if (res > LongTimeAgo) {
                    return res;
                }
            }

            return Optional<DateTime>.Empty();
        }
    }
}

namespace Ezbob.CreditSafeLib
{
	using System;
	using Ezbob.Backend.ModelsWithDB.CreditSafe;

    public class CreditSafeNonLtdModelBuilder
    {
        private string Errors { get; set; }
        private bool HasParsingError { get; set; }

        public CreditSafeNonLtdBaseData Build(CreditSafeNonLtdResponse response, DateTime? insertDate = null, string companyRefNum = null, long? serviceLogId = null)
        {
            CreditSafeNonLtdBaseData data = new CreditSafeNonLtdBaseData();
            data.EzbobCompanyID = companyRefNum ?? "0";
            data.InsertDate = insertDate ?? new DateTime();
            data.ServiceLogID = serviceLogId ?? 0;

            xmlresponsecompanyBody item = (xmlresponsecompanyBody)response.Items[1];
            var company = item.companies[0];
            var baseInfo = company.baseinformation[0];
            var matchedccjsummary = company.matchedccjsummary[0];
            var possibleccjsummary = company.possiblematchedccjsummary[0];
            var seniorexecutive = company.seniorexecutive[0];

            TryRead(() =>
            {
                TryRead(() => data.Number = baseInfo.number, "BaseInfoNumber");
                TryRead(() => data.Name = baseInfo.name, "BaseInfoName");
                TryRead(() => data.Address1 = baseInfo.address1, "BaseInfoAddress1");
                TryRead(() => data.Address2 = baseInfo.address2, "BaseInfoAddress2");
                TryRead(() => data.Address3 = baseInfo.address3, "BaseInfoAddress3");
                TryRead(() => data.Address4 = baseInfo.address4, "BaseInfoAddress4");
                TryRead(() => data.PostCode = baseInfo.postcode, "BaseInfoPostcode");
                TryRead(() =>
                {
                    if (baseInfo.mpsregistered == "N")
                        data.MpsRegistered = false;
                    else
                        data.MpsRegistered = true;
                }, "BaseInfoMpsRegistered");
                data.AddressDate = TryReadDate(baseInfo.addressdate, "BaseInfoAddressDate");
                TryRead(() => data.AddressReason = baseInfo.addressreason, "BaseInfoAddressReason");
                TryRead(() => data.PremiseType = baseInfo.premisestype, "BaseInfoPremiseType");
                TryRead(() => data.Activities = baseInfo.activities, "BaseInfoActivities");
                TryRead(() => data.Employees = Convert.ToInt32(baseInfo.employees), "BaseInfoCompanyType");
                TryRead(() => data.Website = baseInfo.website, "BaseInfoWebsite");
                TryRead(() => data.Email = baseInfo.email, "BaseInfoEmail");

                TryRead(() => data.MatchedCcjValue = Convert.ToInt32(matchedccjsummary.value), "BaseInfoMatchedCcjValue");
                TryRead(() => data.MatchedCcjNumber = Convert.ToInt32(matchedccjsummary.number), "BaseInfoMatchedCcjNumber");
                data.MatchedCcjDateFrom = TryReadDate(matchedccjsummary.datefrom, "BaseInfoMatchedCcjDateFrom");
                data.MatchedCcjDateTo = TryReadDate(matchedccjsummary.dateto, "BaseInfoMatchedCcjDateTo");

                TryRead(() => data.PossibleCcjValue = Convert.ToInt32(possibleccjsummary.value), "BaseInfoPossibleCcjValue");
                TryRead(() => data.PossibleCcjNumber = Convert.ToInt32(possibleccjsummary.number), "BaseInfoPossibleCcjNumber");
                data.PossibleCcjDateFrom = TryReadDate(possibleccjsummary.datefrom, "BaseInfoPossibleCcjDateFrom");
                data.PossibleCcjDateTo = TryReadDate(possibleccjsummary.dateto, "BaseInfoPossibleCcjDateTo");

                TryRead(() => data.ExecutiveName = seniorexecutive.name, "BaseInfoExecutiveName");
                TryRead(() => data.ExecutivePosition = seniorexecutive.position, "BaseInfoExecutivePosition");
                TryRead(() => data.ExecutiveEmail = seniorexecutive.email, "BaseInfoExecutiveEmail");
            }, "BaseInfo");

            foreach (var tel in baseInfo.telephonenumbers)
            {
                CreditSafeNonLtdBaseDataTel newTel = new CreditSafeNonLtdBaseDataTel();
                TryRead(() => newTel.Telephone = tel.telephone, "TelTelephone");
                TryRead(() =>
                {
                    if (tel.tpsregistered == "N")
                        newTel.TpsRegistered = false;
                    else
                        newTel.TpsRegistered = true;
                }, "TelTpsRegistered");
                TryRead(() =>
                {
                    if (tel.main == "N")
                        newTel.Main = false;
                    else
                        newTel.Main = true;
                }, "TelMain");
                data.Tels.Add(newTel);
            }
            foreach (var fax in baseInfo.faxnumbers)
            {
                CreditSafeNonLtdBaseDataFax newFax = new CreditSafeNonLtdBaseDataFax();
                TryRead(() => newFax.Fax = fax.fax, "FaxFaxNumber");
                TryRead(() =>
                {
                    if (fax.fpsregistered == "N")
                        newFax.FpsRegistered = false;
                    else
                        newFax.FpsRegistered = true;
                }, "FaxFpsRegistered");
                TryRead(() =>
                {
                    if (fax.main == "N")
                        newFax.Main = false;
                    else
                        newFax.Main = true;
                }, "FaxMain");
                data.Faxs.Add(newFax);
            }
            foreach (var rating in company.ratings)
            {
                CreditSafeNonLtdRatings newRating = new CreditSafeNonLtdRatings();
                newRating.Date = TryReadDate(rating.date, "RatingsDate");
                TryRead(() => newRating.Score = Convert.ToInt32(rating.score), "RatingsScore");
                TryRead(() => newRating.Description = rating.description, "RatingsDescription");
                data.Ratings.Add(newRating);
            }

            foreach (var limit in company.limits)
            {
                CreditSafeNonLtdLimits newLimit = new CreditSafeNonLtdLimits();
                TryRead(() => newLimit.Limit = Convert.ToInt32(limit.limit), "LimitLimit");
                newLimit.Date = TryReadDate(limit.date, "LimitDate");
                data.Limits.Add(newLimit);
            }
            foreach (var matched in company.matchedrecordofpayments)
            {
                CreditSafeNonLtdMatchedCCJ newMatched = new CreditSafeNonLtdMatchedCCJ();
                TryRead(() => newMatched.CaseNr = matched.casenr, "MatchedCaseNr");
                newMatched.CcjDate = TryReadDate(matched.ccjdate, "MatchedCcjDate");
                newMatched.CcjDatePaid = TryReadDate(matched.ccjdatepaid, "MatchedCcjDatePaid");
                TryRead(() => newMatched.Court = matched.court, "MatchedCourt");
                TryRead(() => newMatched.CcjStatus = matched.ccjstatus, "MatchedCcjStatus");
                TryRead(() => newMatched.CcjAmount = Convert.ToInt32(matched.ccjamount), "MatchedCcjAmount");
                TryRead(() => newMatched.Against = matched.against, "MatchedAgainst");
                TryRead(() => newMatched.Address = matched.address, "MatchedAddress");
                data.MatchedCcj.Add(newMatched);
            }
            foreach (var possible in company.possiblematchedrecordofpayments)
            {
                CreditSafeNonLtdPossibleCCJ newPossible = new CreditSafeNonLtdPossibleCCJ();
                TryRead(() => newPossible.CaseNr = possible.casenr, "PossibleCaseNr");
                newPossible.CcjDate = TryReadDate(possible.ccjdate, "PossibleCcjDate");
                newPossible.CcjDatePaid = TryReadDate(possible.ccjdatepaid, "PossibleCcjDatePaid");
                TryRead(() => newPossible.Court = possible.court, "PossibleCourt");
                TryRead(() => newPossible.CcjStatus = possible.ccjstatus, "PossibleCcjStatus");
                TryRead(() => newPossible.CcjAmount = Convert.ToInt32(possible.ccjamount), "PossibleCcjAmount");
                TryRead(() => newPossible.Against = possible.against, "PossibleAgainst");
                TryRead(() => newPossible.Address = possible.address, "PossibleAddress");
                data.PossibleCcj.Add(newPossible);
            }
            foreach (var events in company.events)
            {
                CreditSafeNonLtdEvents newEvent = new CreditSafeNonLtdEvents();
                newEvent.Date = TryReadDate(events.date, "EventsDate");
                TryRead(() => newEvent.Text = events.text, "EventsText");
                data.Events.Add(newEvent);
            }

            if (HasParsingError)
            {
                data.HasParsingError = true;
                data.Error = Errors;
            }
            else
            {
                data.HasParsingError = false;
            }
            return data;
        }

        private void TryRead(Action a, string key, bool isRequered = true)
        {
            try
            {
                a();
            }
            catch
            {
                if (isRequered)
                {
                    HasParsingError = true;
                    Errors += "Can`t read value for: " + key + Environment.NewLine;
                }
            }//try
        }//TryRead

        private DateTime? TryReadDate(string a, string key, bool isRequired = true)
        {
            try
            {
                DateTime d = Convert.ToDateTime(a);
                return (d.Year < 1900) ? (DateTime?)null : d;
            }
            catch
            {
                if (isRequired)
                {
                    HasParsingError = true;
                    Errors += "Can't read value for: " + key + Environment.NewLine;
                } // if

                return null;
            } // try
        } // TryReadDate
    }
}

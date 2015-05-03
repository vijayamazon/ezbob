namespace Ezbob.CreditSafeLib
{
	using System;
	using Ezbob.Backend.ModelsWithDB.CreditSafe;

    public class CreditSafeLtdModelBuilder
    {
        private string Errors { get; set; }
        private bool HasParsingError { get; set; }

        public CreditSafeBaseData Build(CreditSafeLtdResponse response, DateTime? insertDate = null, string companyRefNum = null, long? serviceLogId = null)
        {
            CreditSafeBaseData data = new CreditSafeBaseData();
            data.CompanyRefNum = companyRefNum ?? "0";
            data.InsertDate = insertDate ?? new DateTime();
            data.ServiceLogID = serviceLogId ?? 0;

            xmlresponseBody item = (xmlresponseBody)response.Items[1];
            var company = item.companies[0];
            var baseInfo = company.baseinformation[0];
            var ccjsummary = company.ccjsummary[0];
            var mortgagesummary = company.mortgagesummary[0];
            var shareholdersummary = company.shareholdersummary[0];

            TryRead(() =>
            {
                TryRead(() => data.Number = baseInfo.number, "BaseInfoNumber");
                TryRead(() => data.Name = baseInfo.name, "BaseInfoName");
                TryRead(() => data.Telephone = baseInfo.telephone, "BaseInfoTelephone");
                TryRead(() =>
                {
                    if (baseInfo.tpsregistered == "N")
                        data.TpsRegistered = false;
                    else
                        data.TpsRegistered = true;
                }, "BaseInfoTpsRegistered");
                TryRead(() => data.Address1 = baseInfo.address1, "BaseInfoAddress1");
                TryRead(() => data.Address2 = baseInfo.address2, "BaseInfoAddress2");
                TryRead(() => data.Address3 = baseInfo.address3, "BaseInfoAddress3");
                TryRead(() => data.Address4 = baseInfo.address4, "BaseInfoAddress4");
                TryRead(() => data.Postcode = baseInfo.postcode, "BaseInfoPostcode");
                TryRead(() => data.SicCode = baseInfo.siccode, "BaseInfoSicCode");
                TryRead(() => data.SicDescription = baseInfo.sicdescription, "BaseInfoSicDescription");
                TryRead(() => data.Website = baseInfo.website, "BaseInfoWebsite");
                TryRead(() => data.CompanyType = baseInfo.companytype, "BaseInfoCompanyType");
                TryRead(() => data.AccountsType = baseInfo.accountstype, "BaseInfoAccountsType");
                TryRead(() =>
                {
                    var date = baseInfo.annualreturndate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.AnnualReturnDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoAnnualReturnDate");
                TryRead(() =>
                {
                    var date = baseInfo.incorporationdate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.IncorporationDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoIncorporationDate");
                TryRead(() =>
                {
                    var date = baseInfo.accountsfilingdate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.AccountsFilingDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoAccountsFilingDate");
                TryRead(() =>
                {
                    var date = baseInfo.latestaccountsdate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.LatestAccountsDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoLatestAccountsDate");
                TryRead(() => data.Quoted = baseInfo.quoted, "BaseInfoQuoted");
                //TryRead(() => data.CompanyStatus = baseInfo.companystatus, "BaseInfoCompanyStatus");

                TryRead(() => data.CCJValues = Convert.ToInt32(ccjsummary.values), "BaseInfoCCJValues");
                TryRead(() => data.CCJNumbers = Convert.ToInt32(ccjsummary.numbers), "BaseInfoCCJNumbers");
                TryRead(() =>
                {
                    var date = ccjsummary.datefrom.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.CCJDateFrom = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoCCJDateFrom");
                TryRead(() =>
                {
                    var date = ccjsummary.dateto.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        data.CCJDateTo = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "BaseInfoCCJDateTo");
                //TryRead(() => data.CCJNumberOfWrits = Convert.ToInt32(ccjsummary.numberofwrits), "BaseInfoCCJNumberOfWrits");
                TryRead(() => data.Outstanding = Convert.ToInt32(mortgagesummary.outstanding), "BaseInfoOutstanding");
                TryRead(() => data.Satisfied = Convert.ToInt32(mortgagesummary.satisfied), "BaseInfoSatisfied");
                TryRead(() => data.ShareCapital = Convert.ToInt32(shareholdersummary.sharecapital), "BaseInfoShareCapital");
            }, "BaseInfo");

            foreach (var secsic in baseInfo.secondarysiccodes)
            {
                CreditSafeBaseData_SecondarySicCodes sic = new CreditSafeBaseData_SecondarySicCodes();
                TryRead(() => sic.SicCode = secsic.siccode, "SecondarySicCode");
                TryRead(() => sic.SicDescription = secsic.sicdescription, "SecondarySicDescription");
                data.SecondarySicCodes.Add(sic);
            }
            foreach (var industry in company.industries)
            {
                CreditSafeIndustries ind = new CreditSafeIndustries();
                TryRead(() => ind.Name = industry.name, "IndustriesName");
                data.Industries.Add(ind);
            }

            foreach (var address in company.tradingaddresses)
            {
                CreditSafeTradingAddresses newAddress = new CreditSafeTradingAddresses();
                TryRead(() => newAddress.Address1 = address.address1, "TradingAddress1");
                TryRead(() => newAddress.Address2 = address.address2, "TradingAddress2");
                TryRead(() => newAddress.Address3 = address.address3, "TradingAddress3");
                TryRead(() => newAddress.Address4 = address.address4, "TradingAddress4");
                TryRead(() => newAddress.PostCode = address.postcode, "TradingAddressPostCode");
                TryRead(() => newAddress.Telephone = address.telephone, "TradingAddressTelephone");
                TryRead(() =>
                {
                    if (address.tpsregistered == "N")
                        newAddress.TpsRegistered = false;
                    else
                        newAddress.TpsRegistered = true;
                }, "TradingAddressTpsRegistered");
                data.TradingAddresseses.Add(newAddress);
            }
            foreach (var rating in company.ratings)
            {
                CreditSafeCreditRatings newRating = new CreditSafeCreditRatings();
                TryRead(() =>
                {
                    var date = rating.date.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newRating.Date = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "RatingDate");
                TryRead(() => newRating.Score = Convert.ToInt32(rating.score), "RatingScore");
                TryRead(() => newRating.Description = rating.description, "RatingDescription");
                data.CreditRatings.Add(newRating);
            }
            foreach (var limit in company.limits)
            {
                CreditSafeCreditLimits newLimit = new CreditSafeCreditLimits();
                TryRead(() => newLimit.Limit = Convert.ToInt32(limit.limit), "Limit");
                TryRead(() =>
                {
                    var date = limit.date.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newLimit.Date = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "LimitDate");
                data.CreditLimits.Add(newLimit);
            }
            foreach (var name in company.previousnames)
            {
                CreditSafePreviousNames newName = new CreditSafePreviousNames();
                TryRead(() => newName.Name = name.name, "PreviousName");
                TryRead(() =>
                {
                    var date = name.date.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newName.Date = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "PreviousNameDate");
                data.PreviousNames.Add(newName);
            }
            foreach (var record in company.recordofpayments)
            {
                CreditSafeCCJDetails newRecord = new CreditSafeCCJDetails();
                TryRead(() => newRecord.CaseNr = record.casenr, "CcjCaseNr");
                TryRead(() =>
                {
                    var date = record.ccjdate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newRecord.CcjDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "CcjDate");
                TryRead(() => newRecord.Court = record.court, "CcjCourt");
                TryRead(() =>
                {
                    var date = record.ccjdatepaid.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newRecord.CcjDatePaid = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "CcjDatePaid");
                TryRead(() => newRecord.CcjStatus = record.ccjstatus, "CcjStatus");
                TryRead(() => newRecord.CcjAmount = Convert.ToInt32(record.ccjamount), "CcjAmount");
                data.CcjDetails.Add(newRecord);
            }

            foreach (var history in company.statushistorys)
            {
                CreditSafeStatusHistory newHistory = new CreditSafeStatusHistory();
                TryRead(() =>
                {
                    var date = history.date.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newHistory.date = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "StatusHistoryDate");
                TryRead(() => newHistory.text = history.text, "StatusHistoryText");
                data.StatusHistory.Add(newHistory);
            }
            foreach (var mortgage in company.mortgages)
            {
                CreditSafeMortgages newMortgage = new CreditSafeMortgages();
                TryRead(() => newMortgage.MortgageType = mortgage.mortgagetype, "MortgageType");
                TryRead(() =>
                {
                    var date = mortgage.createddate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newMortgage.CreateDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "MortgageCreateDate");
                TryRead(() =>
                {
                    var date = mortgage.registereddate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newMortgage.RegisterDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "MortgageRegisterDate");
                TryRead(() =>
                {
                    var date = mortgage.satisfieddate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newMortgage.SatisfiedDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "MortgageSatisfiedDate");
                TryRead(() => newMortgage.Status = mortgage.status, "MortgageStatus");
                TryRead(() => newMortgage.AmountSecured = Convert.ToInt32(mortgage.amountsecured), "MortgageAmountSecured");
                TryRead(() => newMortgage.Details = mortgage.details, "MortgageDetails");
                foreach (var person in mortgage.personsentitled)
                {
                    CreditSafeMortgages_PersonEntitled newPerson = new CreditSafeMortgages_PersonEntitled();
                    TryRead(() => newPerson.Name = person.name, "personEntitledName");
                    newMortgage.PersonEntitled.Add(newPerson);
                }
                data.Mortgages.Add(newMortgage);

            }
            foreach (var shareholder in company.shareholders)
            {
                CreditSafeShareHolders newShareholder = new CreditSafeShareHolders();
                TryRead(() => newShareholder.Name = shareholder.name, "ShareholderName");
                TryRead(() => newShareholder.Shares = shareholder.shares, "ShareholderShares");
                TryRead(() => newShareholder.Currency = shareholder.currency, "ShareholderCurrency");
                data.ShareHolders.Add(newShareholder);
            }
            foreach (var financial in company.financials)
            {
                var period = financial.period[0];
                var profitloss = financial.profitloss[0];
                var balancesheet = financial.balancesheet[0];
                var capitalreserves = financial.capitalreserves[0];
                var miscellaneous = financial.miscellaneous[0];
                var ratios = financial.ratios[0];

                CreditSafeFinancial newFinancial = new CreditSafeFinancial();
                TryRead(() =>
                {
                    var date = period.datefrom.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newFinancial.DateFrom = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "FinancialDateFrom");
                TryRead(() =>
                {
                    var date = period.dateto.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newFinancial.DateTo = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "FinancialDateTo");
                TryRead(() => newFinancial.PeriodMonths = Convert.ToInt32(period.periodmonths), "FinancialPeriodMonths");
                TryRead(() => newFinancial.Currency = period.currency, "FinancialCurrency");
                TryRead(() =>
                {
                    if (profitloss.consolidatedaccounts == "N")
                        newFinancial.ConsolidatedAccounts = false;
                    else
                        newFinancial.ConsolidatedAccounts = true;
                }, "FinancialConsolidatedAccounts");
                TryRead(() => newFinancial.Turnover = Convert.ToInt32(profitloss.turnover), "FinancialTurnover");
                TryRead(() => newFinancial.Export = Convert.ToInt32(profitloss.export), "FinancialExport");
                TryRead(() => newFinancial.CostOfSales = Convert.ToInt32(profitloss.costofsales), "FinancialCostOfSales");
                TryRead(() => newFinancial.GrossProfit = Convert.ToInt32(profitloss.grossprofit), "FinancialGrossProfit");
                TryRead(() => newFinancial.WagesSalaries = Convert.ToInt32(profitloss.wagessalaries), "FinancialWagesSalaries");
                TryRead(() => newFinancial.DirectorEmoluments = Convert.ToInt32(profitloss.directorsemoluments), "FinancialDirectorEmoluments");
                TryRead(() => newFinancial.OperatingProfits = Convert.ToInt32(profitloss.operatingprofits), "FinancialOperatingProfits");
                TryRead(() => newFinancial.Depreciation = Convert.ToInt32(profitloss.depreciation), "FinancialDepreciation");
                TryRead(() => newFinancial.AuditFees = Convert.ToInt32(profitloss.auditfees), "FinancialAuditFees");
                TryRead(() => newFinancial.InterestPayments = Convert.ToInt32(profitloss.interestpayments), "FinancialInterestPayments");
                TryRead(() => newFinancial.Pretax = Convert.ToInt32(profitloss.pretax), "FinancialPretax");
                TryRead(() => newFinancial.Taxation = Convert.ToInt32(profitloss.taxation), "FinancialTaxation");
                TryRead(() => newFinancial.PostTax = Convert.ToInt32(profitloss.posttax), "FinancialPostTax");
                TryRead(() => newFinancial.DividendsPayable = Convert.ToInt32(profitloss.dividendspayable), "FinancialDividendsPayable");
                TryRead(() => newFinancial.RetainedProfits = Convert.ToInt32(profitloss.retainedprofits), "FinancialRetainedProfits");

                TryRead(() => newFinancial.TangibleAssets = Convert.ToInt32(balancesheet.tangibleassets), "FinancialTangibleAssets");
                TryRead(() => newFinancial.IntangibleAssets = Convert.ToInt32(balancesheet.intangibleassets), "FinancialIntangibleAssets");
                TryRead(() => newFinancial.FixedAssets = Convert.ToInt32(balancesheet.fixedassets), "FinancialFixedAssets");
                TryRead(() => newFinancial.CurrentAssets = Convert.ToInt32(balancesheet.currentassets), "FinancialCurrentAssets");
                TryRead(() => newFinancial.TradeDebtors = Convert.ToInt32(balancesheet.tradedebtors), "FinancialTradeDebtors");
                TryRead(() => newFinancial.Stock = Convert.ToInt32(balancesheet.stock), "FinancialStock");
                TryRead(() => newFinancial.Cash = Convert.ToInt32(balancesheet.cash), "FinancialCash");
                TryRead(() => newFinancial.OtherCurrentAssets = Convert.ToInt32(balancesheet.othercurrentassets), "FinancialOtherCurrentAssets");
                TryRead(() => newFinancial.IncreaseInCash = Convert.ToInt32(balancesheet.increaseincash), "FinancialIncreaseInCash");
                TryRead(() => newFinancial.MiscellaneousCurrentAssets = Convert.ToInt32(balancesheet.miscellaneouscurrentassets), "FinancialMiscellaneousCurrentAssets");
                TryRead(() => newFinancial.TotalAssets = Convert.ToInt32(balancesheet.totalassets), "FinancialTotalAssets");
                TryRead(() => newFinancial.TotalCurrentLiabilities = Convert.ToInt32(balancesheet.totalcurrentliabilities), "FinancialTotalCurrentLiabilities");
                TryRead(() => newFinancial.TradeCreditors = Convert.ToInt32(balancesheet.tradecreditors), "FinancialTradeCreditors");
                TryRead(() => newFinancial.OverDraft = Convert.ToInt32(balancesheet.overdraft), "FinancialOverDraft");
                TryRead(() => newFinancial.OtherShortTermFinance = Convert.ToInt32(balancesheet.othershorttermfinance), "FinancialOtherShortTermFinance");
                TryRead(() => newFinancial.MiscellaneousCurrentLiabilities = Convert.ToInt32(balancesheet.miscellaneouscurrentliabilities), "FinancialMiscellaneousCurrentLiabilities");
                TryRead(() => newFinancial.OtherLongTermFinance = Convert.ToInt32(balancesheet.otherlongtermfinance), "FinancialOtherLongTermFinance");
                TryRead(() => newFinancial.LongTermLiabilities = Convert.ToInt32(balancesheet.longtermliabilities), "FinancialLongTermLiabilities");
                TryRead(() => newFinancial.OverDrafeLongTermLiabilities = Convert.ToInt32(balancesheet.overdraftlongtermliabilites), "FinancialOverDrafeLongTermLiabilities");
                TryRead(() => newFinancial.Liabilities = Convert.ToInt32(balancesheet.liabilities), "FinancialLiabilities");
                TryRead(() => newFinancial.NetAssets = Convert.ToInt32(balancesheet.netassets), "FinancialNetAssets");
                TryRead(() => newFinancial.WorkingCapital = Convert.ToInt32(balancesheet.workingcapital), "FinancialWorkingCapital");

                TryRead(() => newFinancial.PaidUpEquity = Convert.ToInt32(capitalreserves.paidupequity), "FinancialPaidUpEquity");
                TryRead(() => newFinancial.ProfitLossReserve = Convert.ToInt32(capitalreserves.profitlossreserve), "FinancialProfitLossReserve");
                TryRead(() => newFinancial.SundryReserves = Convert.ToInt32(capitalreserves.sundryreserves), "FinancialSundryReserves");
                TryRead(() => newFinancial.RevalutationReserve = Convert.ToInt32(capitalreserves.revalutationreserve), "FinancialRevalutationReserve");
                TryRead(() => newFinancial.Reserves = Convert.ToInt32(capitalreserves.reserves), "FinancialReserves");
                TryRead(() => newFinancial.ShareholderFunds = Convert.ToInt32(capitalreserves.shareholderfunds), "FinancialShareholderFunds");
                TryRead(() => newFinancial.Networth = Convert.ToInt32(capitalreserves.networth), "FinancialNetworth");

                TryRead(() => newFinancial.NetCashFlowFromOperations = Convert.ToInt32(miscellaneous.netcashflowfromoperations), "FinancialNetCashFlowFromOperations");
                TryRead(() => newFinancial.NetCashFlowBeforeFinancing = Convert.ToInt32(miscellaneous.netcashflowbeforefinancing), "FinancialNetCashFlowBeforeFinancing");
                TryRead(() => newFinancial.NetCashFlowFromFinancing = Convert.ToInt32(miscellaneous.netcashflowfromfinancing), "FinancialNetCashFlowFromFinancing");
                TryRead(() =>
                {
                    if (miscellaneous.contingentliability == "No")
                        newFinancial.ContingentLiability = false;
                    else
                        newFinancial.ContingentLiability = true;
                }, "FinancialContingentLiability");
                TryRead(() => newFinancial.CapitalEmployed = Convert.ToInt32(miscellaneous.capitalemployed), "FinancialCapitalEmployed");
                TryRead(() => newFinancial.Employees = Convert.ToInt32(miscellaneous.employees), "FinancialEmployees");
                TryRead(() => newFinancial.Auditors = miscellaneous.auditors, "FinancialAuditors");
                TryRead(() => newFinancial.AuditQualification = miscellaneous.auditqualification, "FinancialAuditQualification");
                TryRead(() => newFinancial.Bankers = miscellaneous.bankers, "FinancialBankers");
                TryRead(() => newFinancial.BankBranchCode = miscellaneous.bankbranchcode, "FinancialBankBranchCode");

                TryRead(() => newFinancial.PreTaxMargin = ratios.pretaxmargin, "FinancialPreTaxMargin");
                TryRead(() => newFinancial.CurrentRatio = ratios.currentratio, "FinancialCurrentRatio");
                TryRead(() => newFinancial.NetworkingCapital = ratios.networkingcapital, "FinancialNetworkingCapital");
                TryRead(() => newFinancial.GearingRatio = ratios.gearingratio, "FinancialGearingRatio");
                TryRead(() => newFinancial.Equity = ratios.equity, "FinancialEquity");
                TryRead(() => newFinancial.CreditorDays = ratios.creditordays, "FinancialCreditorDays");
                TryRead(() => newFinancial.DebtorDays = ratios.debtordays, "FinancialDebtorDays");
                TryRead(() => newFinancial.Liquidity = ratios.liquidity, "FinancialLiquidity");
                TryRead(() => newFinancial.ReturnOnCapitalEmployed = ratios.returnoncapitalemployed, "FinancialReturnOnCapitalEmployed");
                TryRead(() => newFinancial.ReturnOnAssetsEmployed = ratios.returnonassetsemployed, "FinancialReturnOnAssetsEmployed");
                TryRead(() => newFinancial.CurrentDebtRatio = ratios.currentdebtratio, "FinancialCurrentDebtRatio");
                TryRead(() => newFinancial.TotalDebtRatio = ratios.totaldebtratio, "FinancialTotalDebtRatio");
                TryRead(() => newFinancial.StockTurnoverRatio = ratios.stockturnoverratio, "FinancialStockTurnoverRatio");
                TryRead(() => newFinancial.ReturnOnNetAssetsEmployed = ratios.returnonnetassetsemployed, "FinancialReturnOnNetAssetsEmployed");

                data.Financial.Add(newFinancial);
            }
            foreach (var eventhistory in company.eventhistory)
            {
                CreditSafeEventHistory newEvent = new CreditSafeEventHistory();
                TryRead(() =>
                {
                    var date = eventhistory.date.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newEvent.Date = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "EventHistoryDate");
                TryRead(() => newEvent.Text = eventhistory.text, "EventHistoryText");

                data.EventHistory.Add(newEvent);
            }
            foreach (var director in company.directors)
            {
                CreditSafeDirectors newDirector = new CreditSafeDirectors();
                TryRead(() => newDirector.Title = director.title, "DirectrosTitle");
                TryRead(() => newDirector.Name = director.name, "DirectrosName");
                TryRead(() => newDirector.Address1 = director.address1, "DirectorsAddress1");
                TryRead(() => newDirector.Address2 = director.address2, "DirectorsAddress2");
                TryRead(() => newDirector.Address3 = director.address3, "DirectorsAddress3");
                TryRead(() => newDirector.Address4 = director.address4, "DirectorsAddress4");
                TryRead(() => newDirector.Address5 = director.address5, "DirectorsAddress5");
                TryRead(() => newDirector.Address6 = director.address6, "DirectorsAddress6");
                TryRead(() => newDirector.PostCode = director.postcode, "DirectorsPostCode");
                TryRead(() =>
                {
                    var date = director.birthdate.Split('/');
                    if (Convert.ToInt32(date[2]) > 1900)
                        newDirector.BirthDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                }, "DirectorsBirthDate");
                TryRead(() => newDirector.Nationality = director.nationality, "DirectorsNationality");
                TryRead(() => newDirector.Honours = director.honours, "DirectorsHonours");
                foreach (var ship in director.directorships[0].directorship)
                {
                    CreditSafeDirectors_Directorships newShip = new CreditSafeDirectors_Directorships();
                    TryRead(() => newShip.CompanyNumber = ship.companynumber, "DirectorshipCompanyNumber");
                    TryRead(() => newShip.CompanyName = ship.companyname, "DirectorshipCompanyName");
                    TryRead(() => newShip.CompanyStatus = ship.companystatus, "DirectorshipCompanyStatus");
                    TryRead(() => newShip.Function = ship.function, "DirectorshipFunction");
                    TryRead(() =>
                    {
                        var date = ship.appointeddate.Split('/');
                        if (Convert.ToInt32(date[2]) > 1900)
                            newShip.AppointedDate = new DateTime(Convert.ToInt32(date[2]), Convert.ToInt32(date[1]), Convert.ToInt32(date[0]));
                    }, "DirectorsAppointedDate");
                    newDirector.Directorships.Add(newShip);
                }
                data.Directors.Add(newDirector);
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
            }

        }
    }
}

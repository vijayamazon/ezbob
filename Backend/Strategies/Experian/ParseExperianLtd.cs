namespace EzBob.Backend.Strategies.Experian {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Linq;
	using System.Reflection;
	using System.Xml;
	using Ezbob.Database;
	using Ezbob.Logger;
	using Ezbob.Utils;

	public class ParseExperianLtd : AStrategy {
		#region static constructor

		static ParseExperianLtd() {
			var oLog = new SafeILog(typeof (ParseExperianLtd));

			ms_oXml2Db = new SortedTable<string, string, TblFld>();
			ms_oConstructors = new SortedDictionary<string, ConstructorInfo>();

			#region fill ms_oXml2Db

			ms_oXml2Db["./REQUEST/DL12", "REGNUMBER"] = new TblFld("ExperianLtd", "RegisteredNumber");
			ms_oXml2Db["./REQUEST/DL12", "LEGALSTATUS"] = new TblFld("ExperianLtd", "LegalStatus");
			ms_oXml2Db["./REQUEST/DL12", "DATEINCORP-YYYY"] = new TblFld("ExperianLtd", "IncorporationDate");
			ms_oXml2Db["./REQUEST/DL12", "DATEDISSVD-YYYY"] = new TblFld("ExperianLtd", "DissolutionDate");
			ms_oXml2Db["./REQUEST/DL12", "COMPANYNAME"] = new TblFld("ExperianLtd", "CompanyName");
			ms_oXml2Db["./REQUEST/DL12", "REGADDR1"] = new TblFld("ExperianLtd", "OfficeAddress1");
			ms_oXml2Db["./REQUEST/DL12", "REGADDR2"] = new TblFld("ExperianLtd", "OfficeAddress2");
			ms_oXml2Db["./REQUEST/DL12", "REGADDR3"] = new TblFld("ExperianLtd", "OfficeAddress3");
			ms_oXml2Db["./REQUEST/DL12", "REGADDR4"] = new TblFld("ExperianLtd", "OfficeAddress4");
			ms_oXml2Db["./REQUEST/DL12", "REGPOSTCODE"] = new TblFld("ExperianLtd", "OfficeAddressPostcode");
			ms_oXml2Db["./REQUEST/DL76", "RISKSCORE"] = new TblFld("ExperianLtd", "CommercialDelphiScore");
			ms_oXml2Db["./REQUEST/DL76", "STABILITYODDS"] = new TblFld("ExperianLtd", "StabilityOdds");
			ms_oXml2Db["./REQUEST/DL76", "RISKBANDTEXT"] = new TblFld("ExperianLtd", "CommercialDelphiBandText");
			ms_oXml2Db["./REQUEST/DL78", "CREDITLIMIT"] = new TblFld("ExperianLtd", "CommercialDelphiCreditLimit");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "DATECHANGED"] = new TblFld("ExperianLtdPrevCompanyNames", "DateChanged");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "PREVREGADDR1"] = new TblFld("ExperianLtdPrevCompanyNames", "OfficeAddress1");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "PREVREGADDR2"] = new TblFld("ExperianLtdPrevCompanyNames", "OfficeAddress2");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "PREVREGADDR3"] = new TblFld("ExperianLtdPrevCompanyNames", "OfficeAddress3");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "PREVREGADDR4"] = new TblFld("ExperianLtdPrevCompanyNames", "OfficeAddress4");
			ms_oXml2Db["./REQUEST/DL15/PREVCOMPNAMES", "PREVREGPOSTCODE"] = new TblFld("ExperianLtdPrevCompanyNames", "OfficeAddressPostcode");
			ms_oXml2Db["./REQUEST/DL13", "ASREGOFFICEFLAG"] = new TblFld("ExperianLtd", "SameTradingAddressG");
			ms_oXml2Db["./REQUEST/DL13", "LEN1992SIC"] = new TblFld("ExperianLtd", "LengthOf1992SICArea");
			ms_oXml2Db["./REQUEST/DL13", "TRADPHONENUM"] = new TblFld("ExperianLtd", "TradingPhoneNumber");
			ms_oXml2Db["./REQUEST/DL13", "PRINACTIVITIES"] = new TblFld("ExperianLtd", "PrincipalActivities");
			ms_oXml2Db["./REQUEST/DL13", "SIC1992DESC1"] = new TblFld("ExperianLtd", "First1992SICCodeDescription");
			ms_oXml2Db["./REQUEST/DL17", "BANKSORTCODE"] = new TblFld("ExperianLtd", "BankSortcode");
			ms_oXml2Db["./REQUEST/DL17", "BANKNAME"] = new TblFld("ExperianLtd", "BankName");
			ms_oXml2Db["./REQUEST/DL17", "BANKADDR1"] = new TblFld("ExperianLtd", "BankAddress1");
			ms_oXml2Db["./REQUEST/DL17", "BANKADDR2"] = new TblFld("ExperianLtd", "BankAddress2");
			ms_oXml2Db["./REQUEST/DL17", "BANKADDR3"] = new TblFld("ExperianLtd", "BankAddress3");
			ms_oXml2Db["./REQUEST/DL17", "BANKADDR4"] = new TblFld("ExperianLtd", "BankAddress4");
			ms_oXml2Db["./REQUEST/DL17", "BANKPOSTCODE"] = new TblFld("ExperianLtd", "BankAddressPostcode");
			ms_oXml2Db["./REQUEST/DL23", "ULTPARREGNUM"] = new TblFld("ExperianLtd", "RegisteredNumberOfTheCurrentUltimateParentCompany");
			ms_oXml2Db["./REQUEST/DL23", "ULTPARNAME"] = new TblFld("ExperianLtd", "RegisteredNameOfTheCurrentUltimateParentCompany");
			ms_oXml2Db["./REQUEST/DL23/SHAREHLDS", "SHLDNAME"] = new TblFld("ExperianLtdShareholders", "DescriptionOfShareholder");
			ms_oXml2Db["./REQUEST/DL23/SHAREHLDS", "SHLDHOLDING"] = new TblFld("ExperianLtdShareholders", "DescriptionOfShareholding");
			ms_oXml2Db["./REQUEST/DL23/SHAREHLDS", "SHLDREGNUM"] = new TblFld("ExperianLtdShareholders", "RegisteredNumberOfALimitedCompanyWhichIsAShareholder");
			ms_oXml2Db["./REQUEST/DLB5", "RECORDTYPE"] = new TblFld("ExperianLtdDLB5", "RecordType");
			ms_oXml2Db["./REQUEST/DLB5", "ISSUINGCOMPANY"] = new TblFld("ExperianLtdDLB5", "IssueCompany");
			ms_oXml2Db["./REQUEST/DLB5", "CURPREVFLAG"] = new TblFld("ExperianLtdDLB5", "CurrentpreviousIndicator");
			ms_oXml2Db["./REQUEST/DLB5", "EFFECTIVEDATE-YYYY"] = new TblFld("ExperianLtdDLB5", "EffectiveDate");
			ms_oXml2Db["./REQUEST/DLB5", "SHARECLASSNUM"] = new TblFld("ExperianLtdDLB5", "ShareClassNumber");
			ms_oXml2Db["./REQUEST/DLB5", "SHAREHOLDINGNUM"] = new TblFld("ExperianLtdDLB5", "ShareholdingNumber");
			ms_oXml2Db["./REQUEST/DLB5", "SHAREHOLDERNUM"] = new TblFld("ExperianLtdDLB5", "ShareholderNumber");
			ms_oXml2Db["./REQUEST/DLB5", "SHAREHOLDERTYPE"] = new TblFld("ExperianLtdDLB5", "ShareholderType");
			ms_oXml2Db["./REQUEST/DLB5", "NAMEPREFIX"] = new TblFld("ExperianLtdDLB5", "Prefix");
			ms_oXml2Db["./REQUEST/DLB5", "FIRSTNAME"] = new TblFld("ExperianLtdDLB5", "FirstName");
			ms_oXml2Db["./REQUEST/DLB5", "MIDNAME"] = new TblFld("ExperianLtdDLB5", "MidName1");
			ms_oXml2Db["./REQUEST/DLB5", "SURNAME"] = new TblFld("ExperianLtdDLB5", "LastName");
			ms_oXml2Db["./REQUEST/DLB5", "NAMESUFFIX"] = new TblFld("ExperianLtdDLB5", "Suffix");
			ms_oXml2Db["./REQUEST/DLB5", "QUAL"] = new TblFld("ExperianLtdDLB5", "ShareholderQualifications");
			ms_oXml2Db["./REQUEST/DLB5", "TITLE"] = new TblFld("ExperianLtdDLB5", "Title");
			ms_oXml2Db["./REQUEST/DLB5", "COMPANYNAME"] = new TblFld("ExperianLtdDLB5", "ShareholderCompanyName");
			ms_oXml2Db["./REQUEST/DLB5", "SHAREHOLDERKGEN"] = new TblFld("ExperianLtdDLB5", "KgenName");
			ms_oXml2Db["./REQUEST/DLB5", "SHAREHOLDERREGNUM"] = new TblFld("ExperianLtdDLB5", "ShareholderRegisteredNumber");
			ms_oXml2Db["./REQUEST/DLB5", "ADDRLINE1"] = new TblFld("ExperianLtdDLB5", "AddressLine1");
			ms_oXml2Db["./REQUEST/DLB5", "ADDRLINE2"] = new TblFld("ExperianLtdDLB5", "AddressLine2");
			ms_oXml2Db["./REQUEST/DLB5", "ADDRLINE3"] = new TblFld("ExperianLtdDLB5", "AddressLine3");
			ms_oXml2Db["./REQUEST/DLB5", "TOWN"] = new TblFld("ExperianLtdDLB5", "Town");
			ms_oXml2Db["./REQUEST/DLB5", "COUNTY"] = new TblFld("ExperianLtdDLB5", "County");
			ms_oXml2Db["./REQUEST/DLB5", "POSTCODE"] = new TblFld("ExperianLtdDLB5", "Postcode");
			ms_oXml2Db["./REQUEST/DLB5", "COUNTRYOFORIGIN"] = new TblFld("ExperianLtdDLB5", "Country");
			ms_oXml2Db["./REQUEST/DLB5", "PUNAPOSTCODE"] = new TblFld("ExperianLtdDLB5", "ShareholderPunaPcode");
			ms_oXml2Db["./REQUEST/DLB5", "RMC"] = new TblFld("ExperianLtdDLB5", "ShareholderRMC");
			ms_oXml2Db["./REQUEST/DLB5", "SUPPRESS"] = new TblFld("ExperianLtdDLB5", "SuppressionFlag");
			ms_oXml2Db["./REQUEST/DLB5", "NOCREF"] = new TblFld("ExperianLtdDLB5", "NOCRefNumber");
			ms_oXml2Db["./REQUEST/DLB5", "LASTUPDATEDDATE-YYYY"] = new TblFld("ExperianLtdDLB5", "LastUpdated");
			ms_oXml2Db["./REQUEST/DL72", "FOREIGNFLAG"] = new TblFld("ExperianLtdDL72", "ForeignAddressFlag");
			ms_oXml2Db["./REQUEST/DL72", "DIRCOMPFLAG"] = new TblFld("ExperianLtdDL72", "IsCompany");
			ms_oXml2Db["./REQUEST/DL72", "DIRNUMBER"] = new TblFld("ExperianLtdDL72", "Number");
			ms_oXml2Db["./REQUEST/DL72", "DIRSHIPLEN"] = new TblFld("ExperianLtdDL72", "LengthOfDirectorship");
			ms_oXml2Db["./REQUEST/DL72", "DIRAGE"] = new TblFld("ExperianLtdDL72", "DirectorsAgeYears");
			ms_oXml2Db["./REQUEST/DL72", "NUMCONVICTIONS"] = new TblFld("ExperianLtdDL72", "NumberOfConvictions");
			ms_oXml2Db["./REQUEST/DL72", "DIRNAMEPREFIX"] = new TblFld("ExperianLtdDL72", "Prefix");
			ms_oXml2Db["./REQUEST/DL72", "DIRFORENAME"] = new TblFld("ExperianLtdDL72", "FirstName");
			ms_oXml2Db["./REQUEST/DL72", "DIRMIDNAME1"] = new TblFld("ExperianLtdDL72", "MidName1");
			ms_oXml2Db["./REQUEST/DL72", "DIRMIDNAME2"] = new TblFld("ExperianLtdDL72", "MidName2");
			ms_oXml2Db["./REQUEST/DL72", "DIRSURNAME"] = new TblFld("ExperianLtdDL72", "LastName");
			ms_oXml2Db["./REQUEST/DL72", "DIRNAMESUFFIX"] = new TblFld("ExperianLtdDL72", "Suffix");
			ms_oXml2Db["./REQUEST/DL72", "DIRQUALS"] = new TblFld("ExperianLtdDL72", "Qualifications");
			ms_oXml2Db["./REQUEST/DL72", "DIRTITLE"] = new TblFld("ExperianLtdDL72", "Title");
			ms_oXml2Db["./REQUEST/DL72", "DIRCOMPNAME"] = new TblFld("ExperianLtdDL72", "CompanyName");
			ms_oXml2Db["./REQUEST/DL72", "DIRCOMPNUM"] = new TblFld("ExperianLtdDL72", "CompanyNumber");
			ms_oXml2Db["./REQUEST/DL72", "DIRSHAREINFO"] = new TblFld("ExperianLtdDL72", "ShareInfo");
			ms_oXml2Db["./REQUEST/DL72", "DATEOFBIRTH-YYYY"] = new TblFld("ExperianLtdDL72", "BirthDate");
			ms_oXml2Db["./REQUEST/DL72", "DIRHOUSENAME"] = new TblFld("ExperianLtdDL72", "HouseName");
			ms_oXml2Db["./REQUEST/DL72", "DIRHOUSENUM"] = new TblFld("ExperianLtdDL72", "HouseNumber");
			ms_oXml2Db["./REQUEST/DL72", "DIRSTREET"] = new TblFld("ExperianLtdDL72", "Street");
			ms_oXml2Db["./REQUEST/DL72", "DIRTOWN"] = new TblFld("ExperianLtdDL72", "Town");
			ms_oXml2Db["./REQUEST/DL72", "DIRCOUNTY"] = new TblFld("ExperianLtdDL72", "County");
			ms_oXml2Db["./REQUEST/DL72", "DIRPOSTCODE"] = new TblFld("ExperianLtdDL72", "Postcode");
			ms_oXml2Db["./REQUEST/DL42", "TOTCURRDIRS"] = new TblFld("ExperianLtd", "TotalNumberOfCurrentDirectors");
			ms_oXml2Db["./REQUEST/DL42", "CURRDIRSHIPSLAST12"] = new TblFld("ExperianLtd", "NumberOfCurrentDirectorshipsLessThan12Months");
			ms_oXml2Db["./REQUEST/DL42", "APPTSLAST12"] = new TblFld("ExperianLtd", "NumberOfAppointmentsInTheLast12Months");
			ms_oXml2Db["./REQUEST/DL42", "RESNSLAST12"] = new TblFld("ExperianLtd", "NumberOfResignationsInTheLast12Months");
			ms_oXml2Db["./REQUEST/DL26", "AGEMOSTRECENTCCJ"] = new TblFld("ExperianLtd", "AgeOfMostRecentCCJDecreeMonths");
			ms_oXml2Db["./REQUEST/DL26", "NUMCCJLAST12"] = new TblFld("ExperianLtd", "NumberOfCCJsDuringLast12Months");
			ms_oXml2Db["./REQUEST/DL26", "VALCCJLAST12"] = new TblFld("ExperianLtd", "ValueOfCCJsDuringLast12Months");
			ms_oXml2Db["./REQUEST/DL26", "NUMCCJ13TO24"] = new TblFld("ExperianLtd", "NumberOfCCJsBetween13And24MonthsAgo");
			ms_oXml2Db["./REQUEST/DL26", "VALCCJ13TO24"] = new TblFld("ExperianLtd", "ValueOfCCJsBetween13And24MonthsAgo");
			ms_oXml2Db["./REQUEST/DL27/SUMMARYLINE", "CREDTYPE"] = new TblFld("ExperianLtdCreditSummary", "CreditEventType");
			ms_oXml2Db["./REQUEST/DL27/SUMMARYLINE", "TYPEDATE-YYYY"] = new TblFld("ExperianLtdCreditSummary", "DateOfMostRecentRecordForType");
			ms_oXml2Db["./REQUEST/DL41", "COMPAVGDBT-3MTH"] = new TblFld("ExperianLtd", "CompanyAverageDBT3Months");
			ms_oXml2Db["./REQUEST/DL41", "COMPAVGDBT-6MTH"] = new TblFld("ExperianLtd", "CompanyAverageDBT6Months");
			ms_oXml2Db["./REQUEST/DL41", "COMPAVGDBT-12MTH"] = new TblFld("ExperianLtd", "CompanyAverageDBT12Months");
			ms_oXml2Db["./REQUEST/DL41", "COMPNUMDBT-1000"] = new TblFld("ExperianLtd", "CompanyNumberOfDbt1000");
			ms_oXml2Db["./REQUEST/DL41", "COMPNUMDBT-10000"] = new TblFld("ExperianLtd", "CompanyNumberOfDbt10000");
			ms_oXml2Db["./REQUEST/DL41", "COMPNUMDBT-100000"] = new TblFld("ExperianLtd", "CompanyNumberOfDbt100000");
			ms_oXml2Db["./REQUEST/DL41", "COMPNUMDBT-100000PLUS"] = new TblFld("ExperianLtd", "CompanyNumberOfDbt100000Plus");
			ms_oXml2Db["./REQUEST/DL41", "INDAVGDBT-3MTH"] = new TblFld("ExperianLtd", "IndustryAverageDBT3Months");
			ms_oXml2Db["./REQUEST/DL41", "INDAVGDBT-6MTH"] = new TblFld("ExperianLtd", "IndustryAverageDBT6Months");
			ms_oXml2Db["./REQUEST/DL41", "INDAVGDBT-12MTH"] = new TblFld("ExperianLtd", "IndustryAverageDBT12Months");
			ms_oXml2Db["./REQUEST/DL41", "INDNUMDBT-1000"] = new TblFld("ExperianLtd", "IndustryNumberOfDbt1000");
			ms_oXml2Db["./REQUEST/DL41", "INDNUMDBT-10000"] = new TblFld("ExperianLtd", "IndustryNumberOfDbt10000");
			ms_oXml2Db["./REQUEST/DL41", "INDNUMDBT-100000"] = new TblFld("ExperianLtd", "IndustryNumberOfDbt100000");
			ms_oXml2Db["./REQUEST/DL41", "INDNUMDBT-100000PLUS"] = new TblFld("ExperianLtd", "IndustryNumberOfDbt100000Plus");
			ms_oXml2Db["./REQUEST/DL41", "COMPPAYPATTN"] = new TblFld("ExperianLtd", "CompanyPaymentPattern");
			ms_oXml2Db["./REQUEST/DL41", "INDPAYPATTN"] = new TblFld("ExperianLtd", "IndustryPaymentPattern");
			ms_oXml2Db["./REQUEST/DL41", "SUPPPAYPATTN"] = new TblFld("ExperianLtd", "SupplierPaymentPattern");
			ms_oXml2Db["./REQUEST/DL48", "FRAUDCATEGORY"] = new TblFld("ExperianLtdDL48", "FraudCategory");
			ms_oXml2Db["./REQUEST/DL48", "SUPPLIERNAME"] = new TblFld("ExperianLtdDL48", "SupplierName");
			ms_oXml2Db["./REQUEST/DL52", "RECORDTYPE"] = new TblFld("ExperianLtdDL52", "NoticeType");
			ms_oXml2Db["./REQUEST/DL52", "DATEOFNOTICE-YYYY"] = new TblFld("ExperianLtdDL52", "DateOfNotice");
			ms_oXml2Db["./REQUEST/DL68", "SUBSIDREGNUM"] = new TblFld("ExperianLtdDL68", "SubsidiaryRegisteredNumber");
			ms_oXml2Db["./REQUEST/DL68", "SUBSIDSTATUS"] = new TblFld("ExperianLtdDL68", "SubsidiaryStatus");
			ms_oXml2Db["./REQUEST/DL68", "SUBSIDLEGALSTATUS"] = new TblFld("ExperianLtdDL68", "SubsidiaryLegalStatus");
			ms_oXml2Db["./REQUEST/DL68", "SUBSIDNAME"] = new TblFld("ExperianLtdDL68", "SubsidiaryName");
			ms_oXml2Db["./REQUEST/DL97", "ACCTSTATE"] = new TblFld("ExperianLtdDL97", "AccountState");
			ms_oXml2Db["./REQUEST/DL97", "COMPANYTYPE"] = new TblFld("ExperianLtdDL97", "CompanyType");
			ms_oXml2Db["./REQUEST/DL97", "ACCTTYPE"] = new TblFld("ExperianLtdDL97", "AccountType");
			ms_oXml2Db["./REQUEST/DL97", "DEFAULTDATE-YYYY"] = new TblFld("ExperianLtdDL97", "DefaultDate");
			ms_oXml2Db["./REQUEST/DL97", "SETTLEMTDATE-YYYY"] = new TblFld("ExperianLtdDL97", "SettlementDate");
			ms_oXml2Db["./REQUEST/DL97", "CURRBALANCE"] = new TblFld("ExperianLtdDL97", "CurrentBalance");
			ms_oXml2Db["./REQUEST/DL97", "STATUS1TO2"] = new TblFld("ExperianLtdDL97", "Status12");
			ms_oXml2Db["./REQUEST/DL97", "STATUS3TO9"] = new TblFld("ExperianLtdDL97", "Status39");
			ms_oXml2Db["./REQUEST/DL97", "CAISLASTUPDATED-YYYY"] = new TblFld("ExperianLtdDL97", "CAISLastUpdatedDate");
			ms_oXml2Db["./REQUEST/DL97", "ACCTSTATUS12"] = new TblFld("ExperianLtdDL97", "AccountStatusLast12AccountStatuses");
			ms_oXml2Db["./REQUEST/DL97", "AGREEMTNUM"] = new TblFld("ExperianLtdDL97", "AgreementNumber");
			ms_oXml2Db["./REQUEST/DL97", "MONTHSDATA"] = new TblFld("ExperianLtdDL97", "MonthsData");
			ms_oXml2Db["./REQUEST/DL97", "DEFAULTBALANCE"] = new TblFld("ExperianLtdDL97", "DefaultBalance");
			ms_oXml2Db["./REQUEST/DL99", "DATEOFACCOUNTS-YYYY"] = new TblFld("ExperianLtdDL99", "Date");
			ms_oXml2Db["./REQUEST/DL99", "CREDDIRLOANS"] = new TblFld("ExperianLtdDL99", "CredDirLoans");
			ms_oXml2Db["./REQUEST/DL99", "DEBTORS"] = new TblFld("ExperianLtdDL99", "Debtors");
			ms_oXml2Db["./REQUEST/DL99", "DEBTORSDIRLOANS"] = new TblFld("ExperianLtdDL99", "DebtorsDirLoans");
			ms_oXml2Db["./REQUEST/DL99", "DEBTORSGROUPLOANS"] = new TblFld("ExperianLtdDL99", "DebtorsGroupLoans");
			ms_oXml2Db["./REQUEST/DL99", "INTNGBLASSETS"] = new TblFld("ExperianLtdDL99", "InTngblAssets");
			ms_oXml2Db["./REQUEST/DL99", "INVENTORIES"] = new TblFld("ExperianLtdDL99", "Inventories");
			ms_oXml2Db["./REQUEST/DL99", "ONCLDIRLOANS"] = new TblFld("ExperianLtdDL99", "OnClDirLoans");
			ms_oXml2Db["./REQUEST/DL99", "OTHDEBTORS"] = new TblFld("ExperianLtdDL99", "OtherDebtors");
			ms_oXml2Db["./REQUEST/DL99", "PREPAYACCRUALS"] = new TblFld("ExperianLtdDL99", "PrepayAccRuals");
			ms_oXml2Db["./REQUEST/DL99", "RETAINEDEARNINGS"] = new TblFld("ExperianLtdDL99", "RetainedEarnings");
			ms_oXml2Db["./REQUEST/DL99", "TNGBLASSETS"] = new TblFld("ExperianLtdDL99", "TngblAssets");
			ms_oXml2Db["./REQUEST/DL99", "TOTALCASH"] = new TblFld("ExperianLtdDL99", "TotalCash");
			ms_oXml2Db["./REQUEST/DL99", "TOTALCURRLBLTS"] = new TblFld("ExperianLtdDL99", "TotalCurrLblts");
			ms_oXml2Db["./REQUEST/DL99", "TOTALNONCURR"] = new TblFld("ExperianLtdDL99", "TotalNonCurr");
			ms_oXml2Db["./REQUEST/DL99", "TOTALSHAREFUND"] = new TblFld("ExperianLtdDL99", "TotalShareFund");
			ms_oXml2Db["./REQUEST/DLA2", "DATEACCS-YYYY"] = new TblFld("ExperianLtdDLA2", "Date");
			ms_oXml2Db["./REQUEST/DLA2", "NUMEMPS"] = new TblFld("ExperianLtdDLA2", "NumberOfEmployees");
			ms_oXml2Db["./REQUEST/DL65", "CHARGENUMBER"] = new TblFld("ExperianLtdDL65", "ChargeNumber");
			ms_oXml2Db["./REQUEST/DL65", "FORMNUMBERFLAG"] = new TblFld("ExperianLtdDL65", "FormNumber");
			ms_oXml2Db["./REQUEST/DL65", "CURRENCYINDICATOR"] = new TblFld("ExperianLtdDL65", "CurrencyIndicator");
			ms_oXml2Db["./REQUEST/DL65", "TOTAMTDEBENTURESECD"] = new TblFld("ExperianLtdDL65", "TotalAmountOfDebentureSecured");
			ms_oXml2Db["./REQUEST/DL65", "CHARGETYPE"] = new TblFld("ExperianLtdDL65", "ChargeType");
			ms_oXml2Db["./REQUEST/DL65", "AMTSECURED"] = new TblFld("ExperianLtdDL65", "AmountSecured");
			ms_oXml2Db["./REQUEST/DL65", "PROPERTYDETAILS"] = new TblFld("ExperianLtdDL65", "PropertyDetails");
			ms_oXml2Db["./REQUEST/DL65", "CHARGEETEXT"] = new TblFld("ExperianLtdDL65", "ChargeeText");
			ms_oXml2Db["./REQUEST/DL65", "RESTRICTINGPROVNS"] = new TblFld("ExperianLtdDL65", "RestrictingProvisions");
			ms_oXml2Db["./REQUEST/DL65", "REGULATINGPROVNS"] = new TblFld("ExperianLtdDL65", "RegulatingProvisions");
			ms_oXml2Db["./REQUEST/DL65", "ALTERATIONSTOORDER"] = new TblFld("ExperianLtdDL65", "AlterationsToTheOrder");
			ms_oXml2Db["./REQUEST/DL65", "PROPERTYRELDFROMCHGE"] = new TblFld("ExperianLtdDL65", "PropertyReleasedFromTheCharge");
			ms_oXml2Db["./REQUEST/DL65", "AMTCHARGEINCRD"] = new TblFld("ExperianLtdDL65", "AmountChargeIncreased");
			ms_oXml2Db["./REQUEST/DL65", "CREATIONDATE-YYYY"] = new TblFld("ExperianLtdDL65", "CreationDate");
			ms_oXml2Db["./REQUEST/DL65", "DATEFULLYSATD-YYYY"] = new TblFld("ExperianLtdDL65", "DateFullySatisfied");
			ms_oXml2Db["./REQUEST/DL65", "FULLYSATDINDICATOR"] = new TblFld("ExperianLtdDL65", "FullySatisfiedIndicator");
			ms_oXml2Db["./REQUEST/DL65", "NUMPARTIALSATNDATES"] = new TblFld("ExperianLtdDL65", "NumberOfPartialSatisfactionDates");
			ms_oXml2Db["./REQUEST/DL65", "NUMPARTIALSATNDATAITEMS"] = new TblFld("ExperianLtdDL65", "NumberOfPartialSatisfactionDataItems");

			#endregion fill ms_oXml2Db

			ms_oXml2Db.ForEach((ignoredOnce, ignoredTwice, oTblFld) => {
				if (ms_oConstructors.ContainsKey(oTblFld.TableName))
					return;

				string sFullTypeName = typeof (ParseExperianLtd).Namespace + "." + oTblFld.TableName;

				Type oTableType = Type.GetType(sFullTypeName);

				if (oTableType == null) {
					oLog.Alert("There is no type {0} ({1}).", oTblFld.TableName, sFullTypeName);
					ms_oConstructors[oTblFld.TableName] = null;
					return;
				} // if

				ConstructorInfo ci = oTableType.GetConstructors().FirstOrDefault();

				if (ci == null)
					oLog.Alert("There is no constructor for type {0}.", oTblFld.TableName);

				ms_oConstructors[oTblFld.TableName] = ci;

				oLog.Debug("Constructor for type {0} ({1}) has been registered successfully.", oTblFld.TableName, sFullTypeName);
			}); // for each table field
		} // static constructor

		#endregion static constructor

		#region public

		#region constructor

		public ParseExperianLtd(long nServiceLogID, AConnection oDB, ASafeLog oLog) : base(oDB, oLog) {
			m_nServiceLogID = nServiceLogID;
		} // constructor

		#endregion constructor

		#region property Name

		public override string Name {
			get { return "ParseExperianLtd"; }
		} // Name

		#endregion property Name

		#region method Execute

		public override void Execute() {
			string sXml = null;
			long nID = 0;

			Log.Debug("Parsing Experian Ltd for service log entry {0}...", m_nServiceLogID);

			DB.ForEachRowSafe(
				(sr, bRowsetStart) => {
					sXml = sr["ResponseData"];
					nID = sr["Id"];

					return ActionResult.SkipAll;
				},
				"LoadServiceLogEntry",
				CommandSpecies.StoredProcedure,
				new QueryParameter("@EntryID", m_nServiceLogID)
			);

			if (nID != m_nServiceLogID) {
				Log.Debug("Parsing Experian Ltd for service log entry {0} failed: entry not found.", m_nServiceLogID);
				return;
			} // if

			XmlDocument oXml = new XmlDocument();

			try {
				oXml.LoadXml(sXml);
			}
			catch (Exception e) {
				Log.Warn(e, "Parsing Experian Ltd for service log entry {0} failed.", m_nServiceLogID);
				return;
			} // try

			if (oXml.DocumentElement == null) {
				Log.Warn("Parsing Experian Ltd for service log entry {0} failed (no root element found).", m_nServiceLogID);
				return;
			} // try

			ParseAndSave(oXml);

			Log.Debug("Parsing Experian Ltd for service log entry {0} complete.", m_nServiceLogID);
		} // Execute

		#endregion method Execute

		#endregion public

		#region private

		private readonly long m_nServiceLogID;

		#region method ParseAndSave

		private void ParseAndSave(XmlDocument oXml) {
			AHasChildren oMainTable = null;
			string sMainTableName = null;
			var oData = new SortedDictionary<string, List<AHasChildren>>();

			ms_oXml2Db.ForEachRow((sGroupNodeName, oGroupFields) => {
				// ReSharper disable PossibleNullReferenceException
				XmlNodeList oGroupNodes = oXml.DocumentElement.SelectNodes(sGroupNodeName);
				// ReSharper restore PossibleNullReferenceException

				if ((oGroupNodes == null) || (oGroupNodes.Count < 1)) {
					Log.Debug("No nodes found for {0}.", sGroupNodeName);
					return;
				} // if

				foreach (XmlNode oGroup in oGroupNodes) {
					var oUpdatedTables = new SortedDictionary<string, bool>();

					foreach (KeyValuePair<string, TblFld> pair in oGroupFields) {
						string sNodeName = pair.Key;
						TblFld oTblFld = pair.Value;

						var oNode = oGroup.SelectSingleNode(sNodeName);

						if (oNode == null)
							continue;

						AHasChildren oCurTable = null;

						if (oTblFld.TableName == sMainTableName)
							oCurTable = oMainTable;
						else if (oUpdatedTables.ContainsKey(oTblFld.TableName))
							oCurTable = oUpdatedTables[oTblFld.TableName] ? oMainTable : oData[oTblFld.TableName].Last();
						else {
							ConstructorInfo ci = ms_oConstructors.ContainsKey(oTblFld.TableName) ? ms_oConstructors[oTblFld.TableName] : null;

							if (ci == null) { // should never happen, all such cases should be eliminated during developers testing.
								Log.Alert("Cannot find constructor for type {0}.", oTblFld.TableName);
								return;
							} // if

							oCurTable = ci.Invoke(new object[0]) as AHasChildren;

							if (oCurTable == null) { // should never happen, all such cases should be eliminated during developers testing.
								Log.Alert("Failed to create an instance of type {0}.", oTblFld.TableName);
								return;
							} // if

							if (oCurTable.IsMainTable()) {
								oMainTable = oCurTable;
								sMainTableName = oTblFld.TableName;
								oUpdatedTables[oTblFld.TableName] = true;
							}
							else {
								if (!oData.ContainsKey(oTblFld.TableName))
									oData[oTblFld.TableName] = new List<AHasChildren>();

								oData[oTblFld.TableName].Add(oCurTable);

								oUpdatedTables[oTblFld.TableName] = false;
							} // if
						} // if

						if (oCurTable == null) {
							Log.Warn(
								"No table found for {0}.{1} from {2}/{3}.",
								oTblFld.TableName,
								oTblFld.FieldName,
								sGroupNodeName,
								sNodeName
							);
							continue;
						} // if

						var pi = oCurTable.GetType().GetProperty(oTblFld.FieldName);

						if (pi.PropertyType == typeof (string))
							pi.SetValue(oCurTable, oNode.InnerText);
						else if (pi.PropertyType == typeof (int?))
							SetInt(pi, oCurTable, oNode.InnerText);
						else if (pi.PropertyType == typeof (double?))
							SetDouble(pi, oCurTable, oNode.InnerText);
						else if (pi.PropertyType == typeof (decimal?))
							SetDecimal(pi, oCurTable, oNode.InnerText);
						else if (pi.PropertyType == typeof (DateTime?)) {
							if (sNodeName.EndsWith("-YYYY")) {
								string sPrefix = sNodeName.Substring(0, sNodeName.Length - 5);

								XmlNode oMonthNode = oGroup.SelectSingleNode(sPrefix + "-MM");

								XmlNode oDayNode = oMonthNode == null ? null : oGroup.SelectSingleNode(sPrefix + "-DD");

								if (oDayNode != null)
									SetDate(pi, oCurTable, oNode.InnerText + MD(oMonthNode) + MD(oDayNode));
							}
							else
								SetDate(pi, oCurTable, oNode.InnerText);
						}
					} // for each node name of the group
				} // for each selected node in the group
			}); // for each

			if (oMainTable == null) {
				Log.Debug("No main table entry extracted.");
				return;
			} // if

			Log.Debug("{0}", oMainTable.Stringify());

			foreach (var pair in oData) {
				Log.Debug("Start of {0} with {1} items.", pair.Key, pair.Value.Count);

				foreach (var oRow in pair.Value)
					Log.Debug("{0}", oRow.Stringify());

				Log.Debug("End of {0} with {1} items.", pair.Key, pair.Value.Count);
			} // for each
		} // ParseAndSave

		#endregion method ParseAndSave

		#region method SetInt

		private static void SetInt(PropertyInfo pi, AHasChildren oCurTable, string sValue) {
			int n;

			if (int.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetInt

		#endregion method SetInt

		#region method SetDouble

		private static void SetDouble(PropertyInfo pi, AHasChildren oCurTable, string sValue) {
			double n;

			if (double.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDouble

		#endregion method SetDouble

		#region method SetDecimal

		private static void SetDecimal(PropertyInfo pi, AHasChildren oCurTable, string sValue) {
			decimal n;

			if (decimal.TryParse(sValue, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDecimal

		#endregion method SetDecimal

		#region method SetDate

		private static void SetDate(PropertyInfo pi, AHasChildren oCurTable, string sValue) {
			DateTime n;

			if (DateTime.TryParseExact(sValue, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out n))
				pi.SetValue(oCurTable, n);
			else
				pi.SetValue(oCurTable, null);
		} // SetDate

		#endregion method SetDate

		#region method MD

		private static string MD(XmlNode oNode) {
			string s = oNode.InnerText;

			if (s.Length < 2)
				return "0" + s;

			return s;
		} // MD

		#endregion method MD

		#region static

		private static readonly SortedTable<string, string, TblFld> ms_oXml2Db;
		private static readonly SortedDictionary<string, ConstructorInfo> ms_oConstructors;

		#endregion static

		#region class TblFld

		private class TblFld {
			public TblFld(string sTableName, string sFieldName) {
				TableName = sTableName;
				FieldName = sFieldName;
			} // constructor

			public string TableName { get; private set; }
			public string FieldName { get; private set; }
		} // class TblFld

		#endregion class TblFld

		#endregion private
	} // class ParseExperianLtd
} // namespace

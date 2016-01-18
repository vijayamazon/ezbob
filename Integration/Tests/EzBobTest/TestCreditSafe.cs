namespace EzBobTest
{
    using System;
    using ExperianLib.Tests.Integration;
    using Ezbob.Backend.Models.ExternalAPI;
    using Ezbob.Backend.Strategies.CreditSafe;
    using Ezbob.Backend.Strategies;
    using Ezbob.Backend.Strategies.Experian;
    using Ezbob.CreditSafeLib;
    using NUnit.Framework;
    using EzServiceAccessor;
    using EzServiceShortcut;
    using StructureMap;
    using Ezbob.Backend.Strategies.CreditSafe;
    using Ezbob.Backend.Strategies.ExternalAPI.Alibaba;

    [TestFixture]
    public class TestCreditSafe : BaseTest
    {

        [Test]
        public void TestGetdata()
        {

           // string response = @"<xmlresponse><header><reportinformation><time>29/03/2015 11:10:21</time><reporttype>getcompanyinformation</reporttype><country>UK</country><version>6.0</version><provider>www.creditsafe.com</provider><chargereference></chargereference></reportinformation></header><body><reportid>X9999999</reportid><reportname>TEST COMPANY LIMITED - THE</reportname><companies><company><baseinformation><number>X9999999</number><name>TEST COMPANY LIMITED - THE</name><telephone>02920886500</telephone><tpsregistered>N</tpsregistered><address1>BRYN HOUSE</address1><address2>CAERPHILLY BUSINESS PARK</address2><address3>VAN ROAD</address3><address4>CARDIFF </address4><postcode>CF83 3GG</postcode><siccode>17401</siccode><sicdescription>MANUFACTURE OF SOFT FURNISHINGS</sicdescription><secondarysiccodes /><website>test.com</website><companytype>Private limited with Share Capital</companytype><accountstype>Group</accountstype><annualreturndate>30/04/2007</annualreturndate><incorporationdate>01/01/2004</incorporationdate><accountsfilingdate>16/04/2011</accountsfilingdate><latestaccountsdate>30/09/2010</latestaccountsdate><quoted /><siccode2007>13921</siccode2007><sicdescription2007>Manufacture of soft furnishings</sicdescription2007><ftse /><charitynumber /><contractlimit>196500</contractlimit><safenumber>UK07499628</safenumber></baseinformation><industries><industry><name>The underwriting of general insurance business in the united kingdom.</name></industry></industries><tradingaddresses /><ratings><ratingdetail><date>24/08/2014</date><score>50</score><description>Moderate Risk</description></ratingdetail><ratingdetail><date>14/01/2013</date><score>-6</score><description>Company is dissolved</description></ratingdetail><ratingdetail><date>04/01/2013</date><score>0</score><description>Error - Object reference not set to an instance of an object.</description></ratingdetail></ratings><limits><limitdetail><limit>28000</limit><date>24/08/2014</date></limitdetail><limitdetail><limit>0</limit><date>14/01/2013</date></limitdetail><limitdetail><limit>500</limit><date>17/08/2011</date></limitdetail></limits><previousnames><previousname><name>TEST COMPANY LIMITED</name><date>24/09/1999</date></previousname></previousnames><recordofpayments><rop><casenr>6XS01897</casenr><ccjdate>28/03/2010</ccjdate><ccjdatepaid /><court>SHEFFIELD</court><ccjstatus>JG</ccjstatus><ccjamount>2593</ccjamount><incomingrecorddetails /><exact>yes</exact><ccjtype>registered</ccjtype></rop><rop><casenr>7DL00350</casenr><ccjdate>01/03/2010</ccjdate><ccjdatepaid>11/09/2010</ccjdatepaid><court>DARLINGTON</court><ccjstatus>SS</ccjstatus><ccjamount>7426</ccjamount><incomingrecorddetails /><exact>yes</exact><ccjtype>registered</ccjtype></rop></recordofpayments><ccjsummary><values>0</values><numbers>0</numbers><datefrom>29/03/2012</datefrom><dateto>29/03/2015</dateto></ccjsummary><statushistorys><status><date>04/01/2006</date><text>Final Meeting of Creditor</text></status><status><date>06/12/2005</date><text>App of Receiver by a court</text></status><status><date>21/11/2005</date><text>Petitions Winding-Up (Gazette)</text></status><status><date>01/08/2005</date><text>Petitions Winding-Up (Gazette)</text></status><status><date>21/01/2003</date><text>Dissolution (First Gazt)</text></status></statushistorys><eventhistory><eventdetail><date>27/08/2014</date><text>Creditsafe Limit Refinement Removed</text></eventdetail><eventdetail><date>27/08/2014</date><text>Creditsafe Limit Refinement Removed</text></eventdetail><eventdetail><date>27/08/2014</date><text>Creditsafe Rating Refinement Removed</text></eventdetail><eventdetail><date>24/08/2014</date><text>Creditsafe Rating Refinement</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Rating Refinement Removed</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Limit Refinement Removed</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Rating Refinement</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Rating Refinement Removed</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Limit Refinement Removed</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Rating Refinement Removed</text></eventdetail><eventdetail><date>14/01/2013</date><text>Creditsafe Rating Refinement</text></eventdetail><eventdetail><date>04/01/2013</date><text>Creditsafe Rating Refinement</text></eventdetail><eventdetail><date>04/01/2013</date><text>Creditsafe Rating Refinement</text></eventdetail><eventdetail><date>04/01/2013</date><text>Creditsafe Rating Refinement</text></eventdetail></eventhistory><mortgages /><mortgagesummary><outstanding>0</outstanding><satisfied>0</satisfied></mortgagesummary><shareholders><shareholderdetails><name>Shareholder 1</name><shares>25,000 ORDINARY  GBP 1.00</shares><currency>GBP</currency></shareholderdetails><shareholderdetails><name>Shareholder 2</name><shares>15,000 ORDINARY  GBP 1.00</shares><currency>GBP</currency></shareholderdetails><shareholderdetails><name>Shareholder 3</name><shares>5,000 ORDINARY  GBP 1.00</shares><currency>GBP</currency></shareholderdetails><shareholderdetails><name>Shareholder 4</name><shares>5,000 ORDINARY  GBP 1.00</shares><currency>GBP</currency></shareholderdetails></shareholders><shareholdersummary><sharecapital>507745</sharecapital></shareholdersummary><financials><financial><period><datefrom>01/10/2009</datefrom><dateto>30/09/2010</dateto><periodmonths>12</periodmonths><currency>GBP</currency></period><profitloss><consolidatedaccounts>Y</consolidatedaccounts><turnover>3276000</turnover><export>3276000</export><costofsales>34000</costofsales><grossprofit>3242000</grossprofit><directorsemoluments>431000</directorsemoluments><operatingprofits>-272000</operatingprofits><depreciation>147000</depreciation><auditfees>85000</auditfees><interestpayments>104000</interestpayments><pretax>-340000</pretax><taxation>55000</taxation><posttax>-285000</posttax><dividendspayable>0</dividendspayable><retainedprofits>257000</retainedprofits><wagessalaries /></profitloss><balancesheet><tangibleassets>64000</tangibleassets><intangibleassets>1112000</intangibleassets><fixedassets>1176000</fixedassets><currentassets>1562000</currentassets><tradedebtors>389000</tradedebtors><stock>0</stock><cash>888000</cash><othercurrentassets>233000</othercurrentassets><increaseincash>-12132000</increaseincash><miscellaneouscurrentassets>52000</miscellaneouscurrentassets><totalassets>2738000</totalassets><totalcurrentliabilities>1742000</totalcurrentliabilities><tradecreditors>312000</tradecreditors><overdraft>0</overdraft><othershorttermfinance>0</othershorttermfinance><miscellaneouscurrentliabilities>1430000</miscellaneouscurrentliabilities><otherlongtermfinance>0</otherlongtermfinance><longtermliabilities>59000</longtermliabilities><overdraftlongtermliabilites>59000</overdraftlongtermliabilites><liabilities>1801000</liabilities><netassets>937000</netassets><workingcapital>-180000</workingcapital></balancesheet><capitalreserves><paidupequity>20936000</paidupequity><profitlossreserve>-71455000</profitlossreserve><sundryreserves>51456000</sundryreserves><revalutationreserve>0</revalutationreserve><reserves>-19999000</reserves><networth>-175000</networth><shareholderfunds>937000</shareholderfunds></capitalreserves><miscellaneous><netcashflowfromoperations>15000</netcashflowfromoperations><netcashflowbeforefinancing>92000</netcashflowbeforefinancing><netcashflowfromfinancing>-717000</netcashflowfromfinancing><contingentliability>Yes</contingentliability><capitalemployed>996000</capitalemployed><employees>74</employees><auditors>ERNST &amp; YOUNG LLP</auditors><auditqualification>Adverse Comments</auditqualification><bankers>BARCLAYS BANK PLC</bankers><bankbranchcode /></miscellaneous><ratios><pretaxmargin>-10.38 %</pretaxmargin><networkingcapital>-18.20</networkingcapital><gearingratio>6.30 %</gearingratio><equity>57.63 %</equity><creditordays>34.76</creditordays><debtordays>43.34</debtordays><liquidity>0.90</liquidity><returnoncapitalemployed>-34.14 %</returnoncapitalemployed><returnonassetsemployed>-12.42 %</returnonassetsemployed><currentratio>0.90</currentratio><totaldebtratio>1.92 %</totaldebtratio><stockturnoverratio>0.00 %</stockturnoverratio><returnonnetassetsemployed>-36.29 %</returnonnetassetsemployed><currentdebtratio>1.86 %</currentdebtratio></ratios></financial><financial><period><datefrom>01/01/2009</datefrom><dateto>31/12/2009</dateto><periodmonths>12</periodmonths><currency>GBP</currency></period><profitloss><consolidatedaccounts>N</consolidatedaccounts><turnover>286095000</turnover><export /><costofsales /><grossprofit /><directorsemoluments>1055000</directorsemoluments><operatingprofits /><depreciation>81000</depreciation><auditfees>92000</auditfees><interestpayments /><pretax>27459000</pretax><taxation>-2299000</taxation><posttax>25160000</posttax><dividendspayable>5120000</dividendspayable><retainedprofits>20040000</retainedprofits><wagessalaries /></profitloss><balancesheet><tangibleassets>529569000</tangibleassets><intangibleassets>-2472000</intangibleassets><fixedassets>527097000</fixedassets><currentassets>69620000</currentassets><tradedebtors>43609000</tradedebtors><stock>0</stock><cash>13020000</cash><othercurrentassets>4531000</othercurrentassets><increaseincash>-1318000</increaseincash><miscellaneouscurrentassets>8460000</miscellaneouscurrentassets><totalassets>596717000</totalassets><totalcurrentliabilities>98188000</totalcurrentliabilities><tradecreditors>17712000</tradecreditors><overdraft>0</overdraft><othershorttermfinance>5188000</othershorttermfinance><miscellaneouscurrentliabilities>75288000</miscellaneouscurrentliabilities><otherlongtermfinance>0</otherlongtermfinance><longtermliabilities>367167000</longtermliabilities><overdraftlongtermliabilites>367167000</overdraftlongtermliabilites><liabilities>465355000</liabilities><netassets>131362000</netassets><workingcapital>-28568000</workingcapital></balancesheet><capitalreserves><paidupequity>74213000</paidupequity><profitlossreserve>41973000</profitlossreserve><sundryreserves>15176000</sundryreserves><revalutationreserve>0</revalutationreserve><reserves>57149000</reserves><networth>133834000</networth><shareholderfunds>131362000</shareholderfunds></capitalreserves><miscellaneous><netcashflowfromoperations>0</netcashflowfromoperations><netcashflowbeforefinancing>0</netcashflowbeforefinancing><netcashflowfromfinancing>0</netcashflowfromfinancing><contingentliability>Yes</contingentliability><capitalemployed>498529000</capitalemployed><employees>88</employees><auditors>KPMG AUDIT PLC</auditors><auditqualification>No Adverse Comments</auditqualification><bankers>BARCLAYS BANK PLC</bankers><bankbranchcode /></miscellaneous><ratios><pretaxmargin>9.60 %</pretaxmargin><networkingcapital>-10.01</networkingcapital><gearingratio>279.51 %</gearingratio><equity>21.92 %</equity><creditordays>22.54</creditordays><debtordays>55.48</debtordays><liquidity>0.70</liquidity><returnoncapitalemployed>5.50 %</returnoncapitalemployed><returnonassetsemployed>4.60 %</returnonassetsemployed><currentratio>0.71</currentratio><totaldebtratio>3.54 %</totaldebtratio><stockturnoverratio>0.0%</stockturnoverratio><returnonnetassetsemployed>20.90 %</returnonnetassetsemployed><currentdebtratio>0.74 %</currentdebtratio></ratios></financial><financial><period><datefrom>01/01/2008</datefrom><dateto>31/12/2008</dateto><periodmonths>12</periodmonths><currency>GBP</currency></period><profitloss><consolidatedaccounts>N</consolidatedaccounts><turnover>381974000</turnover><export /><costofsales /><grossprofit /><directorsemoluments>1449000</directorsemoluments><operatingprofits /><depreciation /><auditfees>93000</auditfees><interestpayments /><pretax>31447000</pretax><taxation>-9621000</taxation><posttax>21826000</posttax><dividendspayable>20800000</dividendspayable><retainedprofits>1026000</retainedprofits><wagessalaries /></profitloss><balancesheet><tangibleassets>535589000</tangibleassets><intangibleassets>-2753000</intangibleassets><fixedassets>532836000</fixedassets><currentassets>69137000</currentassets><tradedebtors>37751000</tradedebtors><stock>0</stock><cash>14338000</cash><othercurrentassets>8496000</othercurrentassets><increaseincash>0</increaseincash><miscellaneouscurrentassets>8552000</miscellaneouscurrentassets><totalassets>601973000</totalassets><totalcurrentliabilities>125199000</totalcurrentliabilities><tradecreditors>15377000</tradecreditors><overdraft>0</overdraft><othershorttermfinance>0</othershorttermfinance><miscellaneouscurrentliabilities>109822000</miscellaneouscurrentliabilities><otherlongtermfinance>0</otherlongtermfinance><longtermliabilities>363071000</longtermliabilities><overdraftlongtermliabilites>363071000</overdraftlongtermliabilites><liabilities>488270000</liabilities><netassets>113703000</netassets><workingcapital>-56062000</workingcapital></balancesheet><capitalreserves><paidupequity>74213000</paidupequity><profitlossreserve>21934000</profitlossreserve><sundryreserves>17556000</sundryreserves><revalutationreserve>0</revalutationreserve><reserves>39490000</reserves><networth>116456000</networth><shareholderfunds>113703000</shareholderfunds></capitalreserves><miscellaneous><netcashflowfromoperations>0</netcashflowfromoperations><netcashflowbeforefinancing>0</netcashflowbeforefinancing><netcashflowfromfinancing>0</netcashflowfromfinancing><contingentliability>Yes</contingentliability><capitalemployed>476774000</capitalemployed><employees>66</employees><auditors>KPMG AUDIT PLC</auditors><auditqualification>No Adverse Comments</auditqualification><bankers>BARCLAYS BANK PLC</bankers><bankbranchcode /></miscellaneous><ratios><pretaxmargin>8.23 %</pretaxmargin><networkingcapital>-6.81</networkingcapital><gearingratio>319.32 %</gearingratio><equity>18.80 %</equity><creditordays>14.65</creditordays><debtordays>35.97</debtordays><liquidity>0.55</liquidity><returnoncapitalemployed>6.59 %</returnoncapitalemployed><returnonassetsemployed>5.22 %</returnonassetsemployed><currentratio>0.55</currentratio><totaldebtratio>4.29 %</totaldebtratio><stockturnoverratio>0.0%</stockturnoverratio><returnonnetassetsemployed>27.65 %</returnonnetassetsemployed><currentdebtratio>1.10 %</currentdebtratio></ratios></financial></financials><directors /></company></companies></body></xmlresponse>";
            
/*            CreditsafeServicesSoapClient client = new CreditsafeServicesSoapClient("CreditsafeServicesSoap");
            var builder = new CreditsafeRequestBuilder();
            var model = new CreditsafeRequestModel();

            model.UserName = "ORAN01";
            model.Password = "Jd4xDKpy";
            model.Operation = "GetCompanyInformation";
            model.ChargeReference = "";
            model.Package = "standard";
            model.Country = "UK";
            model.companynumber = "X9999999";

            string request = builder.GenerateRequestXML(model);
            string response = client.GetData(request);

            XmlSerializer serializer = new XmlSerializer(typeof(CreditSafeLtdResponse), new XmlRootAttribute("xmlresponse"));
            CreditSafeLtdResponse ei = (CreditSafeLtdResponse)serializer.Deserialize(new StringReader(response));
            CreditSafeLtdModelBuilder modelBuilder=new CreditSafeLtdModelBuilder();
            CreditSafeBaseData baseData = modelBuilder.Build(ei);


            Console.WriteLine(baseData.ID);*/

            //CreditSafeLtdGetData test = new CreditSafeLtdGetData();
            //test.LtdGetData("asd");
			//var test = new ServiceLogCreditSafeNonLtd(12614);
			//test.Execute();
        }

        [SetUp]
        public void Init()
        {
            Library.Initialize(this.oLog4NetCfg.Environment, this.m_oDB, this.m_oLog);
            ObjectFactory.Configure(x => x.For<IEzServiceAccessor>().Use<EzServiceAccessorShort>());
        }

        [Test]
        [Ignore]
        public void TestSaveToDB()
        {
           /* var builder = new CreditSafeLtdModelBuilder();
            var creditSafeSerializer = new XmlSerializer(typeof(CreditSafeLtdResponse), new XmlRootAttribute("xmlresponse"));
            var creditSafeResponse = (CreditSafeLtdResponse)creditSafeSerializer.Deserialize(new StringReader(response));
            var data = builder.Build(creditSafeResponse);
            data.ServiceLogID = 1;
            data.InsertDate=DateTime.UtcNow;
            data.AnnualReturnDate = data.AnnualReturnDate ?? DateTime.UtcNow;
            data.IncorporationDate = data.IncorporationDate ?? DateTime.UtcNow;
            data.AccountsFilingDate = data.AccountsFilingDate ?? DateTime.UtcNow;
            data.LatestAccountsDate = data.LatestAccountsDate ?? DateTime.UtcNow;
            data.CCJDateFrom = data.CCJDateFrom ?? DateTime.UtcNow;
            data.CCJDateTo = data.CCJDateTo ?? DateTime.UtcNow;*/
            //ParseCreditSafeLtd test = new ParseCreditSafeLtd(1);
            //test.Execute();
            //ParseCreditSafeLtd saveTest = new ParseCreditSafeLtd(1);
            //saveTest.Execute();

            /*var con = m_oDB.GetPersistent();
            con.BeginTransaction();
            var id = m_oDB.ExecuteScalar<long>(
                    con,
                    "SaveCreditSafeBaseData",
                    CommandSpecies.StoredProcedure,
                    m_oDB.CreateTableParameter<CreditSafeBaseData>("Tbl", new List<CreditSafeBaseData> { data })
                    );
            if (data.CreditRatings.Any())
            {
                foreach (var rating in data.CreditRatings)
                {
                    rating.CreditSafeBaseDataID = id;
                }
                m_oDB.ExecuteNonQuery(
                    con,
                    "SaveCreditSafeCreditRatings",
                    CommandSpecies.StoredProcedure,
                    m_oDB.CreateTableParameter<CreditSafeCreditRatings>("Tbl", data.CreditRatings)
                    );
            }
            con.Commit();*/
            var test = new SaleContract(model);
            test.Execute();
        }

        [Test]
        [Ignore]
        public void TestSaveToServiceLog() {
            var test = new ParseCreditSafeLtd(1);
            test.Execute();
            //var serviceLogTest = new CreditSafeService();
            //serviceLogTest.ServiceLogCreditSafeLtdData("X9999999", 46);
            //AConnection oDB=new SqlConnection();
            //EBusinessService ser = new EBusinessService(new SqlConnection());
            // ser.DownloadOneLimitedFromExperian("X9999999", 27);
        }
        [Test]
        [Ignore]
        public void BackFillTest()
        {
            //var serviceLogTest = new CreditSafeLtdService();
            //serviceLogTest.ServiceLogCreditSafeLtdData("X9999999", 46);
            //var test = new BackfillExperianLtdScoreText();
            //test.Execute();
            //AConnection oDB=new SqlConnection();
            //EBusinessService ser = new EBusinessService(new SqlConnection());
            // ser.DownloadOneLimitedFromExperian("X9999999", 27);
        }
        		private readonly AlibabaContractDto model = new AlibabaContractDto {
                requestId = "142857",
                responseId = "543",
                loanId = 24314474,
                orderNumber = "23456780",
                sellerBusinessName = "Dabao Trading Ltd.",
                sellerAliMemberId = "41523",
                sellerStreet1 = "128 Xihu Road",
                sellerCity = "Hangzhou",
                sellerState = "Zhejiang",
                sellerCountry = "China",
                sellerPostalCode = "310016",
                sellerAuthRepFname = "Hong",
                sellerAuthRepLname = "Zhang",
                sellerPhone = "865218526",
                sellerFax = "865218526",
                sellerEmail = "zhang.hong@163.com",
                buyerBusinessName = "A PLAZA DRIVING SCHOOL",
                aliMemberId = 131,
                aId = "01703638",
                buyerStreet1 = "926 E LEWELLING BLVD",
                buyerCity = "HAYWARD",
                buyerState = "CA",
                buyerCountry = "U.S.A",
                buyerZip = "94541",
                buyerAuthRepFname = "MICHAEL",
                buyerAuthRepLname = "ARTE",
                buyerPhone = "415222444",
                buyerEmail = "marte@aplaza.net",
                shippingMark = "WT12345678",
                totalOrderAmount = 88000,
                deviationQuantityAllowed = 20,
                orderAddtlDetails = "",
                shippingTerms = "asd",
                shippingDate = new DateTime(2015,03,01),
                loadingPort = "Shanghai",
                destinationPort = "Oakland,CA",
                orderDeposit = 1000,
                beneficiaryBank = "Bank of China",
                bankAccountNumber = 1234567890,
                bankStreetAddr1 = "108 Ganjiang Road",
                bankCity = "Hangzhou",
                bankState = "Zhejiang",
                bankCountry = "China",
                bankPostalCode = "310020",
                swiftcode = "ADBNCNBJCD1",
                orderCurrency = "gbp",
                orderItems = new OrderItems[] {
                    new OrderItems(){orderProdNumber = 0105, productName = "Battery", productSpecs = "AAA", productQuantity = 20000, productUnit = 6, productUnitPrice = 2, productTotalAmount = 48000},
                    new OrderItems(){orderProdNumber = 0106, productName = "Screw", productSpecs = "Phillips", productQuantity = 50000, productUnit = 100, productUnitPrice = 8, productTotalAmount = 40000}
                }
			};
        private const string response = @"<?xml version=""1.0""?>
<xmlresponse>
  <header>
    <reportinformation>
      <time>29/03/2015 13:26:59</time>
      <reporttype>getcompanyinformation</reporttype>
      <country>UK</country>
      <version>6.0</version>
      <provider>www.creditsafe.com</provider>
      <chargereference>
      </chargereference>
    </reportinformation>
  </header>
  <body>
    <reportid>X9999999</reportid>
    <reportname>TEST COMPANY LIMITED - THE</reportname>
    <companies>
      <company>
        <baseinformation>
          <number>X9999999</number>
          <name>TEST COMPANY LIMITED - THE</name>
          <telephone>02920886500</telephone>
          <tpsregistered>N</tpsregistered>
          <address1>BRYN HOUSE</address1>
          <address2>CAERPHILLY BUSINESS PARK</address2>
          <address3>VAN ROAD</address3>
          <address4>CARDIFF </address4>
          <postcode>CF83 3GG</postcode>
          <siccode>17401</siccode>
          <sicdescription>MANUFACTURE OF SOFT FURNISHINGS</sicdescription>
          <secondarysiccodes />
          <website>test.com</website>
          <companytype>Private limited with Share Capital</companytype>
          <accountstype>Group</accountstype>
          <annualreturndate>30/04/2007</annualreturndate>
          <incorporationdate>01/01/2004</incorporationdate>
          <accountsfilingdate>16/04/2011</accountsfilingdate>
          <latestaccountsdate>30/09/2010</latestaccountsdate>
          <quoted />
          <siccode2007>13921</siccode2007>
          <sicdescription2007>Manufacture of soft furnishings</sicdescription2007>
          <ftse />
          <charitynumber />
          <contractlimit>196500</contractlimit>
          <safenumber>UK07499628</safenumber>
        </baseinformation>
        <industries>
          <industry>
            <name>The underwriting of general insurance business in the united kingdom.</name>
          </industry>
        </industries>
        <tradingaddresses />
        <ratings>
          <ratingdetail>
            <date>24/08/2014</date>
            <score>50</score>
            <description>Moderate Risk</description>
          </ratingdetail>
          <ratingdetail>
            <date>14/01/2013</date>
            <score>-6</score>
            <description>Company is dissolved</description>
          </ratingdetail>
          <ratingdetail>
            <date>04/01/2013</date>
            <score>0</score>
            <description>Error - Object reference not set to an instance of an object.</description>
          </ratingdetail>
        </ratings>
        <limits>
          <limitdetail>
            <limit>28000</limit>
            <date>24/08/2014</date>
          </limitdetail>
          <limitdetail>
            <limit>0</limit>
            <date>14/01/2013</date>
          </limitdetail>
          <limitdetail>
            <limit>500</limit>
            <date>17/08/2011</date>
          </limitdetail>
        </limits>
        <previousnames>
          <previousname>
            <name>TEST COMPANY LIMITED</name>
            <date>24/09/1999</date>
          </previousname>
        </previousnames>
        <recordofpayments>
          <rop>
            <casenr>6XS01897</casenr>
            <ccjdate>28/03/2010</ccjdate>
            <ccjdatepaid />
            <court>SHEFFIELD</court>
            <ccjstatus>JG</ccjstatus>
            <ccjamount>2593</ccjamount>
            <incomingrecorddetails />
            <exact>yes</exact>
            <ccjtype>registered</ccjtype>
          </rop>
          <rop>
            <casenr>7DL00350</casenr>
            <ccjdate>01/03/2010</ccjdate>
            <ccjdatepaid>11/09/2010</ccjdatepaid>
            <court>DARLINGTON</court>
            <ccjstatus>SS</ccjstatus>
            <ccjamount>7426</ccjamount>
            <incomingrecorddetails />
            <exact>yes</exact>
            <ccjtype>registered</ccjtype>
          </rop>
        </recordofpayments>
        <ccjsummary>
          <values>0</values>
          <numbers>0</numbers>
          <datefrom>29/03/2012</datefrom>
          <dateto>29/03/2015</dateto>
        </ccjsummary>
        <statushistorys>
          <status>
            <date>04/01/2006</date>
            <text>Final Meeting of Creditor</text>
          </status>
          <status>
            <date>06/12/2005</date>
            <text>App of Receiver by a court</text>
          </status>
          <status>
            <date>21/11/2005</date>
            <text>Petitions Winding-Up (Gazette)</text>
          </status>
          <status>
            <date>01/08/2005</date>
            <text>Petitions Winding-Up (Gazette)</text>
          </status>
          <status>
            <date>21/01/2003</date>
            <text>Dissolution (First Gazt)</text>
          </status>
        </statushistorys>
        <eventhistory>
          <eventdetail>
            <date>27/08/2014</date>
            <text>Creditsafe Limit Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>27/08/2014</date>
            <text>Creditsafe Limit Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>27/08/2014</date>
            <text>Creditsafe Rating Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>24/08/2014</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Rating Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Limit Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Rating Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Limit Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Rating Refinement Removed</text>
          </eventdetail>
          <eventdetail>
            <date>14/01/2013</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
          <eventdetail>
            <date>04/01/2013</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
          <eventdetail>
            <date>04/01/2013</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
          <eventdetail>
            <date>04/01/2013</date>
            <text>Creditsafe Rating Refinement</text>
          </eventdetail>
        </eventhistory>
        <mortgages />
        <mortgagesummary>
          <outstanding>0</outstanding>
          <satisfied>0</satisfied>
        </mortgagesummary>
        <shareholders>
          <shareholderdetails>
            <name>Shareholder 1</name>
            <shares>25,000 ORDINARY  GBP 1.00</shares>
            <currency>GBP</currency>
          </shareholderdetails>
          <shareholderdetails>
            <name>Shareholder 2</name>
            <shares>15,000 ORDINARY  GBP 1.00</shares>
            <currency>GBP</currency>
          </shareholderdetails>
          <shareholderdetails>
            <name>Shareholder 3</name>
            <shares>5,000 ORDINARY  GBP 1.00</shares>
            <currency>GBP</currency>
          </shareholderdetails>
          <shareholderdetails>
            <name>Shareholder 4</name>
            <shares>5,000 ORDINARY  GBP 1.00</shares>
            <currency>GBP</currency>
          </shareholderdetails>
        </shareholders>
        <shareholdersummary>
          <sharecapital>507745</sharecapital>
        </shareholdersummary>
        <financials>
          <financial>
            <period>
              <datefrom>01/10/2009</datefrom>
              <dateto>30/09/2010</dateto>
              <periodmonths>12</periodmonths>
              <currency>GBP</currency>
            </period>
            <profitloss>
              <consolidatedaccounts>Y</consolidatedaccounts>
              <turnover>3276000</turnover>
              <export>3276000</export>
              <costofsales>34000</costofsales>
              <grossprofit>3242000</grossprofit>
              <directorsemoluments>431000</directorsemoluments>
              <operatingprofits>-272000</operatingprofits>
              <depreciation>147000</depreciation>
              <auditfees>85000</auditfees>
              <interestpayments>104000</interestpayments>
              <pretax>-340000</pretax>
              <taxation>55000</taxation>
              <posttax>-285000</posttax>
              <dividendspayable>0</dividendspayable>
              <retainedprofits>257000</retainedprofits>
              <wagessalaries />
            </profitloss>
            <balancesheet>
              <tangibleassets>64000</tangibleassets>
              <intangibleassets>1112000</intangibleassets>
              <fixedassets>1176000</fixedassets>
              <currentassets>1562000</currentassets>
              <tradedebtors>389000</tradedebtors>
              <stock>0</stock>
              <cash>888000</cash>
              <othercurrentassets>233000</othercurrentassets>
              <increaseincash>-12132000</increaseincash>
              <miscellaneouscurrentassets>52000</miscellaneouscurrentassets>
              <totalassets>2738000</totalassets>
              <totalcurrentliabilities>1742000</totalcurrentliabilities>
              <tradecreditors>312000</tradecreditors>
              <overdraft>0</overdraft>
              <othershorttermfinance>0</othershorttermfinance>
              <miscellaneouscurrentliabilities>1430000</miscellaneouscurrentliabilities>
              <otherlongtermfinance>0</otherlongtermfinance>
              <longtermliabilities>59000</longtermliabilities>
              <overdraftlongtermliabilites>59000</overdraftlongtermliabilites>
              <liabilities>1801000</liabilities>
              <netassets>937000</netassets>
              <workingcapital>-180000</workingcapital>
            </balancesheet>
            <capitalreserves>
              <paidupequity>20936000</paidupequity>
              <profitlossreserve>-71455000</profitlossreserve>
              <sundryreserves>51456000</sundryreserves>
              <revalutationreserve>0</revalutationreserve>
              <reserves>-19999000</reserves>
              <networth>-175000</networth>
              <shareholderfunds>937000</shareholderfunds>
            </capitalreserves>
            <miscellaneous>
              <netcashflowfromoperations>15000</netcashflowfromoperations>
              <netcashflowbeforefinancing>92000</netcashflowbeforefinancing>
              <netcashflowfromfinancing>-717000</netcashflowfromfinancing>
              <contingentliability>Yes</contingentliability>
              <capitalemployed>996000</capitalemployed>
              <employees>74</employees>
              <auditors>ERNST &amp; YOUNG LLP</auditors>
              <auditqualification>Adverse Comments</auditqualification>
              <bankers>BARCLAYS BANK PLC</bankers>
              <bankbranchcode />
            </miscellaneous>
            <ratios>
              <pretaxmargin>-10.38 %</pretaxmargin>
              <networkingcapital>-18.20</networkingcapital>
              <gearingratio>6.30 %</gearingratio>
              <equity>57.63 %</equity>
              <creditordays>34.76</creditordays>
              <debtordays>43.34</debtordays>
              <liquidity>0.90</liquidity>
              <returnoncapitalemployed>-34.14 %</returnoncapitalemployed>
              <returnonassetsemployed>-12.42 %</returnonassetsemployed>
              <currentratio>0.90</currentratio>
              <totaldebtratio>1.92 %</totaldebtratio>
              <stockturnoverratio>0.00 %</stockturnoverratio>
              <returnonnetassetsemployed>-36.29 %</returnonnetassetsemployed>
              <currentdebtratio>1.86 %</currentdebtratio>
            </ratios>
          </financial>
          <financial>
            <period>
              <datefrom>01/01/2009</datefrom>
              <dateto>31/12/2009</dateto>
              <periodmonths>12</periodmonths>
              <currency>GBP</currency>
            </period>
            <profitloss>
              <consolidatedaccounts>N</consolidatedaccounts>
              <turnover>286095000</turnover>
              <export />
              <costofsales />
              <grossprofit />
              <directorsemoluments>1055000</directorsemoluments>
              <operatingprofits />
              <depreciation>81000</depreciation>
              <auditfees>92000</auditfees>
              <interestpayments />
              <pretax>27459000</pretax>
              <taxation>-2299000</taxation>
              <posttax>25160000</posttax>
              <dividendspayable>5120000</dividendspayable>
              <retainedprofits>20040000</retainedprofits>
              <wagessalaries />
            </profitloss>
            <balancesheet>
              <tangibleassets>529569000</tangibleassets>
              <intangibleassets>-2472000</intangibleassets>
              <fixedassets>527097000</fixedassets>
              <currentassets>69620000</currentassets>
              <tradedebtors>43609000</tradedebtors>
              <stock>0</stock>
              <cash>13020000</cash>
              <othercurrentassets>4531000</othercurrentassets>
              <increaseincash>-1318000</increaseincash>
              <miscellaneouscurrentassets>8460000</miscellaneouscurrentassets>
              <totalassets>596717000</totalassets>
              <totalcurrentliabilities>98188000</totalcurrentliabilities>
              <tradecreditors>17712000</tradecreditors>
              <overdraft>0</overdraft>
              <othershorttermfinance>5188000</othershorttermfinance>
              <miscellaneouscurrentliabilities>75288000</miscellaneouscurrentliabilities>
              <otherlongtermfinance>0</otherlongtermfinance>
              <longtermliabilities>367167000</longtermliabilities>
              <overdraftlongtermliabilites>367167000</overdraftlongtermliabilites>
              <liabilities>465355000</liabilities>
              <netassets>131362000</netassets>
              <workingcapital>-28568000</workingcapital>
            </balancesheet>
            <capitalreserves>
              <paidupequity>74213000</paidupequity>
              <profitlossreserve>41973000</profitlossreserve>
              <sundryreserves>15176000</sundryreserves>
              <revalutationreserve>0</revalutationreserve>
              <reserves>57149000</reserves>
              <networth>133834000</networth>
              <shareholderfunds>131362000</shareholderfunds>
            </capitalreserves>
            <miscellaneous>
              <netcashflowfromoperations>0</netcashflowfromoperations>
              <netcashflowbeforefinancing>0</netcashflowbeforefinancing>
              <netcashflowfromfinancing>0</netcashflowfromfinancing>
              <contingentliability>Yes</contingentliability>
              <capitalemployed>498529000</capitalemployed>
              <employees>88</employees>
              <auditors>KPMG AUDIT PLC</auditors>
              <auditqualification>No Adverse Comments</auditqualification>
              <bankers>BARCLAYS BANK PLC</bankers>
              <bankbranchcode />
            </miscellaneous>
            <ratios>
              <pretaxmargin>9.60 %</pretaxmargin>
              <networkingcapital>-10.01</networkingcapital>
              <gearingratio>279.51 %</gearingratio>
              <equity>21.92 %</equity>
              <creditordays>22.54</creditordays>
              <debtordays>55.48</debtordays>
              <liquidity>0.70</liquidity>
              <returnoncapitalemployed>5.50 %</returnoncapitalemployed>
              <returnonassetsemployed>4.60 %</returnonassetsemployed>
              <currentratio>0.71</currentratio>
              <totaldebtratio>3.54 %</totaldebtratio>
              <stockturnoverratio>0.0%</stockturnoverratio>
              <returnonnetassetsemployed>20.90 %</returnonnetassetsemployed>
              <currentdebtratio>0.74 %</currentdebtratio>
            </ratios>
          </financial>
          <financial>
            <period>
              <datefrom>01/01/2008</datefrom>
              <dateto>31/12/2008</dateto>
              <periodmonths>12</periodmonths>
              <currency>GBP</currency>
            </period>
            <profitloss>
              <consolidatedaccounts>N</consolidatedaccounts>
              <turnover>381974000</turnover>
              <export />
              <costofsales />
              <grossprofit />
              <directorsemoluments>1449000</directorsemoluments>
              <operatingprofits />
              <depreciation />
              <auditfees>93000</auditfees>
              <interestpayments />
              <pretax>31447000</pretax>
              <taxation>-9621000</taxation>
              <posttax>21826000</posttax>
              <dividendspayable>20800000</dividendspayable>
              <retainedprofits>1026000</retainedprofits>
              <wagessalaries />
            </profitloss>
            <balancesheet>
              <tangibleassets>535589000</tangibleassets>
              <intangibleassets>-2753000</intangibleassets>
              <fixedassets>532836000</fixedassets>
              <currentassets>69137000</currentassets>
              <tradedebtors>37751000</tradedebtors>
              <stock>0</stock>
              <cash>14338000</cash>
              <othercurrentassets>8496000</othercurrentassets>
              <increaseincash>0</increaseincash>
              <miscellaneouscurrentassets>8552000</miscellaneouscurrentassets>
              <totalassets>601973000</totalassets>
              <totalcurrentliabilities>125199000</totalcurrentliabilities>
              <tradecreditors>15377000</tradecreditors>
              <overdraft>0</overdraft>
              <othershorttermfinance>0</othershorttermfinance>
              <miscellaneouscurrentliabilities>109822000</miscellaneouscurrentliabilities>
              <otherlongtermfinance>0</otherlongtermfinance>
              <longtermliabilities>363071000</longtermliabilities>
              <overdraftlongtermliabilites>363071000</overdraftlongtermliabilites>
              <liabilities>488270000</liabilities>
              <netassets>113703000</netassets>
              <workingcapital>-56062000</workingcapital>
            </balancesheet>
            <capitalreserves>
              <paidupequity>74213000</paidupequity>
              <profitlossreserve>21934000</profitlossreserve>
              <sundryreserves>17556000</sundryreserves>
              <revalutationreserve>0</revalutationreserve>
              <reserves>39490000</reserves>
              <networth>116456000</networth>
              <shareholderfunds>113703000</shareholderfunds>
            </capitalreserves>
            <miscellaneous>
              <netcashflowfromoperations>0</netcashflowfromoperations>
              <netcashflowbeforefinancing>0</netcashflowbeforefinancing>
              <netcashflowfromfinancing>0</netcashflowfromfinancing>
              <contingentliability>Yes</contingentliability>
              <capitalemployed>476774000</capitalemployed>
              <employees>66</employees>
              <auditors>KPMG AUDIT PLC</auditors>
              <auditqualification>No Adverse Comments</auditqualification>
              <bankers>BARCLAYS BANK PLC</bankers>
              <bankbranchcode />
            </miscellaneous>
            <ratios>
              <pretaxmargin>8.23 %</pretaxmargin>
              <networkingcapital>-6.81</networkingcapital>
              <gearingratio>319.32 %</gearingratio>
              <equity>18.80 %</equity>
              <creditordays>14.65</creditordays>
              <debtordays>35.97</debtordays>
              <liquidity>0.55</liquidity>
              <returnoncapitalemployed>6.59 %</returnoncapitalemployed>
              <returnonassetsemployed>5.22 %</returnonassetsemployed>
              <currentratio>0.55</currentratio>
              <totaldebtratio>4.29 %</totaldebtratio>
              <stockturnoverratio>0.0%</stockturnoverratio>
              <returnonnetassetsemployed>27.65 %</returnonnetassetsemployed>
              <currentdebtratio>1.10 %</currentdebtratio>
            </ratios>
          </financial>
        </financials>
        <directors />
      </company>
    </companies>
  </body>
</xmlresponse>";
    }
}

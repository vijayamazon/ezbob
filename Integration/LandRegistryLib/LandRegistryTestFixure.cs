namespace LandRegistryLib
{
	using System;
	using NUnit.Framework;

	[TestFixture]
	public class LandRegistryTestFixure
	{
		private static readonly LandRegistryApi Lr = new LandRegistryApi();
		private static readonly LandRegistryTestApi LrTest = new LandRegistryTestApi();
		[Test]
		public void test_prod_enquiry()
		{
			var model = Lr.EnquiryByPropertyDescription(buildingNumber: "27", streetName: "Church Road", cityName: "Exeter", customerId: 1);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_prod_res()
		{
			/*
			BM253452
			WYK874430
			HW153409
			SGL433128
			SGL410307
			SGL348466
			TGL70137
			*/
			var model = Lr.Res("BM253452", 2348);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_enquiry()
		{
			var model = LrTest.EnquiryByPropertyDescription(buildingNumber: "27", streetName: "Church Road", cityName: "Exeter", customerId: 1);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_enquiry_poll()
		{
			var model = LrTest.EnquiryByPropertyDescriptionPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res()
		{
			var model = LrTest.Res(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Success, model.ResponseType);
		}

		[Test]
		public void test_res_poll()
		{
			var model = LrTest.ResPoll(null);
			Assert.NotNull(model.Response);
			Assert.AreEqual(LandRegistryResponseType.Acknowledgement, model.ResponseType);
		}

		[Test]
		public void test_res_builder()
		{
			var responseBM253452 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResBM253452);
			var b = new LandRegistryModelBuilder();

			var enqModel = b.BuildEnquiryModel(TestEnquiry);
			Assert.IsNotNull(enqModel);
			Console.WriteLine(XmlHelper.SerializeObject(enqModel));

			var model = b.BuildResModel(responseBM253452);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));

			var responseHW153409 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResHW153409);
			model = b.BuildResModel(responseHW153409);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
			var responseSGL348466 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResSGL348466);
			model = b.BuildResModel(responseSGL348466);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
			var responseSGL410307 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResSGL410307);
			model = b.BuildResModel(responseSGL410307);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
			var responseSGL433128 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResSGL433128);
			model = b.BuildResModel(responseSGL433128);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
			var responseTGL70137 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResTGL70137);
			model = b.BuildResModel(responseTGL70137);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
			var responseWYK874430 = XmlHelper.XmlDeserializeFromString<LandRegistryLib.LRResServiceNS.ResponseOCWithSummaryV2_1Type>(TestResWYK874430);
			model = b.BuildResModel(responseWYK874430);
			Assert.IsNotNull(model);
			Console.WriteLine(XmlHelper.SerializeObject(model));
		}

		public const string TestEnquiry = @"<?xml version=""1.0""?>
<ResponseSearchByPropertyDescriptionV2_0Type xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseSearchByPropertyDescriptionV2_0"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>12345</Reference>
      </ExternalReference>
      <Title>
        <TitleNumber>GR518195</TitleNumber>
        <TenureInformation>
          <TenureTypeCode>20</TenureTypeCode>
        </TenureInformation>
        <Address>
          <BuildingNumber>27</BuildingNumber>
          <StreetName>CHURCH ROAD</StreetName>
          <CityName>EXETER</CityName>
          <PostcodeZone>
            <Postcode>EX56 4HY</Postcode>
          </PostcodeZone>
        </Address>
      </Title>
      <Title>
        <TitleNumber>GR518197</TitleNumber>
        <TenureInformation>
          <TenureTypeCode>20</TenureTypeCode>
        </TenureInformation>
        <Address>
          <BuildingNumber>27</BuildingNumber>
          <StreetName>CHURCH ROAD</StreetName>
          <CityName>EXETER</CityName>
          <PostcodeZone>
            <Postcode>EX56 4HY</Postcode>
          </PostcodeZone>
        </Address>
      </Title>
    </Results>
  </GatewayResponse>
</ResponseSearchByPropertyDescriptionV2_0Type>";

		public const string TestResBM253452 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2348</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:14:44</OfficialCopyDateTime>
        <EditionDate>2013-02-11</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
            <RegistrationDate>2013-02-11</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2013-02-06</Date>
              <Amount>£242,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>SE18 1HT</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>88 Benares Road</Line>
            <Line>London</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>SGL410307</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>GREENWICH</DistrictName>
            <AdministrativeArea>GREATER LONDON</AdministrativeArea>
            <LandRegistryOfficeName>Telford Office</LandRegistryOfficeName>
            <LatestEditionDate>2013-02-11</LatestEditionDate>
            <PostcodeZone>
              <Postcode>SE18 1HT</Postcode>
            </PostcodeZone>
            <RegistrationDate>1930-07-26</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2013-02-11</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Dario</ForenamesName>
                <SurnameName>Lopez</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1HT</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>88 Benares Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1NS</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>38 Rippolson Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                  <ChargeParty>The Mortgage Works (UK) PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2013-02-06</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>The Mortgage Works (UK) PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>SN38 1NW</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Nationwide House</Line>
                    <Line>Pipers Way</Line>
                    <Line>Swindon</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2013-02-06</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C3</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>GREENWICH</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1930-07-26</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 88 Benares Road, London (SE18 1HT).</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: DARIO LOPEZ of 88 Benares Road, London SE18 1HT and of 38 Rippolson Road, London SE18 1NS.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Transfer of the land in this title and other land dated 10 February 1965 made between (1) The Provost and Scholars of The Queen's College in the University of Oxford (The College) and (2) Mountview Estates Limited (Purchasers) contains the following covenants:-</EntryText>
            <EntryText>""THE Purchasers hereby covenant with the college for the benefit of the rest of the land in the Borough of Woolwich which belonged to the College on the 26 July 1930 (which land is hereinafter referred to as ""The Plumstead Estate"") and so that this covenant shall so far as practicable be enforceable by the College and the owners and occupiers for the time being of all other parts of the Plumstead Estate and shall run with the land hereby transferred but not so as to render the Purchasers personally liable in damage for any breach of this covenant committed after they shall have parted with all interest in the premises hereby transferred that they the Purchasers will at all times hereafter perform and observe the following stipulations and restrictions in relation to the said premises:-</EntryText>
            <EntryText>(a)  THAT they the Purchasers will at all times hereafter pay and allow a reasonable proportion for and towards the expenses of making supporting and repairing all or any roads paths pavements fences and party wall sewers and drains which now or at any time hereafter shall belong to the said premises or any part thereof in common with other messuages or tenements or lands.</EntryText>
            <EntryText>(b)  THAT they the Purchasers will not carry on or permit or suffer any noisy or offensive trade or business on the land hereby transferred nor permit or suffer the same or any part thereof to be used for the purposes of a public house or beershop nor permit or suffer anything thereon which may be or become a nuisance or annoyance to the college the owners and occupiers of the adjoining property or the neighbourhood.</EntryText>
            <EntryText>(c)  THAT they the Purchasers will not at any time hereafter erect or build upon the land hereby transfered or any part thereof any erection or building which may lessen the air or obstruct the light now enjoyed by the adjoining or neighbouring building.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Subjective Easements - C Register</EntryType>
            <EntryText>The land is subject to the following rights reserved by the Transfer dated 10 February 1965 referred to above:-</EntryText>
            <EntryText>""(1)  FULL right and liberty for the college or other owner or owners for the time being of the houses adjoining to the said premises and the tenants and occupiers of such adjoining houses with or without workmen and others at reasonable times in the day time to come into or upon the said premises or any part theoreof to repair such adjoining houses as often as occasion shall require.</EntryText>
            <EntryText>(2)  FULL right and liberty for the college or other the owner or owners for the time being of the houses which are or may be contiguous or near to the said premises and the tenants and occupiers thereof or watercourse and drainage in and through the said premises to carry off the water and drainage from such near or contiguous houses and for the tenants and occupiers of such near or contiguous houses to enter into and upon the said premises to empty and cleanse the cesspools gutters sewers and drains of and belonging to such near or contiguous houses making good all damage (if any) done thereby.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>
		";

		public const string TestResHW153409 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2346</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:12:34</OfficialCopyDateTime>
        <EditionDate>2008-08-06</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 25 July 2008 was £268,000.</EntryText>
            <RegistrationDate>2008-08-06</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2008-07-25</Date>
              <Amount>£268,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>WR4 0HJ</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>58 Marsh Avenue</Line>
            <Line>Long Meadow</Line>
            <Line>Worcester</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>HW153409</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>WORCESTER</DistrictName>
            <AdministrativeArea>WORCESTERSHIRE</AdministrativeArea>
            <LandRegistryOfficeName>Coventry Office</LandRegistryOfficeName>
            <LatestEditionDate>2008-08-06</LatestEditionDate>
            <PostcodeZone>
              <Postcode>WR4 0HJ</Postcode>
            </PostcodeZone>
            <RegistrationDate>1988-11-14</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2008-08-06</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>James Anthony Thomas</ForenamesName>
                <SurnameName>Flynn</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>WR4 0HJ</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>58 Marsh Avenue</Line>
                <Line>Long Meadow</Line>
                <Line>Worcester</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 25 July 2008 in favour of Barclays Bank PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2008-07-25</ChargeDate>
                  <ChargeParty>Barclays Bank PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2008-07-25</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 25 July 2008.</EntryText>
                <RegistrationDate>2008-08-06</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2008-07-25</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>Barclays Bank PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>LS11 1AN</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>P.O. Box 187</Line>
                    <Line>Leeds</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>5</EntryNumber>
                <EntryText>Proprietor: BARCLAYS BANK PLC (Co. Regn. No. 1026167) of P.O. Box 187, Leeds LS11 1AN.</EntryText>
                <RegistrationDate>2008-08-06</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2008-07-25</DocumentDate>
            <EntryNumber>B4</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <EntryNumber>C5</EntryNumber>
            <EntryNumber>C6</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>130</DocumentType>
            <DocumentDate>1995-03-30</DocumentDate>
            <EntryNumber>A2</EntryNumber>
            <EntryNumber>A3</EntryNumber>
            <EntryNumber>C3</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Transfer</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>WORCESTERSHIRE : WORCESTER</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1988-11-14</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 58 Marsh Avenue, Long Meadow, Worcester (WR4 0HJ).</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>1995-04-13</EntryDate>
            <EntryType>Beneficial/Subjective Easements - A Register</EntryType>
            <EntryText>The land has the benefit of the rights granted by but is subject to the rights reserved by the Transfer dated 30 March 1995 referred to in the Charges Register.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>1995-04-13</EntryDate>
            <EntryType>Provisions</EntryType>
            <EntryText>The Transfer dated 30 March 1995 referred to above contains a provision as to light or air.</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: JAMES ANTHONY THOMAS FLYNN of 58 Marsh Avenue, Long Meadow, Worcester WR4 0HJ.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 25 July 2008 was £268,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Personal Covenants</EntryType>
            <EntryText>The Transfer to the proprietor contains a covenant to observe and perform the covenants referred to in the Charges Register and of indemnity in respect thereof.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 25 July 2008 in favour of Barclays Bank PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Wayleave Agreement and Consent dated 13 February 1990 made betwen (1) Wimpey Homes Holdings Limited (Grantors) and (2) The Midlands Electricity Board (Board) relates to the erection laying and use of lines for the transmission and distribution of electricity and contains restrictive conditions.</EntryText>
            <EntryText>A copy of the material parts of the Agreement and Consent is set out below:-</EntryText>
            <EntryText>""The Grantor(s) hereby give the Board full and free licence and liberty and consent for the Board its servants workmen and others authorised by them to erect and or lay and use and thereafter from time to time repair inspect and maintain re-erect re-lay and remove electric lines either overhead or under ground as the Board shall require for the transmission and distribution of electricity and the necessary service turrets poles stays ducts pipes and other apparatus appurtenant thereto (herein collectively referred to as ""the said electric lines"")  (the right hereby granted to include the right to erect and or lay additional apparatus to that originally erected and laid in contradistinction from and in addition to the right already given to replace apparatus) over on and or under the said land and in the position and along the route approximately shown by green lines on the plan Nod. SO8656 annexed hereto and for any of the purposes aforesaid to enter upon the said land to execute all or any of such works as aforesaid and to break up and excavate so much of the said land as may from time to time be necessary and remove and dispose of any surplus earth PROVIDED that in so doing the Board shall cause as little damage as may be to the said land and shall so far as practicable make good and restore the surface thereof.</EntryText>
            <EntryText>2. THE Board hereby AGREES with the Grantor(s) as follows:-</EntryText>
            <EntryText>(a) To make good to the reasonable satisfaction of the Grantor(s) all damage as may be to the said land as is occasioned by the exercise of the rights licensed by this Agreement</EntryText>
            <EntryText>3. THE Grantor(s) hereby AGREE with the Board:-</EntryText>
            <EntryText>(i) That they will not erect or permit to be erected any building or erection of any kind whatsoever or plant any trees under over or in close proximity to the said electric lines without first obtaining the prior approval of the Board such approval not to be unreasonably withheld</EntryText>
            <EntryText>(ii) Not to raise or lower the level of the said land which would in any way affect the rights hereby Licensed</EntryText>
            <EntryText>(iii) That they will on any sale lease or other disposition of the said land or any part thereof sell lease or dispose of such land subject to this agreement</EntryText>
            <EntryText>4. THIS Agreement shall remain in force for a term of 99 years computed from the date hereof and shall continue thereafter from year to year until determined by either party giving to the other six months notice in writing.""</EntryText>
            <EntryText>NOTE: The said land referred to includes the land in this title. The green lines referred to do not affect the land in this title.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>An Agreement and Consent dated 21 February 1992 made between (1) Tarmac Homes Midlands Limited (Grantor(s)) and (2) Midlands Electricity PLC (Company) relates to the erection laying and use of lines for the transmission and distribution of electricity and contains restrictive conditions.</EntryText>
            <EntryText>A Copy of the material parts of the Agreement and consent is set out below:</EntryText>
            <EntryText>""The Grantor(s) hereby give(s) the Company full and free licence and liberty and consent for the Company their servants workmen and others authorised by them to erect and or lay and use and thereafter from time to time repair inspect and maintain re-erect re-lay and remove electric lines either overhead or underground as the Company shall require for the transmission and distribution of electricity and the necessary service turrets poles stays ducts pipes and other apparatus appurtenant thereto (herein collectively referred to as ""the said electric lines"") (the right hereby granted to include the right to erect and or lay additional apparatus to that originally erected and laid in contradistinction from and in addition to the right already given to replace apparatus) over on or under the said land And in the position and along the route appoximately shown by broken green lines on the plan numbered SO8656 annexed hereto and for any of the purposes aforesaid to enter upon the said land to execute all or any of such works as aforesaid and to break up and excavate so much of the said land as may from time to time be necessary and remove and dispose of any surplus earth PROVIDED that in so doing the Company shall cause as little damage as may be to the said land and shall so far as practicable make good and restore the surface thereof</EntryText>
            <EntryText>THE Company hereby AGREE with the Grantor(s) as follows:-</EntryText>
            <EntryText>(a) To make good to the reasonable satisfaction of the Grantor(s) all damage as may be to the said land as is occasioned by the exercise of the rights licensed by this Agreement</EntryText>
            <EntryText>THE Grantor(s) hereby AGREE(S) with the Company:-</EntryText>
            <EntryText>(i) That he/she/they will not erect or permit to be erected any building or erection of any kind whatsoever or plant any trees under over or in close proximity to the said electric lines without first obtaining the prior approval of the Company such approval not to be unreasonably withheld</EntryText>
            <EntryText>(ii) Not to raise or lower the level of the said land which would in any way affect the rights hereby licensed</EntryText>
            <EntryText>(iii) That he/she/they will on any sale lease or other disposition of the said land or any part thereof sell lease or dispose of such land subject to this agreement</EntryText>
            <EntryText>THIS Agreement shall remain in force for a term of 99 years computed from the date hereof and shall continue thereafter from year to year until determined by either party giving to the other six months notice in writing.""</EntryText>
            <EntryText>NOTE: The said land referred to includes the land in this title</EntryText>
            <EntryText>The green lines referred to do not affect the land in this title.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>1995-04-13</EntryDate>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Transfer of the land in this title dated 30 March 1995 made between (1) Tarmac Homes Midlands Limited and Midland &amp; General Developments Limited and (2) Derek Murray and carol Murray contains restrictive covenants.</EntryText>
            <EntryText>NOTE: Original filed.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 25 July 2008.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>5</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: BARCLAYS BANK PLC (Co. Regn. No. 1026167) of P.O. Box 187, Leeds LS11 1AN.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>6</EntryNumber>
            <EntryDate>2008-08-06</EntryDate>
            <EntryType>Obligation</EntryType>
            <EntryText>The proprietor of the Charge dated 25 July 2008 referred to above is under an obligation to make further advances. These advances will have priority to the extent afforded by section 49(3) Land Registration Act 2002.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";
		public const string TestResSGL348466 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2348</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:14:44</OfficialCopyDateTime>
        <EditionDate>2013-02-11</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
            <RegistrationDate>2013-02-11</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2013-02-06</Date>
              <Amount>£242,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>SE18 1HT</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>88 Benares Road</Line>
            <Line>London</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>SGL410307</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>GREENWICH</DistrictName>
            <AdministrativeArea>GREATER LONDON</AdministrativeArea>
            <LandRegistryOfficeName>Telford Office</LandRegistryOfficeName>
            <LatestEditionDate>2013-02-11</LatestEditionDate>
            <PostcodeZone>
              <Postcode>SE18 1HT</Postcode>
            </PostcodeZone>
            <RegistrationDate>1930-07-26</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2013-02-11</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Dario</ForenamesName>
                <SurnameName>Lopez</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1HT</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>88 Benares Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1NS</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>38 Rippolson Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                  <ChargeParty>The Mortgage Works (UK) PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2013-02-06</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>The Mortgage Works (UK) PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>SN38 1NW</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Nationwide House</Line>
                    <Line>Pipers Way</Line>
                    <Line>Swindon</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2013-02-06</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C3</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>GREENWICH</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1930-07-26</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 88 Benares Road, London (SE18 1HT).</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: DARIO LOPEZ of 88 Benares Road, London SE18 1HT and of 38 Rippolson Road, London SE18 1NS.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Transfer of the land in this title and other land dated 10 February 1965 made between (1) The Provost and Scholars of The Queen's College in the University of Oxford (The College) and (2) Mountview Estates Limited (Purchasers) contains the following covenants:-</EntryText>
            <EntryText>""THE Purchasers hereby covenant with the college for the benefit of the rest of the land in the Borough of Woolwich which belonged to the College on the 26 July 1930 (which land is hereinafter referred to as ""The Plumstead Estate"") and so that this covenant shall so far as practicable be enforceable by the College and the owners and occupiers for the time being of all other parts of the Plumstead Estate and shall run with the land hereby transferred but not so as to render the Purchasers personally liable in damage for any breach of this covenant committed after they shall have parted with all interest in the premises hereby transferred that they the Purchasers will at all times hereafter perform and observe the following stipulations and restrictions in relation to the said premises:-</EntryText>
            <EntryText>(a)  THAT they the Purchasers will at all times hereafter pay and allow a reasonable proportion for and towards the expenses of making supporting and repairing all or any roads paths pavements fences and party wall sewers and drains which now or at any time hereafter shall belong to the said premises or any part thereof in common with other messuages or tenements or lands.</EntryText>
            <EntryText>(b)  THAT they the Purchasers will not carry on or permit or suffer any noisy or offensive trade or business on the land hereby transferred nor permit or suffer the same or any part thereof to be used for the purposes of a public house or beershop nor permit or suffer anything thereon which may be or become a nuisance or annoyance to the college the owners and occupiers of the adjoining property or the neighbourhood.</EntryText>
            <EntryText>(c)  THAT they the Purchasers will not at any time hereafter erect or build upon the land hereby transfered or any part thereof any erection or building which may lessen the air or obstruct the light now enjoyed by the adjoining or neighbouring building.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Subjective Easements - C Register</EntryType>
            <EntryText>The land is subject to the following rights reserved by the Transfer dated 10 February 1965 referred to above:-</EntryText>
            <EntryText>""(1)  FULL right and liberty for the college or other owner or owners for the time being of the houses adjoining to the said premises and the tenants and occupiers of such adjoining houses with or without workmen and others at reasonable times in the day time to come into or upon the said premises or any part theoreof to repair such adjoining houses as often as occasion shall require.</EntryText>
            <EntryText>(2)  FULL right and liberty for the college or other the owner or owners for the time being of the houses which are or may be contiguous or near to the said premises and the tenants and occupiers thereof or watercourse and drainage in and through the said premises to carry off the water and drainage from such near or contiguous houses and for the tenants and occupiers of such near or contiguous houses to enter into and upon the said premises to empty and cleanse the cesspools gutters sewers and drains of and belonging to such near or contiguous houses making good all damage (if any) done thereby.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";
		public const string TestResSGL410307 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2348</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:14:44</OfficialCopyDateTime>
        <EditionDate>2013-02-11</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
            <RegistrationDate>2013-02-11</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2013-02-06</Date>
              <Amount>£242,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>SE18 1HT</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>88 Benares Road</Line>
            <Line>London</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>SGL410307</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>GREENWICH</DistrictName>
            <AdministrativeArea>GREATER LONDON</AdministrativeArea>
            <LandRegistryOfficeName>Telford Office</LandRegistryOfficeName>
            <LatestEditionDate>2013-02-11</LatestEditionDate>
            <PostcodeZone>
              <Postcode>SE18 1HT</Postcode>
            </PostcodeZone>
            <RegistrationDate>1930-07-26</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2013-02-11</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Dario</ForenamesName>
                <SurnameName>Lopez</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1HT</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>88 Benares Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1NS</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>38 Rippolson Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                  <ChargeParty>The Mortgage Works (UK) PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2013-02-06</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>The Mortgage Works (UK) PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>SN38 1NW</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Nationwide House</Line>
                    <Line>Pipers Way</Line>
                    <Line>Swindon</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2013-02-06</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C3</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>GREENWICH</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1930-07-26</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 88 Benares Road, London (SE18 1HT).</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: DARIO LOPEZ of 88 Benares Road, London SE18 1HT and of 38 Rippolson Road, London SE18 1NS.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Transfer of the land in this title and other land dated 10 February 1965 made between (1) The Provost and Scholars of The Queen's College in the University of Oxford (The College) and (2) Mountview Estates Limited (Purchasers) contains the following covenants:-</EntryText>
            <EntryText>""THE Purchasers hereby covenant with the college for the benefit of the rest of the land in the Borough of Woolwich which belonged to the College on the 26 July 1930 (which land is hereinafter referred to as ""The Plumstead Estate"") and so that this covenant shall so far as practicable be enforceable by the College and the owners and occupiers for the time being of all other parts of the Plumstead Estate and shall run with the land hereby transferred but not so as to render the Purchasers personally liable in damage for any breach of this covenant committed after they shall have parted with all interest in the premises hereby transferred that they the Purchasers will at all times hereafter perform and observe the following stipulations and restrictions in relation to the said premises:-</EntryText>
            <EntryText>(a)  THAT they the Purchasers will at all times hereafter pay and allow a reasonable proportion for and towards the expenses of making supporting and repairing all or any roads paths pavements fences and party wall sewers and drains which now or at any time hereafter shall belong to the said premises or any part thereof in common with other messuages or tenements or lands.</EntryText>
            <EntryText>(b)  THAT they the Purchasers will not carry on or permit or suffer any noisy or offensive trade or business on the land hereby transferred nor permit or suffer the same or any part thereof to be used for the purposes of a public house or beershop nor permit or suffer anything thereon which may be or become a nuisance or annoyance to the college the owners and occupiers of the adjoining property or the neighbourhood.</EntryText>
            <EntryText>(c)  THAT they the Purchasers will not at any time hereafter erect or build upon the land hereby transfered or any part thereof any erection or building which may lessen the air or obstruct the light now enjoyed by the adjoining or neighbouring building.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Subjective Easements - C Register</EntryType>
            <EntryText>The land is subject to the following rights reserved by the Transfer dated 10 February 1965 referred to above:-</EntryText>
            <EntryText>""(1)  FULL right and liberty for the college or other owner or owners for the time being of the houses adjoining to the said premises and the tenants and occupiers of such adjoining houses with or without workmen and others at reasonable times in the day time to come into or upon the said premises or any part theoreof to repair such adjoining houses as often as occasion shall require.</EntryText>
            <EntryText>(2)  FULL right and liberty for the college or other the owner or owners for the time being of the houses which are or may be contiguous or near to the said premises and the tenants and occupiers thereof or watercourse and drainage in and through the said premises to carry off the water and drainage from such near or contiguous houses and for the tenants and occupiers of such near or contiguous houses to enter into and upon the said premises to empty and cleanse the cesspools gutters sewers and drains of and belonging to such near or contiguous houses making good all damage (if any) done thereby.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";
		public const string TestResSGL433128 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2347</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:13:45</OfficialCopyDateTime>
        <EditionDate>2012-04-12</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 2 April 2012 was £190,000.</EntryText>
            <RegistrationDate>2012-04-04</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2012-04-02</Date>
              <Amount>£190,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>SE18 1EW</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>32 Brookdene Road</Line>
            <Line>London</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>SGL433128</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>GREENWICH</DistrictName>
            <AdministrativeArea>GREATER LONDON</AdministrativeArea>
            <LandRegistryOfficeName>Telford Office</LandRegistryOfficeName>
            <LatestEditionDate>2012-04-12</LatestEditionDate>
            <PostcodeZone>
              <Postcode>SE18 1EW</Postcode>
            </PostcodeZone>
            <RegistrationDate>1959-12-08</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2012-04-04</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Dario</ForenamesName>
                <SurnameName>Lopez</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1EW</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>32 Brookdene Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1NS</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>38 Rippolson Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate or by the proprietor of any registered charge, not being a charge registered before the entry of this restriction, is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 April 2012 in favour of Ipswich Building Society referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2012-04-02</ChargeDate>
                  <ChargeParty>Ipswich Building Society</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2012-04-02</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>1</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 2 April 2012.</EntryText>
                <RegistrationDate>2012-04-04</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2012-04-02</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>Ipswich Building Society</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>IP3 9WZ</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>P O Box 547</Line>
                    <Line>Freehold House</Line>
                    <Line>The Havens</Line>
                    <Line>Ipswich</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>2</EntryNumber>
                <EntryText>Proprietor: IPSWICH BUILDING SOCIETY of P O Box 547, Freehold House, The Havens, Ipswich IP3 9WZ.</EntryText>
                <RegistrationDate>2012-04-04</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2012-04-02</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C1</EntryNumber>
            <EntryNumber>C2</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>GREENWICH</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1959-12-08</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 32 Brookdene Road, London (SE18 1EW).</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2012-04-04</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: DARIO LOPEZ of 32 Brookdene Road, London SE18 1EW and of 38 Rippolson Road, London SE18 1NS.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2012-04-04</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 2 April 2012 was £190,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2012-04-04</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate or by the proprietor of any registered charge, not being a charge registered before the entry of this restriction, is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 April 2012 in favour of Ipswich Building Society referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2012-04-04</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 2 April 2012.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2012-04-04</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: IPSWICH BUILDING SOCIETY of P O Box 547, Freehold House, The Havens, Ipswich IP3 9WZ.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";
		public const string TestResTGL70137 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2348</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:14:44</OfficialCopyDateTime>
        <EditionDate>2013-02-11</EditionDate>
        <PricePaidEntry>
          <EntryDetails>
            <EntryNumber>2</EntryNumber>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
            <RegistrationDate>2013-02-11</RegistrationDate>
            <SubRegisterCode>B</SubRegisterCode>
            <Infills>
              <Date>2013-02-06</Date>
              <Amount>£242,000</Amount>
            </Infills>
          </EntryDetails>
        </PricePaidEntry>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>SE18 1HT</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>88 Benares Road</Line>
            <Line>London</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>SGL410307</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>GREENWICH</DistrictName>
            <AdministrativeArea>GREATER LONDON</AdministrativeArea>
            <LandRegistryOfficeName>Telford Office</LandRegistryOfficeName>
            <LatestEditionDate>2013-02-11</LatestEditionDate>
            <PostcodeZone>
              <Postcode>SE18 1HT</Postcode>
            </PostcodeZone>
            <RegistrationDate>1930-07-26</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>false</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>true</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>false</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2013-02-11</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Dario</ForenamesName>
                <SurnameName>Lopez</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1HT</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>88 Benares Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
            <Address>
              <PostcodeZone>
                <Postcode>SE18 1NS</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>38 Rippolson Road</Line>
                <Line>London</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                  <ChargeParty>The Mortgage Works (UK) PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2013-02-06</ChargeDate>
            <RegisteredCharge>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-02-06</ChargeDate>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>The Mortgage Works (UK) PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>SN38 1NW</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Nationwide House</Line>
                    <Line>Pipers Way</Line>
                    <Line>Swindon</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
                <RegistrationDate>2013-02-11</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2013-02-06</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C3</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>GREENWICH</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1930-07-26</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above Title filed at the Registry and being 88 Benares Road, London (SE18 1HT).</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: DARIO LOPEZ of 88 Benares Road, London SE18 1HT and of 38 Rippolson Road, London SE18 1NS.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Price Paid</EntryType>
            <EntryText>The price stated to have been paid on 6 February 2013 was £242,000.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 6 February 2013 in favour of The Mortgage Works (UK) PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Transfer of the land in this title and other land dated 10 February 1965 made between (1) The Provost and Scholars of The Queen's College in the University of Oxford (The College) and (2) Mountview Estates Limited (Purchasers) contains the following covenants:-</EntryText>
            <EntryText>""THE Purchasers hereby covenant with the college for the benefit of the rest of the land in the Borough of Woolwich which belonged to the College on the 26 July 1930 (which land is hereinafter referred to as ""The Plumstead Estate"") and so that this covenant shall so far as practicable be enforceable by the College and the owners and occupiers for the time being of all other parts of the Plumstead Estate and shall run with the land hereby transferred but not so as to render the Purchasers personally liable in damage for any breach of this covenant committed after they shall have parted with all interest in the premises hereby transferred that they the Purchasers will at all times hereafter perform and observe the following stipulations and restrictions in relation to the said premises:-</EntryText>
            <EntryText>(a)  THAT they the Purchasers will at all times hereafter pay and allow a reasonable proportion for and towards the expenses of making supporting and repairing all or any roads paths pavements fences and party wall sewers and drains which now or at any time hereafter shall belong to the said premises or any part thereof in common with other messuages or tenements or lands.</EntryText>
            <EntryText>(b)  THAT they the Purchasers will not carry on or permit or suffer any noisy or offensive trade or business on the land hereby transferred nor permit or suffer the same or any part thereof to be used for the purposes of a public house or beershop nor permit or suffer anything thereon which may be or become a nuisance or annoyance to the college the owners and occupiers of the adjoining property or the neighbourhood.</EntryText>
            <EntryText>(c)  THAT they the Purchasers will not at any time hereafter erect or build upon the land hereby transfered or any part thereof any erection or building which may lessen the air or obstruct the light now enjoyed by the adjoining or neighbouring building.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Subjective Easements - C Register</EntryType>
            <EntryText>The land is subject to the following rights reserved by the Transfer dated 10 February 1965 referred to above:-</EntryText>
            <EntryText>""(1)  FULL right and liberty for the college or other owner or owners for the time being of the houses adjoining to the said premises and the tenants and occupiers of such adjoining houses with or without workmen and others at reasonable times in the day time to come into or upon the said premises or any part theoreof to repair such adjoining houses as often as occasion shall require.</EntryText>
            <EntryText>(2)  FULL right and liberty for the college or other the owner or owners for the time being of the houses which are or may be contiguous or near to the said premises and the tenants and occupiers thereof or watercourse and drainage in and through the said premises to carry off the water and drainage from such near or contiguous houses and for the tenants and occupiers of such near or contiguous houses to enter into and upon the said premises to empty and cleanse the cesspools gutters sewers and drains of and belonging to such near or contiguous houses making good all damage (if any) done thereby.""</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 6 February 2013.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2013-02-11</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: THE MORTGAGE WORKS (UK) PLC (Co. Regn. No. 2222856) of Nationwide House, Pipers Way, Swindon SN38 1NW.</EntryText>
          </RegisterEntry>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";
		public const string TestResWYK874430 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<ResponseOCWithSummaryV2_1Type xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"">
  <GatewayResponse xmlns=""http://www.oscre.org/ns/eReg-Final/2011/ResponseOCWithSummaryV2_1"">
    <TypeCode>30</TypeCode>
    <Results>
      <ExternalReference>
        <Reference>ezbob2345</Reference>
      </ExternalReference>
      <ActualPrice>
        <GrossPriceAmount>6.00</GrossPriceAmount>
      </ActualPrice>
      <OCSummaryData>
        <OfficialCopyDateTime>2014-02-02T08:09:41</OfficialCopyDateTime>
        <EditionDate>2013-10-10</EditionDate>
        <PropertyAddress>
          <PostcodeZone>
            <Postcode>WF9 5EY</Postcode>
          </PostcodeZone>
          <AddressLine>
            <Line>39 Wakefield Road</Line>
            <Line>Kinsley</Line>
            <Line>Pontefract</Line>
          </AddressLine>
        </PropertyAddress>
        <Title>
          <TitleNumber>WYK874430</TitleNumber>
          <ClassOfTitleCode>10</ClassOfTitleCode>
          <CommonholdIndicator>false</CommonholdIndicator>
          <TitleRegistrationDetails>
            <DistrictName>WAKEFIELD</DistrictName>
            <AdministrativeArea>WEST YORKSHIRE</AdministrativeArea>
            <LandRegistryOfficeName>Nottingham Office</LandRegistryOfficeName>
            <LatestEditionDate>2013-10-10</LatestEditionDate>
            <PostcodeZone>
              <Postcode>WF9 5EY</Postcode>
            </PostcodeZone>
            <RegistrationDate>1980-10-14</RegistrationDate>
          </TitleRegistrationDetails>
        </Title>
        <RegisterEntryIndicators>
          <AgreedNoticeIndicator>false</AgreedNoticeIndicator>
          <BankruptcyIndicator>false</BankruptcyIndicator>
          <CautionIndicator>false</CautionIndicator>
          <CCBIIndicator>false</CCBIIndicator>
          <ChargeeIndicator>true</ChargeeIndicator>
          <ChargeIndicator>true</ChargeIndicator>
          <ChargeRelatedRestrictionIndicator>false</ChargeRelatedRestrictionIndicator>
          <ChargeRestrictionIndicator>true</ChargeRestrictionIndicator>
          <CreditorsNoticeIndicator>false</CreditorsNoticeIndicator>
          <DeathOfProprietorIndicator>false</DeathOfProprietorIndicator>
          <DeedOfPostponementIndicator>false</DeedOfPostponementIndicator>
          <DiscountChargeIndicator>false</DiscountChargeIndicator>
          <EquitableChargeIndicator>false</EquitableChargeIndicator>
          <GreenOutEntryIndicator>false</GreenOutEntryIndicator>
          <HomeRightsChangeOfAddressIndicator>false</HomeRightsChangeOfAddressIndicator>
          <HomeRightsIndicator>false</HomeRightsIndicator>
          <LeaseHoldTitleIndicator>false</LeaseHoldTitleIndicator>
          <MultipleChargeIndicator>true</MultipleChargeIndicator>
          <NonChargeRestrictionIndicator>false</NonChargeRestrictionIndicator>
          <NotedChargeIndicator>false</NotedChargeIndicator>
          <PricePaidIndicator>false</PricePaidIndicator>
          <PropertyDescriptionNotesIndicator>false</PropertyDescriptionNotesIndicator>
          <RentChargeIndicator>false</RentChargeIndicator>
          <RightOfPreEmptionIndicator>false</RightOfPreEmptionIndicator>
          <ScheduleOfLeasesIndicator>true</ScheduleOfLeasesIndicator>
          <SubChargeIndicator>false</SubChargeIndicator>
          <UnidentifiedEntryIndicator>false</UnidentifiedEntryIndicator>
          <UnilateralNoticeBeneficiaryIndicator>false</UnilateralNoticeBeneficiaryIndicator>
          <UnilateralNoticeIndicator>false</UnilateralNoticeIndicator>
          <VendorsLienIndicator>false</VendorsLienIndicator>
        </RegisterEntryIndicators>
        <Proprietorship>
          <CurrentProprietorshipDate>2009-04-08</CurrentProprietorshipDate>
          <RegisteredProprietorParty>
            <PrivateIndividual>
              <Name>
                <ForenamesName>Jason Craig</ForenamesName>
                <SurnameName>Begg</SurnameName>
              </Name>
            </PrivateIndividual>
            <Address>
              <PostcodeZone>
                <Postcode>WF9 5EY</Postcode>
              </PostcodeZone>
              <AddressLine>
                <Line>39 Wakefield Road</Line>
                <Line>Kinsley</Line>
                <Line>Pontefract</Line>
              </AddressLine>
            </Address>
          </RegisteredProprietorParty>
        </Proprietorship>
        <RestrictionDetails>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>1</ChargeID>
              <EntryDetails>
                <EntryNumber>3</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 February 2011 in favour of Santander UK PLC referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2011-02-02</ChargeDate>
                  <ChargeParty>Santander UK PLC</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
          <RestrictionEntry>
            <ChargeRestriction>
              <RestrictionTypeCode>30</RestrictionTypeCode>
              <ChargeID>2</ChargeID>
              <EntryDetails>
                <EntryNumber>5</EntryNumber>
                <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 October 2013 in favour of Nemo Personal Finance Limited referred to in the Charges Register.</EntryText>
                <SubRegisterCode>B</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-10-02</ChargeDate>
                  <ChargeParty>Nemo Personal Finance Limited</ChargeParty>
                </Infills>
              </EntryDetails>
            </ChargeRestriction>
          </RestrictionEntry>
        </RestrictionDetails>
        <Charge>
          <ChargeEntry>
            <ChargeID>1</ChargeID>
            <ChargeDate>2011-02-02</ChargeDate>
            <RegisteredCharge>
              <MultipleTitleIndicator>2</MultipleTitleIndicator>
              <EntryDetails>
                <EntryNumber>4</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 2 February 2011 affecting also title WYK267589.</EntryText>
                <RegistrationDate>2011-02-18</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2011-02-02</ChargeDate>
                  <TitleNumber>WYK267589</TitleNumber>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>Santander UK PLC</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>MK9 1AA</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Deeds Services</Line>
                    <Line>101 Midsummer Boulevard</Line>
                    <Line>Milton Keynes</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>5</EntryNumber>
                <EntryText>Proprietor: SANTANDER UK PLC (Co. Regn. No. 2294747) of Deeds Services, 101 Midsummer Boulevard, Milton Keynes MK9 1AA.</EntryText>
                <RegistrationDate>2011-02-18</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
          <ChargeEntry>
            <ChargeID>2</ChargeID>
            <ChargeDate>2013-10-02</ChargeDate>
            <RegisteredCharge>
              <MultipleTitleIndicator>2</MultipleTitleIndicator>
              <EntryDetails>
                <EntryNumber>7</EntryNumber>
                <EntryText>REGISTERED CHARGE dated 2 October 2013 affecting also title WYK267589.</EntryText>
                <RegistrationDate>2013-10-10</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
                <Infills>
                  <ChargeDate>2013-10-02</ChargeDate>
                  <TitleNumber>WYK267589</TitleNumber>
                </Infills>
              </EntryDetails>
            </RegisteredCharge>
            <ChargeProprietor>
              <ChargeeParty>
                <Organization>
                  <Name>Nemo Personal Finance Limited</Name>
                </Organization>
                <Address>
                  <PostcodeZone>
                    <Postcode>CF24 0ED</Postcode>
                  </PostcodeZone>
                  <AddressLine>
                    <Line>Trafalgar House</Line>
                    <Line>5 Fitzalan Place</Line>
                    <Line>Cardiff</Line>
                  </AddressLine>
                </Address>
              </ChargeeParty>
              <EntryDetails>
                <EntryNumber>8</EntryNumber>
                <EntryText>Proprietor: NEMO PERSONAL FINANCE LIMITED (Co. Regn. No. 5188059) of Trafalgar House, 5 Fitzalan Place, Cardiff CF24 0ED.</EntryText>
                <RegistrationDate>2013-10-10</RegistrationDate>
                <SubRegisterCode>C</SubRegisterCode>
              </EntryDetails>
            </ChargeProprietor>
          </ChargeEntry>
        </Charge>
        <DocumentDetails>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2011-02-02</DocumentDate>
            <EntryNumber>B3</EntryNumber>
            <EntryNumber>C4</EntryNumber>
            <EntryNumber>C5</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <FiledUnder>WYK267589</FiledUnder>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>50</DocumentType>
            <DocumentDate>2013-10-02</DocumentDate>
            <EntryNumber>B5</EntryNumber>
            <EntryNumber>C7</EntryNumber>
            <EntryNumber>C8</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <FiledUnder>WYK267589</FiledUnder>
            <RegisterDescription>Charge</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>70</DocumentType>
            <DocumentDate>1991-06-19</DocumentDate>
            <EntryNumber>C3</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <FiledUnder>WYK213052</FiledUnder>
            <RegisterDescription>Deed</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>90</DocumentType>
            <DocumentDate>2011-06-23</DocumentDate>
            <EntryNumber>L1</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <FiledUnder>WYK936787</FiledUnder>
            <RegisterDescription>Lease</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>130</DocumentType>
            <DocumentDate>1995-12-21</DocumentDate>
            <EntryNumber>A3</EntryNumber>
            <PlanOnlyIndicator>true</PlanOnlyIndicator>
            <FiledUnder>WYK583610</FiledUnder>
            <RegisterDescription>Transfer</RegisterDescription>
          </Document>
          <Document>
            <DocumentType>130</DocumentType>
            <DocumentDate>2008-03-14</DocumentDate>
            <EntryNumber>A4</EntryNumber>
            <PlanOnlyIndicator>false</PlanOnlyIndicator>
            <RegisterDescription>Transfer</RegisterDescription>
          </Document>
        </DocumentDetails>
      </OCSummaryData>
      <OCRegisterData>
        <PropertyRegister>
          <DistrictDetails>
            <EntryText>WEST YORKSHIRE : WAKEFIELD</EntryText>
          </DistrictDetails>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>1980-10-14</EntryDate>
            <EntryType>Property Description</EntryType>
            <EntryText>The Freehold land shown edged with red on the plan of the above title filed at the Registry and being 39 Wakefield Road, Kinsley, Pontefract (WF9 5EY).</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Mines &amp; Minerals - Short Entries</EntryType>
            <EntryText>The mines and minerals are excepted.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>1996-02-09</EntryDate>
            <EntryType>Beneficial/Subjective Easements - A Register</EntryType>
            <EntryText>The land has the benefit of the following rights reserved by but is subject to the following rights granted by a Transfer of land to the South of the land in this title made between (1) John Curran and Pamela Margaret Curran (Transferors) and (2) Michaela Joanne Begg (Transferee):-</EntryText>
            <EntryText>""(2)  The rights granted to and for the benefit of the property hereby transferred contained in the First Schedule hereto</EntryText>
            <EntryText>(3)  Subject to the exception and reservation contained in the Second Schedule hereto</EntryText>
            <EntryText>THE FIRST SCHEDULE                            </EntryText>
            <EntryText>Rights Granted                              </EntryText>
            <EntryText>A full and free right for the buyer and her successors in title in common with the seller and all persons deriving title under them and the owners and occupiers for the time being of the adjoining property on the south east side known as ""Stoneybeck"" Wakefield Road Kinsley aforesaid and their successors in title and all other persons having the like right to pass and repass on foot and with motor vehicles at all times and for all purposes over and along the land coloured brown on the said plan subject to the payment of a proportionate part according to user of the cost of maintaining and repairing the same</EntryText>
            <EntryText>A full and free right for the Buyer and her successors in title to use (in common with the sellers and the persons deriving title under them and the owners and occupiers for the time being of ""Stoneybeck"" and the persons deriving title under them and all other persons entitled to the like right all (if any) foul and surface water sewers and drains electricity and gas lines or pipes and cables and other services laid within and under the land on the north west and south east sides of the property for the passage and running of water soil gas electricity and other services to and from the property subject to the payment of a proportionate part of the cost of maintaining and repairing the same  AND TOGETHER ALSO WITH the right to enter upon such part of the said adjoining properties as may be necessary with any necessary workman materials and tools for the purpose of repairing maintaining renewing or replacing such sewers drains pipes wires cables and other services subject to the buyer or her successors in title making good or paying reasonable compensation for the damage done</EntryText>
            <EntryText>SECOND SCHEDULE                             </EntryText>
            <EntryText>Rights Reserved                             </EntryText>
            <EntryText>A full and free right for the seller and their successors in title and the owners and occupiers for the time being of ""Stoneybeck"" and their successors in title and all other persons entitled to the like right (in common with the buyer and the persons deriving title under her) to use all (if any) foul and surface water drains and any electricity or gas pipes wires or cables and other services laid in or under any part of the Property for the passage and running of water soil gas electricity and other services to and from the said adjoining properties subject to the payment of a proportionate part of the cost of maintaining and repairing the same and the right to enter upon such part of the property as may be necessary with or without workman tools and materials to repair maintain renew relay or replace the said sewers drains pipes wires cables and other services upon giving reasonable notice except in the case of emergency provided the person or persons exercising such rights making good or pay reasonable compensation for any damage done.""</EntryText>
            <EntryText>NOTE: The land coloured brown referred to above is not shown on the plan held by Land Registry.</EntryText>
            <EntryText>NOTE: Copy of plan filed under WYK583610.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2008-03-17</EntryDate>
            <EntryType>Beneficial/Subjective Easements - A Register</EntryType>
            <EntryText>The land has the benefit of the rights granted by but is subject to the rights reserved by a Transfer of the land in this title dated 14 March 2008 made between (1) John Curran and Pamela Margaret Curran and (2) Amanda Jane Curran.</EntryText>
            <EntryText>NOTE: Copy filed.</EntryText>
          </RegisterEntry>
        </PropertyRegister>
        <ProprietorshipRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryDate>2009-04-08</EntryDate>
            <EntryType>Proprietor</EntryType>
            <EntryText>PROPRIETOR: JASON CRAIG BEGG of 39 Wakefield Road, Kinsley, Pontefract WF9 5EY.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryDate>2009-04-08</EntryDate>
            <EntryType>Personal Covenants</EntryType>
            <EntryText>The Transfer to the proprietor contains a covenant to observe and perform the covenants referred to in the Charges Register and of indemnity in respect thereof.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>2011-02-18</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 February 2011 in favour of Santander UK PLC referred to in the Charges Register.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2011-02-18</EntryDate>
            <EntryType>Proprietor - Change of address for service</EntryType>
            <EntryText>The proprietor's address for service has been changed.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>5</EntryNumber>
            <EntryDate>2013-10-10</EntryDate>
            <EntryType>Charge Restriction - B Register</EntryType>
            <EntryText>RESTRICTION: No disposition of the registered estate by the proprietor of the registered estate is to be registered without a written consent signed by the proprietor for the time being of the Charge dated 2 October 2013 in favour of Nemo Personal Finance Limited referred to in the Charges Register.</EntryText>
          </RegisterEntry>
        </ProprietorshipRegister>
        <ChargesRegister>
          <RegisterEntry>
            <EntryNumber>1</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Conveyance of the land in this title and other land dated 25 June 1919 made between (1) The Halifax Permanent Building Society (2) Alexander McKenzie (Vendor) and (3) The Right Reverend Joseph Robert Cowgill and  others (Purchasers) contains covenants details of which are set out in the schedule of restrictive covenants hereto.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>2</EntryNumber>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>A Conveyance of the land in this title and other land dated 12 August 1980 made between (1) The Right Reverend William Gordon  Wheeler and and others (Vendors) and (2) John  Curran (Purchaser) contains covenants details of which are set out in the schedule of restrictive covenants hereto.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>3</EntryNumber>
            <EntryDate>1991-07-09</EntryDate>
            <EntryType>Restrictive Covenants/Stipulations - Deed</EntryType>
            <EntryText>By a Deed dated 19 June 1991 made between (1) The Trustees of the Roman Catholic Diocese of Leeds (2) John Curran and Pamela Margaret Curran and (3) Britannia Building Society, the covenants contained in the Conveyance dated 12 August 1980 referred to above were expressed to be modified.</EntryText>
            <EntryText>NOTE: Copy filed under WYK213052.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>4</EntryNumber>
            <EntryDate>2011-02-18</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 2 February 2011 affecting also title WYK267589.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>5</EntryNumber>
            <EntryDate>2011-02-18</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: SANTANDER UK PLC (Co. Regn. No. 2294747) of Deeds Services, 101 Midsummer Boulevard, Milton Keynes MK9 1AA.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>6</EntryNumber>
            <EntryDate>2011-07-01</EntryDate>
            <EntryType>Lease Related - Register</EntryType>
            <EntryText>The parts of the land affected thereby are subject to the leases set out in the schedule of leases hereto.</EntryText>
            <EntryText>The leases grants easements as therein mentioned.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>7</EntryNumber>
            <EntryDate>2013-10-10</EntryDate>
            <EntryType>Registered Charges</EntryType>
            <EntryText>REGISTERED CHARGE dated 2 October 2013 affecting also title WYK267589.</EntryText>
          </RegisterEntry>
          <RegisterEntry>
            <EntryNumber>8</EntryNumber>
            <EntryDate>2013-10-10</EntryDate>
            <EntryType>Chargee</EntryType>
            <EntryText>Proprietor: NEMO PERSONAL FINANCE LIMITED (Co. Regn. No. 5188059) of Trafalgar House, 5 Fitzalan Place, Cardiff CF24 0ED.</EntryText>
          </RegisterEntry>
          <Schedule>
            <ScheduleType>SCHEDULE OF RESTRICTIVE COVENANTS</ScheduleType>
            <ScheduleEntry>
              <EntryNumber>1</EntryNumber>
              <EntryType>Schedule of Restrictive Covenants</EntryType>
              <EntryText>The following are details of the covenants contained in the Conveyance dated 25 June 1919 referred to in the Charges Register:-</EntryText>
              <EntryText>The Purchasers as to the plot of land hereby conveyed and with intent to bind all persons in whom the said plot of land shall for the time being be vested but not so as to be permanently liable under this covenant after they have parted with the said plot of land jointly and severally covenant with the Vendor his heirs and assigns as follows:-</EntryText>
              <EntryText>1.  THAT no building or erection of any kind whatsoever other than fences shall be erected or placed nearer to Wakefield Road aforesaid than the line marked ""Building Line"" in the said plan annexed hereto</EntryText>
              <EntryText>2.  THAT the Purchaser shall within twelve calendar months from the date of these presents erect temporary fences and within five years thereafter erect and forever afterwards maintain good and substantial fences not less than five feet in height between the points marked A and B in the said plan annexed hereto also on the Western side of the said plot of land</EntryText>
              <EntryText>3.  THAT so much of the said plot of land as forms part of the said proposed new street thirty six feet side shall for ever hereafter remain open and unbuilt upon and be used as and for the purposes of such street.</EntryText>
              <EntryText>4.  THAT the Purchasers and their heirs successors and assigns owner or owners for the time being of the hereditaments hereby conveyed shall when called on by the Vendor his heirs or assigns at the expense of the Purchasers and their heirs successors and assigns owner or owners as aforesaid make and forever afterwards make and forever afterwards until the same shall be adopted by the Local Authority maintain and keep in repair one moiety in width of the said proposed new street so far as the same is co-extensive with the said plot of land hereby contracted to be sold with such materials at such levels on such plan and in such manner in all respects as the Surveyor for the time being of the Vendor his heirs or assigns shall direct or decide</EntryText>
              <EntryText>5.  THAT the Purchasers their heirs successors and assigns owner or owners as aforesaid shall and will pay and contribute one moiety of the cost of sewering and draining the said proposed new street according to such plan and directions as the Surveyor for the time being of the Vendor his heirs and assigns shall direct and impose and thereafter of repairing and maintaining and cleansing the sewers and drains thereon and thereunder</EntryText>
              <EntryText>6.  THAT in case the Purchasers their heirs successors and assigns owner or owners as aforesaid shall fail for two calendar months after receiving from the Vendor his heirs or assigns notice requiring them him or them to perform and carry out any of the works aforesaid to perform or carry out the same either alone or in conjunction with the works required to be carried out by any other purchaser or purchasers and the costs of the expenses of so doing shall be apportioned between the various persons liable to carry out and perform such work by the Surveyor of the Vendor his heirs or assigns and shall forthwith thereafter become payable to the Vendor his heirs or assigns owner or owners as aforesaid and be a charge on the plot of land hereby conveyed.</EntryText>
              <EntryText>NOTE: No dimensions for the building line were discernable on the copy plan supplied on First Registration.  Points A and B do not affect the land in this title and the South-Western boundary of the land in this title forms part of the Western side referred to.</EntryText>
            </ScheduleEntry>
            <ScheduleEntry>
              <EntryNumber>2</EntryNumber>
              <EntryType>Schedule of Restrictive Covenants</EntryType>
              <EntryText>The following are details of the covenants contained in the Conveyance dated 12 August 1980 referred to in the Charges Register:-</EntryText>
              <EntryText>""The Purchaser hereby covenants with the Vendors as follows:-</EntryText>
              <EntryText>(1)  within six months of the date hereof to erect and thereafter maintain a Four feet six inches high wall in nine inch facing bricks with brick on edge coping along the Northern boundary of the property to the reasonable satisfaction of the Vendor's Surveyors</EntryText>
              <EntryText>(2)  for the benefit of the adjoining or neighbouring land of the Vendors or any part thereof capable of being benefited not to use the said property except for the construction of a dwellinghouse with the necessary outbuildings thereto to be used for residential purposes only which shall be in accordance with plans sections and materials which shall be previously approved by the Vendors.""</EntryText>
            </ScheduleEntry>
          </Schedule>
          <Schedule>
            <ScheduleType>SCHEDULE OF NOTICES OF LEASE</ScheduleType>
            <ScheduleEntry>
              <EntryNumber>1</EntryNumber>
              <EntryType>Schedule of Notices of Leases</EntryType>
              <EntryText>01.07.2011      Airspace above the south      23.06.2011      WYK936787  </EntryText>
              <EntryText>facing roof as more           25 years from              </EntryText>
              <EntryText>particularly described in     23.06.2011                 </EntryText>
              <EntryText>the lease.</EntryText>
            </ScheduleEntry>
          </Schedule>
        </ChargesRegister>
      </OCRegisterData>
    </Results>
  </GatewayResponse>
</ResponseOCWithSummaryV2_1Type>";

	}
}

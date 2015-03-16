-- Create indexes on prod

-- Created on QA and should be done on prod
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Askville_MarketPlaceId_GISSC' AND object_id = OBJECT_ID('Askville'))
	create index IX_Askville_MarketPlaceId_GISSC on Askville(MarketPlaceId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CashRequests_OO_ISUSMU' AND object_id = OBJECT_ID('CashRequests'))
	create index IX_CashRequests_OO_ISUSMU on CashRequests(OfferStart, OfferValidUntil);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CashRequests_UC' AND object_id = OBJECT_ID('CashRequests'))
	create index IX_CashRequests_UC on CashRequests(UnderwriterDecision, CreationDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_Filled' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_Filled on Customer(WizardStep, IsTest);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_GreetingMailSentDate_NCSARW' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_GreetingMailSentDate_NCSARW on Customer(GreetingMailSentDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IC_I' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IC_I on Customer(IsTest, CciMark);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IG_I' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IG_I on Customer(IsTest, GreetingMailSentDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IIG_INW' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IIG_INW on Customer(IsTest, IsOffline, GreetingMailSentDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IR_INFI' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IR_INFI on Customer(IsTest, ReferenceSource);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IsRegistered' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IsRegistered on Customer(WizardStep);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IsTest' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_IsTest on Customer(IsTest);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_SICAV_C' AND object_id = OBJECT_ID('Customer'))
	create index IX_Customer_SICAV_C on Customer(Status, IsTest, CreditSum, ApplyForLoan, ValidFor);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CustomerAddress_ID' AND object_id = OBJECT_ID('CustomerAddress'))
	create index IX_CustomerAddress_ID on CustomerAddress(addressId, addressType);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Export_Results_FType' AND object_id = OBJECT_ID('Export_Results'))
	create index IX_Export_Results_FType on Export_Results(FileType, ApplicationId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_RequestCashId' AND object_id = OBJECT_ID('Loan'))
	create index IX_Loan_RequestCashId on Loan(RequestCashId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Status_LR' AND object_id = OBJECT_ID('Loan'))
	create index IX_Loan_Status_LR on Loan(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanAgreement_Loan' AND object_id = OBJECT_ID('LoanAgreement'))
	create index IX_LoanAgreement_Loan on LoanAgreement(LoanId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_Date' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_Date on LoanSchedule(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_DS_AL' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_DS_AL on LoanSchedule(Date, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_DSA_ILPLF' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_DSA_ILPLF on LoanSchedule(Date, Status, AmountDue);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_LoanId_Date' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_LoanId_Date on LoanSchedule(LoanId, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_SD' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_SD on LoanSchedule(Status, Date);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_S' AND object_id = OBJECT_ID('LoanSchedule'))
	create index IX_LoanSchedule_S on LoanSchedule(Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanScheduleTransaction_LoanID_ISTPFISS' AND object_id = OBJECT_ID('LoanScheduleTransaction'))
	create index IX_LoanScheduleTransaction_LoanID_ISTPFISS on LoanScheduleTransaction(LoanID);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TSP' AND object_id = OBJECT_ID('LoanTransaction'))
	create index IX_LoanTransaction_TSP on LoanTransaction(Type, Status, PostDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TypeStatus' AND object_id = OBJECT_ID('LoanTransaction'))
	create index IX_LoanTransaction_TypeStatus on LoanTransaction(Type, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LT_ID_TYPE' AND object_id = OBJECT_ID('LoanTransaction'))
	create index IX_LT_ID_TYPE on LoanTransaction(Id, Type, Status);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AA' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_AA on MP_AnalyisisFunctionValues(AnalysisFunctionTimePeriodId, AnalyisisFunctionId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AA_UCV' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_AA_UCV on MP_AnalyisisFunctionValues(AnalysisFunctionTimePeriodId, AnalyisisFunctionId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AFI' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_AFI on MP_AnalyisisFunctionValues(AnalyisisFunctionId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AFTPI' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_AFTPI on MP_AnalyisisFunctionValues(AnalysisFunctionTimePeriodId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_Include' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_Include on MP_AnalyisisFunctionValues(AnalyisisFunctionId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='MP_AnalyisisFunctionValues_AFTP' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index MP_AnalyisisFunctionValues_AFTP on MP_AnalyisisFunctionValues(AnalysisFunctionTimePeriodId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CurrencyRateHistory_CurId' AND object_id = OBJECT_ID('MP_CurrencyRateHistory'))
	create index IX_MP_CurrencyRateHistory_CurId on MP_CurrencyRateHistory(CurrencyId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CurrencyRateHistory_UpdateId' AND object_id = OBJECT_ID('MP_CurrencyRateHistory'))
	create index IX_MP_CurrencyRateHistory_UpdateId on MP_CurrencyRateHistory(CurrencyId, Updated);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	create index IX_MP_CustomerMarketPlace on MP_CustomerMarketPlace(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_CustId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	create index IX_MP_CustomerMarketPlace_CustId on MP_CustomerMarketPlace(CustomerId, UpdatingEnd);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	create index IX_MP_CustomerMarketPlace_MarketPlaceId on MP_CustomerMarketPlace(MarketPlaceId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId_ICC' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	create index IX_MP_CustomerMarketPlace_MarketPlaceId_ICC on MP_CustomerMarketPlace(MarketPlaceId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
	create index IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart on MP_CustomerMarketPlaceUpdatingHistory(UpdatingStart);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MPEbayAmazonInventoryCreatedIncludeUMI' AND object_id = OBJECT_ID('MP_EbayAmazonInventory'))
	create index IX_MPEbayAmazonInventoryCreatedIncludeUMI on MP_EbayAmazonInventory(Created);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayAmazonInventoryItem_Id' AND object_id = OBJECT_ID('MP_EbayAmazonInventoryItem'))
	create index IX_MP_EbayAmazonInventoryItem_Id on MP_EbayAmazonInventoryItem(InventoryId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayFeedback'))
	create index IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayFeedback(CustomerMarketPlaceUpdatingHistoryRecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayFeedbackCreatedDateIncludeMUI' AND object_id = OBJECT_ID('MP_EbayFeedback'))
	create index IX_MP_EbayFeedbackCreatedDateIncludeMUI on MP_EbayFeedback(Created);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EBayOrderItemDetail_PrimaryCategoryId' AND object_id = OBJECT_ID('MP_EBayOrderItemDetail'))
	create index IX_MP_EBayOrderItemDetail_PrimaryCategoryId on MP_EBayOrderItemDetail(PrimaryCategoryId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayTransaction_OItId' AND object_id = OBJECT_ID('MP_EbayTransaction'))
	create index IX_MP_EbayTransaction_OItId on MP_EbayTransaction(OrderItemId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
	create index IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayUserAccountData(CustomerMarketPlaceUpdatingHistoryRecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserAccountDataCreatedDateIncludeMUI' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
	create index IX_MP_EbayUserAccountDataCreatedDateIncludeMUI on MP_EbayUserAccountData(Created);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserData_CreatedDateIncludeUMI' AND object_id = OBJECT_ID('MP_EbayUserData'))
	create index IX_MP_EbayUserData_CreatedDateIncludeUMI on MP_EbayUserData(Created);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ExperianDataCache_CustomerId_INSBLE' AND object_id = OBJECT_ID('MP_ExperianDataCache'))
	create index IX_MP_ExperianDataCache_CustomerId_INSBLE on MP_ExperianDataCache(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_PayPalTransactionItem2_TransactionId' AND object_id = OBJECT_ID('MP_PayPalTransactionItem2'))
	create index IX_MP_PayPalTransactionItem2_TransactionId on MP_PayPalTransactionItem2(TransactionId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ServiceLog_CustomerId' AND object_id = OBJECT_ID('MP_ServiceLog'))
	create index IX_MP_ServiceLog_CustomerId on MP_ServiceLog(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_TeraPeakOrder'))
	create index IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId on MP_TeraPeakOrder(CustomerMarketPlaceUpdatingHistoryRecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='PacnetPaypointServiceLog_CustId' AND object_id = OBJECT_ID('PacnetPaypointServiceLog'))
	create index PacnetPaypointServiceLog_CustId on PacnetPaypointServiceLog(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PayPointBalance_date' AND object_id = OBJECT_ID('PayPointBalance'))
	create index IX_PayPointBalance_date on PayPointBalance(date);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='PostcodeServiceLog_CustId' AND object_id = OBJECT_ID('PostcodeServiceLog'))
	create index PostcodeServiceLog_CustId on PostcodeServiceLog(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_UiEvents_EventTime' AND object_id = OBJECT_ID('UiEvents'))
	create index IX_UiEvents_EventTime on UiEvents(EventTime);

-- Not done in QA but should be done in prod
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianDefaultAccount_CustomerId' AND object_id = OBJECT_ID('ExperianDefaultAccount'))
	create index IX_ExperianDefaultAccount_CustomerId on ExperianDefaultAccount(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LOAN_CustId' AND object_id = OBJECT_ID('Loan'))
	create index IX_LOAN_CustId on Loan(CustomerId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	create index IX_MP_AnalyisisFunctionValues_CustomerMarketPlaceUpdatingHistoryRecordId on MP_AnalyisisFunctionValues(CustomerMarketPlaceUpdatingHistoryRecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='_dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
	create index _dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5 on MP_CustomerMarketPlaceUpdatingHistory(Id);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserData_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayUserData'))
	create index IX_MP_EbayUserData_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayUserData(CustomerMarketPlaceUpdatingHistoryRecordId);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ServiceLog_InsertDate' AND object_id = OBJECT_ID('MP_ServiceLog'))
	create index IX_MP_ServiceLog_InsertDate on MP_ServiceLog(InsertDate);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_DateClosed' AND object_id = OBJECT_ID('Loan'))
	create index IX_Loan_DateClosed on Loan(DateClosed);

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date' AND object_id = OBJECT_ID('Loan'))
	create index IX_Loan_Date on Loan(Date);

GO

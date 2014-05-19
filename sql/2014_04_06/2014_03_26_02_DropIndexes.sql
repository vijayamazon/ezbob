-- Drop indexes on prod

-- Done on QA and should be done on prod
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Askville_MarketPlaceId_GISSC' AND object_id = OBJECT_ID('Askville'))
	drop index IX_Askville_MarketPlaceId_GISSC on Askville;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CashRequests_OO_ISUSMU' AND object_id = OBJECT_ID('CashRequests'))
	drop index IX_CashRequests_OO_ISUSMU on CashRequests;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CashRequests_UC' AND object_id = OBJECT_ID('CashRequests'))
	drop index IX_CashRequests_UC on CashRequests;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_Filled' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_Filled on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_G_CR' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_G_CR on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_GreetingMailSentDate_NCSARW' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_GreetingMailSentDate_NCSARW on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IC_I' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IC_I on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IG_I' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IG_I on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IG_INCSAFAMRW' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IG_INCSAFAMRW on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IG_INFSDM' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IG_INFSDM on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IIG_INW' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IIG_INW on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IR_INFI' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IR_INFI on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IsRegistered' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IsRegistered on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IsTest' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IsTest on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_IsTest_IDRGMTFRI' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_IsTest_IDRGMTFRI on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_SICAV_C' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_SICAV_C on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Customer_WizardStep_INFML' AND object_id = OBJECT_ID('Customer'))
	drop index IX_Customer_WizardStep_INFML on Customer;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_CustomerAddress_ID' AND object_id = OBJECT_ID('CustomerAddress'))
	drop index IX_CustomerAddress_ID on CustomerAddress;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Export_Results_FType' AND object_id = OBJECT_ID('Export_Results'))
	drop index IX_Export_Results_FType on Export_Results;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_C' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_C on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_CS' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_CS on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_IC' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_IC on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_ICRIL' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_ICRIL on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_ILC' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_ILC on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_ILCI' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_ILCI on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_L' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_L on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_LR' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_LR on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Date_LS' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Date_LS on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_DateClosed_C' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_DateClosed_C on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_DateClosed_LC' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_DateClosed_LC on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_RequestCashId_I' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_RequestCashId_I on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_RequestCashId_L' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_RequestCashId_L on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_RequestCashId_LS' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_RequestCashId_LS on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_Loan_Status_LR' AND object_id = OBJECT_ID('Loan'))
	drop index IX_Loan_Status_LR on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanAgreement_Loan' AND object_id = OBJECT_ID('LoanAgreement'))
	drop index IX_LoanAgreement_Loan on LoanAgreement;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_Date' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_Date on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_DS_AL' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_DS_AL on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_DSA_ILPLF' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_DSA_ILPLF on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_LoanId' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_LoanId on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_LoanId_Date' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_LoanId_Date on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_SD_IAL' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_SD_IAL on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_SD_ILC' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_SD_ILC on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanSchedule_Status_IDIALDC' AND object_id = OBJECT_ID('LoanSchedule'))
	drop index IX_LoanSchedule_Status_IDIALDC on LoanSchedule;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanScheduleTransaction_LoanID_ISTPFISS' AND object_id = OBJECT_ID('LoanScheduleTransaction'))
	drop index IX_LoanScheduleTransaction_LoanID_ISTPFISS on LoanScheduleTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TS_ADL' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TS_ADL on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TS_LP' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TS_LP on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TSP' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TSP on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TSP_ALIFLR' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TSP_ALIFLR on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TSP_ALL' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TSP_ALL on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LoanTransaction_TypeStatus' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LoanTransaction_TypeStatus on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LT_ID_TYPE' AND object_id = OBJECT_ID('LoanTransaction'))
	drop index IX_LT_ID_TYPE on LoanTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AA' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_AA on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AA_UCV' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_AA_UCV on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AFI' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_AFI on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_AFTPI' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_AFTPI on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_Include' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_Include on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='MP_AnalyisisFunctionValues_AFTP' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index MP_AnalyisisFunctionValues_AFTP on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CurrencyRateHistory_CurId' AND object_id = OBJECT_ID('MP_CurrencyRateHistory'))
	drop index IX_MP_CurrencyRateHistory_CurId on MP_CurrencyRateHistory;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CurrencyRateHistory_UpdateId' AND object_id = OBJECT_ID('MP_CurrencyRateHistory'))
	drop index IX_MP_CurrencyRateHistory_UpdateId on MP_CurrencyRateHistory;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	drop index IX_MP_CustomerMarketPlace on MP_CustomerMarketPlace;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_CUstId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	drop index IX_MP_CustomerMarketPlace_CUstId on MP_CustomerMarketPlace;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	drop index IX_MP_CustomerMarketPlace_MarketPlaceId on MP_CustomerMarketPlace;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlace_MarketPlaceId_ICC' AND object_id = OBJECT_ID('MP_CustomerMarketPlace'))
	drop index IX_MP_CustomerMarketPlace_MarketPlaceId_ICC on MP_CustomerMarketPlace;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
	drop index IX_MP_CustomerMarketPlaceUpdatingHistory_DateStart on MP_CustomerMarketPlaceUpdatingHistory;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MPEbayAmazonInventoryCreatedIncludeUMI' AND object_id = OBJECT_ID('MP_EbayAmazonInventory'))
	drop index IX_MPEbayAmazonInventoryCreatedIncludeUMI on MP_EbayAmazonInventory;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayFeedback'))
	drop index IX_MP_EbayFeedback_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayFeedback;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayFeedbackCreatedDateIncludeMUI' AND object_id = OBJECT_ID('MP_EbayFeedback'))
	drop index IX_MP_EbayFeedbackCreatedDateIncludeMUI on MP_EbayFeedback;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EBayOrderItemDetail_PrimaryCategoryId' AND object_id = OBJECT_ID('MP_EBayOrderItemDetail'))
	drop index IX_MP_EBayOrderItemDetail_PrimaryCategoryId on MP_EBayOrderItemDetail;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayTransaction_OItId' AND object_id = OBJECT_ID('MP_EbayTransaction'))
	drop index IX_MP_EbayTransaction_OItId on MP_EbayTransaction;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
	drop index IX_MP_EbayUserAccountData_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayUserAccountData;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserAccountDataCreatedDateIncludeMUI' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
	drop index IX_MP_EbayUserAccountDataCreatedDateIncludeMUI on MP_EbayUserAccountData;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserData_CreatedDateIncludeUMI' AND object_id = OBJECT_ID('MP_EbayUserData'))
	drop index IX_MP_EbayUserData_CreatedDateIncludeUMI on MP_EbayUserData;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ExperianDataCache_CustomerId_INSBLE' AND object_id = OBJECT_ID('MP_ExperianDataCache'))
	drop index IX_MP_ExperianDataCache_CustomerId_INSBLE on MP_ExperianDataCache;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_PayPalTransactionItem2_TransactionId' AND object_id = OBJECT_ID('MP_PayPalTransactionItem2'))
	drop index IX_MP_PayPalTransactionItem2_TransactionId on MP_PayPalTransactionItem2;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ServiceLog_CustomerId' AND object_id = OBJECT_ID('MP_ServiceLog'))
	drop index IX_MP_ServiceLog_CustomerId on MP_ServiceLog;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_TeraPeakOrder'))
	drop index IX_MP_TeraPeakOrder_CustomerMarketPlaceUpdatingHistoryRecordId on MP_TeraPeakOrder;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='PacnetPaypointServiceLog_CustId' AND object_id = OBJECT_ID('PacnetPaypointServiceLog'))
	drop index PacnetPaypointServiceLog_CustId on PacnetPaypointServiceLog;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_PayPointBalance_date' AND object_id = OBJECT_ID('PayPointBalance'))
	drop index IX_PayPointBalance_date on PayPointBalance;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='PostcodeServiceLog_CustId' AND object_id = OBJECT_ID('PostcodeServiceLog'))
	drop index PostcodeServiceLog_CustId on PostcodeServiceLog;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_UiEvents_EventTime' AND object_id = OBJECT_ID('UiEvents'))
	drop index IX_UiEvents_EventTime on UiEvents;

-- Not done in QA but should be done in prod
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_ExperianDefaultAccount_CustomerId' AND object_id = OBJECT_ID('ExperianDefaultAccount'))
	drop index IX_ExperianDefaultAccount_CustomerId on ExperianDefaultAccount;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_LOAN_CustId' AND object_id = OBJECT_ID('Loan'))
	drop index IX_LOAN_CustId on Loan;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_AnalyisisFunctionValues_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_AnalyisisFunctionValues'))
	drop index IX_MP_AnalyisisFunctionValues_CustomerMarketPlaceUpdatingHistoryRecordId on MP_AnalyisisFunctionValues;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='_dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5' AND object_id = OBJECT_ID('MP_CustomerMarketPlaceUpdatingHistory'))
	drop index _dta_index_MP_CustomerMarketPlaceUpdatingHi_7_498100815__K1_2_3_4_5 on MP_CustomerMarketPlaceUpdatingHistory;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayUserData_CustomerMarketPlaceUpdatingHistoryRecordId' AND object_id = OBJECT_ID('MP_EbayUserData'))
	drop index IX_MP_EbayUserData_CustomerMarketPlaceUpdatingHistoryRecordId on MP_EbayUserData;

IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_ServiceLog_InsertDate' AND object_id = OBJECT_ID('MP_ServiceLog'))
	drop index IX_MP_ServiceLog_InsertDate on MP_ServiceLog;

-- Done in qa but should not be done in prod
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name='IX_MP_EbayAmazonInventoryItem_Id' AND object_id = OBJECT_ID('MP_EbayAmazonInventoryItem'))
	drop index IX_MP_EbayAmazonInventoryItem_Id on MP_EbayAmazonInventoryItem;

GO

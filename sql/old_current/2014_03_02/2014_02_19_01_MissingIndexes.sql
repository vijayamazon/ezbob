IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TSP' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP
ON [dbo].[LoanTransaction] ([Type],[Status],[PostDate])
INCLUDE ([Amount],[LoanId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TSP_ALIFLR' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP_ALIFLR
ON [dbo].[LoanTransaction] ([Type],[Status],[PostDate])
INCLUDE ([Amount],[LoanId],[Interest],[Fees],[LoanRepayment],[Rollover])
END
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayUserAccountData_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_EbayUserAccountData'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayUserAccountData_CustomerMarketPlaceId
ON [dbo].[MP_EbayUserAccountData] ([CustomerMarketPlaceId])
END
GO 

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_TeraPeakCategoryStatistics_OrderItemId' AND object_id = OBJECT_ID('MP_TeraPeakCategoryStatistics'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_TeraPeakCategoryStatistics_OrderItemId
ON [dbo].[MP_TeraPeakCategoryStatistics] ([OrderItemId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_MP_EbayUserData_CustomerMarketPlaceId' AND object_id = OBJECT_ID('MP_EbayUserData'))
BEGIN
CREATE NONCLUSTERED INDEX IX_MP_EbayUserData_CustomerMarketPlaceId
ON [dbo].[MP_EbayUserData] ([CustomerMarketPlaceId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CashRequests_UC' AND object_id = OBJECT_ID('CashRequests'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CashRequests_UC
ON [dbo].[CashRequests] ([UnderwriterDecision],[CreationDate])
INCLUDE ([Id],[IdCustomer],[UnderwriterDecisionDate],[ManagerApprovedSum],[UnderwriterComment])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_LoanId' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN
CREATE NONCLUSTERED INDEX IX_LoanTransaction_LoanId
ON [dbo].[LoanTransaction] ([LoanId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CustomerLoyaltyProgram_CustomerMarketPlaceID' AND object_id = OBJECT_ID('CustomerLoyaltyProgram'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CustomerLoyaltyProgram_CustomerMarketPlaceID
ON [dbo].[CustomerLoyaltyProgram] ([CustomerMarketPlaceID])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_Application_Application_CreatorUserId' AND object_id = OBJECT_ID('Application_Application'))
BEGIN
CREATE NONCLUSTERED INDEX IX_Application_Application_CreatorUserId
ON [dbo].[Application_Application] ([CreatorUserId],[State])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_DecisionHistory_CustomerId' AND object_id = OBJECT_ID('DecisionHistory'))
BEGIN
CREATE NONCLUSTERED INDEX IX_DecisionHistory_CustomerId
ON [dbo].[DecisionHistory] ([CustomerId])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CustomerAddress_CompanyId' AND object_id = OBJECT_ID('CustomerAddress'))
BEGIN
CREATE NONCLUSTERED INDEX IX_CustomerAddress_CompanyId
ON [dbo].[CustomerAddress] ([CompanyId],[addressType])
END
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_CustomerSession_CustomerId' AND object_id = OBJECT_ID('CustomerSession'))
BEGIN

CREATE NONCLUSTERED INDEX IX_CustomerSession_CustomerId ON [ezbob].[dbo].[CustomerSession] ([CustomerId])

CREATE NONCLUSTERED INDEX IX_CustomerAddress_CA ON [ezbob].[dbo].[CustomerAddress] ([CustomerId], [addressType])
CREATE NONCLUSTERED INDEX IX_CustomerAddress_CustomerId ON [ezbob].[dbo].[CustomerAddress] ([CustomerId])
CREATE NONCLUSTERED INDEX IX_CustomerAddress_DirectorId ON [ezbob].[dbo].[CustomerAddress] ([DirectorId])
CREATE NONCLUSTERED INDEX IX_CustomerAddress_DA ON [ezbob].[dbo].[CustomerAddress] ([DirectorId],[addressType])

CREATE NONCLUSTERED INDEX IX_MP_ExperianDataCache_CDS ON [ezbob].[dbo].[MP_ExperianDataCache] ([CustomerId], [DirectorId],[ExperianScore])
CREATE NONCLUSTERED INDEX IX_MP_ExperianDataCache_CD ON [ezbob].[dbo].[MP_ExperianDataCache] ([CustomerId],[DirectorId])
CREATE NONCLUSTERED INDEX IX_MP_ExperianDataCache_CompanyRefNumber ON [ezbob].[dbo].[MP_ExperianDataCache] ([CompanyRefNumber])
CREATE NONCLUSTERED INDEX IX_MP_ExperianDataCache_CustomerId ON [ezbob].[dbo].[MP_ExperianDataCache] ([CustomerId])
CREATE NONCLUSTERED INDEX IX_MP_ExperianDataCache_CustomerId_INSBLE ON [ezbob].[dbo].[MP_ExperianDataCache] ([CustomerId]) INCLUDE ([Id], [Name], [Surname], [BirthDate], [LastUpdateDate], [ExperianScore])

CREATE NONCLUSTERED INDEX IX_Askville_MarketPlaceId_GISSC ON [ezbob].[dbo].[Askville] ([MarketPlaceId]) INCLUDE ([Guid], [isPassed], [Status], [SendStatus], [CreationDate])
CREATE NONCLUSTERED INDEX IX_Askville_MarketPlaceId ON [ezbob].[dbo].[Askville] ([MarketPlaceId])

CREATE NONCLUSTERED INDEX IX_CustomerScoringResult_CustomerId ON [ezbob].[dbo].[CustomerScoringResult] ([CustomerId])

CREATE NONCLUSTERED INDEX IX_PayPointBalance_amount ON [ezbob].[dbo].[PayPointBalance] ([amount],[date])
CREATE NONCLUSTERED INDEX IX_PayPointBalance_date ON [ezbob].[dbo].[PayPointBalance] ([date]) INCLUDE ([amount], [auth_code])

CREATE NONCLUSTERED INDEX IX_MP_AmazonOrderItemDetail_SellerSKU ON [ezbob].[dbo].[MP_AmazonOrderItemDetail] ([SellerSKU])
CREATE NONCLUSTERED INDEX IX_MP_EbayAmazonCategory_ServiceCategoryId ON [ezbob].[dbo].[MP_EbayAmazonCategory] ([ServiceCategoryId])
CREATE NONCLUSTERED INDEX IX_MP_AmazonFeedbackItem_AmazonFeedbackId ON [ezbob].[dbo].[MP_AmazonFeedbackItem] ([AmazonFeedbackId])
CREATE NONCLUSTERED INDEX IX_MP_CustomerMarketPlace_MarketPlaceId_ICC ON [ezbob].[dbo].[MP_CustomerMarketPlace] ([MarketPlaceId]) INCLUDE ([Id], [CustomerId], [Created])
CREATE NONCLUSTERED INDEX IX_MP_AnalyisisFunctionValues_AA ON [ezbob].[dbo].[MP_AnalyisisFunctionValues] ([AnalysisFunctionTimePeriodId],[AnalyisisFunctionId]) INCLUDE ([Updated], [CustomerMarketPlaceId])

CREATE NONCLUSTERED INDEX IX_UiEvents_EventTime ON [ezbob].[dbo].[UiEvents] ([EventTime]) INCLUDE ([UserID])

CREATE NONCLUSTERED INDEX IX_Loan_Status_LR ON [ezbob].[dbo].[Loan] ([Status]) INCLUDE ([LoanAmount], [RequestCashId])
CREATE NONCLUSTERED INDEX IX_Loan_RequestCashId_I ON [ezbob].[dbo].[Loan] ([RequestCashId]) INCLUDE ([Id])
CREATE NONCLUSTERED INDEX IX_Loan_RequestCashId_LS ON [ezbob].[dbo].[Loan] ([RequestCashId]) INCLUDE ([LoanAmount], [Status])
CREATE NONCLUSTERED INDEX IX_Loan_RequestCashId_L ON [ezbob].[dbo].[Loan] ([RequestCashId]) INCLUDE ([LoanAmount])
CREATE NONCLUSTERED INDEX IX_Loan_Date_LS ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([LoanAmount], [SetupFee])
CREATE NONCLUSTERED INDEX IX_Loan_Date_IC ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([Id], [CustomerId])
CREATE NONCLUSTERED INDEX IX_Loan_Date_LR ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([LoanAmount], [RequestCashId])
CREATE NONCLUSTERED INDEX IX_Loan_Date_ILC ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([Id], [LoanAmount], [CustomerId])
CREATE NONCLUSTERED INDEX IX_Loan_Date_L ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([LoanAmount])
CREATE NONCLUSTERED INDEX IX_Loan_Date_C ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([CustomerId])
CREATE NONCLUSTERED INDEX IX_Loan_Date_ILCI ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([Id], [LoanAmount], [CustomerId], [InterestRate])
CREATE NONCLUSTERED INDEX IX_Loan_Date_ICRIL ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([Id], [CustomerId], [RequestCashId], [InterestRate], [LoanTypeId])
CREATE NONCLUSTERED INDEX IX_Loan_Date_CS ON [ezbob].[dbo].[Loan] ([Date]) INCLUDE ([CustomerId], [SetupFee])
CREATE NONCLUSTERED INDEX IX_Loan_DateClosed ON [ezbob].[dbo].[Loan] ([DateClosed])
CREATE NONCLUSTERED INDEX IX_Loan_DateClosed_LC ON [ezbob].[dbo].[Loan] ([DateClosed]) INCLUDE ([LoanAmount], [CustomerId])
CREATE NONCLUSTERED INDEX IX_Loan_DateClosed_C ON [ezbob].[dbo].[Loan] ([DateClosed]) INCLUDE ([CustomerId])

CREATE NONCLUSTERED INDEX IX_LoanSchedule_DS_AL ON [ezbob].[dbo].[LoanSchedule] ([Date], [Status]) INCLUDE ([AmountDue], [LoanId])
CREATE NONCLUSTERED INDEX IX_LoanSchedule_DSA_ILPLF ON [ezbob].[dbo].[LoanSchedule] ([Date], [Status], [AmountDue]) INCLUDE ([Interest], [LoanId], [Position], [LoanRepayment], [Fees])
CREATE NONCLUSTERED INDEX IX_LoanSchedule_SD_ILC ON [ezbob].[dbo].[LoanSchedule] ([Status],[Date]) INCLUDE ([Id], [LoanId], [CustomInstallmentDate])
CREATE NONCLUSTERED INDEX IX_LoanSchedule_SD_IAL ON [ezbob].[dbo].[LoanSchedule] ([Status],[Date]) INCLUDE ([Id], [AmountDue], [LoanId])
CREATE NONCLUSTERED INDEX IX_LoanSchedule_Status_IDIALDC ON [ezbob].[dbo].[LoanSchedule] ([Status]) INCLUDE ([Id], [Date], [Interest], [AmountDue], [LoanId], [Delinquency], [CustomInstallmentDate])

CREATE NONCLUSTERED INDEX IX_LoanTransaction_TSP_ALL ON [ezbob].[dbo].[LoanTransaction] ([Type], [Status],[PostDate]) INCLUDE ([Amount], [LoanId], [LoanTransactionMethodId])
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TrackingNumber ON [ezbob].[dbo].[LoanTransaction] ([TrackingNumber])

CREATE NONCLUSTERED INDEX IX_LoanScheduleTransaction_LoanID_ISTPFISS ON [ezbob].[dbo].[LoanScheduleTransaction] ([LoanID]) INCLUDE ([ID], [ScheduleID], [TransactionID], [PrincipalDelta], [FeesDelta], [InterestDelta], [StatusBefore], [StatusAfter])
CREATE NONCLUSTERED INDEX IX_LoanScheduleTransaction_LoanID ON [ezbob].[dbo].[LoanScheduleTransaction] ([LoanID])

CREATE NONCLUSTERED INDEX IX_CashRequests_UnderwriterCommentCreationDate ON [ezbob].[dbo].[CashRequests] ([UnderwriterComment],[CreationDate])
CREATE NONCLUSTERED INDEX IX_CashRequests_OO_ISUSMU ON [ezbob].[dbo].[CashRequests] ([OfferStart], [OfferValidUntil]) INCLUDE ([Id], [SystemDecision], [UnderwriterDecision], [SystemCalculatedSum], [ManagerApprovedSum], [UnderwriterComment])

CREATE NONCLUSTERED INDEX IX_Customer_SICAV_C ON [ezbob].[dbo].[Customer] ([Status], [IsTest],[CreditSum], [ApplyForLoan], [ValidFor]) INCLUDE ([CreditResult])
CREATE NONCLUSTERED INDEX IX_Customer_Name ON [ezbob].[dbo].[Customer] ([Name])
CREATE NONCLUSTERED INDEX IX_Customer_IG_I ON [ezbob].[dbo].[Customer] ([IsTest],[GreetingMailSentDate]) INCLUDE ([Id])
CREATE NONCLUSTERED INDEX IX_Customer_IR_INFI ON [ezbob].[dbo].[Customer] ([IsTest],[ReferenceSource]) INCLUDE ([Id], [Name], [Fullname], [IsOffline])
CREATE NONCLUSTERED INDEX IX_Customer_IG_INCSAFAMRW ON [ezbob].[dbo].[Customer] ([IsTest],[GreetingMailSentDate]) INCLUDE ([Id], [Name], [CreditSum], [Status], [AccountNumber], [FirstName], [ApplyForLoan], [MedalType], [ReferenceSource], [WizardStep])
CREATE NONCLUSTERED INDEX IX_Customer_IGW ON [ezbob].[dbo].[Customer] ([IsTest],[GreetingMailSentDate], [WizardStep])
CREATE NONCLUSTERED INDEX IX_Customer_IG ON [ezbob].[dbo].[Customer] ([IsTest],[GreetingMailSentDate])
CREATE NONCLUSTERED INDEX IX_Customer_GreetingMailSentDate ON [ezbob].[dbo].[Customer] ([GreetingMailSentDate])
CREATE NONCLUSTERED INDEX IX_Customer_IC_I ON [ezbob].[dbo].[Customer] ([IsTest], [CciMark]) INCLUDE ([Id])
CREATE NONCLUSTERED INDEX IX_Customer_G_CR ON [ezbob].[dbo].[Customer] ([GreetingMailSentDate]) INCLUDE ([CreditSum], [ReferenceSource])
CREATE NONCLUSTERED INDEX IX_Customer_IIG_INW ON [ezbob].[dbo].[Customer] ([IsTest], [IsOffline],[GreetingMailSentDate]) INCLUDE ([Id], [Name], [WizardStep])
CREATE NONCLUSTERED INDEX IX_Customer_WizardStep_INFML ON [ezbob].[dbo].[Customer] ([WizardStep]) INCLUDE ([Id], [Name], [Fullname], [ManagerApprovedSum], [LastStatus])
CREATE NONCLUSTERED INDEX IX_Customer_GreetingMailSentDate_NCSARW ON [ezbob].[dbo].[Customer] ([GreetingMailSentDate]) INCLUDE ([Name], [CreditSum], [Status], [AccountNumber], [ReferenceSource], [WizardStep])
CREATE NONCLUSTERED INDEX IX_Customer_IG_INFSDM ON [ezbob].[dbo].[Customer] ([IsTest],[GreetingMailSentDate]) INCLUDE ([Id], [Name], [FirstName], [Surname], [DaytimePhone], [MobilePhone])
CREATE NONCLUSTERED INDEX IX_Customer_IsTest_IDRGMTFRI ON [ezbob].[dbo].[Customer] ([IsTest]) INCLUDE ([Id], [DateOfBirth], [ResidentialStatus], [Gender], [MaritalStatus], [TypeOfBusiness], [Fullname], [ReferenceSource], [IsOffline])

END 
GO


IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_LoanTransaction_TS_LP' AND object_id = OBJECT_ID('LoanTransaction'))
BEGIN

CREATE NONCLUSTERED INDEX IX_LoanTransaction_TS_LP ON [ezbob].[dbo].[LoanTransaction] ([Type], [Status]) INCLUDE ([LoanId], [Principal])
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TAP ON [ezbob].[dbo].[LoanTransaction] ([Type], [Amount],[PostDate])
CREATE NONCLUSTERED INDEX IX_MP_AnalyisisFunctionValues_AA_UCV ON [ezbob].[dbo].[MP_AnalyisisFunctionValues] ([AnalysisFunctionTimePeriodId],[AnalyisisFunctionId]) INCLUDE ([Updated], [CustomerMarketPlaceId], [ValueFloat])
CREATE NONCLUSTERED INDEX IX_LoanTransaction_TS_ADL ON [ezbob].[dbo].[LoanTransaction] ([Type], [Status]) INCLUDE ([Amount], [Description], [LoanId])
CREATE NONCLUSTERED INDEX IX_Customer_FirstName ON [ezbob].[dbo].[Customer] ([FirstName])

END 
GO





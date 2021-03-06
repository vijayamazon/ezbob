SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name='IX_ExperianConsumerDataCaisCardHistory_ExperianConsumerDataCaisId' AND object_id = OBJECT_ID('ExperianConsumerDataCaisCardHistory'))
BEGIN

CREATE INDEX IX_ExperianConsumerDataCaisCardHistory_ExperianConsumerDataCaisId ON [ezbob].[dbo].[ExperianConsumerDataCaisCardHistory] ([ExperianConsumerDataCaisId])
CREATE INDEX IX_ExperianConsumerDataCaisBalance_ExperianConsumerDataCaisId ON [ezbob].[dbo].[ExperianConsumerDataCaisBalance] ([ExperianConsumerDataCaisId])
CREATE INDEX IX_DecisionHistory_CashRequestId ON [ezbob].[dbo].[DecisionHistory] ([CashRequestId])
CREATE INDEX IX_MP_ServiceLog_CompanyRefNum ON [ezbob].[dbo].[MP_ServiceLog] ([CompanyRefNum])
CREATE INDEX IX_CustomerAnalyticsCompany_CustomerIDIsActive ON [ezbob].[dbo].[CustomerAnalyticsCompany] ([CustomerID],[IsActive]) 
CREATE INDEX IX_ExperianDirectors_CustomerIDIsDeleted ON [ezbob].[dbo].[ExperianDirectors] ([CustomerID],[IsDeleted])
CREATE INDEX IX_ExperianDirectors_CustomerIDIsShareholderIsDeleted ON [ezbob].[dbo].[ExperianDirectors] ([CustomerID], [IsShareholder],[IsDeleted])
CREATE INDEX IX_ExperianDirectors_CustomerIDIsDirectorIsDeleted ON [ezbob].[dbo].[ExperianDirectors] ([CustomerID], [IsDirector],[IsDeleted])
CREATE INDEX IX_ExperianNonLimitedResults_RefNumberIsActiveIncorporationDate ON [ezbob].[dbo].[ExperianNonLimitedResults] ([RefNumber], [IsActive], [IncorporationDate])
CREATE INDEX IX_MP_AlertDocument_CustomerId ON [ezbob].[dbo].[MP_AlertDocument] ([CustomerId])
CREATE INDEX IX_ExperianLtdDL99_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdDL99] ([ExperianLtdID])
CREATE INDEX IX_ExperianLtdDLB5_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdDLB5] ([ExperianLtdID])
CREATE INDEX IX_ExperianLtdDL72_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdDL72] ([ExperianLtdID])
CREATE INDEX IX_ExperianLtdCaisMonthly_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdCaisMonthly] ([ExperianLtdID])
CREATE INDEX IX_MP_ExperianHistory_CompanyRefNum ON [ezbob].[dbo].[MP_ExperianHistory] ([CompanyRefNum])
CREATE INDEX IX_ExperianLtdDL65_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdDL65] ([ExperianLtdID])
CREATE INDEX IX_ExperianNonLimitedResults_RefNumberIsActive ON [ezbob].[dbo].[ExperianNonLimitedResults] ([RefNumber], [IsActive])
CREATE INDEX IX_ExperianLtdPrevCompanyNames_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdPrevCompanyNames] ([ExperianLtdID])
CREATE INDEX IX_Security_User_EMail ON [ezbob].[dbo].[Security_User] ([EMail])
CREATE INDEX IX_ExperianLtdShareholders_ExperianLtdID ON [ezbob].[dbo].[ExperianLtdShareholders] ([ExperianLtdID])
CREATE INDEX IX_LandRegistry_CustomerIdRequestType ON [ezbob].[dbo].[LandRegistry] ([CustomerId], [RequestType])
CREATE INDEX IX_CustomerAnalyticsPersonal_CustomerIDIsActive ON [ezbob].[dbo].[CustomerAnalyticsPersonal] ([CustomerID],[IsActive])
CREATE INDEX IX_Loan_DateAmountFee ON [ezbob].[dbo].[Loan] ([Date], [LoanAmount], [SetupFee])
CREATE INDEX IX_Customer_BrokerID ON [ezbob].[dbo].[Customer] ([BrokerID])

END
GO

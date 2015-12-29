SET QUOTED_IDENTIFIER ON
GO

SET ANSI_NULLS ON
GO

-------------------------------------------------------------------------------
-------------------------------------------------------------------------------

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanBrokerCommission') AND name = 'NLLoanID')
	ALTER TABLE LoanBrokerCommission ADD NLLoanID BIGINT NULL
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'LoanHistoryID')
	ALTER TABLE CollectionLog ADD LoanHistoryID BIGINT NULL
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('CollectionLog') AND name = 'Comments')
	ALTER TABLE CollectionLog ADD Comments NVARCHAR(MAX) NULL
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLDecisionID')
	ALTER TABLE DecisionTrail ADD NLDecisionID BIGINT NULL
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('Esignatures') AND name = 'DecisionID')
	ALTER TABLE Esignatures ADD DecisionID BIGINT NULL
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('DecisionTrail') AND name = 'NLCashRequestID')
	ALTER TABLE DecisionTrail ADD NLCashRequestID BIGINT NULL
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_CollectionLog_NL_LoanHistory')
	ALTER TABLE CollectionLog ADD CONSTRAINT FK_CollectionLog_NL_LoanHistory FOREIGN KEY (LoanHistoryID) REFERENCES NL_LoanHistory (LoanHistoryID)
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_Esignatures_NL_Decisions')
	ALTER TABLE Esignatures ADD CONSTRAINT FK_Esignatures_NL_Decisions FOREIGN KEY (DecisionID) REFERENCES NL_Decisions (DecisionID)
GO

IF (SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID AND ob.name = 'MedalCalculationsAV' AND cl.name = 'NLCashRequestID') IS NULL
	ALTER TABLE MedalCalculationsAV ADD NLCashRequestID BIGINT NULL
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_MedalCalculationsAV_NL_CashRequests')
	ALTER TABLE MedalCalculationsAV ADD CONSTRAINT FK_MedalCalculationsAV_NL_CashRequests FOREIGN KEY (NLCashRequestID) REFERENCES NL_CashRequests (CashRequestID)
GO

IF (SELECT cl.OBJECT_ID FROM sys.all_objects ob inner join sys.all_columns cl on ob.OBJECT_ID = cl.OBJECT_ID AND ob.name = 'MedalCalculations' AND cl.name = 'NLCashRequestID') IS NULL
	ALTER TABLE MedalCalculations ADD NLCashRequestID BIGINT NULL
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_MedalCalculations_NL_CashRequests')
	ALTER TABLE MedalCalculations ADD CONSTRAINT FK_MedalCalculations_NL_CashRequests FOREIGN KEY (NLCashRequestID) REFERENCES NL_CashRequests (CashRequestID)
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_DecisionTrail_NL_CashRequests')
	ALTER TABLE DecisionTrail ADD CONSTRAINT FK_DecisionTrail_NL_CashRequests FOREIGN KEY(NLCashRequestID) REFERENCES NL_CashRequests (CashRequestID)
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' AND name = 'FK_LoanBrokerCommission_NL_Loan')
	ALTER TABLE LoanBrokerCommission ADD CONSTRAINT FK_LoanBrokerCommission_NL_Loan FOREIGN KEY (NLLoanID) REFERENCES NL_Loans (LoanID)
GO

IF NOT EXISTS (SELECT id FROM syscolumns WHERE id = OBJECT_ID('LoanAgreementTemplate') AND name = 'TemplateTypeID')
BEGIN
	ALTER TABLE LoanAgreementTemplate ADD TemplateTypeID INT NULL CONSTRAINT DF_LoanAgreementTemplate_TemplateTypeID DEFAULT (1)
	EXECUTE('UPDATE LoanAgreementTemplate SET TemplateTypeID = TemplateType')
END
GO

IF NOT EXISTS (SELECT OBJECT_ID FROM sys.all_objects WHERE type_desc = 'FOREIGN_KEY_CONSTRAINT' and name = 'FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes')
	ALTER TABLE LoanAgreementTemplate ADD CONSTRAINT FK_LoanAgreementTemplate_NL_LoanAgreementTemplateTypes FOREIGN KEY (TemplateTypeID) REFERENCES NL_LoanAgreementTemplateTypes (TemplateTypeID)
GO

update [dbo].[LoanSource] set [IsDefault] = 0 where [LoanSourceName]= 'Standard';
update [dbo].[LoanSource] set [IsDefault] = 1 where [LoanSourceName]= 'COSME'; 


IF EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_Collector_MAX_INTERVALS' ) BEGIN 	
	DELETE FROM [dbo].[ConfigurationVariables]  WHERE  Name = 'RescheduleOutOfLoan_Collector_MAX_INTERVALS';
END;

IF EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_Collector_MAX_AMOUNT' ) BEGIN 	
	DELETE FROM [dbo].[ConfigurationVariables]  WHERE  Name = 'RescheduleOutOfLoan_Collector_MAX_AMOUNT';
END;

IF EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_CollectorSenior_MAX_INTERVALS' ) BEGIN 	
	DELETE FROM [dbo].[ConfigurationVariables]  WHERE  Name = 'RescheduleOutOfLoan_CollectorSenior_MAX_INTERVALS';
END;

IF EXISTS( SELECT [Id] FROM [dbo].[ConfigurationVariables] WHERE [Name] = 'RescheduleOutOfLoan_CollectorSenior_MAX_AMOUNT' ) BEGIN 	
	DELETE FROM [dbo].[ConfigurationVariables]  WHERE  Name = 'RescheduleOutOfLoan_CollectorSenior_MAX_AMOUNT';
END;



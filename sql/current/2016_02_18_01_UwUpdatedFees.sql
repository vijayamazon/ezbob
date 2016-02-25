SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('CashRequests') AND name = 'UwUpdatedFees')
BEGIN
	ALTER TABLE CashRequests DROP COLUMN TimestampCounter

	ALTER TABLE CashRequests ADD UwUpdatedFees BIT NULL

	EXECUTE('UPDATE CashRequests SET UwUpdatedFees = 1')

	EXECUTE('ALTER TABLE CashRequests ALTER COLUMN UwUpdatedFees BIT NOT NULL')

	ALTER TABLE CashRequests ADD TimestampCounter ROWVERSION
END
GO

-------------------------------------------------------------------------------
--
-- LoanLegal table.
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanLegal') AND name = 'TimestampCounter')
	ALTER TABLE LoanLegal DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanLegal') AND name = 'ManualSetupFeePercent')
BEGIN
	ALTER TABLE LoanLegal ADD ManualSetupFeePercent DECIMAL(18, 4) NULL

	EXECUTE('UPDATE LoanLegal SET ManualSetupFeePercent = c.ManualSetupFeePercent FROM LoanLegal l INNER JOIN CashRequests c ON l.CashRequestsID = c.Id')
END
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('LoanLegal') AND name = 'BrokerSetupFeePercent')
BEGIN
	ALTER TABLE LoanLegal ADD BrokerSetupFeePercent DECIMAL(18, 4) NULL

	EXECUTE('UPDATE LoanLegal SET BrokerSetupFeePercent = c.BrokerSetupFeePercent FROM LoanLegal l INNER JOIN CashRequests c ON l.CashRequestsID = c.Id')
END
GO

ALTER TABLE LoanLegal ADD TimestampCounter ROWVERSION
GO

-------------------------------------------------------------------------------
--
-- NL_LoanLegals table.
--
-------------------------------------------------------------------------------

IF EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('NL_LoanLegals') AND name = 'TimestampCounter')
	ALTER TABLE NL_LoanLegals DROP COLUMN TimestampCounter
GO

IF NOT EXISTS (SELECT * FROM syscolumns WHERE id = OBJECT_ID('NL_LoanLegals') AND name = 'BrokerSetupFeePercent')
BEGIN
	ALTER TABLE NL_LoanLegals ADD BrokerSetupFeePercent DECIMAL(18, 4) NULL

	EXECUTE('UPDATE NL_LoanLegals SET BrokerSetupFeePercent = c.BrokerSetupFeePercent FROM NL_LoanLegals l INNER JOIN NL_Offers c ON l.OfferID = c.OfferID')
END
GO

ALTER TABLE NL_LoanLegals ADD TimestampCounter ROWVERSION
GO

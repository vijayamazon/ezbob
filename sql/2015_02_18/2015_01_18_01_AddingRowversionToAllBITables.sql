IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Bug'))
BEGIN
	ALTER TABLE Bug ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Campaign'))
BEGIN
	ALTER TABLE Campaign ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CampaignClients'))
BEGIN
	ALTER TABLE CampaignClients ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CampaignSourceRef'))
BEGIN
	ALTER TABLE CampaignSourceRef ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CampaignType'))
BEGIN
	ALTER TABLE CampaignType ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CardInfo'))
BEGIN
	ALTER TABLE CardInfo ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CashRequests'))
BEGIN
	ALTER TABLE CashRequests ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Company'))
BEGIN
	ALTER TABLE Company ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CompanyEmployeeCount'))
BEGIN
	ALTER TABLE CompanyEmployeeCount ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CRMActions'))
BEGIN
	ALTER TABLE CRMActions ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CRMRanks'))
BEGIN
	ALTER TABLE CRMRanks ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CRMStatuses'))
BEGIN
	ALTER TABLE CRMStatuses ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CRMStatusGroup'))
BEGIN
	ALTER TABLE CRMStatusGroup ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Customer'))
BEGIN
	ALTER TABLE Customer ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerAddress'))
BEGIN
	ALTER TABLE CustomerAddress ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerInviteFriend'))
BEGIN
	ALTER TABLE CustomerInviteFriend ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerPropertyStatuses'))
BEGIN
	ALTER TABLE CustomerPropertyStatuses ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerPropertyStatusGroups'))
BEGIN
	ALTER TABLE CustomerPropertyStatusGroups ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerReason'))
BEGIN
	ALTER TABLE CustomerReason ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerRelations'))
BEGIN
	ALTER TABLE CustomerRelations ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerRequestedLoan'))
BEGIN
	ALTER TABLE CustomerRequestedLoan ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerSession'))
BEGIN
	ALTER TABLE CustomerSession ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerStatuses'))
BEGIN
	ALTER TABLE CustomerStatuses ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('CustomerStatusHistory'))
BEGIN
	ALTER TABLE CustomerStatusHistory ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('DecisionHistory'))
BEGIN
	ALTER TABLE DecisionHistory ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('DecisionHistoryRejectReason'))
BEGIN
	ALTER TABLE DecisionHistoryRejectReason ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Director'))
BEGIN
	ALTER TABLE Director ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('DiscountPlan'))
BEGIN
	ALTER TABLE DiscountPlan ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('ExperianConsumerData'))
BEGIN
	ALTER TABLE ExperianConsumerData ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('ExperianNonLimitedResults'))
BEGIN
	ALTER TABLE ExperianNonLimitedResults ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('ExperianNonLimitedResultSicCodes'))
BEGIN
	ALTER TABLE ExperianNonLimitedResultSicCodes ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Loan'))
BEGIN
	ALTER TABLE Loan ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanChangesHistory'))
BEGIN
	ALTER TABLE LoanChangesHistory ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanCharges'))
BEGIN
	ALTER TABLE LoanCharges ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanSchedule'))
BEGIN
	ALTER TABLE LoanSchedule ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanScheduleTransaction'))
BEGIN
	ALTER TABLE LoanScheduleTransaction ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanTransaction'))
BEGIN
	ALTER TABLE LoanTransaction ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanTransactionMethod'))
BEGIN
	ALTER TABLE LoanTransactionMethod ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('LoanType'))
BEGIN
	ALTER TABLE LoanType ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('MP_CustomerMarketPlace'))
BEGIN
	ALTER TABLE MP_CustomerMarketPlace ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('MP_MarketplaceType'))
BEGIN
	ALTER TABLE MP_MarketplaceType ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('PaymentRollover'))
BEGIN
	ALTER TABLE PaymentRollover ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('RejectReason'))
BEGIN
	ALTER TABLE RejectReason ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('SicNaceCodeMap'))
BEGIN
	ALTER TABLE SicNaceCodeMap ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('SiteAnalytics'))
BEGIN
	ALTER TABLE SiteAnalytics ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('SiteAnalyticsCodes'))
BEGIN
	ALTER TABLE SiteAnalyticsCodes ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('Security_User'))
BEGIN
	ALTER TABLE Security_User ADD TimestampCounter ROWVERSION
END
GO
IF NOT EXISTS (SELECT 1 FROM syscolumns WHERE name='TimestampCounter' AND id=object_id('VipRequest'))
BEGIN
	ALTER TABLE VipRequest ADD TimestampCounter ROWVERSION
END
GO
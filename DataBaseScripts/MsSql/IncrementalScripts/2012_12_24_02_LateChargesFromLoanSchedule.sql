/*
ALTER TABLE dbo.LoanSchedule
	DROP CONSTRAINT DF_LoanSchedule_FirstLateChargeApplyed
GO
ALTER TABLE dbo.LoanSchedule
	DROP CONSTRAINT DF_LoanSchedule_SecondLateChargeApplyed
GO
ALTER TABLE dbo.LoanSchedule
	DROP COLUMN FirstLateChargeApplyed, SecondLateChargeApplyed
*/
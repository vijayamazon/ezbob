IF NOT EXISTS(SELECT 1 FROM sys.columns WHERE Name = N'CustomInstallmentDate' and Object_ID = Object_ID(N'LoanSchedule'))
ALTER TABLE dbo.LoanSchedule ADD CustomInstallmentDate date NULL
GO

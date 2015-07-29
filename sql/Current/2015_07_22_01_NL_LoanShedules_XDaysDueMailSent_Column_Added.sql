IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'FiveDaysDueMailSent' AND Object_ID = Object_ID(N'NL_LoanSchedules'))
BEGIN
	ALTER TABLE NL_LoanSchedules ADD FiveDaysDueMailSent BIT
END
GO

IF NOT EXISTS(SELECT * FROM sys.columns WHERE Name = N'TwoDaysDueMailSent' AND Object_ID = Object_ID(N'NL_LoanSchedules'))
BEGIN
	ALTER TABLE NL_LoanSchedules ADD TwoDaysDueMailSent BIT
END
GO

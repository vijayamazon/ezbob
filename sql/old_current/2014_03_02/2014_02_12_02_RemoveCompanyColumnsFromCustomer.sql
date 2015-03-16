IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LimitedCompanyNumber' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN LimitedCompanyNumber
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LimitedCompanyName' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN LimitedCompanyName
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LimitedTimeAtAddress' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN LimitedTimeAtAddress
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LimitedBusinessPhone' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN LimitedBusinessPhone
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'LimitedRefNum' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN LimitedRefNum
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NonLimitedCompanyName' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN NonLimitedCompanyName
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NonLimitedTimeInBusiness' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN NonLimitedTimeInBusiness
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NonLimitedTimeAtAddress' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN NonLimitedTimeAtAddress
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NonLimitedBusinessPhone' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN NonLimitedBusinessPhone
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'NonLimitedRefNum' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN NonLimitedRefNum
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'PropertyOwnedByCompany' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN PropertyOwnedByCompany
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'YearsInCompany' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN YearsInCompany
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'RentMonthsLeft' and Object_ID = Object_ID(N'Customer'))
BEGIN
	ALTER TABLE Customer DROP COLUMN RentMonthsLeft
END 
GO
IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'CapitalExpenditure' and Object_ID = Object_ID(N'Customer'))
BEGIN
	DECLARE @DefaultConstraint sysname

	SELECT @DefaultConstraint = dc.name
	FROM sys.default_constraints dc
	INNER JOIN syscolumns c
		ON dc.parent_column_id = c.colorder
		AND dc.parent_object_id = c.id
		AND c.name = 'CapitalExpenditure'
		AND c.id = OBJECT_ID('Customer')

	EXEC('ALTER TABLE Customer DROP CONSTRAINT ' + @DefaultConstraint)
	
	ALTER TABLE Customer DROP COLUMN CapitalExpenditure
END 
GO

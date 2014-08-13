IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'EliminationPassed' and Object_ID = Object_ID(N'MP_CustomerMarketPlace'))
BEGIN 
	ALTER TABLE MP_CustomerMarketPlace DROP COLUMN EliminationPassed
END 
GO

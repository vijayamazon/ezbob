
SET QUOTED_IDENTIFIER ON
GO

IF object_id('BankAccountWhiteList') IS NULL
BEGIN
	CREATE TABLE BankAccountWhiteList (
		BankAccountWhiteListID INT NOT NULL IDENTITY(1,1),
		BankAccountNumber NVARCHAR(10),
		Sortcode NVARCHAR(10),
		TimestampCounter ROWVERSION
		CONSTRAINT PK_BankAccountWhiteList PRIMARY KEY (BankAccountWhiteListID),
	)
END
GO

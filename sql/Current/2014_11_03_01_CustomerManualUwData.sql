SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('CustomerManualUwData') IS NULL
BEGIN
	CREATE TABLE CustomerManualUwData (
		EntryID BIGINT IDENTITY(1, 1) NOT NULL,
		CustomerID INT NOT NULL,
		IsActive BIT NOT NULL,
		EntryTime DATETIME NOT NULL,
		AnnualTurnover DECIMAL(18, 2) NOT NULL,
		Comment NVARCHAR(255) NOT NULL,
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerManualUwData PRIMARY KEY (EntryID)
	)
END
GO

IF NOT EXISTS(SELECT * FROM sys.indexes WHERE name = 'IDX_CustomerManualUwData')
	CREATE NONCLUSTERED INDEX IDX_CustomerManualUwData ON CustomerManualUwData(CustomerID) WHERE IsActive = 1
GO

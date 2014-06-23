SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('EsignerStatus') IS NULL
	CREATE TYPE EsignerStatus AS TABLE (
		EsignerID INT NOT NULL,
		StatusID INT NOT NULL,
		SignatureTime DATETIME NULL
	)
GO

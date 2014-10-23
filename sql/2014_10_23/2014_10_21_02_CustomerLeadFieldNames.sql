SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

IF NOT EXISTS (SELECT * FROM sysobjects WHERE name = 'CustomerLeadFieldNames')
BEGIN
	CREATE TABLE CustomerLeadFieldNames (
		LinkID BIGINT IDENTITY(1, 1) NOT NULL,
		UiControlName NVARCHAR(255),
		LeadDatumFieldName NVARCHAR(255),
		TimestampCounter ROWVERSION,
		CONSTRAINT PK_CustomerLeadFieldNames PRIMARY KEY (LinkID),
		CONSTRAINT UC_CustomerLeadFieldNames UNIQUE (UiControlName)
	)
END
GO

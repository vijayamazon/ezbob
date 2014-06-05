SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('VatReturnRawRecordList') IS NULL
BEGIN
	CREATE TYPE VatReturnRawRecordList AS TABLE (
		DateFrom DATETIME,
		DateTo DATETIME,
		DateDue DATETIME,
		Period NVARCHAR(256),
		RegistrationNo BIGINT,
		BusinessName NVARCHAR(256),
		Address NVARCHAR(4000),
		RecordID INT,
		SourceID INT,
		IsDeleted BIT,
		InternalID UNIQUEIDENTIFIER
	)
END
GO

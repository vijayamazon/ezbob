SET QUOTED_IDENTIFIER ON
GO

IF TYPE_ID('VatReturnHistoryList') IS NULL
BEGIN
	CREATE TYPE VatReturnHistoryList AS TABLE (
		DeleteRecordInternalID UNIQUEIDENTIFIER,
		ReasonRecordID INT,
		ReasonID INT
	)
END
GO

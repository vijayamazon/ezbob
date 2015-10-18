IF OBJECT_ID('PacnetTopUpConfirmationRequest') IS NULL
BEGIN
	CREATE TABLE [PacnetTopUpConfirmationRequest] (
		[PacnetTopUpConfirmationRequestID] BIGINT NOT NULL IDENTITY(1,1),		
		[UnderwriterID] INT NULL,
		[Amount] INT NULL,
		[DateSent] DATETIME NULL,
		[DateConfirmed] DATETIME NULL,
		[TimestampCounter] ROWVERSION
	)
END
GO


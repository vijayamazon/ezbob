IF OBJECT_ID('SavePacnetTopUpConfirmationRequest') IS NULL
	EXECUTE('CREATE PROCEDURE SavePacnetTopUpConfirmationRequest AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SavePacnetTopUpConfirmationRequest
@UnderwriterID INT,
@Amount DECIMAL(18, 2),
@DateSent DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	INSERT INTO PacnetTopUpConfirmationRequest (UnderwriterID, Amount, DateSent)
		VALUES (@UnderwriterID, @Amount, @DateSent)	
END
GO
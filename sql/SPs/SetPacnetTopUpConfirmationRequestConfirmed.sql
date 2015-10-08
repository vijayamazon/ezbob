IF OBJECT_ID('SetPacnetTopUpConfirmationRequestConfirmed') IS NULL
	EXECUTE('CREATE PROCEDURE SetPacnetTopUpConfirmationRequestConfirmed AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SetPacnetTopUpConfirmationRequestConfirmed
@DateSent DATETIME,
@DateConfirmed DATETIME
AS
BEGIN
UPDATE PacnetTopUpConfirmationRequest SET
		DateConfirmed = @DateConfirmed
	WHERE
		DateSent = @DateSent
END
GO
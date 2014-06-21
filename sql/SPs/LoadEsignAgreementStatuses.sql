IF OBJECT_ID('LoadEsignAgreementStatuses') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEsignAgreementStatuses AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadEsignAgreementStatuses
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		StatusID,
		StatusName,
		IsTerminal
	FROM
		EsignAgreementStatus
END
GO

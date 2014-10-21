IF OBJECT_ID('LoadCustomerLeadFieldNames') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerLeadFieldNames AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerLeadFieldNames
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		LinkID,
		UiControlName,
		LeadDatumFieldName
	FROM
		CustomerLeadFieldNames
END
GO

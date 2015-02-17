SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadEsignTemplatesByCustomer') IS NULL
	EXECUTE('CREATE PROCEDURE LoadEsignTemplatesByCustomer AS SELECT 1')
GO

ALTER PROCEDURE LoadEsignTemplatesByCustomer
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		t.EsignTemplateID,
		t.EsignTemplateTypeID,
		tt.DocumentName,
		tt.FileNameBase
	FROM
		EsignTemplates t
		INNER JOIN EsignTemplateTypes tt ON t.EsignTemplateTypeID = tt.EsignTemplateTypeID
		INNER JOIN Customer c ON t.CustomerOriginID = c.OriginID
	WHERE
		c.Id = @CustomerID
END
GO

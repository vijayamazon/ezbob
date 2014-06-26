IF OBJECT_ID('LoadCustomerEsignatures') IS NULL
	EXECUTE('CREATE PROCEDURE LoadCustomerEsignatures AS SELECT 1')

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadCustomerEsignatures
@CustomerID INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		e.CustomerID,
		e.EsignatureID,
		e.SendDate,
		e.EsignTemplateID,
		tt.DocumentName,
		e.StatusID AS SignatureStatusID,
		s.StatusName AS SignatureStatus,
		CONVERT(BIT, CASE WHEN e.SignedDocument IS NULL THEN 0 ELSE 1 END) AS HasDocument,
		ed.EsignerID,
		ed.DirectorID,
		ISNULL(d.Email, c.Name) AS SignerEmail,
		ISNULL(d.Name, c.FirstName) AS SignerFirstName,
		ISNULL(d.Surname, c.Surname) AS SignerLastName,
		ed.StatusID AS SignerStatusID,
		es.StatusName AS SignerStatus,
		ed.SignDate
	FROM
		Esignatures e
		INNER JOIN Customer c ON e.CustomerID = c.Id
		INNER JOIN EsignAgreementStatus s ON e.StatusID = s.StatusID
		INNER JOIN Esigners ed ON e.EsignatureID = ed.EsignatureID
		INNER JOIN EsignUserAgreementStatus es ON ed.StatusID = es.StatusID
		INNER JOIN EsignTemplates t ON e.EsignTemplateID = t.EsignTemplateID
		INNER JOIN EsignTemplateTypes tt ON t.EsignTemplateTypeID = tt.EsignTemplateTypeID
		LEFT JOIN Director d ON ed.DirectorID = d.id
	WHERE
		@CustomerID IS NULL OR e.CustomerID = @CustomerID
	ORDER BY
		e.CustomerID,
		e.EsignatureID,
		ed.EsignerID
END
GO

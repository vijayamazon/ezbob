IF OBJECT_ID('LoadPendingEsignatures') IS NULL
	EXECUTE('CREATE PROCEDURE LoadPendingEsignatures AS SELECT 1')

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE LoadPendingEsignatures
@CustomerID INT = NULL
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		e.EsignatureID,
		e.CustomerID,
		e.DocumentKey,
		ed.EsignerID,
		ed.DirectorID,
		ISNULL(d.Email, c.Name) AS DirectorEmail
	FROM
		Esignatures e
		INNER JOIN Customer c ON e.CustomerID = c.Id
		INNER JOIN EsignAgreementStatus s ON e.StatusID = s.StatusID
		INNER JOIN Esigners ed ON e.EsignatureID = ed.EsignatureID
		LEFT JOIN Director d ON ed.DirectorID = d.id
	WHERE
		s.IsTerminal != 1
		AND
		(@CustomerID IS NULL OR e.CustomerID = @CustomerID)
	ORDER BY
		e.EsignatureID,
		ed.EsignerID
END
GO

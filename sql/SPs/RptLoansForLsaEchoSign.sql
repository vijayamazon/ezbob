IF OBJECT_ID('RptLoansForLsaEchoSign') IS NULL
	EXECUTE('CREATE PROCEDURE RptLoansForLsaEchoSign AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptLoansForLsaEchoSign
AS
BEGIN
	SET NOCOUNT ON;

	SELECT DISTINCT
		LoanID = l.RefNum,
		es.CustomerID,
		es.SendDate,
		ess.StatusName,
		estt.FileNameBase,
		es.SignedDocument
	FROM
		Loan l
		INNER JOIN PollenLoans lsa ON l.Id = lsa.LoanID
		INNER JOIN Esignatures es ON es.CustomerID = l.CustomerID AND l.[Date] > es.SendDate
		INNER JOIN EsignAgreementStatus ess ON es.StatusID = ess.StatusID
		INNER JOIN EsignTemplates est ON es.EsignTemplateID = est.EsignTemplateID
		INNER JOIN EsignTemplateTypes estt ON est.EsignTemplateTypeID = estt.EsignTemplateTypeID
END
GO

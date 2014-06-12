IF OBJECT_ID('RptBrokerInvoice') IS NULL
	EXECUTE('CREATE PROCEDURE RptBrokerInvoice AS SELECT 1')
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptBrokerInvoice
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT
		C.Fullname,
		B.FirmName,
		B.ContactName,
		B.ContactEmail,
		L.SetupFee,
		L.LoanAmount
	FROM
		Customer C
		INNER JOIN Broker B ON B.BrokerID = C.BrokerID
		INNER JOIN Loan L ON L.CustomerId = C.Id
	WHERE
		C.IsTest = 0
		AND
		C.BrokerID IS NOT NULL
		AND
		@DateStart <= L.[Date] AND L.[Date] < @DateEnd
END
GO

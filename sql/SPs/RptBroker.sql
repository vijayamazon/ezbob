IF OBJECT_ID('RptBroker') IS NULL
	EXECUTE('CREATE PROCEDURE RptBroker AS SELECT 1')
GO

ALTER PROCEDURE RptBroker
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	SET NOCOUNT ON;

	SELECT 
		B.ContactName Name,
		B.FirmName Company,
		B.ContactMobile Mobile,
		B.ContactOtherPhone Phone,
		B.ContactEmail Email,
		B.AgreedToTermsDate SignUpDate,
		count(DISTINCT C.Id) NumOfClients,
		count(L.Id) NumOfLoans,
		sum(L.LoanAmount) ValueOfLoans,
		sum(L.SetupFee) ValueOfCommission
	FROM 
		Broker B LEFT JOIN Customer C ON C.BrokerID = B.BrokerID LEFT JOIN Loan L ON L.CustomerId = C.Id
	WHERE
		@DateStart <= B.AgreedToTermsDate AND B.AgreedToTermsDate < @DateEnd
		AND
		B.IsTest = 0
	GROUP BY B.ContactName, B.FirmName, B.ContactMobile, B.ContactOtherPhone, B.ContactEmail, B.AgreedToTermsDate
	ORDER BY
		B.AgreedToTermsDate DESC
END
GO
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
		B.BrokerID BrokerID,
		B.ContactName Name,
		B.FirmName Company,
		B.ContactMobile Mobile,
		B.ContactOtherPhone Phone,
		B.ContactEmail Email,
		u.CreationDate SignUpDate,
		CASE WHEN count(DISTINCT C.Id) = 0 THEN NULL ELSE count(DISTINCT C.Id) END NumOfClients,
		CASE WHEN count(L.Id) = 0 THEN NULL ELSE count(L.Id) END NumOfLoans,
		CAST(sum(L.LoanAmount) AS INT) ValueOfLoans,
		CAST(sum(L.SetupFee) AS INT) ValueOfCommission,
		B.ReferredBy AS SourceRef
	FROM 
		Broker B
		INNER JOIN Security_User u ON B.BrokerID = u.UserId
		LEFT JOIN Customer C ON C.BrokerID = B.BrokerID
		LEFT JOIN Loan L ON L.CustomerId = C.Id
	WHERE
		@DateStart <= u.CreationDate AND u.CreationDate < @DateEnd
		AND
		B.IsTest = 0
	GROUP BY B.BrokerID, B.ContactName, B.FirmName, B.ContactMobile, B.ContactOtherPhone, B.ContactEmail, u.CreationDate, B.ReferredBy
	ORDER BY
		u.CreationDate DESC
END
GO

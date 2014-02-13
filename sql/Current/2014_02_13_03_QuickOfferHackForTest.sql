IF OBJECT_ID('QuickOfferHackForTest') IS NULL
	EXECUTE('CREATE PROCEDURE QuickOfferHackForTest AS SELECT 1')
GO

ALTER PROCEDURE QuickOfferHackForTest
@CustomerID INT,
@BusinessScore INT
AS
BEGIN
	IF NOT EXISTS (SELECT * FROM Customer WHERE Id = @CustomerID AND IsTest = 1)
	BEGIN
		RAISERROR('Customer %d was not found or is not a test customer.', 11, 1, @CustomerID)
		RETURN
	END

	DECLARE @CompanyRefNum NVARCHAR(50) = 'EB' + RIGHT('00000000' + CONVERT(NVARCHAR, @CustomerID), 8)
	DECLARE @IncorpDate NVARCHAR(8)

	SELECT
		@IncorpDate = CONVERT(NVARCHAR, DATEADD(month, - qoc.CompanySeniorityMonths - 6, GETUTCDATE()), 112)
	FROM
		QuickOfferConfiguration qoc
	WHERE
		ID = 1

	INSERT INTO MP_ExperianDataCache
	SELECT
		c.FirstName,
		c.Surname,
		'AB101BA',
		DATEADD(year, - qoc.ApplicantMinAgeYears - 6, GETUTCDATE()),
		DATEADD(hour, 8, GETUTCDATE()),
		'{"Output":{"FullConsumerData":{"ConsumerData":{"CAIS":[{"CAISDetails":[{}]}]}}}}',
		'{"Request":"Quick Offer Hack for Test"}',
		NULL,
		qoc.PersonalScoreMin + 36,
		NULL,
		NULL,
		NULL,
		NULL,
		@CustomerID,
		0
	FROM
		QuickOfferConfiguration qoc
		INNER JOIN Customer c ON c.Id = @CustomerID
	WHERE
		qoc.ID = 1

	UPDATE Customer SET FraudStatus = 0 WHERE Id = @CustomerID

	INSERT INTO MP_ServiceLog (ServiceType, InsertDate, RequestData, ResponseData, CustomerId, DirectorId)
	SELECT
		'AML A check',
		DATEADD(hour, 8, GETUTCDATE()),
		'<request>Quick Offer Hack for Test</request>',
		
		'<?xml version="1.0" encoding="utf-16"?><ProcessConfigResponseType xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" xmlns:xsd="http://www.w3.org/2001/XMLSchema"><ProcessConfigResultsBlock xmlns="http://schema.uk.experian.com/eih/2011/03"><EIAResultBlock><AuthenticationIndex>'
		+ CONVERT(NVARCHAR, qoc.AmlMin + 6) +
		'</AuthenticationIndex></EIAResultBlock></ProcessConfigResultsBlock></ProcessConfigResponseType>',
		
		@CustomerID,
		NULL
	FROM
		QuickOfferConfiguration qoc
	WHERE
		qoc.ID = 1

	INSERT INTO MP_ExperianDataCache (
		Name, Surname, PostCode, BirthDate, LastUpdateDate,
		JsonPacket,
		JsonPacketInput,
		ExperianError, ExperianScore, ExperianResult, ExperianWarning, ExperianReject,
		CompanyRefNumber, CustomerId, DirectorId
	)
	SELECT
		null, null, null, null, DATEADD(hour, 8, GETUTCDATE()),
		'<?xml version="1.0" standalone="yes" ?><GEODS><REQUEST><DL72><DIRFORENAME>' +
		c.FirstName +
		'</DIRFORENAME><DIRSURNAME>' +
		c.Surname +
		'</DIRSURNAME></DL72><DL76><RISKSCORE>' +
		CONVERT(NVARCHAR, @BusinessScore) +
		'</RISKSCORE></DL76><DL12><DATEINCORP-YYYY>' +
		SUBSTRING(@IncorpDate, 1, 4) +
		'</DATEINCORP-YYYY><DATEINCORP-MM>' +
		SUBSTRING(@IncorpDate, 5, 2) +
		'</DATEINCORP-MM><DATEINCORP-DD>' +
		SUBSTRING(@IncorpDate, 7, 2) +
		'</DATEINCORP-DD></DL12><DL99><TOTALSHAREFUND>200000</TOTALSHAREFUND><TOTALCURRASSETS>100000</TOTALCURRASSETS><DATEOFACCOUNTS-YYYY>2012</DATEOFACCOUNTS-YYYY><DATEOFACCOUNTS-MM>12</DATEOFACCOUNTS-MM><DATEOFACCOUNTS-DD>31</DATEOFACCOUNTS-DD></DL99></REQUEST></GEODS>',
		'{"Request":"Quick Offer Hack for Test"}',
		null, 97, null, null, null,
		@CompanyRefNum, @CustomerID, null
	FROM
		Customer c
	WHERE
		c.Id = @CustomerID

	UPDATE Company SET
		ExperianRefNum = @CompanyRefNum
	FROM
		Company b,
		Customer c
	WHERE
		c.Id = @CustomerID
		AND
		c.CompanyId = b.Id
END
GO

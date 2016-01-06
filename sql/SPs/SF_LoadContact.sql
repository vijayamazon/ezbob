IF object_ID('SF_LoadContact') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE SF_LoadContact AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SF_LoadContact
@CustomerID INT,
@DirectorID INT,
@DirectorEmail NVARCHAR(300)
AS
BEGIN

IF @DirectorEmail IS NOT NULL AND @DirectorID IS NULL
BEGIN
	SELECT @DirectorID = d.id FROM Director d WHERE d.Email = @DirectorEmail
END

IF @DirectorID IS NOT NULL
BEGIN
	SELECT
		c.Name AS Email,
		o.Name AS Origin,
		d.Email AS ContactEmail,
		isnull(d.Name, '') + ' ' + isnull(d.Middle, '') + ' ' + isnull(d.Surname, '') AS Name,
		'Director' AS Type,
		d.Gender AS Gender,
		d.Phone AS PhoneNumber,
		d.DateOfBirth AS DateOfBirth,
		a.Line1 AS AddressLine1,
		a.Line2 AS AddressLine2,
		a.Line3 AS AddressLine3,
		a.Town AS AddressTown,
		a.County AS AddressCounty,
		a.Country AS AddressCountry,
		a.Postcode AS AddressPostcode
	FROM Director d
	INNER JOIN Customer c ON c.Id = d.CustomerId
	LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
	LEFT JOIN CustomerAddress a ON d.id = a.DirectorId AND a.addressType IN (4,6)
	WHERE d.id=@DirectorID
		
	RETURN
END

SELECT 
	c.Name AS Email,
	o.Name AS Origin,
	c.Name AS ContactEmail,
	c.Fullname AS Name,
	'MainApplicant' AS Type,
	c.Gender AS Gender,
	c.DaytimePhone AS PhoneNumber,
	c.DateOfBirth AS DateOfBirth,
	a.Line1 AS AddressLine1,
	a.Line2 AS AddressLine2,
	a.Line3 AS AddressLine3,
	a.Town AS AddressTown,
	a.County AS AddressCounty,
	a.Country AS AddressCountry,
	a.Postcode AS AddressPostcode

FROM Customer c 
LEFT JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType=1
LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
WHERE c.Id=@CustomerID

END
GO

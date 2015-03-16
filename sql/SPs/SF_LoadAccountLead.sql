IF object_ID('SF_LoadAccountLead') IS NULL
BEGIN
	EXECUTE('CREATE PROCEDURE SF_LoadAccountLead AS SELECT 1')
END
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE SF_LoadAccountLead
@CustomerID INT,
@Email NVARCHAR(300),
@IsBrokerLead BIT,
@IsVipLead BIT
AS
BEGIN

-- registered customer
IF @CustomerID IS NOT NULL
BEGIN 
	SELECT 
		c.Name AS Email,
		c.Fullname AS Name,
	 	c.Gender AS Gender,
		c.DaytimePhone AS PhoneNumber,
		c.DateOfBirth AS DateOfBirth,
		a.Line1 AS AddressLine1,
		a.Line2 AS AddressLine2,
		a.Line3 AS AddressLine3,
		a.Town AS AddressTown,
		a.County AS AddressCounty,
		a.Country AS AddressCountry,
		a.Postcode AS AddressPostcode,
		CAST(CASE WHEN c.BrokerID IS NULL THEN 0 ELSE 1 END AS BIT) AS IsBroker,
		c.GreetingMailSentDate AS RegistrationDate,
		co.ExperianCompanyName AS CompanyName,
		co.ExperianRefNum AS CompanyNumber,
		co.TypeOfBusiness AS TypeOfBusiness,
		c.IndustryType AS Industry,
		'Wizard' AS EzbobSource,
		w.WizardStepTypeDescription AS EzbobStatus,
		s.RSource AS LeadSource,
		r.Amount AS RequestedLoanAmount,
		o.Name AS Origin,
		c.IsTest AS IsTest
	FROM Customer c 
	LEFT JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType=1
	LEFT JOIN Company co ON co.Id = c.CompanyId
	INNER JOIN WizardStepTypes w ON w.WizardStepTypeID = c.WizardStep
	LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
	LEFT JOIN CustomerRequestedLoan r ON r.CustomerId = c.Id
	LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
	WHERE c.Id=@CustomerID
	
	RETURN
END 

--- vip request before registation begin
IF @IsVipLead = 1 
BEGIN
	SELECT TOP 1 
		v.Email AS Email,
		v.FullName AS Name,
		v.Phone AS PhoneNumber,
		'VIP' AS EzbobSource,
		v.RequestDate AS RegistrationDate
	FROM VipRequest v
	WHERE v.Email = @Email
	ORDER BY v.Id DESC
	RETURN
END

-- a broker lead
IF @IsBrokerLead = 1 
BEGIN
	SELECT 
		l.Email AS Email,
		isnull(l.FirstName, '') + ' ' + isnull(l.LastName, '') AS Name,
		l.DateCreated AS RegistrationDate,
		'Broker lead' AS EzbobSource,
		CAST(1 AS BIT) IsBroker
	FROM BrokerLeads l
	WHERE Email = @Email
    RETURN	
END

------------ should not reach here
SELECT @Email AS Email,
	   'Unknown' AS EzbobSource

END



GO


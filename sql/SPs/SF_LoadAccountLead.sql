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
IF @CustomerID IS NOT NULL AND @CustomerId <> 0
BEGIN 
	DECLARE @NumOfLoans INT = (SELECT count(*) FROM Loan WHERE CustomerId = @CustomerID)
	DECLARE @RequestedAmount DECIMAL(18,0) = (SELECT TOP 1 Amount FROM CustomerRequestedLoan WHERE CustomerId = @CustomerID ORDER BY Created DESC)
    SELECT 
        c.Name AS Email,
        CAST(@CustomerID AS NVARCHAR(10)) AS CustomerID,
        isnull(c.Fullname, 'NoName') AS Name,
        c.Gender AS Gender,
        c.DaytimePhone AS PhoneNumber,
        c.MobilePhone AS MobilePhoneNumber,
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
        isnull(co.ExperianCompanyName, co.CompanyName) AS CompanyName,
        co.ExperianRefNum AS CompanyNumber,
        co.TypeOfBusiness AS TypeOfBusiness,
        c.IndustryType AS Industry,
        'Wizard' AS EzbobSource,
        w.WizardStepTypeDescription AS EzbobStatus,
        s.RSource AS LeadSource,
        @RequestedAmount AS RequestedLoanAmount,
        o.Name AS Origin,
        c.IsTest AS IsTest,
        @NumOfLoans AS NumOfLoans,
        c.PromoCode AS Promocode,
        b.ContactName AS BrokerName,
        b.FirmName AS BrokerFirmName,
        b.ContactEmail AS BrokerEmail,
        b.ContactMobile AS BrokerPhoneNumber,
	b.BrokerID AS BrokerID,
        cs.Name AS CollectionStatus,
        ecs.Name AS ExternalCollectionStatus
    FROM Customer c 
    LEFT JOIN CustomerAddress a ON c.Id = a.CustomerId AND a.addressType=1
    LEFT JOIN Company co ON co.Id = c.CompanyId
    INNER JOIN WizardStepTypes w ON w.WizardStepTypeID = c.WizardStep
    LEFT JOIN CampaignSourceRef s ON s.CustomerId = c.Id
    LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
    LEFT JOIN Broker b ON b.BrokerID = c.BrokerID
    LEFT JOIN CustomerStatuses cs ON cs.Id = c.CollectionStatus
    LEFT JOIN ExternalCollectionStatuses ecs ON ecs.ExternalCollectionStatusID = c.ExternalCollectionStatusID
    WHERE c.Id=@CustomerID
    
    RETURN
END

--- vip request before registation begin
IF @IsVipLead = 1 
BEGIN
    SELECT TOP 1 
        v.Email AS Email,
        CAST(v.CustomerId AS NVARCHAR(10)) AS CustomerID,
        isnull(v.Fullname, 'NoName') AS Name,
        v.Phone AS PhoneNumber,
        'VIP' AS EzbobSource,
        v.RequestDate AS RegistrationDate,
        'ezbob' AS Origin
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
        CAST(l.CustomerID AS NVARCHAR(10)) AS CustomerID,
        isnull(l.FirstName, '') + ' ' + isnull(l.LastName, '') AS Name,
        l.DateCreated AS RegistrationDate,
        'Broker lead' AS EzbobSource,
        CAST(1 AS BIT) IsBroker,
        b.ContactName AS BrokerName,
        b.FirmName AS BrokerFirmName,
        b.ContactEmail AS BrokerEmail,
        b.ContactMobile AS BrokerPhoneNumber,
	b.BrokerID AS BrokerID,
        o.Name AS Origin
        b.IsTest AS IsTest
    FROM BrokerLeads l 
    INNER JOIN Broker b ON b.BrokerID = l.BrokerID
    INNER JOIN CustomerOrigin o ON o.CustomerOriginID = b.OriginID
    WHERE l.Email = @Email
   RETURN    
END

------------ should not reach here
SELECT @Email AS Email,
       'Unknown' AS EzbobSource

END
GO


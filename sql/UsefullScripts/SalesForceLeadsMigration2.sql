-- customers
;WITH
	crl_id AS (
		SELECT
			CustomerId,
			MAX(Id) AS Id
		FROM
			CustomerRequestedLoan
		GROUP BY
			CustomerId
	),
	crl AS (
		SELECT
			c.CustomerId,
			c.Amount AS Amount
		FROM
			crl_id d
			INNER JOIN CustomerRequestedLoan c ON d.Id = c.Id
	)
SELECT
isnull(c.Surname, 'NoName') AS 'LASTNAME',
c.FirstName AS 'FIRSTNAME',
c.FullName AS 'NAME',
isnull(co.ExperianCompanyName, 'NoCompany') AS 'COMPANY',
isnull(ca.Line1, '') + ' ' + isnull(ca.Line2, '') + ' ' + isnull(ca.Line3, '') AS 'STREET', 
ca.Town AS 'CITY',
ca.County AS 'STATE',
ca.Postcode AS 'POSTALCODE', 
ca.Country AS 'COUNTRY',
isnull(ca.Line1, '') + ' ' + isnull(ca.Line2, '') + ' ' + isnull(ca.Line3, '') + ' ' + isnull(ca.Town, '') + ' ' + isnull(ca.County, '') + ' ' + isnull(ca.Postcode, '') + ' ' + isnull(ca.Country, '') AS 'ADDRESS',
isnull(c.MobilePhone, c.DaytimePhone) AS 'PHONE',
c.Name AS 'EMAIL',
'' AS 'WEBSITE',
'' AS 'DESCRIPTION',
'' AS 'RATING', --(Hot/Warm/Cold)
isnull(cal.AnnualTurnover, 0) AS 'ANNUALREVENUE',
isnull(cec.EmployeeCount, 0) AS 'NUMBEROFEMPLOYEES',
'' AS 'OWNERID',
convert(date, c.GreetingMailSentDate) AS 'CREATEDDATE',
CASE c.TypeOfBusiness 
	WHEN 'Entrepreneur' THEN 'Sole trader (not inc.)'
	WHEN 'Limited' THEN 'Limited company'
	WHEN 'LLP' THEN 'Limited liability partnership'
	WHEN 'PShip' THEN 'Partnership (less than three)'
	WHEN 'PShip3P' THEN 'Partnership (less than three)'
	WHEN 'SoleTrader' THEN 'Sole trader (inc.)'
	ELSE '' END AS 'BUSINESS_TYPE__C',
co.ExperianRefNum AS 'COMPANY_NUMBER__C',
'Wizard' AS 'EZBOB_SOURCE__C',
w.WizardStepTypeDescription AS 'EZBOB_STATUS__C',
CASE c.Gender WHEN 'F' THEN 'Female' WHEN 'M' THEN 'Male' ELSE '' END AS 'GENDER__C',
CASE WHEN c.BrokerID IS NULL THEN 'No' ELSE 'Yes' END 'IS_BROKER__C',
'' AS 'MARKETING_CAMPAIGN_TYPE__C',
CASE cs.RMedium
	WHEN 'affiliate' THEN 'Affiliate' 
	WHEN 'cpc' THEN 'PPC'
	WHEN 'email' THEN 'Mailing'
	WHEN 'Organic' THEN 'Organic'
	WHEN 'Referral' THEN 'Referral'
	WHEN 'referrals' THEN 'Referral'
	ELSE '' END AS 'OFFLINE_ONLINE_CAMPAIGN_TYPE__C',  -- 'Offline Campaign' / 'Online Campaign'
o.Name AS 'ORIGIN__C',
convert(date,c.GreetingMailSentDate) AS	'REGISTRATION_DATE__C',
isnull(crl.Amount,0) AS 'REQUESTED_LOAN_AMOUNT__C',
'Open' AS 'Lead Status',
convert(date,isnull(c.DateOfBirth, '1900-01-01')) AS 'Date Of Birth',
isnull(c.IndustryType, '') AS 'Industry',
c.IsTest AS 'IS_TEST__C'
FROM Customer c
LEFT JOIN CustomerAddress ca ON c.Id = ca.CustomerId AND ca.addressType=1
LEFT JOIN crl ON crl.CustomerId = c.Id
LEFT JOIN Company co ON co.Id = ca.CompanyId
LEFT JOIN CompanyEmployeeCount cec ON cec.CompanyId = co.Id
LEFT JOIN CampaignSourceRef cs ON cs.CustomerId = c.Id
LEFT JOIN CustomerAnalyticsLocalData cal ON cal.CustomerID = c.Id AND cal.IsActive=1
LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID
LEFT JOIN WizardStepTypes w ON w.WizardStepTypeID = c.WizardStep
UNION

--broker leads
SELECT
isnull(bl.LastName, 'NoName') AS 'LASTNAME',
bl.FirstName AS 'FIRSTNAME',
isnull(bl.FirstName, '') + ' ' + isnull(bl.LastName, '')   AS 'NAME',
'NoCompany' AS 'COMPANY',
'' AS 'STREET', 
'' AS 'CITY',
'' AS 'STATE',
'' AS 'POSTALCODE', 
'' AS 'COUNTRY',
'' AS 'ADDRESS',
'' AS 'PHONE',
bl.Email AS 'EMAIL',
'' AS 'WEBSITE',
'' AS 'DESCRIPTION',
'' AS 'RATING', --(Hot/Warm/Cold)
0 AS 'ANNUALREVENUE',
0 AS 'NUMBEROFEMPLOYEES',
'' AS 'OWNERID',
convert(date, bl.DateCreated) AS 'CREATEDDATE',
'' AS 'BUSINESS_TYPE__C',
'' AS 'COMPANY_NUMBER__C',
'Broker lead' AS 'EZBOB_SOURCE__C',
'New' AS 'EZBOB_STATUS__C',
'' AS 'GENDER__C',
'Yes' AS 'IS_BROKER__C',
'' AS 'MARKETING_CAMPAIGN_TYPE__C',
'' AS 'OFFLINE_ONLINE_CAMPAIGN_TYPE__C',  -- 'Offline Campaign' / 'Online Campaign'
'' AS 'ORIGIN__C',
'' AS	'REGISTRATION_DATE__C',
0 AS 'REQUESTED_LOAN_AMOUNT__C',
'Open' AS 'Lead Status',
convert(date,'1900-01-01') AS 'Date Of Birth',
'' AS 'Industry',
CAST(0 AS BIT) AS 'IS_TEST__C'
FROM BrokerLeads bl LEFT JOIN Customer c ON bl.Email = c.Name
WHERE bl.Email IS NOT NULL AND c.Name IS NULL

UNION

--vip
SELECT
isnull(v.FullName, 'NoName') AS 'LASTNAME',
v.FullName AS 'FIRSTNAME',
v.FullName AS 'NAME',
'NoCompany' AS 'COMPANY',
'' AS 'STREET', 
'' AS 'CITY',
'' AS 'STATE',
'' AS 'POSTALCODE', 
'' AS 'COUNTRY',
'' AS 'ADDRESS',
v.Phone AS 'PHONE',
v.Email AS 'EMAIL',
'' AS 'WEBSITE',
'' AS 'DESCRIPTION',
'' AS 'RATING', --(Hot/Warm/Cold)
0 AS 'ANNUALREVENUE',
0 AS 'NUMBEROFEMPLOYEES',
'' AS 'OWNERID',
convert(date, v.RequestDate) AS 'CREATEDDATE',
'' AS 'BUSINESS_TYPE__C',
'' AS 'COMPANY_NUMBER__C',
'VIP' AS 'EZBOB_SOURCE__C',
'New' AS 'EZBOB_STATUS__C',
'' AS 'GENDER__C',
'No' AS 'IS_BROKER__C',
'' AS 'MARKETING_CAMPAIGN_TYPE__C',
'' AS 'OFFLINE_ONLINE_CAMPAIGN_TYPE__C',  -- 'Offline Campaign' / 'Online Campaign'
'' AS 'ORIGIN__C',
'' AS	'REGISTRATION_DATE__C',
0 AS 'REQUESTED_LOAN_AMOUNT__C',
'Open' AS 'Lead Status',
convert(date,'1900-01-01') AS 'Date Of Birth',
'' AS 'Industry',
CAST(0 AS BIT) AS 'IS_TEST__C'
FROM VipRequest v 
LEFT JOIN Customer c ON c.Name = v.Email
WHERE c.Name IS NULL AND v.Email IS NOT NULL AND v.CustomerId IS NULL

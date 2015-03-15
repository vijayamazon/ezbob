SELECT
isnull(c.Surname, 'NoName') AS 'Last Name',
c.FirstName AS 'First Name',
isnull(co.ExperianCompanyName, 'NoCompany') AS 'Company',
isnull(ca.Line1, '') + ' ' + isnull(ca.Line2, '') + ' ' + isnull(ca.Line3, '') AS Street, 
ca.Town AS 'City',
ca.County AS 'State/Provice',
ca.Postcode AS 'Zip/Postal Code', 
ca.Country AS 'Country',
isnull(c.MobilePhone, c.DaytimePhone) AS 'Phone',
c.Name AS 'Email',
'' AS 'Website',
'Open' AS 'Lead Status',
'' AS Rating, --(Hot/Warm/Cold)
isnull(cal.AnnualTurnover, 0) AS 'Annual Revune',
isnull(cec.EmployeeCount, 0) AS 'Number of Employees',
'' AS 'Lead Owner Full Name',
FORMAT(c.GreetingMailSentDate, 'yyyy-MM-dd') AS 'Created Date',
CASE c.TypeOfBusiness 
	WHEN 'Entrepreneur' THEN 'Sole trader (not inc.)'
	WHEN 'Limited' THEN 'Limited company'
	WHEN 'LLP' THEN 'Limited liability partnership'
	WHEN 'PShip' THEN 'Partnership (less than three)'
	WHEN 'PShip3P' THEN 'Partnership (less than three)'
	WHEN 'SoleTrader' THEN 'Sole trader (inc.)'
	ELSE '' END AS 'Business Type',
	
co.ExperianRefNum AS 'Company Number',
'Wizard' AS 'Ezbob Source',
'Open' AS 'Ezbob Status',
CASE c.Gender 
	WHEN 'F' THEN 'Female'
	WHEN 'M' THEN 'Male'
	ELSE '' END AS 'Gender',
o.Name AS 'Origin',
CASE WHEN c.BrokerID IS NULL THEN 'No' ELSE 'Yes' END 'Is Broker',
'' AS 'Marketing Campaign Type',
CASE cs.RMedium
	WHEN 'affiliate' THEN 'Affiliate' 
	WHEN 'cpc' THEN 'PPC'
	WHEN 'email' THEN 'Mailing'
	WHEN 'Organic' THEN 'Organic'
	WHEN 'Referral' THEN 'Referral'
	WHEN 'referrals' THEN 'Referral'
	ELSE '' END AS 'Offline/Online campaign type',
o.Name AS 'Origin',
FORMAT(c.GreetingMailSentDate, 'yyyy-MM-dd') AS	'Registration Date',
isnull(crl.Amount,0) AS 'Requested loan amount',
FORMAT(isnull(c.DateOfBirth, '1900-01-01'), 'yyyy-MM-dd') AS 'Date Of Birth',
CASE c.IndustryType
	WHEN 'AccommodationOrFood' THEN 'Food & Beverage' 
	WHEN 'Automotive' THEN ''
	WHEN 'BusinessServices' THEN ''
	WHEN 'Construction' THEN 'Construction'
	WHEN 'ConstructionServices' THEN 'Construction'
	WHEN 'ConsumerServices' THEN ''
	WHEN 'Education' THEN 'Education'
	WHEN 'Food' THEN 'Food & Beverage'
	WHEN 'HealthBeauty' THEN ''
	WHEN 'Healthcare' THEN 'Healthcare'
	WHEN 'Manufacturing' THEN 'Manufacturing'
	WHEN 'Online' THEN ''
	WHEN 'Other' THEN 'Other'
	WHEN 'Retail' THEN 'Retail'
	WHEN 'Transportation' THEN 'Transportation'
	WHEN 'Wholesale'  THEN ''
	ELSE '' END AS 'Industry'
FROM Customer c
LEFT JOIN CustomerAddress ca ON c.Id = ca.CustomerId AND ca.addressType=1
LEFT JOIN CustomerRequestedLoan crl ON crl.CustomerId = c.Id
LEFT JOIN Company co ON co.Id = ca.CompanyId
LEFT JOIN CompanyEmployeeCount cec ON cec.CompanyId = co.Id
LEFT JOIN CampaignSourceRef cs ON cs.CustomerId = c.Id
LEFT JOIN CustomerAnalyticsLocalData cal ON cal.CustomerID = c.Id AND cal.IsActive=1
LEFT JOIN CustomerOrigin o ON o.CustomerOriginID = c.OriginID

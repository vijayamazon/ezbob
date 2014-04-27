DECLARE @DateStart DATETIME = '2013-09-01'
DECLARE @DateEnd DATETIME = '2014-04-01'

------------------A1_Borrowers---------------------------------------------------
SELECT DISTINCT c.RefNumber AS 'Borrower ID', CASE 
	--Midland east
	WHEN a.Postcode LIKE 'AL%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'CB%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'CM%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'CO%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'EN%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'IG%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'IP%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'LU%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'MK%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'NR%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'PE%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'RM%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'SG%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'SS%' THEN 'UKF' 
	WHEN a.Postcode LIKE 'WD%' THEN 'UKF' 
	
	--Midland west
	WHEN a.Postcode LIKE 'B[0-9]%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'CV%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'DE%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'DY%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'LE%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'NG%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'NN%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'ST%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'WS%' THEN 'UKG' 
	WHEN a.Postcode LIKE 'WV%' THEN 'UKG' 
	
	--Yorkshire
	WHEN a.Postcode LIKE 'BD%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'DL%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'DN%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'HD%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'HG%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'HU%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'HX%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'LS%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'S[0-9]%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'WF%' THEN 'UKE' 
	WHEN a.Postcode LIKE 'YO%' THEN 'UKE' 
	
	--North east
	WHEN a.Postcode LIKE 'DH%' THEN 'UKC' 
	WHEN a.Postcode LIKE 'LN%' THEN 'UKC' 
	WHEN a.Postcode LIKE 'NE%' THEN 'UKC' 
	WHEN a.Postcode LIKE 'SR%' THEN 'UKC' 
	WHEN a.Postcode LIKE 'TS%' THEN 'UKC' 
	
	--North west
	WHEN a.Postcode LIKE 'BB%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'BL%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'CA%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'CW%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'FY%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'L[0-9]%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'LA%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'M[0-9]%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'OL%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'PR%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'SK%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'SY%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'TF%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'WA%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'WN%' THEN 'UKD' 
	WHEN a.Postcode LIKE 'CH%' THEN 'UKD' 
	
	--London
	WHEN a.Postcode LIKE 'E[0-9]%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'EC%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'N[0-9]%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'NW%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'SE%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'SW%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'W[0-9]%' THEN 'UKI' 
	WHEN a.Postcode LIKE 'WC%' THEN 'UKI' 
	
	--South east
	WHEN a.Postcode LIKE 'GU%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'HA%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'HP%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'OX%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'PO%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'RG%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'SL%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'SN%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'SO%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'SP%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'UB%' THEN 'UKJ' 
	
	WHEN a.Postcode LIKE 'BN%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'BR%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'CR%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'CT%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'DA%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'KT%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'ME%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'RH%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'SM%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'TN%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'TW%' THEN 'UKJ' 
	WHEN a.Postcode LIKE 'BA%' THEN 'UKJ' 
	
	--South west
	WHEN a.Postcode LIKE 'BH%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'BS%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'DT%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'EX%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'GL%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'HR%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'PL%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'TA%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'TQ%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'TR%' THEN 'UKK' 
	WHEN a.Postcode LIKE 'WR%' THEN 'UKK' 
	
	--Whales
	WHEN a.Postcode LIKE 'CF%' THEN 'UKL' 
	WHEN a.Postcode LIKE 'LD%' THEN 'UKL' 
	WHEN a.Postcode LIKE 'LL%' THEN 'UKL' 
	WHEN a.Postcode LIKE 'NP%' THEN 'UKL' 
	WHEN a.Postcode LIKE 'SA%' THEN 'UKL' 
	
	--Scotland
	WHEN a.Postcode LIKE 'AB%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'DD%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'DG%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'EH%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'FK%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'G[0-9]%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'HS%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'IV%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'KA%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'KW%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'KY%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'ML%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'PA%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'PH%' THEN 'UKM' 
	WHEN a.Postcode LIKE 'TD%' THEN 'UKM' 	
	WHEN a.Postcode LIKE 'ZE%' THEN 'UKM' 	
	
	--Ireland
	WHEN a.Postcode LIKE 'BT%' THEN 'UKN' 
		
	ELSE 'n/i' END AS 'Region', 'GB' AS Country,  'n/i' AS 'Date of establishment', 'n/i' AS 'Sector (NACE code)', 'Employed' AS 'Employment status', coc.EmployeeCount AS 'Current number of employees', c.OverallTurnOver AS 'Annual turn-over', 1 AS 'Total Assets', c.IndustryType AS 'Comments'
FROM Loan l 
JOIN LoanSource s ON s.LoanSourceID = l.LoanSourceID
JOIN Customer c ON l.CustomerId = c.Id 
JOIN CustomerAddress a ON a.CustomerId = c.Id 
LEFT JOIN Company co ON c.CompanyId = co.Id
LEFT JOIN CompanyEmployeeCount coc ON coc.CompanyId = co.Id
WHERE s.LoanSourceName='EU'
AND a.addressType=1
AND c.IsTest=0
AND l.[Date]>=@DateStart AND l.[Date]<@DateEnd

-----------------A2_Loans--------------------------------------------------------------
SELECT DISTINCT c.RefNumber AS 'Borrower ID', l.RefNum AS 'Loan reference' , 'GBP' AS Currency,  l.LoanAmount AS 'Loan amount', 12 AS 'Loan maturity (months)', CONVERT(VARCHAR(10), l.[Date], 103) AS 'Loan signature date', CONVERT(VARCHAR(10),dateadd(month, 1, l.[Date]), 103) AS 'First disbursement date'
FROM Loan l 
JOIN LoanSource s ON s.LoanSourceID = l.LoanSourceID
JOIN Customer c ON l.CustomerId = c.Id 
LEFT JOIN Company co ON c.CompanyId = co.Id
LEFT JOIN CompanyEmployeeCount coc ON coc.CompanyId = co.Id
WHERE s.LoanSourceName='EU'
AND c.IsTest=0
AND l.[Date]>=@DateStart AND l.[Date]<@DateEnd

-----------------A.4. Number of MC requests/rejections----------------------------------------------------------------------
SELECT COUNT(DISTINCT cr.Id) AS 'A.4.1 Total Number of formal micro-credit requests'
FROM CashRequests cr INNER JOIN Customer c ON cr.IdCustomer=c.Id
WHERE c.IsTest=0
AND cr.CreationDate>=@DateStart 
AND cr.CreationDate<@DateEnd

SELECT COUNT(DISTINCT cr.Id) AS 'A.4.2 Total Number of formal micro-credit rejections' 
FROM CashRequests cr INNER JOIN Customer c ON cr.IdCustomer=c.Id
WHERE c.IsTest=0 
AND cr.CreationDate>=@DateStart 
AND cr.CreationDate<@DateEnd
AND cr.UnderwriterDecision='Rejected'

----------------Part B - List of included operations-------------------------------------------------------------------------
SELECT c.RefNumber AS 'Borrower ID', l.RefNum AS 'Loan reference' , 'GBP' AS Currency,  l.LoanAmount AS 'Nominal loan amount', l.Repayments AS 'Total repayment of loan amount', l.Balance AS 'Outstanding - loan amount', 0 'Remaining loan amount to be disbursed', CASE WHEN l.DateClosed IS NULL THEN 'No' ELSE 'Yes' END AS 'End of disbursement period'
FROM Loan l 
JOIN LoanSource s ON s.LoanSourceID = l.LoanSourceID
JOIN Customer c ON l.CustomerId = c.Id 
WHERE s.LoanSourceName='EU'
AND c.IsTest=0
AND l.[Date]>=@DateStart AND l.[Date]<@DateEnd

----------------------Part D - Expired Loans------------------------------------------------------------------------
--What is it?

----------------------Part E - Canceled Loans------------------------------------------------------------------------
--What is it?

----------------------Part F - Modifications------------------------------------------------------------------------
-- 1. Loan maturity extension
-- 2. Change of Micro-Enterprise ID
-- 3. Change of loan ID
-- 4. Change Loan amount
--What is it?


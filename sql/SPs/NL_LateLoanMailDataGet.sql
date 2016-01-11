IF OBJECT_ID('NL_LateLoanMailDataGet') IS  NULL
	EXECUTE('CREATE PROCEDURE NL_LateLoanMailDataGet AS SELECT 1')
GO

ALTER PROCEDURE NL_LateLoanMailDataGet
@CustomerID INT,
@LoanID INT
AS
BEGIN
-----------------------------------------------------------------
-- Customer Address
DECLARE @CAddress1 VARCHAR(200)
DECLARE @CAddress2 VARCHAR(200)
DECLARE @CAddress3 VARCHAR(200)
DECLARE @CAddress4 VARCHAR(200)
DECLARE @CPostcode VARCHAR(200)
-- Business Address name and type
DECLARE @BAddress1 VARCHAR(200)
DECLARE @BAddress2 VARCHAR(200)
DECLARE @BAddress3 VARCHAR(200)
DECLARE @BAddress4 VARCHAR(200)
DECLARE @BPostcode VARCHAR(200)
DECLARE @CompanyName NVARCHAR(200)
DECLARE @IsLimited BIT = 0
--Guarantor Address and name - todo
DECLARE @GuarantorName NVARCHAR(200)
DECLARE @GAddress1 VARCHAR(200)
DECLARE @GAddress2 VARCHAR(200)
DECLARE @GAddress3 VARCHAR(200)
DECLARE @GAddress4 VARCHAR(200)
DECLARE @GPostcode VARCHAR(200)
--loan and missed payments details
DECLARE @LoanAmount INT 
DECLARE @LoanDate DATETIME
DECLARE @LoanRef NVARCHAR(20)

-----------------------------------------------------------------
-- business type and name
SELECT 
	@IsLimited = CAST(CASE WHEN c.TypeOfBusiness IN ('Limited', 'LLP') THEN 1	ELSE 0 END	AS BIT),
		@CompanyName = (CASE WHEN co.ExperianCompanyName IS NULL THEN co.CompanyName ELSE co.ExperianCompanyName END)
FROM Customer c 
LEFT JOIN Company co ON c.CompanyId=co.Id
WHERE c.Id=@CustomerID
-----------------------------------------------------------------
--Customer Address
SELECT 
	@CAddress1=x.Line1, 
	@CAddress2=x.Line2, 
	@CAddress3=x.Line3, 
	@CAddress4=x.Town, 
	@CPostcode=x.Postcode 
FROM
(
	SELECT TOP 1 *
	FROM CustomerAddress a
	WHERE 
		a.addressType=1 
	AND 
		a.CustomerId=@CustomerID
	ORDER BY 1 DESC
) AS x
-----------------------------------------------------------------
--Business Address
SELECT
	 @BAddress1=y.Line1,
	 @BAddress2=y.Line2,
	 @BAddress3=y.Line3, 
	 @BAddress4=y.Town, 
	 @BPostcode=y.Postcode 
FROM
(
	SELECT TOP 1 a.*
	FROM CustomerAddress a 
	INNER JOIN Company co ON co.Id = a.CompanyId 
	INNER JOIN Customer c ON co.Id = c.CompanyId
	WHERE a.addressType IN (3,5) AND c.Id=@CustomerID
	ORDER BY 1 DESC
) AS y
-----------------------------------------------------------------
--Loan Details
SELECT 
	@LoanAmount =lh.Amount, 
   	@LoanDate = lh.EventTime, 
	@LoanRef = l.RefNum 
FROM NL_Loans l
	INNER JOIN (select top 1 * from NL_LoanHistory lh where LoanID = @LoanID order by LoanHistoryID desc) lh on l.LoanID = lh.LoanID
	INNER JOIN vw_NL_LoansCustomer v  on v.LoanID = l.LoanID	
WHERE 
	l.LoanID=@LoanID 
AND 
	v.CustomerId=@CustomerID

--final select
SELECT @CompanyName CompanyName, @IsLimited IsLimited,
	   @CAddress1 CAddress1, @CAddress2 CAddress2, @CAddress3 CAddress3, @CAddress4 CAddress4, @CPostcode CPostcode,
	   @BAddress1 BAddress1, @BAddress2 BAddress2, @BAddress3 BAddress3, @BAddress4 BAddress4, @BPostcode BPostcode,
	   @GAddress1 GAddress1, @GAddress2 GAddress2, @GAddress3 GAddress3, @GAddress4 GAddress4, @GPostcode GPostcode, @GuarantorName GuarantorName,
 	   @LoanAmount LoanAmount,@LoanDate LoanDate,@LoanRef LoanRef
END

IF OBJECT_ID('GetDataForCollectionMail') IS NULL
	EXECUTE('CREATE PROCEDURE GetDataForCollectionMail AS SELECT 1')
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


ALTER PROCEDURE GetDataForCollectionMail
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
DECLARE @OutstandingBalance DECIMAL(18,2)
DECLARE @OutstandingPrincipal DECIMAL(18,2)
DECLARE @SchedID INT 
DECLARE @PreviousSchedID INT 
DECLARE @AmountDue DECIMAL(18,2)
DECLARE @PreviousAmountDue DECIMAL(18,2)
DECLARE @SchedDate DATETIME
DECLARE @PreviousSchedDate DATETIME
DECLARE @Fees DECIMAL(18,2)
DECLARE @PreviousFees DECIMAL(18,2)
DECLARE @Interest DECIMAL (18,2)
DECLARE @PreviousInterest DECIMAL(18,2)
DECLARE @RepaidAmount DECIMAL(18,2)
DECLARE @PreviousRepaidAmount DECIMAL(18,2)
DECLARE @RepaidDate DATETIME
DECLARE @PreviousRepaidDate DATETIME

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
	@LoanAmount = l.LoanAmount, 
   	@LoanDate = l.[Date], 
	@LoanRef = l.RefNum, 
	@OutstandingBalance = l.Balance, 
	@OutstandingPrincipal = l.Principal
FROM Loan l
WHERE 
	l.Id=@LoanID 
AND 
	l.CustomerId=@CustomerID

-----------------------------------------------------------------
--missed schedule details
SELECT @SchedID = x.Id, @SchedDate = x.[Date],@AmountDue = x.AmountDue, @Fees = x.Fees, @Interest = x.Interest 
FROM
(
 SELECT TOP 1 ls.* 
 FROM LoanSchedule ls 
 LEFT JOIN LoanScheduleTransaction lst ON lst.ScheduleID = ls.Id
 LEFT JOIN LoanTransaction lt ON lt.Id = lst.TransactionID
 WHERE 
 	ls.LoanId=@LoanID 
 	AND
 	ls.Status = 'Late'
 ORDER BY ls.[Date] DESC 	
) x


SELECT @RepaidAmount = 0, @RepaidDate = NULL --TODO retrieve the real value
 
-----------------------------------------------------------------
-- previous missed schedule details
SELECT @PreviousSchedID = x.Id, @PreviousSchedDate = x.[Date], @PreviousAmountDue = isnull(x.AmountDue, 0), @PreviousFees = isnull(x.Fees,0), @PreviousInterest = isnull(x.Interest,0)
FROM
(
 SELECT TOP 1 ls.* 
 FROM LoanSchedule ls 
 LEFT JOIN LoanScheduleTransaction lst ON lst.ScheduleID = ls.Id
 LEFT JOIN LoanTransaction lt ON lt.Id = lst.TransactionID
 WHERE 
 	ls.LoanId=@LoanID 
 	AND
 	ls.Status = 'Late'
 	AND
 	ls.Id<>@SchedId
 ORDER BY ls.[Date] DESC 	
) x


SELECT @PreviousRepaidAmount = 0, @PreviousRepaidDate = NULL --TODO retrieve the real value
 
-----------------------------------------------------------------
--final select
SELECT @CompanyName CompanyName, @IsLimited IsLimited,
	   @CAddress1 CAddress1, @CAddress2 CAddress2, @CAddress3 CAddress3, @CAddress4 CAddress4, @CPostcode CPostcode,
	   @BAddress1 BAddress1, @BAddress2 BAddress2, @BAddress3 BAddress3, @BAddress4 BAddress4, @BPostcode BPostcode,
	   @GAddress1 GAddress1, @GAddress2 GAddress2, @GAddress3 GAddress3, @GAddress4 GAddress4, @GPostcode GPostcode, @GuarantorName GuarantorName,
 	   @LoanAmount LoanAmount,@LoanDate LoanDate,@LoanRef LoanRef,@OutstandingBalance OutstandingBalance,@OutstandingPrincipal OutstandingPrincipal,
	   @AmountDue AmountDue ,@SchedDate SchedDate,@Fees Fees,@Interest Interest,@RepaidAmount RepaidAmount,@RepaidDate RepaidDate,
	   @PreviousAmountDue PreviousAmountDue,@PreviousSchedDate PreviousSchedDate,@PreviousFees PreviousFees,@PreviousInterest PreviousInterest,@PreviousRepaidAmount PreviousRepaidAmount,@PreviousRepaidDate PreviousRepaidDate
END

GO

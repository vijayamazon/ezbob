IF OBJECT_ID('RptPendingApprovedPaidList') IS NULL
	EXECUTE('CREATE PROCEDURE RptPendingApprovedPaidList AS SELECT 1')
GO

SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE RptPendingApprovedPaidList
   			   		(@DateStart DATETIME,
					 @DateEnd   DATETIME)

AS
BEGIN

SET NOCOUNT ON

if OBJECT_ID('tempdb..#temp1') is not NULL
BEGIN
	DROP TABLE #temp1
END

if OBJECT_ID('tempdb..#temp2') is not NULL
BEGIN
	DROP TABLE #temp2
END

----------------------------------------------------------------
/*DECLARE @DateStart DATE,
		@DateEnd DATE;

SET @DateStart = '2014-01-01'
SET @DateEnd = '2014-10-01'
*/

DECLARE @tmp_approved_didnt_take TABLE
										(
										IdCustomer INT,
										Fullname NVARCHAR (250),
										Email NVARCHAR (128),
										ReferenceSource NVARCHAR (1000),
										PersonalScore INT,
										UnderwriterDecisionDate DATETIME,
										ManagerApprovedSum INT,
										InterestRate DECIMAL (18, 4),
										RepaymentPeriod INT,
										LoansType VARCHAR (13),
										HadLoan VARCHAR (3),
										BrokerOrNot VARCHAR (12)
										)

INSERT INTO @tmp_approved_didnt_take
EXECUTE RptApprovedDidntTake @DateStart,@DateEnd;


DECLARE @tmp_paid_off TABLE
										(
										CustomerId INT,
										EmailAddress NVARCHAR (128),
										Fullname NVARCHAR (250),
										SegmentType VARCHAR (20),
										CustomerStatus VARCHAR (15),
										NumOfLoansTook INT,
										PersonalScore INT,
										DatePaidLastLoan DATETIME,
										DaytimePhone NVARCHAR (50),
										MobilePhone NVARCHAR (50),
										CRMNoteDate DATETIME,
										CRMUsername NVARCHAR (100),
										CRMComment VARCHAR (1000),
										CRMStatus NVARCHAR (100),
										CRMAction NVARCHAR (100),
										BrokerOrNot VARCHAR (12)
										)

INSERT INTO @tmp_paid_off
EXECUTE RptPaidLoanDidntTakeNew @DateStart,@DateEnd;


CREATE TABLE #temp1		  (
						  CustomerID INT,
						  SegmentType VARCHAR (60),
						  )


INSERT INTO #temp1 (CustomerId,SegmentType)
SELECT C.Id,'Pending'
FROM Customer C
WHERE C.CreditResult = 'ApprovedPending'


INSERT INTO #temp1 (CustomerId,SegmentType)
SELECT  T1.IdCustomer,'Approved Didnt Take Loan'
FROM @tmp_approved_didnt_take T1
WHERE T1.BrokerOrNot = 'NotBroker'



INSERT INTO #temp1 (CustomerId,SegmentType)
SELECT  T2.CustomerID,'Paid More Than 50%'
FROM @tmp_paid_off T2
WHERE T2.BrokerOrNot = 'NonBroker'
	  AND T2.SegmentType = 'Paid More than 50%'


INSERT INTO #temp1 (CustomerId,SegmentType)
SELECT  T2.CustomerID,'Paid Off Loan Completely'
FROM @tmp_paid_off T2
WHERE T2.BrokerOrNot = 'NonBroker'
	  AND T2.SegmentType = 'Fully Repaid'


SELECT T1.CustomerID,
	   C.Fullname,
	   C.GreetingMailSentDate AS SignUpDate,
	   C.Name AS EmailAddress,
	   C.DaytimePhone,
	   C.MobilePhone,
	   T1.SegmentType,
	   CASE 
	   WHEN count(L.Id) > 0 THEN 'Old'
	   ELSE 'New'
	   END AS NewOldClient
	   
FROM #temp1 T1
JOIN Customer C ON C.Id = T1.CustomerID
LEFT JOIN Loan L ON L.CustomerId = T1.CustomerID

GROUP BY   T1.CustomerID,
		   C.Fullname,
		   C.GreetingMailSentDate,
		   C.Name,
		   C.DaytimePhone,
		   C.MobilePhone,
		   T1.SegmentType
		   
END
GO


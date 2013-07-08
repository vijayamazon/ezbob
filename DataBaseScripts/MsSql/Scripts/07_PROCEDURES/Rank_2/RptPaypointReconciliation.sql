IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[RptPaypointReconciliation]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[RptPaypointReconciliation]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE RptPaypointReconciliation
@DateStart DATETIME,
@DateEnd DATETIME
AS
BEGIN
	DECLARE @Date DATE
	DECLARE @IncludeFive BIT

	SELECT @Date = CONVERT(DATE, @DateStart)

	IF EXISTS (SELECT * FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five')
		SET @IncludeFive = CASE (SELECT Value FROM ConfigurationVariables WHERE Name = 'Recon_Paypoint_Include_Five') WHEN 'yes' THEN 1 ELSE 0 END

	CREATE TABLE #out (
		SortOrder INT IDENTITY(1, 1) NOT NULL,
		Caption NVARCHAR(1000) NOT NULL,
		EzbobAmount DECIMAL(18, 2) NULL,
		PaypointAmount DECIMAL(18, 2) NULL,
		TransactionID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #out (Caption, Css) VALUES ('Successful Transactions', 'total2')
	INSERT INTO #out (Caption) VALUES ('5-Amount Transactions: ' + (CASE @IncludeFive WHEN 1 THEN 'included' ELSE 'excluded' END))
	
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 1
	
	INSERT INTO #out (Caption, Css) VALUES ('Failed Transactions', 'total2')
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 0

	SELECT
		o.SortOrder,
		o.Caption,
		o.EzbobAmount,
		o.PaypointAmount,
		t.Id,
		t.PostDate,
		t.LoanId,
		c.Id AS ClientID,
		c.Name AS ClientEmail,
		c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname AS ClientName,
		t.Description,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TransactionID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
END
GO

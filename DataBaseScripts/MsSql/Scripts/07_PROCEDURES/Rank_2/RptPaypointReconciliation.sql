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

	INSERT INTO #out (Caption, Css) VALUES ('Successful Transactions', 'Successful')
	INSERT INTO #out (Caption) VALUES ('Transactions of Amount 5 Are ' + (CASE @IncludeFive WHEN 1 THEN 'Included' ELSE 'Excluded' END))
	
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 1
	
	INSERT INTO #out (Caption, Css) VALUES ('Failed Transactions', 'Failed')
	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 0

	SELECT
		o.SortOrder,
		o.Caption,
		o.EzbobAmount,
		o.PaypointAmount,
		o.TransactionID AS Id,
		(CASE o.Caption WHEN 'Paypoint' THEN b.date ELSE t.PostDate END) AS PostDate,
		(CASE o.Caption WHEN 'Paypoint' THEN NULL ELSE t.LoanId END) AS LoanId,
		(CASE o.Caption WHEN 'Paypoint' THEN NULL ELSE c.Id END) AS ClientID,
		(CASE o.Caption WHEN 'Paypoint' THEN NULL ELSE c.Name END) AS ClientEmail,
		(CASE o.Caption WHEN 'Paypoint' THEN b.name ELSE c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname END) AS ClientName,
		(CASE o.Caption WHEN 'Paypoint' THEN 'card ' + b.lastfive + ' from ' + b.ip ELSE t.Description END) AS Description,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TransactionID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
		LEFT JOIN PayPointBalance b ON o.TransactionID = b.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
END
GO

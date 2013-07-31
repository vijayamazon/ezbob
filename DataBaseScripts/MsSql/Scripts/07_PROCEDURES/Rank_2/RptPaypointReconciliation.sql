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
		Caption NVARCHAR(512) NULL,
		Amount DECIMAL(18, 2) NULL,
		TranID INT NULL,
		Css NVARCHAR(128) NULL
	)

	INSERT INTO #out (Caption) VALUES ('Transactions of Amount 5 Are ' + (CASE @IncludeFive WHEN 1 THEN 'Included' ELSE 'Excluded' END))

	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 1, 'Successful'

	EXECUTE PaypointOneTypeReconciliation @Date, @IncludeFive, 0, 'Failed'

	SELECT
		o.SortOrder,
		o.Caption,
		ISNULL(o.Amount, (CASE o.Caption WHEN 'Ezbob' THEN t.Amount ELSE b.amount END)) AS Amount,
		o.TranID,
		(CASE o.Caption WHEN 'Ezbob' THEN t.PostDate ELSE b.date END) AS Date,
		c.Id AS ClientID,
		(CASE o.Caption WHEN 'Ezbob' THEN c.FirstName + ' ' + c.MiddleInitial + ' ' + c.Surname ELSE b.name END) AS ClientName,
		(CASE o.Caption WHEN 'Ezbob' THEN t.Description ELSE b.trans_id END) AS Description,
		o.Css
	FROM
		#out o
		LEFT JOIN LoanTransaction t ON o.TranID = t.Id
		LEFT JOIN Loan l ON t.LoanId = l.Id
		LEFT JOIN Customer c ON l.CustomerId = c.Id
		LEFT JOIN PayPointBalance b ON o.TranID = b.Id
	ORDER BY
		SortOrder

	DROP TABLE #out
END
GO

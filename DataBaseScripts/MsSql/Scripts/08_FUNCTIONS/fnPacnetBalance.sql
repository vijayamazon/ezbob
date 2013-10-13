IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnPacnetBalance]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnPacnetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION fnPacnetBalance ()
RETURNS @ResultSet TABLE (
	[Id]             INT,
	[PacnetBalance]  DECIMAL(18, 4),
	[ReservedAmount] DECIMAL(18, 4),
	[Date]           DATETIME,
	[Adjusted]       DECIMAL(18, 4),
	[Loans]          DECIMAL(18, 4)
) AS
BEGIN
	DECLARE @reservedAmount DECIMAL(18, 4)
	DECLARE @pacNet         DECIMAL(18, 4)
	DECLARE @lastUpdate     DATETIME
	DECLARE @loans          DECIMAL(18, 4)

	SELECT
		@reservedAmount = SUM(c.CreditSum)
	FROM
		Customer c
	WHERE
		c.CreditSum > 0
		AND
		c.CreditResult = 'Approved'
		AND
		c.Status = 'Approved'
		AND
		c.ValidFor >= GETUTCDATE()
		AND
		c.ApplyForLoan <= GETUTCDATE()
		AND
		c.IsTest = 0

	SELECT TOP(1)
		@pacNet = pb.CurrentBalance,
		@lastUpdate = pb.Date
	FROM
		PacNetBalance pb
	ORDER BY
		pb.Date DESC,
		pb.Id DESC

	SELECT
		@loans = ISNULL(SUM(l.LoanAmount - l.SetupFee), 0)
	FROM
		Loan l
	WHERE
		l.Date > DATEADD(DAY, 1, @lastUpdate)

	INSERT @ResultSet (Id, PacnetBalance, ReservedAmount, Date, Loans, Adjusted)
		VALUES (1, ISNULL(@pacNet, 0), @reservedAmount, @lastUpdate, @loans, (ISNULL(@pacNet, 0) - @loans));

	RETURN;
END
GO

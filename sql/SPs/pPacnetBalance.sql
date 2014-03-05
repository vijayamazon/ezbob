IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[pPacnetBalance]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[pPacnetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[pPacnetBalance]
AS
BEGIN
	DECLARE @reservedAmount decimal(18, 4);
	DECLARE @pacNet decimal(18, 4);
	DECLARE @lastUpdate datetime;
	DECLARE @loans decimal(18,4);


	select @reservedAmount = sum(c.CreditSum)  from Customer c where c.CreditSum > 0 and c.CreditResult = 'Approved' 
	and c.Status = 'Approved' and c.ValidFor >= GETUTCDATE() and c.ApplyForLoan <= GETUTCDATE()

	select top(1) @pacNet = pb.Amount, @lastUpdate = pb.Date from PacNetBalance pb order by pb.Date desc

	select @loans = sum(l.LoanAmount - l.SetupFee) from Loan l where l.Date > @lastUpdate

	select ISNULL(@pacNet, 0) as PacnetBalance, @reservedAmount as ReservedAmount, @lastUpdate as Date, @loans as Loans, (ISNULL(@pacNet, 0) - @loans) as Adjusted
END
GO

IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[fnPacnetBalance]') AND type in (N'FN', N'IF', N'TF', N'FS', N'FT'))
DROP FUNCTION [dbo].[fnPacnetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
create function fnPacnetBalance (
) returns @ResultSet table (
	   [Id] int,
       [PacnetBalance] decimal(18, 4),
	   [ReservedAmount] decimal(18, 4),
	   [Date] datetime,
	   [Adjusted]       decimal(18, 4),
	   [Loans] decimal(18, 4)
) as
begin
	DECLARE @reservedAmount decimal(18, 4)
	DECLARE @pacNet decimal(18, 4)
	DECLARE @lastUpdate datetime
	DECLARE @loans decimal(18,4)
    
	select @reservedAmount = sum(c.CreditSum)  from Customer c where c.CreditSum > 0 and c.CreditResult = 'Approved' and c.Status = 'Approved' and c.ValidFor >= GETUTCDATE() and c.ApplyForLoan <= GETUTCDATE()

	select top(1) @pacNet = pb.Amount, @lastUpdate = pb.Date from PacNetBalance pb order by pb.Date desc

	select @loans = ISNULL(sum(l.LoanAmount - l.SetupFee), 0) from Loan l where l.Date > @lastUpdate

	insert @ResultSet (Id, PacnetBalance, ReservedAmount, Date, Loans, Adjusted)
	values (1, ISNULL(@pacNet, 0), @reservedAmount, @lastUpdate, @loans, (ISNULL(@pacNet, 0) - @loans));
    
	return;
end
GO

SET QUOTED_IDENTIFIER ON
GO

IF OBJECT_ID('LoadLastCustomerCashRequest') IS NULL
	EXECUTE('CREATE PROCEDURE LoadLastCustomerCashRequest AS SELECT 1')
GO


ALTER PROCEDURE [dbo].[LoadLastCustomerCashRequest]
@CustomerID INT
AS
BEGIN
	SET NOCOUNT ON;
		
	declare @cr table(CashRequestID bigint, UnderwriterDecision nvarchar(50), NLCashRequestID bigint null);

	INSERT INTO @cr
		SELECT TOP 1
			CashRequestID = r.Id,
			r.UnderwriterDecision,
			0
		FROM
			CashRequests r
		WHERE
			r.IdCustomer = @CustomerID
		ORDER BY
			r.Id DESC;			
	
	if OBJECT_ID('NL_CashRequests') is not null
	begin
		update @cr set NLCashRequestID = (select Max(cr.CashRequestID) from NL_CashRequests cr where OldCashRequestID = (select  cr.CashRequestID from @cr cr));
	end

	select * from @cr;

END

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateAutoApproval]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateAutoApproval]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateAutoApproval] 
	(@CustomerId INT,
    @AutoApproveAmount INT)
AS
BEGIN
	UPDATE CashRequests SET SystemCalculatedSum=@AutoApproveAmount WHERE IdCustomer=@CustomerId
	UPDATE Customer SET SystemCalculatedSum = @AutoApproveAmount, CreditSum=@AutoApproveAmount, IsLoanTypeSelectionAllowed=1, LastStatus='Approve' WHERE Id=@CustomerId
END
GO

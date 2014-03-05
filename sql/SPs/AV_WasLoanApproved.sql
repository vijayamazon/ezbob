IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[AV_WasLoanApproved]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[AV_WasLoanApproved]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AV_WasLoanApproved] 
	(@CustomerId INT)
AS
BEGIN
	SET NOCOUNT ON
	IF EXISTS (SELECT * 
			   FROM CashRequests 
			   WHERE IdCustomer = @CustomerId 
			   AND (SystemDecision = 'Approved' OR UnderwriterDecision='Approved')
			  )
		SELECT 'true'
	ELSE
		SELECT 'false'		
	SET NOCOUNT OFF
END
GO

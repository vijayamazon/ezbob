IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetApprovalData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetApprovalData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetApprovalData] 
	(@CustomerId INT)
AS
BEGIN
	DECLARE @NumOfApprovals INT
	
	SELECT @NumOfApprovals = count(1) FROM DecisionHistory WHERE CustomerId = @CustomerId AND Action='Approve'

	SELECT @NumOfApprovals AS NumOfApprovals, ValidFor, ApplyForLoan FROM Customer WHERE Id = @CustomerId
END
GO

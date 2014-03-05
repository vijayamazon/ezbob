IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateUserApplyForLoanTime]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateUserApplyForLoanTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateUserApplyForLoanTime] 
	(@UserId int, 
@ApplyForLoan  datetime)
AS
BEGIN
	UPDATE [dbo].[Customer]
SET [ApplyForLoan] = @ApplyForLoan--, [CreditSum] = @LoanAmount, [MedalType] = @Medal
WHERE Id = @UserId
END
GO

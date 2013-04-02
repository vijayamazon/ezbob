IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateUserApplyForLoanTime]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateUserApplyForLoanTime]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateUserApplyForLoanTime] 
(@UserId int, 
@ApplyForLoan  datetime)
--@LoanAmount int,
--@Medal nvarchar(128)

AS
BEGIN

UPDATE [dbo].[Customer]
SET [ApplyForLoan] = @ApplyForLoan--, [CreditSum] = @LoanAmount, [MedalType] = @Medal
WHERE Id = @UserId

END
GO

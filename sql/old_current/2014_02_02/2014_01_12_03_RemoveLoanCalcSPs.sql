IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUnpaidLoans]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUnpaidLoans]
GO
IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateLoanInterest]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateLoanInterest]
GO

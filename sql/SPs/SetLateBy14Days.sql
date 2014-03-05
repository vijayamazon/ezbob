IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetLateBy14Days]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetLateBy14Days]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetLateBy14Days] 
	(@LoanId INT)
AS
BEGIN
	UPDATE Loan SET Is14DaysLate = 1 WHERE Id = @LoanId
END
GO

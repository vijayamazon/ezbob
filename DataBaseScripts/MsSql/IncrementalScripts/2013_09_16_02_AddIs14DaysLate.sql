IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE Name = N'Is14DaysLate' and Object_ID = Object_ID(N'Loan'))    
BEGIN
	ALTER TABLE Loan ADD Is14DaysLate BIT DEFAULT 0 NOT NULL
END
GO

IF EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SetLateBy14Days]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[SetLateBy14Days]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SetLateBy14Days]
	@LoanId INT
AS
BEGIN
	UPDATE Loan SET Is14DaysLate = 1 WHERE Id = @LoanId
END
GO




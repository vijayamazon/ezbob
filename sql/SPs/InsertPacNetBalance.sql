IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertPacNetBalance]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertPacNetBalance]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsertPacNetBalance] 
	(@Date		             DATETIME
 ,@Amount                FLOAT
 ,@Fees				     FLOAT
 ,@CurrentBalance        FLOAT
 ,@IsCredit              BIT = 0)
AS
BEGIN
	INSERT INTO dbo.PacNetBalance ( Date, Amount ,Fees ,CurrentBalance, IsCredit )
	VALUES(	@Date, @Amount, @Fees, @CurrentBalance, @IsCredit )
END
GO

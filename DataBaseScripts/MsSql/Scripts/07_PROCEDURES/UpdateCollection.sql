IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateCollection]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateCollection]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateCollection] 
(@LoanId int,
 @Late30 Numeric(18),
 @Late30Num int,
 @Late60 Numeric(18),
 @Late60Num int,
 @Late90 Numeric(18),
 @Late90Num int,
 @PastDues Numeric(18),
 @PastDuesNum int,
 @IsDefaulted int,
 @Late90Plus numeric(18),
 @Late90PlusNum numeric(18),
 @MaxDelinquencyDays int,
 @InstallmentDelinquencyDays int,
 @InstallmentId int)
 

AS
BEGIN

UPDATE [dbo].[Loan]
   SET  Late30 = @Late30,
	    Late30Num = @Late30Num,
		Late60 = @Late60,
		Late60Num = Late60Num,
		Late90 = @Late90,
		Late90Num = @Late90Num,
		PastDues = @PastDues,
		PastDuesNum = @PastDuesNum,
		IsDefaulted = @IsDefaulted,
		Late90Plus = @Late90Plus,
		Late90PlusNum = @Late90PlusNum,
		MaxDelinquencyDays = @MaxDelinquencyDays
 WHERE Id = @LoanId

UPDATE [dbo].[LoanSchedule]
   SET  Delinquency = @MaxDelinquencyDays
 WHERE Id = @InstallmentId

 SET NOCOUNT ON;
SELECT @@IDENTITY;
END
GO

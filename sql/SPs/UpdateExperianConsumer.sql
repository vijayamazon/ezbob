IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianConsumer]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianConsumer]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianConsumer] 
	(@Name nvarchar(500),
 @Surname nvarchar(500),
 @PostCode nvarchar(500),
 @ExperianError nvarchar(max),
 @ExperianScore int, 
 --@ExperianResult nvarchar(500),
 --@ExperianWarning nvarchar(max),
 --@ExperianReject nvarchar(max),
 @CustomerId bigint,
 @DirectorId bigint)
AS
BEGIN
	UPDATE [dbo].[MP_ExperianDataCache]
   SET [ExperianError]  = @ExperianError, 
       [ExperianScore]  = @ExperianScore, 
--     [ExperianResult] = @ExperianResult,
--     [ExperianWarning]= @ExperianWarning,
--     [ExperianReject] = @ExperianReject,
       [CustomerId]     = @CustomerId,
       [DirectorId]     = @DirectorId 
 WHERE Name = @Name and Surname = @Surname and PostCode=@PostCode


 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO

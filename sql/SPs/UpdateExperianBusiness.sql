IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianBusiness]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianBusiness]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianBusiness] 
	(@CompanyRefNumber nvarchar(50),
 @ExperianError nvarchar(max),
 @ExperianScore int, 
 --@ExperianResult nvarchar(500),
 --@ExperianWarning nvarchar(max),
 --@ExperianReject nvarchar(max),
 @CustomerId bigint)
AS
BEGIN
	UPDATE [dbo].[MP_ExperianDataCache]
   SET [ExperianError] = @ExperianError, 
    [ExperianScore] = @ExperianScore, 
--    [ExperianResult] = @ExperianResult,
--    [ExperianWarning] = @ExperianWarning,
--    [ExperianReject] = @ExperianReject,
    [CustomerId] = @CustomerId
 WHERE CompanyRefNumber = @CompanyRefNumber


 SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO

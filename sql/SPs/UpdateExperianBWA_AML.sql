IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateExperianBWA_AML]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateExperianBWA_AML]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UpdateExperianBWA_AML] 
	(@CustomerId int,
 @BWAResult nvarchar(100),
 @AMLResult nvarchar(100))
AS
BEGIN
	UPDATE [dbo].[Customer]
   SET [BWAResult] = @BWAResult, 
    [AMLResult] = @AMLResult

WHERE Id = @CustomerId

UPDATE [dbo].[CardInfo]
   SET [BWAResult] = @BWAResult

WHERE CustomerId = @CustomerId

SET NOCOUNT ON;
SELECT @@IDENTITY
END
GO

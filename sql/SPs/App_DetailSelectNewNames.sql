IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailSelectNewNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailSelectNewNames]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_DetailSelectNewNames] 
	(@pLastParameterId BIGINT)
AS
BEGIN
	SET NOCOUNT ON;

     SELECT
          [Application_DetailName].[DetailNameID] AS [id],
          [Application_DetailName].[Name] AS [name]
     FROM [Application_DetailName] WITH (NOLOCK)
     WHERE [Application_DetailName].[DetailNameID] > @pLastParameterId;
END
GO

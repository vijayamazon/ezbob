IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[App_DetailInsertNewNames]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[App_DetailInsertNewNames]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[App_DetailInsertNewNames] 
	(@pDetailName NVARCHAR(255))
AS
BEGIN
	SET NOCOUNT ON;

    INSERT INTO [Application_DetailName]([Name]) VALUES(@pDetailName); 
    SELECT @@IDENTITY;
END
GO

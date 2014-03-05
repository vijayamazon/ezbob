IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetCaisFileData]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetCaisFileData]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetCaisFileData] 
	(@CaisId INT)
AS
BEGIN
	SELECT 
		FileName, 
		DirName
	FROM 
		CaisReportsHistory
	WHERE 
		Id = @CaisId
END
GO

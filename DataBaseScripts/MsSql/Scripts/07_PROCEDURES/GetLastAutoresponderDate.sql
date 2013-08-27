IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetLastAutoresponderDate]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetLastAutoresponderDate]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE GetLastAutoresponderDate
	@Email NVARCHAR(300)
AS
BEGIN
SELECT TOP 1 DateOfAutoResponse FROM AutoresponderLog WHERE Email = @Email ORDER BY DateOfAutoResponse DESC 
END
GO

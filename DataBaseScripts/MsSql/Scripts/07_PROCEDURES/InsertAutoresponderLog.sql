IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[InsertAutoresponderLog]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[InsertAutoresponderLog]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE InsertAutoresponderLog
	@Email NVARCHAR(300),
	@Name NVARCHAR(300)
AS
BEGIN
INSERT INTO AutoresponderLog(Email, Name, DateOfAutoResponse) VALUES (@Email, @Name, getdate())
END
GO

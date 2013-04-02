IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Insert_Security_Role]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Insert_Security_Role]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Insert_Security_Role]
       @pName nvarchar(255),
       @pDescription nvarchar(50),
       @pNewRoleId int OUTPUT
AS
BEGIN
     SET NOCOUNT ON;
     INSERT INTO Security_Role (Name, Description) 
     VALUES (@pName, @pDescription);

   SELECT @pNewRoleId = SCOPE_IDENTITY() 

END
GO

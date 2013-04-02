IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Security_GrandServiceAction]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Security_GrandServiceAction]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Security_GrandServiceAction]
(
		@pCommandName  nvarchar(255),
		@pAppId int,
		@pRoleId int
)
AS
BEGIN
     insert into Security.ServiceAction
     (Command, Name, AppId)
     values
     (@pCommandName, @pCommandName, @pAppId)
     
     
     insert into Security.RoleServiceActionRelation
     (RoleId, ServiceActionId)
     values
     (@pRoleId, @@IDENTITY)
  /* Procedure body */
END
GO

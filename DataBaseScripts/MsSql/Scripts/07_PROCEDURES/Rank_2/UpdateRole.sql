IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[UpdateRole]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[UpdateRole]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[UpdateRole]
    @pRoleId int, 
    @pRoleName nVarchar(255),
    @pRoleDescr nvarchar(500),
    @pUsersIds nvarchar(4000)
as
begin
  BEGIN TRANSACTION
  DECLARE @l_new_role_id int;
  SET @l_new_role_id = @pRoleId;
  
  if @l_new_Role_id > 0 
  BEGIN 
   UPDATE Security_Role 
      SET Name = @pRoleName, 
          Description = @pRoleDescr WHERE RoleId = @pRoleId;
          
   DELETE FROM Security_UserRoleRelation WHERE RoleId = @pRoleId;          
  END; 
  ELSE
    EXEC dbo.insert_security_role @pRoleName, @pRoleDescr, @l_new_role_id OUTPUT;  
  
  INSERT INTO Security_UserRoleRelation
    Select x.item, @l_new_role_id from dbo.GetTableFromList(@pUsersIds, ',') x;

  IF @@ERROR <> 0
  BEGIN
     ROLLBACK TRANSACTION
     RETURN (-1)
  END

 COMMIT TRANSACTION
 RETURN (0)
end;
GO

IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[GetUserFrom]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[GetUserFrom]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetUserFrom] 
	(@iIsDeleted int,
  @iUserRoleId int)
AS
BEGIN
	if ( @iUserRoleId <= -1)
		begin
			if (@iIsDeleted > -1)
				begin	
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su where su.isdeleted = @iIsDeleted;
				end;
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su;
				end;
		end;
	else
		begin
		if (@iIsDeleted > -1)
				begin	
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 
						su.isdeleted = @iIsDeleted and
						surr.RoleId = @iUserRoleId
				end;
			else
				begin
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su
						left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 						
						surr.RoleId = @iUserRoleId
				end;
		end
END
GO

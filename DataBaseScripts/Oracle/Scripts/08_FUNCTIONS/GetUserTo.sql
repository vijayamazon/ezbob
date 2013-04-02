create or replace function GetUserTo( iUserId in number, iIsDeleted in number, iUserRoleId in number) return sys_refcursor
as
 l_cur sys_refcursor;
begin
 if ( iUserRoleId <= -1) then
			if (iIsDeleted > -1) then
      		open l_cur for
					select
						su.userid
						,su.username
						,su.fullname
						,su.EMail
					from
						security_user su 
					where 
						su.isdeleted = iIsDeleted and su.userid != iUserId;
			else
          open l_cur for
					select
						su.userid
						,su.username
					  ,su.fullname
            ,su.EMail
          from
            security_user su 
          where 
            su.userid != iUserId;
      end if;
  else
      if (iIsDeleted > -1) then
          open l_cur for
          select
            su.userid
            ,su.username
            ,su.fullname
            ,su.EMail
          from
            security_user su 
            left join Security_UserRoleRelation surr on su.UserId = surr.UserId
          where 
            su.isdeleted = iIsDeleted and 
            su.userid != iUserId and
            surr.RoleId = iUserRoleId;
      else
          open l_cur for
          select
            su.userid
            ,su.username
            ,su.fullname
            ,su.EMail
          from
            security_user su 
            left join Security_UserRoleRelation surr on su.UserId = surr.UserId
					where 
						su.userid != iUserId and
						surr.RoleId = iUserRoleId;
			end if;
end if;
return l_cur;
end GetUserTo;
/
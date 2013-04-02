create or replace procedure Insert_security_role(Name in Varchar2,
                                                 Description in Varchar2,
                                                 pNewRoleId out Number)
is
  l_RoleId Number;
begin
  SELECT seq_insert_security_role.Nextval into l_RoleId from dual;
  INSERT INTO Security_Role (RoleId, Name, Description)
      values(l_RoleId, Name, Description);
  pNewRoleId := l_RoleId;

end Insert_security_role;
/

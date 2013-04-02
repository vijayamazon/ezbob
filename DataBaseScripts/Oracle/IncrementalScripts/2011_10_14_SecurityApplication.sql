execute immediate 'alter table SECURITY_ROLEAPPREL
  add constraint FK_RoleAppRel_Role foreign key (ROLEID)
  references security_role (ROLEID)';

execute immediate 'alter table SECURITY_ROLEAPPREL
  add constraint FK_RoleAppRel_Application foreign key (APPID)
  references security_application (APPLICATIONID)';
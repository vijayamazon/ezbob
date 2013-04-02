-- Create table
create table SECURITY_ROLEAPPREL
(
  ROLEID NUMBER not null,
  APPID  NUMBER not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table SECURITY_ROLEAPPREL
  add constraint PK_APPID_ROLEID primary key (ROLEID, APPID);
-- Create table
create table SECURITY_ACCOUNTLOG
(
  ID        NUMBER not null,
  EVENTDATE DATE default sysdate,
  EVENTTYPE NUMBER,
  USERID    NUMBER,
  DATA      VARCHAR2(2048)
);
-- Add comments to the columns 
comment on column SECURITY_ACCOUNTLOG.EVENTTYPE
  is '1- password change';
-- Create/Recreate primary, unique and foreign key constraints 
alter table SECURITY_ACCOUNTLOG
  add constraint PK_SECURITY_ACCLOG primary key (ID);

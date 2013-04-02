-- Create table
create table APPLICATION_ERROR
(
  APPLICATIONID  NUMBER not null,
  ERRORMESSAGE   VARCHAR2(3000),
  ACTIONDATETIME DATE default sysdate not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table APPLICATION_ERROR
  add constraint AE_APPLICATIONID_PK primary key (APPLICATIONID)
  using index 
  pctfree 10;

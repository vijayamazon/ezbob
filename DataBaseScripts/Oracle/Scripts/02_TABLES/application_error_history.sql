-- Create table
create table APPLICATION_ERROR_HISTORY
(
  APPLICATIONID  NUMBER not null,
  ERRORMESSAGE   VARCHAR2(3000),
  ACTIONDATETIME DATE default sysdate not null
);

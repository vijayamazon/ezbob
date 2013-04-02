-- Create table
create table APPLICATION_NODETIME
(
  ID                    NUMBER not null,
  APPLICATIONID         NUMBER,
  NODEID                NUMBER,
  USERID                NUMBER,
  SECURITYAPPLICATIONID NUMBER,
  FIRSTTIMEOUTAGE       NUMBER,
  OUTAGETIMELOCKUNLOCK  NUMBER,
  TIMEOFFLY             NUMBER,
  WORKTIME              NUMBER,
  COMINGTIME            DATE,
  EXITTIME              DATE,
  EXITTYPE              NUMBER default 0
);

-- Create/Recreate primary, unique and foreign key constraints 
alter table APPLICATION_NODETIME
  add (constraint PK_APP_NODETIME primary key (ID));

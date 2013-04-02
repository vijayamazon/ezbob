-- Create table
create table APPLICATION_HISTORYSUMM
(
  ID                       NUMBER not null,
  APPLICATIONID            NUMBER,
  NODEID                   NUMBER,
  SECURITYAPPLICATIONID    NUMBER,
  SUMMFIRSTTIMEOUTAGE      NUMBER,
  SUMMOUTAGETIMELOCKUNLOCK NUMBER,
  GENERALOUTAGETIME        NUMBER,
  SUMMTIMEOFFLY            NUMBER,
  SUMMWORKTIME             NUMBER,
  FIRSTCOMINGTIME          DATE,
  LASTEXITTIME             DATE,
  ABSOLUTETIMEOFFLYNODE    NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table APPLICATION_HISTORYSUMM
  add (constraint PK_APP_HISTORYSUMM primary key (ID));

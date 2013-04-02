create table Security_log4net
(
  Id NUMBER not null,
  Message    CLOB,
  EventDate   DATE default CURRENT_TIMESTAMP not null,
  EventJournal  number,
  EventType  number,
  UserName varchar2(1024),
  "Level" varchar2(1024),
  "Thread" VARCHAR2 (16),
  "AppID" VARCHAR2(20),
  "Logger" VARCHAR2(255),
  "Exception" CLOB 
);
 
alter table Security_log4net
  add constraint PK_Security_log4net primary key (Id);
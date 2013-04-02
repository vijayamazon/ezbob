create table HistoryRecordKinds
(
  ID           NUMBER not null,
  DISPLAYNAME  VARCHAR2(255) not null,
  ISALIAS      NUMBER not null
);

alter table HistoryRecordKinds
  add constraint PK_HistoryRecordKinds primary key (ID);


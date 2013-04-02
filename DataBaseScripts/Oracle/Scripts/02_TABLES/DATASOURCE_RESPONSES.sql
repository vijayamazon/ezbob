create table DATASOURCE_RESPONSES
(
  REQUESTID NUMBER not null,
  RESPONSE  CLOB,
  CREATIONDATE DATE default sysdate
);

alter table DATASOURCE_RESPONSES
  add constraint PK_DATASOURCE_REQUESTID primary key (REQUESTID);

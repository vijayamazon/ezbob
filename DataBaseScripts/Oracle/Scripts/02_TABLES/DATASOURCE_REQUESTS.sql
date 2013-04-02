create table DATASOURCE_REQUESTS
(
  REQUESTID     NUMBER not null,
  APPLICATIONID NUMBER,
  REQUEST       CLOB,
  CREATIONDATE    DATE default sysdate
);

alter table DATASOURCE_REQUESTS
  add constraint PK_REQUESTID primary key (REQUESTID);

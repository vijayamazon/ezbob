create table DATASOURCE_KEYDATA
(
  KEYVALUEID NUMBER not null,
  REQUESTID  NUMBER,
  KEYNAMEID  NUMBER,
  VALUE      CLOB
);

alter table DATASOURCE_KEYDATA
  add constraint PK_KEYVALUEID primary key (KEYVALUEID);

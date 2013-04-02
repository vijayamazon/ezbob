create table DATASOURCE_KEYFIELDS
(
  KEYNAMEID NUMBER not null,
  KEYNAME   VARCHAR2(512)
);
alter table DATASOURCE_KEYFIELDS
  add constraint PK_KEYNAMEID primary key (KEYNAMEID);



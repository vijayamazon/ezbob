create table DictRisk
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictRisk add constraint PK_DictRisk primary key (ID);
alter table DictRisk add constraint IX_DictRisk unique (VALUE);


create table DictSex
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictSex add constraint PK_DictSex primary key (ID);
alter table DictSex add constraint IX_DictSex unique (VALUE);


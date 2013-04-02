create table DictSavings
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictSavings add constraint PK_DictSavings primary key (ID);
alter table DictSavings add constraint IX_DictSavings unique (VALUE);


create table DictChecking
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictChecking add constraint PK_DictChecking primary key (ID);
alter table DictChecking add constraint IX_DictChecking unique (VALUE);


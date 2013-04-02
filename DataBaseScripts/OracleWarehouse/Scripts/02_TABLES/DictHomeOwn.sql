create table DictHomeOwn
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictHomeOwn add constraint PK_DictHomeOwn primary key (ID);
alter table DictHomeOwn add constraint IX_DictHomeOwn unique (VALUE);


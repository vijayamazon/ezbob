create table DictMaritalStatus
(
ID NUMBER not null ,
VALUE VARCHAR(256)
);

alter table DictMaritalStatus add constraint PK_DictMaritalStatus primary key (ID);
alter table DictMaritalStatus add constraint IX_DictMaritalStatus unique (VALUE);


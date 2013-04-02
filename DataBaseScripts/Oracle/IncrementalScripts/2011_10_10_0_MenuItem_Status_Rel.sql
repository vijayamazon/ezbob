execute immediate 'create table MenuItem_Status_Rel
(
  MenuItemId NUMBER not null,
  StatusId  NUMBER not null
)';

execute immediate 'alter table MenuItem_Status_Rel add constraint PK_MenuItem_Status primary key (MenuItemId, StatusId)';
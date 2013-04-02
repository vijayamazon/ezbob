execute immediate 'alter table MENUITEM
  add constraint PK_MenuItem primary key (ID)';
  
execute immediate 'alter table APPSTATUS
  add constraint PK_APPSTATUS primary key (ID)';

execute immediate 'alter table MENUITEM_STATUS_REL
add constraint FK_MenuItem_Status foreign key (MENUITEMID)
references menuitem (ID)';

execute immediate 'alter table MENUITEM_STATUS_REL
  add constraint FK_Status_MenuItem foreign key (STATUSID)
  references appstatus (ID)';
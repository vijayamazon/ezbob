execute immediate 'ALTER TABLE MenuItem ADD  ParentId NUMBER NULL';

execute immediate 'alter table MenuItem
add constraint FK_MenuItem_MenuItem foreign key (ParentId)
references menuitem (ID)';
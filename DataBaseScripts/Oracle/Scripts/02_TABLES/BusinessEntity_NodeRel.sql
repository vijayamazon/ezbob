-- Create table
create table BusinessEntity_NodeRel
(
  ID               NUMBER not null,
  NodeId           NUMBER not null,
  BusinessEntityId NUMBER not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table BusinessEntity_NodeRel
  add constraint PK_BusinessEntity_NodeRel primary key (ID);
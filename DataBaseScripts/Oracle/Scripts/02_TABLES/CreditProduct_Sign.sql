-- Create table
create table CreditProduct_Sign
(
  ID              NUMBER not null,
  CREDITPRODUCTID NUMBER not null,
  CREATIONDATE    DATE   not null,
  DATA            CLOB   not null,
  SIGNEDDOCUMENT  CLOB   not null,
  USERID          NUMBER not null
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table CreditProduct_Sign
  add constraint PK_CreditProduct_Sign primary key (ID);
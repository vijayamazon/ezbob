-- Create table
create table CREDITPRODUCT_PRODUCTS
(
  ID           NUMBER not null,
  NAME         VARCHAR2(1024),
  DESCRIPTION  VARCHAR2(1024),
  CREATIONDATE DATE,
  USERID       NUMBER,
  ISDELETED    NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table CREDITPRODUCT_PRODUCTS
  add constraint PK_CREDITPRODUCT_PRODUCTS primary key (ID);


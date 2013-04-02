-- Create table
create table CREDITPRODUCT_PARAMS
(
  ID              NUMBER not null,
  NAME            VARCHAR2(1024),
  TYPE            VARCHAR2(1024),
  DESCRIPTION     VARCHAR2(1024),
  CREDITPRODUCTID NUMBER,
  VALUE           VARCHAR2(2096)
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table CREDITPRODUCT_PARAMS
  add constraint PK_CREDITPRODUCT_PARAMS primary key (ID);
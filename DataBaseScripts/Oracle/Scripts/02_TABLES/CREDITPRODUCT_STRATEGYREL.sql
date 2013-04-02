-- Create table
create table CREDITPRODUCT_STRATEGYREL
(
  ID              NUMBER not null,
  CREDITPRODUCTID NUMBER,
  STRATEGYID      NUMBER
);
-- Create/Recreate primary, unique and foreign key constraints 
alter table CREDITPRODUCT_STRATEGYREL
  add constraint PK_CREDITPRODUCT_STRATEGYREL primary key (ID);
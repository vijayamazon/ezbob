-- Create table
create table STRATEGY_PUBLICNAME
(
  PUBLICNAMEID         NUMBER not null,
  NAME                 VARCHAR2(255) not null,
  ISSTOPPED            NUMBER,
  IsDeleted            NUMBER DEFAULT 0 NOT NULL,
  DeleterUserId        NUMBER NULL,
  TERMINATIONDATE      DATE NULL,
  SignedDocumentDelete CLOB NULL
);
-- Add comments to the columns 
comment on column STRATEGY_PUBLICNAME.PUBLICNAMEID
  is 'Primary key';
comment on column STRATEGY_PUBLICNAME.NAME
  is 'Public name';
comment on column STRATEGY_PUBLICNAME.ISSTOPPED
  is 'Is public name stopped or not';

alter table STRATEGY_PUBLICNAME
  add constraint PK_STRATEGY_PUBLICNAME primary key (PUBLICNAMEID);


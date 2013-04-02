-- Create table
create table SV_REPORTINGINFO
(
  ID               NUMBER not null,
  PATHDIR          VARCHAR2(255) not null,
  USERID           NUMBER not null,
  CREATIONDATE     DATE not null,
  CREATINGSTATE    NUMBER not null,
  ERRORMESSAGE     VARCHAR2(500),
  TIMECREATIONCUBE NUMBER not null
);
-- Add comments to the columns 
comment on column SV_REPORTINGINFO.PATHDIR
  is 'Path to directory';
comment on column SV_REPORTINGINFO.USERID
  is 'Owner';
comment on column SV_REPORTINGINFO.CREATIONDATE
  is 'date creation';
comment on column SV_REPORTINGINFO.CREATINGSTATE
  is 'Initial = 0,
SentSrv=1,
GotSrv=2, Creating=3, Created=4, Error=5, StatusNotSupported=6';
comment on column SV_REPORTINGINFO.TIMECREATIONCUBE
  is 'in sec';
-- Create/Recreate primary, unique and foreign key constraints 
alter table SV_REPORTINGINFO
  add constraint PK_SV_REPORTINGINFO primary key (ID)


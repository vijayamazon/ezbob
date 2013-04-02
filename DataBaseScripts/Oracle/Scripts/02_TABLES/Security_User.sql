-- Create table
create table SECURITY_USER
(
  USERID            NUMBER not null,
  USERNAME          VARCHAR2(30) not null,
  FULLNAME          VARCHAR2(250) not null,
  PASSWORD          VARCHAR2(200),
  CREATIONDATE      DATE default CURRENT_TIMESTAMP not null,
  ISDELETED         NUMBER default 0 not null,
  EMAIL             VARCHAR2(255),
  CREATEUSERID      NUMBER,
  DELETIONDATE      DATE,
  DELETEUSERID      NUMBER,
  BRANCHID          NUMBER not null,
  PASSSETTIME       DATE default sysdate,
  LOGINFAILEDCOUNT  NUMBER,
  DISABLEDATE       DATE,
  LASTBADLOGIN      DATE,
  PASSEXPPERIOD     NUMBER,
  FORCEPASSCHANGE   NUMBER,
  DISABLEPASSCHANGE NUMBER,
  DELETEID          NUMBER,
  CERTIFICATETHUMBPRINT VARCHAR2(40),
  DOMAINUSERNAME    VARCHAR2(250)
);
-- Add comments to the columns 
comment on column SECURITY_USER.USERID
  is 'Primary key';
comment on column SECURITY_USER.USERNAME
  is 'User login';
comment on column SECURITY_USER.FULLNAME
  is 'User full name';
comment on column SECURITY_USER.PASSWORD
  is 'User password (Only for Forms Authentication)';
comment on column SECURITY_USER.CREATIONDATE
  is 'User Creation date';
comment on column SECURITY_USER.ISDELETED
  is '0-Active; 1 - is deleted; 2 - is blocked';
comment on column SECURITY_USER.EMAIL
  is 'User email';
comment on column SECURITY_USER.CREATEUSERID
  is 'User created record';
comment on column SECURITY_USER.DELETIONDATE
  is 'User deletion date';
comment on column SECURITY_USER.DELETEUSERID
  is 'User who deleted the record';
-- Create/Recreate primary, unique and foreign key constraints 
alter table SECURITY_USER
  add constraint PK_SECURITY_USER primary key (USERID);



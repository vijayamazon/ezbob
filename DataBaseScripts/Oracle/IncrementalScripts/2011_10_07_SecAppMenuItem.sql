execute immediate 'alter table SECURITY_APPLICATION modify name VARCHAR2(256)';
execute immediate 'alter table SECURITY_APPLICATION modify description VARCHAR2(256)';

execute immediate 'alter table MENUITEM modify caption VARCHAR2(256)';
execute immediate 'alter table MENUITEM modify description VARCHAR2(256)';
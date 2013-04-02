execute immediate 'create sequence SEQ_APPSTATUS
minvalue 1
maxvalue 999999999999999999999999999
start with 10000
increment by 1000
cache 20';

execute immediate 'create sequence SEQ_SEC_APP
minvalue 1
maxvalue 999999999999999999999999999
start with 1000
increment by 1
cache 20';

execute immediate 'create sequence SEQ_MENUITEM
minvalue 1
maxvalue 999999999999999999999999999
start with 1000
increment by 1
cache 20';
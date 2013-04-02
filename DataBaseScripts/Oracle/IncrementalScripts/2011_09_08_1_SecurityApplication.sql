execute immediate 'alter table SECURITY_APPLICATION drop constraint IX_SECURITY_APPLICATION cascade';
execute immediate 'alter table SECURITY_APPLICATION drop column version';
execute immediate 'alter table SECURITY_APPLICATION drop column state';
execute immediate 'alter table SECURITY_APPLICATION add constraint IX_SECURITY_APPLICATION unique (NAME) '; 
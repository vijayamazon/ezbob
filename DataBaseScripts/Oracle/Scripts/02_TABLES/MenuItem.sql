CREATE TABLE MenuItem(
	Id			NUMBER NOT NULL,
	Caption		VARCHAR2(256) NOT NULL,
	Description		VARCHAR2(256) NULL,
	Url			VARCHAR2(512) NOT NULL,
	SecAppId	NUMBER NULL,
	Position	NUMBER NULL,
	FilterId	NUMBER NULL,
	Filter		CLOB NULL,
	ParentId	NUMBER NULL
);

alter table MENUITEM
  add constraint PK_MenuItem primary key (ID);
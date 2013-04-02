CREATE TABLE Application_DetailName (
       DetailNameId         NUMBER NOT NULL,
       Name                 VARCHAR2(255) NOT NULL
);

COMMENT ON TABLE Application_DetailName IS 'Application Detail item Names';
COMMENT ON COLUMN Application_DetailName.DetailNameId IS 'Primary key';
COMMENT ON COLUMN Application_DetailName.Name IS 'Application detail item name';
CREATE UNIQUE INDEX PK_Application_DetailName ON Application_DetailName
(
       DetailNameId                   ASC
);


ALTER TABLE Application_DetailName
       ADD  ( CONSTRAINT PK_Application_DetailName PRIMARY KEY (
              DetailNameId) ) ;



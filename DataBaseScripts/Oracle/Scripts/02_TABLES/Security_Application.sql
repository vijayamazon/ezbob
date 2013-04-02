CREATE TABLE Security_Application (
       ApplicationId        NUMBER NOT NULL,
       Name                 VARCHAR2(256) NOT NULL,
       Description          VARCHAR2(256) NULL,
       ApplicationType      NUMBER NULL
);

COMMENT ON COLUMN Security_Application.ApplicationId IS 'Primary key';
COMMENT ON COLUMN Security_Application.Name IS 'Application name';
COMMENT ON COLUMN Security_Application.ApplicationType IS '0 -  Server Application; 1 - Presentation Application';
CREATE UNIQUE INDEX PK_Security_Application ON Security_Application
(
       ApplicationId                  ASC
);


ALTER TABLE Security_Application
       ADD  ( CONSTRAINT PK_Security_Application PRIMARY KEY (
              ApplicationId) ) ;

CREATE TABLE Security_Branch (
       BranchId             NUMBER NOT NULL,
       Name                 VARCHAR2(255) NULL,
       Description          VARCHAR2(500) NULL,
       Identifier           VARCHAR2(255) NULL,
       CreationDate         DATE NOT NULL,
       ModifyDate           DATE NOT NULL
);

CREATE UNIQUE INDEX PK_SECURITY_BRANCH ON Security_Branch
(
       BranchId                       ASC
);


ALTER TABLE Security_Branch
       ADD  ( CONSTRAINT PK_SECURITY_BRANCH PRIMARY KEY (BranchId) ) ;


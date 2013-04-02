CREATE TABLE Application_NodeDataSign (
       Id                   NUMBER NOT NULL,
       ApplicationId        NUMBER NOT NULL,
       NodeId               NUMBER NULL,
       OutletName           VARCHAR2(50 BYTE) NOT NULL,
       DATEADDED            DATE NOT NULL,
       signedData           CLOB NOT NULL,
       data                 CLOB NOT NULL,
       nodeName             VARCHAR2(250 BYTE) NOT NULL,
       userName             VARCHAR2(30) NOT NULL
);

CREATE UNIQUE INDEX PK_App_NodeDataSign ON Application_NodeDataSign
(
       Id           ASC
);


ALTER TABLE Application_NodeDataSign
       ADD  ( CONSTRAINT PK_App_NodeDataSign PRIMARY KEY (Id) ) ;

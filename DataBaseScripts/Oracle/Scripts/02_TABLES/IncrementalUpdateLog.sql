CREATE TABLE IncrementalUpdateLog (
       Id        NUMBER NOT NULL,
       HostName         VARCHAR(50) NULL,
       AccountName        VARCHAR(50) NULL,
       ActionDate           DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       FileName       VARCHAR(200) NULL,
       FileBody                CLOB NULL,
       CheckSum       VARCHAR(50) NULL
);
CREATE UNIQUE INDEX PK_IncrementalUpdateLog ON IncrementalUpdateLog(Id ASC);
ALTER TABLE IncrementalUpdateLog   ADD  ( CONSTRAINT PK_IncrementalUpdateLog PRIMARY KEY (Id)) ;


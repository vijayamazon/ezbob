-- Create table
CREATE TABLE ServiceRegistration
(
  Id             NUMBER                         NOT NULL,
  "KEY"          VARCHAR2(255)                  NOT NULL
);

ALTER TABLE ServiceRegistration 
       ADD ( CONSTRAINT PK_ServiceRegistration PRIMARY KEY
              (Id)
           );



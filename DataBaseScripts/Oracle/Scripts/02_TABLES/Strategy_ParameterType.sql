CREATE TABLE Strategy_ParameterType (
       ParamTypeId          INTEGER NOT NULL,
       Name                 VARCHAR2(50) NOT NULL,
       Description          VARCHAR2(500) NULL
);

COMMENT ON COLUMN Strategy_ParameterType.ParamTypeId IS 'PK';
COMMENT ON COLUMN Strategy_ParameterType.Name IS 'Type name';
COMMENT ON COLUMN Strategy_ParameterType.Description IS 'Description';
CREATE UNIQUE INDEX PK_Strategy_ParameterType ON Strategy_ParameterType
(
       ParamTypeId                    ASC
);


ALTER TABLE Strategy_ParameterType
       ADD  ( CONSTRAINT PK_Strategy_ParameterType PRIMARY KEY (
              ParamTypeId) ) ;
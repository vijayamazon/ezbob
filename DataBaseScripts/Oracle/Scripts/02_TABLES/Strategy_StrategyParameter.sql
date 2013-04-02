CREATE TABLE Strategy_StrategyParameter (
       StratParamId         NUMBER NOT NULL,
       TypeId               NUMBER NOT NULL,
       OwnerId              NUMBER NOT NULL,
       Name                 VARCHAR2(255) NOT NULL,
       Description          VARCHAR2(500) NULL,
       IsInput              SMALLINT NOT NULL,
       IsOutput             SMALLINT NOT NULL
);

COMMENT ON COLUMN Strategy_StrategyParameter.StratParamId IS 'parameter id';
COMMENT ON COLUMN Strategy_StrategyParameter.TypeId IS 'parameter type id';
COMMENT ON COLUMN Strategy_StrategyParameter.OwnerId IS 'Owner Strategy Id';
COMMENT ON COLUMN Strategy_StrategyParameter.Name IS 'name';
COMMENT ON COLUMN Strategy_StrategyParameter.Description IS 'description';
COMMENT ON COLUMN Strategy_StrategyParameter.IsInput IS 'is input';
COMMENT ON COLUMN Strategy_StrategyParameter.IsOutput IS 'is output';
CREATE UNIQUE INDEX PK_Strategy_StrategyParameter ON Strategy_StrategyParameter
(
       StratParamId                   ASC
);


ALTER TABLE Strategy_StrategyParameter
       ADD  ( CONSTRAINT PK_Strategy_StrategyParameter PRIMARY KEY (
              StratParamId) ) ;
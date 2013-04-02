CREATE TABLE StrategyEngine_ExecutionState (
       Id                   NUMBER NOT NULL,
       ApplicationId        NUMBER NOT NULL,
       CurrentNodeId        NUMBER NULL,
       CurrentNodePostfix   VARCHAR2(1000),
       Data                 CLOB NULL,
       StartTime            DATE DEFAULT CURRENT_TIMESTAMP NOT NULL,
       IsTimeoutReported    NUMBER NULL
);

COMMENT ON COLUMN StrategyEngine_ExecutionState.Id IS 'PK';
COMMENT ON COLUMN StrategyEngine_ExecutionState.ApplicationId IS 'Application identifier';
COMMENT ON COLUMN StrategyEngine_ExecutionState.CurrentNodeId IS 'Strategy''s current node identifier';
COMMENT ON COLUMN StrategyEngine_ExecutionState.CurrentNodePostfix IS 'Numeric postfix for current node instance';
COMMENT ON COLUMN StrategyEngine_ExecutionState.Data IS 'XML of current strategy state';
CREATE UNIQUE INDEX PK_StrategyEngine_ExecutionSta ON StrategyEngine_ExecutionState
(
       Id                             ASC
);
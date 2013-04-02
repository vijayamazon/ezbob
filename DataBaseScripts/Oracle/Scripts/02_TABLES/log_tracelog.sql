CREATE TABLE Log_TraceLog (
       Id                   NUMBER NOT NULL,
       ApplicationId        NUMBER NULL,
       Type                 NUMBER NULL,
       Code                 NUMBER NULL,
       Message              CLOB NULL,
       Data                 CLOB NULL,
       InsertDate           DATE NULL,
       ThreadId             VARCHAR2(50) NULL
);

COMMENT ON COLUMN Log_TraceLog.Id IS 'Id';
COMMENT ON COLUMN Log_TraceLog.ApplicationId IS 'Application Id';
COMMENT ON COLUMN Log_TraceLog.Type IS '0- Critical, 1 - Error, 2 - Information, 3 - Resume, 4 - Start, 5 - Stop, 6 - Suspend, 7 - Transfer, 8 - Verbose, 9 - Warning';
COMMENT ON COLUMN Log_TraceLog.Code IS 'Trace message code';
COMMENT ON COLUMN Log_TraceLog.Message IS 'Trace message';
COMMENT ON COLUMN Log_TraceLog.Data IS 'Trace Data';
COMMENT ON COLUMN Log_TraceLog.InsertDate IS 'When record was inserted';
COMMENT ON COLUMN Log_TraceLog.ThreadId IS 'thread id';
CREATE UNIQUE INDEX PK_Log_TraceLog ON Log_TraceLog
(
       Id                             ASC
);


ALTER TABLE Log_TraceLog
       ADD  ( CONSTRAINT PK_Log_TraceLog PRIMARY KEY (Id) ) ;


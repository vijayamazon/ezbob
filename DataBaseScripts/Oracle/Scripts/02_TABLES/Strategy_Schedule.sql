CREATE TABLE Strategy_Schedule (
	ID              NUMBER NOT NULL,
	Name            VARCHAR2(256),
	StrategyId      NUMBER NOT NULL,
        ScheduleType    NUMBER DEFAULT 0 NOT NULL,
        ExecutionType   NUMBER DEFAULT 0 NOT NULL,
	ScheduleMask    VARCHAR2(512),
	NextRun         DATE,
	CreatorUserId   NUMBER,
	ISPAUSED      NUMBER
);

ALTER TABLE Strategy_Schedule
	ADD CONSTRAINT PK_Strategy_Schedule PRIMARY KEY (ID);

CREATE OR REPLACE PROCEDURE StrategyScheduleParam_Delete
(
    pName  Varchar2,
    pScheduleId NUMBER
)
AS
BEGIN
    UPDATE Strategy_ScheduleParam
    SET Deleted = Id
    WHERE Name = pName
      AND Deleted IS null
      AND StrategyScheduleId = pScheduleId;
END;
/

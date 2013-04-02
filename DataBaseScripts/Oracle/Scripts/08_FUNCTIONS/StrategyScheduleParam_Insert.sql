CREATE OR REPLACE FUNCTION StrategyScheduleParam_Insert
  (
      pScheduleId in Number
      ,pName in varchar2
      ,pDescription in varchar2
      ,pUserId in Number
      ,pData in varchar2
      ,pSignedDocument in varchar2
  )
RETURN NUMBER
AS
  l_ParameterTypeID NUMBER := null;
  l_ParameterId NUMBER := null;
BEGIN
     begin
       SELECT Id INTO l_ParameterTypeID
       FROM Strategy_ScheduleParam
       WHERE Name = pName 
         AND Deleted IS null
         AND StrategyScheduleId = pScheduleId;
     exception when no_data_found then
       null;
     end;

     if not (l_ParameterTypeID is null) then
          UPDATE Strategy_ScheduleParam
          SET Deleted = l_ParameterTypeID
          WHERE Id = l_ParameterTypeID;
     end if;
     SELECT SEQ_StrategyScheduleParam.NEXTVAL INTO l_ParameterId FROM DUAL;

     INSERT INTO Strategy_ScheduleParam
           (Id
           ,StrategyScheduleId
           ,CurrentVersionId
           ,Name
           ,Description
           ,Data
           ,UserId
           ,Deleted
           ,SignedDocument)
     VALUES
           (l_ParameterId
           ,pScheduleId
           ,0
           ,pName
           ,pDescription
           ,pData
           ,pUserId
           ,null
           ,pSignedDocument);

     return l_ParameterId;
END;
/

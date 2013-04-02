CREATE OR REPLACE PROCEDURE UpdateApplicationNodeSetting(
   pApplicationId IN NUMBER,
   pNodeName      IN VARCHAR2,
   pNodePostfix   IN VARCHAR2,
   pSettingName   IN VARCHAR2,
   pValue         IN NUMBER
 )
AS
  l_data_exists    Number := 0;
  l_node_id        Number := 0;
  l_NodeSetting_id Number;
BEGIN
  begin
    SELECT NodeId
    INTO l_node_id
    FROM STRATEGY_NODE
    WHERE UPPER(Name) = UPPER(pNodeName)
      AND IsDeleted = 0;
  exception
    when NO_DATA_FOUND then
    l_node_id := null;
  end;

  SELECT count(*)
  INTO l_data_exists
  FROM Application_NodeSetting
  WHERE  ApplicationId = pApplicationId
    AND  (NodeId = l_node_id OR NodeId IS NULL)
    AND  NODEPOSTFIX = pNodePostfix;
  
  If l_data_exists = 0 then
   Select SEQ_app_NodeSetting.Nextval into l_NodeSetting_id from dual;
   INSERT INTO Application_NodeSetting
      (ID, ApplicationId, NodeId, NODEPOSTFIX, Name, Value)
    VALUES
      (l_NodeSetting_id, pApplicationId, l_node_id, pNodePostfix, pSettingName, pValue);
  else
    UPDATE Application_NodeSetting
    SET
      Name = pSettingName,
      Value = pValue
    WHERE  ApplicationId = pApplicationId
      AND  (NodeId = l_node_id OR NodeId IS NULL)
      AND  NODEPOSTFIX = pNodePostfix;
  end if;
  
END UpdateApplicationNodeSetting;
/
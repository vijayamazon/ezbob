CREATE OR REPLACE PROCEDURE Strategy_StrategyParamInsert
  (
      pParameterTypeName in varchar2,
      pStrategyID in Number,
      pVariableName in varchar2,
      pVariableDescription in varchar2,
      pVariableIsInput in Number,
      pVariableIsOutput in Number

  )
AS
  l_ParameterTypeID Number := null;
  l_StratParameterId Number;
BEGIN
     begin
       SELECT Paramtypeid into l_ParameterTypeID
       FROM Strategy_ParameterType spt
       WHERE spt.name = pParameterTypeName;
     exception when no_data_found then
       null;
     end;

     if l_ParameterTypeID is null then
          Select SEQ_STRATEGY_PARAMTYPE.NEXTVAL into l_ParameterTypeID from dual;
          INSERT INTO Strategy_ParameterType(ParamTypeId, Name)
                 VALUES (l_ParameterTypeID, pParameterTypeName );

     end if;

     Select SEQ_STRATEGY_PARAMETER.NEXTVAL into l_stratParameterId from dual;
     INSERT INTO Strategy_StrategyParameter
                 (StratParamId, TypeID, OwnerID, Name, Description, IsInput, IsOutput)
            VALUES(l_stratParameterId, l_ParameterTypeID, pStrategyID, pVariableName, pVariableDescription, pVariableIsInput, pVariableIsOutput);

exception when others then
    raise;
END;
/

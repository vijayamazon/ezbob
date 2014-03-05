IF EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_StrategyParamInsert]') AND TYPE IN (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_StrategyParamInsert]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_StrategyParamInsert] 
	(@pParameterTypeName nvarchar(max)
                  , @pStrategyID int
                  , @pVariableName nvarchar(max)
                  , @pVariableDescription nvarchar(max)
                  , @pVariableIsInput bit
                  , @pVariableIsOutput bit)
AS
BEGIN
	DECLARE @pParameterTypeID int;
   
     SELECT @pParameterTypeID = NULL;
     
     SELECT TOP 1 @pParameterTypeID = Strategy_ParameterType.ParamTypeID
     FROM Strategy_ParameterType
     WHERE @pParameterTypeName = [Strategy_ParameterType].[Name];
     IF @pParameterTypeID IS NULL
     BEGIN
          INSERT INTO [Strategy_ParameterType]( [Name] )
                 VALUES ( @pParameterTypeName )
                 
          SELECT @pParameterTypeID = SCOPE_IDENTITY()
     END
     INSERT INTO [Strategy_StrategyParameter]( [TypeID]
                 , [OwnerID]
                 , [Name]
                 , [Description]
                 , [IsInput]
                 , [IsOutput]
                 )
            VALUES( @pParameterTypeID
                  , @pStrategyID
                  , @pVariableName
                  , @pVariableDescription
                  , @pVariableIsInput
                  , @pVariableIsOutput
                  ) 
END
GO

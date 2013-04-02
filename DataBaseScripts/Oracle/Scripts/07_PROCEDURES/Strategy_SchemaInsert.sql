CREATE OR REPLACE PROCEDURE Strategy_SchemaInsert
(
    pStrategyId IN NUMBER,
    pBinaryData IN BLOB
)
AS
BEGIN
   INSERT INTO Strategy_Schemas
           (StrategyId,BinaryData)
     VALUES
           (pStrategyId, pBinaryData);
END;

/

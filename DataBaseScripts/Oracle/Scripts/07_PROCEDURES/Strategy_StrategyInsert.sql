CREATE OR REPLACE PROCEDURE Strategy_StrategyInsert
   (
      pStrategyName in varchar2,
      pStrategyDescription in varchar,
      pStrategyIcon in blob,
      pStrategyIsEmbeddingAllowed in Number,
      pStrategyXml in clob,
      pStrategyUserID in Number,
      pStrategyID out Number,
      pStrategyType in varchar2,
      pSignedDocument in clob,
      pDisplayName in varchar2,
      pRepublish in number
   )
AS
pOldState number;
BEGIN
   begin
     select state into  pOldState from strategy_strategy
     where displayname = pDisplayName and termdate is null;
   exception when no_data_found then
     pOldState := 0;
   end;

   update Strategy_Strategy set termdate = sysdate,
   substate = 1
   where displayname = pDisplayName and termdate is null;

    Select Seq_strategy_strategy.Nextval into pStrategyID from dual;

    INSERT INTO Strategy_Strategy
           (StrategyId, Name, Description, Icon, IsEmbeddingAllowed, XML, UserID, AuthorId, StrategyType, DisplayName, SignedDocument)
    VALUES(pStrategyID, pStrategyName, pStrategyDescription, pStrategyIcon, pStrategyIsEmbeddingAllowed,
           pStrategyXML, pStrategyUserID, pStrategyUserID, pStrategyType, pDisplayName, pSignedDocument
          );

    if pRepublish is not null then
      
      update strategy_publicrel set
      strategyid = pStrategyID
      where strategyid in ( 
      select strategyid from strategy_strategy where
      displayname = pDisplayName and termdate is not null);
      
      update strategy_strategy set
      state = 0
      where displayname = pDisplayName and termdate is not null;
      
      update strategy_strategy set
      state = pOldState
      where strategyid = pStrategyID;
    end if;


END;

/

CREATE OR REPLACE TRIGGER StrategyUpdateTrigger
 AFTER INSERT OR UPDATE ON Strategy_Strategy for each row
declare
 l_logId Number;
 l_flag Number := 0;
begin
  if not PKG_STRATEGYUPDATE.latch then

    if Inserting then
      if (:new.name is not null and :new.description is not null and :new.xml is not null and :new.icon is not null) then
        l_flag := 1;
      end if;
    end if;

    if Updating('NAME') and Updating('DESCRIPTION') and Updating('XML') and Updating('ICON') then
        l_flag := 1;
    end if;

    if l_flag = 1 then
     Select SEQ_LOG_STRATEGY.NEXTVAL into l_logid from dual;

     INSERT INTO Log_Strategy
             ( LogStrategyId, StrategyID, Name, Description, IsEmbeddingAllowed, XML, AuthorId,
							 State, SubState, CreationDate
              )
            VALUES( l_logid,
                    :new.StrategyId,
                    :new.name,
                    :new.Description,
                    :new.IsEmbeddingAllowed,
                    :new.XML,
      				      :new.AuthorId,
				            :new.State,
                    :new.SubState,
                    :new.CreationDate
                  );

      PKG_STRATEGYUPDATE.t_logIds(l_logid) := l_logid;
    end if;
  end if;
end;
/
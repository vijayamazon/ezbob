create or replace function Strategy_strategyList
(
  pUserId in Number,
  pIsPublished in Number,
  pIsEmbeddingAllowed in Number
) return sys_refcursor
is
 p_cur sys_refcursor;
 publishedEnabled number;
begin
  publishedEnabled:= pIsPublished;

 open p_cur for
 
 
 
  Select s.StrategyId as StrategyId,
                 s.name as "Name",
		 s.displayname as "DisplayName",
		 s.Description as "Description",
		 u.fullname as "Author",
		 s.Termdate,
		 s.Creationdate as "PublishingDate",
		 (
		   case s.SubState
			when 0 then
			  case s.UserId
				   when pUserId then
					  0
				   else 1
				 end
			when 1 then 0
			else 1
		   end
		 ) as "IsReadOnly",
		s.IsEmbeddingAllowed as "IsEmbeddingAllowed",
		(
		   case (select COUNT(publicid)  from strategy_publicrel e
				 where e.strategyid = s.strategyid)
		   when 0 then 0
		   else 1
		   end
		) as "IsPublished",
		s.Icon as "Icon",
		s.UserId as "UserId",
                s.IsMigrationSupported  as IsMigrationSupported
    from
    Strategy_Strategy s JOIN Security_User u
       ON s.AuthorID = u.userid
    WHERE ( s.State = pIsPublished OR pIsPublished IS NULL )
              AND ( s.IsEmbeddingAllowed = pIsEmbeddingAllowed OR pIsEmbeddingAllowed IS NULL )
              AND ( s.IsDeleted = 0)
    order by s.displayname, s.termdate desc;
    return p_cur;
end;
/

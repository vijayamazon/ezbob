IF  EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Strategy_UpdatePublishName]') AND type in (N'P', N'PC'))
DROP PROCEDURE [dbo].[Strategy_UpdatePublishName]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Strategy_UpdatePublishName] 
	@pId as bigint OUTPUT,
	@pName as nvarchar(max),
	@pIsStopped as int
AS
BEGIN
	DECLARE @l_id int;

	if @pId is not null 
		begin
		  select @l_id = COUNT(publicnameid)  from strategy_publicname
		  where (upper(name) = upper(@pName))
                    and publicnameid <> @pId
                    and (IsDeleted is null or IsDeleted = 0);
		  if (@l_id > 0) begin
			RAISERROR('dublicated_name', 16,1);
			RETURN;
			end;
      
		   update strategy_publicname
			set name = @pName,
				isstopped = @pIsStopped
		  where publicnameid = @pId;
		end

else
	begin
      select @l_id = COUNT(publicnameid) from strategy_publicname
      where (upper(name) = upper(@pName))
        and (IsDeleted is null or IsDeleted = 0);
      if (@l_id > 0) begin
			RAISERROR('dublicated_name', 16,1);
			RETURN;
			end;
      insert into strategy_publicname
        (name)
      values
        (@pName);
	 set @pId = @@IDENTITY;
	end;

END
GO

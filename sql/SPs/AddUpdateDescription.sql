IF OBJECT_ID('AddUpdateDescription') IS NULL
	EXECUTE('CREATE PROCEDURE AddUpdateDescription AS SELECT 1')
GO

/****** Object:  StoredProcedure [dbo].[AddUpdateDescription]    Script Date: 4/7/2015 2:33:44 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[AddUpdateDescription]
--	@name sysname,
	@value sql_variant			= NULL,
	@level0type	varchar(128)	= NULL,
	@level0name	sysname			= NULL,
	@level1type	varchar(128)	= NULL,
	@level1name	sysname			= NULL,
	@level2type	varchar(128)	= NULL,
	@level2name	sysname			= NULL
as
begin
	SET NOCOUNT ON;

	declare  @DescriptionExists int;	

	declare @name sysname;
	set @name = N'ColDescription';

	set @DescriptionExists = (select ep.minor_id from sys.all_objects o inner join sys.all_columns c on o.object_id = c.object_id and o.name = @level1name and c.name = @level2name
inner join sys.extended_properties ep on ep.minor_id = c.column_id and ep.major_id = o.object_id and ep.name = @name);

--select @DescriptionExists;
--return 1;

	if (@DescriptionExists > 0) -- IS NOT NULL
		begin 

		--select @DescriptionExists;

		--print cast( @name as nvarchar(256)); 		print cast( @value as nvarchar(256));return 1;

			exec [sys].[sp_updateextendedproperty] 	@name,	@value,	@level0type, @level0name, @level1type, @level1name, @level2type, @level2name	;		
		return (1);
	end


	declare @ret int

	if datalength(@value) > 7500
	begin
		raiserror(15097,-1,-1)
		return 1
	end
	
	if @name is null
	begin
		raiserror(15600,-1,-1,'sp_addextendedproperty')
		return (1)
	end

	execute @ret = sys.sp_validname @name
	if (@ret <> 0)
	begin
		raiserror(15600,-1,-1,'sp_addextendedproperty')
		return (1)
	end

	BEGIN TRANSACTION
	
	begin

	EXEC [sys].[sp_addextendedproperty] @name,	@value,	@level0type, @level0name, @level1type, @level1name, @level2type, @level2name	;
		--EXEC %%ExtendedPropertySet().AddValue(Name = @name, Value = @value, Level0type = @level0type, Level0name = @level0name, Level1type = @level1type, Level1name = @level1name, Level2type = @level2type, Level2name = @level2name)
		IF @@error <> 0
		begin
			COMMIT TRANSACTION
			return (1)
		end
	end
	
	COMMIT TRANSACTION
	return (0)
END
GO

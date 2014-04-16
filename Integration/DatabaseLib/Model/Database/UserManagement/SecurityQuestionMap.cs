namespace EZBob.DatabaseLib.Model.Database.UserManagement
{
	using FluentNHibernate.Mapping;

	public class SecurityQuestionMap : ClassMap<SecurityQuestion>
	{
		public SecurityQuestionMap()
		{
			Not.LazyLoad();
			Table("Security_Question");
			Cache.ReadWrite().Region("Longest").ReadWrite();
			Id(sq => (object)sq.Id).Column("Id");
			Map(sq => sq.Name).Length(200);
		}
	}
}

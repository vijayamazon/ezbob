using FluentNHibernate.Mapping;
using System;
namespace NHibernateWrapper.NHibernate.Model
{
	public class SecurityQuestionMap : ClassMap<SecurityQuestion>
	{
		public SecurityQuestionMap()
		{
			base.Not.LazyLoad();
			base.Table("Security_Question");
			base.Cache.ReadWrite().Region("Longest").ReadWrite();
			this.Id((SecurityQuestion x) => (object)x.Id).Column("Id");
			base.Map((SecurityQuestion x) => x.Name).Length(200);
		}
	}
}

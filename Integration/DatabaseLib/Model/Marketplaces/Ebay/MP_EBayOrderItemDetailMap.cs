using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	public class MP_EBayOrderItemDetailMap : ClassMap<MP_EBayOrderItemDetail>
	{
		public MP_EBayOrderItemDetailMap()
		{
			Table("MP_EBayOrderItemDetail");
			Id(x => x.Id);

			Map( x => x.ItemID );
			References( x => x.PrimaryCategory, "PrimaryCategoryId" );
			References( x => x.SecondaryCategory, "SecondaryCategoryId" );
			References( x => x.FreeAddedCategory, "FreeAddedCategoryId" );
			Map( x => x.Title );
		}
	}
}
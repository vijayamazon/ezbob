﻿using System;
using FluentNHibernate.Mapping;

namespace EZBob.DatabaseLib.Model.Database
{
	using Iesi.Collections.Generic;

	public enum CustomerAddressType
	{
		PersonalAddress = 1,
		PrevPersonAddresses = 2,
		LimitedCompanyAddress = 3,
		LimitedDirectorHomeAddress = 4,
		NonLimitedCompanyAddress = 5,
		NonLimitedDirectorHomeAddress = 6,
		LimitedCompanyAddressPrev = 7,
		LimitedDirectorHomeAddressPrev = 8,
		NonLimitedCompanyAddressPrev = 9,
		NonLimitedDirectorHomeAddressPrev = 10,
		OtherPropertyAddress = 11,
		OtherPropertyAddressPrev = 12
	}

	[Serializable]
	public class CustomerAddress
	{
		[Newtonsoft.Json.JsonIgnore]
		public virtual Customer Customer { get; set; }
		[Newtonsoft.Json.JsonIgnore]
		public virtual Director Director { get; set; }

		public virtual int AddressId { get; set; }
		public virtual CustomerAddressType AddressType { get; set; }
		public virtual string Id { get; set; }
		public virtual string Organisation { get; set; }
		public virtual string Line1 { get; set; }
		public virtual string Line2 { get; set; }
		public virtual string Line3 { get; set; }
		public virtual string Town { get; set; }
		public virtual string County { get; set; }
		public virtual string Postcode { get; set; }
		public virtual string Country { get; set; }
		public virtual string Rawpostcode { get; set; }
		public virtual string Deliverypointsuffix { get; set; }
		public virtual string Nohouseholds { get; set; }
		public virtual string Smallorg { get; set; }
		public virtual string Pobox { get; set; }
		public virtual string Mailsortcode { get; set; }
		public virtual string Udprn { get; set; }

		[Newtonsoft.Json.JsonIgnore]
		public virtual ISet<Zoopla> Zoopla { get; set; }
		public virtual string FormattedAddress
		{
			get
			{
				return string.IsNullOrEmpty(Postcode) ? null : string.Format("{0} {1} {2}, {3}, {4}, {5}", 
					new object[] { Line1, Line2, Line3, Town, Country, Postcode });
			}
		}

		public virtual string[] AddressArray()
		{
			return new string[] { Line1, Line2, Line3, Town, Postcode };
		} // AddressArray
	}
	public static class CustomerAddressExtenstions
	{
		public static string GetFormatted(this CustomerAddress addr)
		{
			return addr == null ? "" : addr.FormattedAddress;
		}
	}
}

namespace EZBob.DatabaseLib.Model.Database.Mappings
{
	using Database;

	public sealed class CustomerAddressMap : ClassMap<CustomerAddress>
	{
		public CustomerAddressMap()
		{
			Table("CustomerAddress");
			Id(x => x.AddressId).Column("addressId").GeneratedBy.Native();
			Map(x => x.AddressType).Column("addressType").CustomType<CustomerAddressType>();
			Map(x => x.Id).Column("id").Length(50);
			Map(x => x.Organisation).Column("Organisation").Length(200);
			Map(x => x.Line1).Column("Line1").Length(200);
			Map(x => x.Line2).Column("Line2").Length(200);
			Map(x => x.Line3).Column("Line3").Length(200);
			Map(x => x.Town).Column("Town").Length(200);
			Map(x => x.County).Column("County").Length(200);
			Map(x => x.Postcode).Column("Postcode").Length(200);
			Map(x => x.Country).Column("Country").Length(200);
			Map(x => x.Rawpostcode).Column("Rawpostcode").Length(200);
			Map(x => x.Deliverypointsuffix).Column("Deliverypointsuffix").Length(200);
			Map(x => x.Nohouseholds).Column("Nohouseholds").Length(200);
			Map(x => x.Smallorg).Column("Smallorg").Length(200);
			Map(x => x.Pobox).Column("Pobox").Length(200);
			Map(x => x.Mailsortcode).Column("Mailsortcode").Length(200);
			Map(x => x.Udprn).Column("Udprn").Length(200);
			References(x => x.Director, "DirectorId");
			References(x => x.Customer, "CustomerId");

			HasMany<Zoopla>(x => x.Zoopla)
				.AsSet()
				.Inverse()
				.KeyColumn("CustomerAddressId")
				.Cascade.AllDeleteOrphan();
		}
	}
}
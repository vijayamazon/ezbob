using System;
using FluentNHibernate.Mapping;
using NHibernate.Type;

namespace EZBob.DatabaseLib.Model.Database
{
    public enum AskvilleStatus
    {
        NotPerformed, Confirmed, NotConfirmed, ReCheck
    }
    public enum AskvilleSendStatus
    {
        Success = 0,
        InvalidAmazonLogin = 1,
        InvalidSellerOrMarketplace = 2,
        Exception = 3
    }
    public class AskvilleStatusType : EnumStringType<AskvilleStatus> { }
    public class AskvilleSendStatusType : EnumStringType<AskvilleSendStatus> { }

    public class Askville
    {
        public virtual string Guid { get; set; }
        public virtual MP_CustomerMarketPlace MarketPlace { get; set; }
        public virtual bool IsPassed { get; set; }
        public virtual AskvilleStatus Status { get; set; }
        public virtual AskvilleSendStatus? SendStatus { get; set; }
        public virtual string MessageBody { get; set; }
        public virtual DateTime? CreationDate { get; set; }
    }

    public sealed class AskvilleMap : ClassMap<Askville>
    {
        public AskvilleMap()
        {
            Table("Askville");
            Id( x => x.Guid );
            References(x => x.MarketPlace, "MarketPlaceId");
            Map(x => x.IsPassed);
            Map(x => x.Status).CustomType<AskvilleStatusType>();
            Map(x => x.SendStatus).CustomType<AskvilleSendStatusType>();
            Map(x => x.MessageBody).CustomType("StringClob").LazyLoad();
            Map(x => x.CreationDate);
        }
    }
}
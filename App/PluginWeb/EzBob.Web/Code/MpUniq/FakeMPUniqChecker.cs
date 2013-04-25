using System;
using EZBob.DatabaseLib.Model.Database;

namespace EzBob.Web.Code.MpUniq
{
    public class FakeMPUniqChecker : AMPUniqChecker
    {
        public override void Check(Guid marketplaceType, Customer customer, string token)
        {
        }
    }
}
using ApplicationMng.Model;
using EZBob.DatabaseLib.Model.Database;
using EzBob.Web.Infrastructure;

namespace ezmanage
{
    public class EzContext : IEzbobWorkplaceContext
    {
        public SecurityApplication SecApp { get { return null; } }
        public int SecAppId { get { return 0; } }
        public User User { get; set; }
        public int UserId { get; set; }
        public string SessionId { get; set; }
        public Customer Customer { get; set; }
    }
}
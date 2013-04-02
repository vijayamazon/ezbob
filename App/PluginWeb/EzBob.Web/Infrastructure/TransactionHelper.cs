using System.Data;
using System.Data.SqlClient;
using NHibernate;

namespace EzBob.Web.Infrastructure
{
    public static class TransactionHelper
    {
        public static SqlTransaction GetTransaction(this ISession session)
        {
            using (IDbCommand command = session.Connection.CreateCommand())
            {
                session.Transaction.Enlist(command);
                return command.Transaction as SqlTransaction;
            }
        }
    }
}
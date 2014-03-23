using NHibernate;
using System;
using System.Data;
namespace NHibernateWrapper.NHibernate
{
	public static class SessionUtils
	{
		public static void Enlist(this ISession session, System.Data.IDbCommand command)
		{
			if (session.Transaction != null && session.Transaction.IsActive)
			{
				session.Transaction.Enlist(command);
			}
			else
			{
				command.Connection = session.Connection;
			}
		}
	}
}

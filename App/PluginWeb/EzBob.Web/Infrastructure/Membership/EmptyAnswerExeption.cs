using System;

namespace EzBob.Web.Infrastructure.Membership
{
    public class EmptyAnswerExeption: Exception
    {
        public EmptyAnswerExeption(string message)
            : base(message)
        {
        }
    }
}
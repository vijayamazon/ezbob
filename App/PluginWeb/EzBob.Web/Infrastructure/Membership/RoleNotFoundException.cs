using System;

namespace EzBob.Web.Infrastructure
{
    [Serializable]
    public class RoleNotFoundException : Exception
    {
        public RoleNotFoundException(string message) : base(message)
        {
        }
    }
}
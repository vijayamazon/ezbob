using System;
using System.Text;
using Microsoft.Web.Services3.Security;
using Microsoft.Web.Services3.Security.Cryptography;
using Microsoft.Web.Services3.Security.Tokens;

namespace ExperianLib
{
    public sealed class WaspToken : BinarySecurityToken
    {
        // The lifetime of the actual token is not known,
        // the current default is two hours but can be specified
        // differently.  The LifeTime here allows the software to
        // refresh the token after a period of time.  This should
        // help prevent several failed calls to the protected 
        // service when the token does actually expire provided the 
        // time period chosen is the same or less than that of the 
        // actual token expiry.
        private readonly LifeTime _lifeTime;

        public WaspToken(string token)
            : base("ExperianWASP")
        {
            // Store the token data into
            RawData = (new UTF8Encoding()).GetBytes(token);
            // Set the token to become invalid after 90
            // minutes
            _lifeTime = new LifeTime(DateTime.Now.AddMinutes(90));
        }

        public override string ToString()
        {
            return Encoding.ASCII.GetString(RawData);
        }

        public override bool Equals(SecurityToken token)
        {
            if (token == null || token.GetType() != GetType())
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            if (RawData != null)
                return RawData.GetHashCode();
            return 0;
        }

        public override bool IsExpired
        {
            get { return _lifeTime.IsExpired; }
        }

        public override bool IsCurrent
        {
            get { return _lifeTime.IsCurrent; }
        }

        public override bool SupportsDigitalSignature
        {
            get { return false; }
        }

        public override bool SupportsDataEncryption
        {
            get { return false; }
        }
        public override KeyAlgorithm Key
        {
            get { return null; }
        }

    }
}

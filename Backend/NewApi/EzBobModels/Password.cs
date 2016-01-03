using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBobModels {
    using System.Runtime.Serialization;
    using EzBobCommon.Utils.Encryption;

    /// <summary>
    /// represents user's password
    /// </summary>
    [DataContract]
    public class Password {
        /// <summary>
        /// The primary
        /// </summary>
        [DataMember]
        private string primary;

        /// <summary>
        /// The confirmation
        /// </summary>
        [DataMember]
        private string confirmation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        public Password()
            : this(null, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="primary">The primary.</param>
        public Password(string primary)
            : this(primary, null) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Password"/> class.
        /// </summary>
        /// <param name="primary">The primary.</param>
        /// <param name="confirmation">The confirmation.</param>
        public Password(string primary, string confirmation) {
            this.primary = null;
            this.confirmation = null;

            if (!string.IsNullOrWhiteSpace(primary))
                this.primary = new Encrypted(primary);

            if (!string.IsNullOrWhiteSpace(confirmation))
                this.confirmation = new Encrypted(confirmation);
        }

        /// <summary>
        /// Gets the primary.
        /// </summary>
        /// <value>
        /// The primary.
        /// </value>
        public string Primary
        {
            get { return string.IsNullOrWhiteSpace(this.primary) ? string.Empty : Encrypted.Decrypt(this.primary); }
        }

        /// <summary>
        /// Gets the confirmation.
        /// </summary>
        /// <value>
        /// The confirmation.
        /// </value>
        public string Confirmation
        {
            get { return string.IsNullOrWhiteSpace(this.confirmation) ? string.Empty : Encrypted.Decrypt(this.confirmation); }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString() {
            bool bHasPrimary = !string.IsNullOrWhiteSpace(Primary);
            bool bHasConfirm = !string.IsNullOrWhiteSpace(Confirmation);

            if (bHasConfirm && bHasPrimary)
                return "x*";

            if (bHasConfirm)
                return "x";

            return bHasPrimary ? "*" : "";
        }
    }
}

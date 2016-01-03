using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzBob3dParties
{
    public class VATRetunFile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VATRetunFile"/> class.
        /// </summary>
        /// <param name="content">The content.</param>
        public VATRetunFile(byte[] content) {
            Content = content;
        }

        /// <summary>
        /// Gets the content of the file.
        /// </summary>
        /// <value>
        /// The content.
        /// </value>
        public byte[] Content { get; private set; }
    }
}

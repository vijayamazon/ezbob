using System;

namespace EzBobModels.Amazon {
    public class AmazonProductCategory : IEquatable<AmazonProductCategory> {
        public string CategoryId { get; set; }
        public string CategoryName { get; set; }
        public AmazonProductCategory Parent { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(AmazonProductCategory other) {
            if (other == null) {
                return false;
            }

            return string.Compare(this.CategoryId, other.CategoryId, StringComparison.InvariantCulture) == 0;
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj) {
            AmazonProductCategory other = obj as AmazonProductCategory;
            if (other == null) {
                return false;
            }

            return this.Equals(other);
        }

        /// <summary>
        /// Serves as a hash function for a particular type. 
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode() {
            if (this.CategoryId != null) {
                return this.CategoryId.GetHashCode();
            }

            return base.GetHashCode();
        }
    }
}

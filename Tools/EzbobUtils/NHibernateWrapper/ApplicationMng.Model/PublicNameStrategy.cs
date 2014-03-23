using System;
namespace ApplicationMng.Model
{
	public class PublicNameStrategy : System.IEquatable<PublicNameStrategy>
	{
		private PublicName _publicName;
		private Strategy _strategy;
		public virtual PublicName PublicName
		{
			get
			{
				return this._publicName;
			}
			set
			{
				this._publicName = value;
			}
		}
		public virtual Strategy Strategy
		{
			get
			{
				return this._strategy;
			}
			set
			{
				this._strategy = value;
			}
		}
		public virtual double Percent
		{
			get;
			set;
		}
		public virtual bool Equals(PublicNameStrategy other)
		{
			return !object.ReferenceEquals(null, other) && (object.ReferenceEquals(this, other) || (object.Equals(other._strategy, this._strategy) && object.Equals(other._publicName, this._publicName)));
		}
		public override bool Equals(object obj)
		{
			return !object.ReferenceEquals(null, obj) && (object.ReferenceEquals(this, obj) || (!(obj.GetType() != typeof(PublicNameStrategy)) && this.Equals((PublicNameStrategy)obj)));
		}
		public override int GetHashCode()
		{
			return ((this._strategy != null) ? this._strategy.GetHashCode() : 0) * 397 ^ ((this._publicName != null) ? this._publicName.GetHashCode() : 0);
		}
		public static bool operator ==(PublicNameStrategy left, PublicNameStrategy right)
		{
			return object.Equals(left, right);
		}
		public static bool operator !=(PublicNameStrategy left, PublicNameStrategy right)
		{
			return !object.Equals(left, right);
		}
	}
}

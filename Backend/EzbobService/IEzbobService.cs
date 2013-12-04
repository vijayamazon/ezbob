namespace EzBob.Backend.EzbobService
{
	using System.Runtime.Serialization;
	using System.ServiceModel;
	
	[ServiceContract]
	public interface IEzbobService
	{
		[OperationContract]
		void Greeting(string customerEmail, string confirmEmailAddress, int custumerId);

		/* Examples
		[OperationContract]
		string GetData(int value);

		[OperationContract]
		CompositeType GetDataUsingDataContract(CompositeType composite);
		*/
	}


	/* Example of custom return type
	[DataContract]
	public class CompositeType
	{
		bool boolValue = true;
		string stringValue = "Hello ";

		[DataMember]
		public bool BoolValue
		{
			get { return boolValue; }
			set { boolValue = value; }
		}

		[DataMember]
		public string StringValue
		{
			get { return stringValue; }
			set { stringValue = value; }
		}
	}*/
}

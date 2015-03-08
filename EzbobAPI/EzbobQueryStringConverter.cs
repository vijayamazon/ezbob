namespace EzbobAPI {
	using System;
	using System.ServiceModel.Description;
	using System.ServiceModel.Dispatcher;

	public class EzbobQueryStringConverter : QueryStringConverter {
		public override bool CanConvert(Type type) {
			return (type == typeof(Int32)) || base.CanConvert(type);
		}

		public override object ConvertStringToValue(string parameter, Type parameterType) {
			string[] parts = parameter.Split(',');

			if (parameterType == typeof(Int32)) {
				//	string[] parts = parameter.Split(',');
				//	return new InputData { FirstName = parts[0], LastName = parts[1] };
				try {
					return Convert.ToInt32(parts[0]);
				} catch (OverflowException overflowException) {
					Console.WriteLine(overflowException.Message);
				}
			} else if (parameterType == typeof(int)) {
				//	return new InputData { FirstName = parts[0], LastName = parts[1] };
				try {
					int x;
					return Int32.TryParse(parts[0], out x);
				} catch (OverflowException overflowException) {
					Console.WriteLine("====>" + overflowException.Message);
				}
			} //else {
			return base.ConvertStringToValue(parameter, parameterType);
			//}
		}
	}

	public class EzbobWebHttpBehavior : WebHttpBehavior {
		protected override QueryStringConverter GetQueryStringConverter(OperationDescription operationDescription) {
			//Console.WriteLine(operationDescription);
			return new EzbobQueryStringConverter();
		}
	}
}

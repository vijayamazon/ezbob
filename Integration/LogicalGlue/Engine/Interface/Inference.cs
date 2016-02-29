namespace Ezbob.Integration.LogicalGlue.Engine.Interface {
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.Runtime.Serialization;
	using Ezbob.Utils.Lingvo;

	[DataContract]
	public class Inference {
		[DataMember]
		public long ResponseID { get; set; }

		[DataMember]
		public DateTime ReceivedTime { get; set; }

		[DataMember]
		public Bucket Bucket { get; set; }

		[DataMember]
		public Guid UniqueID { get; set; }

		[DataMember]
		public decimal MonthlyRepayment { get; set; }

		[DataMember]
		public bool IsTryOut { get; set; }

		[DataMember]
		public string Reason { get; set; }

		[DataMember]
		public string Outcome { get; set; }

		public decimal? Score {
			get {
				return ModelOutputs.ContainsKey(ModelNames.NeuralNetwork)
					? ModelOutputs[ModelNames.NeuralNetwork].Grade.Score
					: null;
			} // get
		} // Score

		[DataMember]
		public SortedDictionary<ModelNames, ModelOutput> ModelOutputs {
			get {
				if (this.modelOutputs == null)
					this.modelOutputs = new SortedDictionary<ModelNames, ModelOutput>();

				return this.modelOutputs;
			} // get
			set {
				this.modelOutputs = value ?? new SortedDictionary<ModelNames, ModelOutput>();
			} // set
		} // ModelOutputs

		[DataMember]
		public EtlData Etl {
			get {
				if (this.etl == null)
					this.etl = new EtlData();

				return this.etl;
			} // get

			set {
				if ((this.etl == null) || (value == null))
					this.etl = new EtlData();

				if (value != null) {
					this.etl.Code = value.Code;
					this.etl.Message = value.Message;
				} // if
			} // set
		} // Etl

		[DataMember]
		public InferenceError Error { get; set; }

		[DataMember]
		public InferenceStatus Status { get; set; }

		/// <summary>
		/// Returns a string that represents the current object.
		/// </summary>
		/// <returns>
		/// A string that represents the current object.
		/// </returns>
		public override string ToString() {
			return string.Format(
				"Response #{0} received at {1}: bucket '{2}' with {3} isTryout {4}. ",
				ResponseID,
				ReceivedTime.ToString("d/MMM/yyyy H:mm:ss", CultureInfo.InvariantCulture),
				Bucket,
				Grammar.Number(ModelOutputs.Count, "model"),
				IsTryOut
			);
		} // ToString

		private SortedDictionary<ModelNames, ModelOutput> modelOutputs;
		private EtlData etl;
	} // class Inference
} // namespace

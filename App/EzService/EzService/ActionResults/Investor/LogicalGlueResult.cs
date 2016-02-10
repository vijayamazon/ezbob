namespace EzService.ActionResults.Investor {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.Serialization;
	using Ezbob.Backend.ModelsWithDB.OpenPlatform;
	using Ezbob.Database;
	using Ezbob.Integration.LogicalGlue.Engine.Interface;
	using Ezbob.Logger;
	using Ezbob.Utils.ParsedValue;

	[DataContract]
	public class LogicalGlueResult : EzService.ActionResult {
		[DataMember]
		public DateTime Date { get; set; }

		[DataMember]
		public Bucket? Bucket { get; set; }

		[DataMember]
		public string BucketStr { get; set; }

		[DataMember]
		public string Error { get; set; }

		[DataMember]
		public decimal? NNScore { get; set; }

		[DataMember]
		public decimal? FLScore { get; set; }

		[DataMember]
		public decimal MonthlyRepayment { get; set; }

		[DataMember]
		public decimal BucketPercent { get; set; }

		[DataMember]
		public Guid UniqueID { get; set; }

		[DataMember]
		public bool IsTryout { get; set; }

		[DataMember]
		public bool IsHardReject { get; set; }

		[DataMember]
		public bool ScoreIsReliable { get; set; }

		public static LogicalGlueResult FromInference(Inference inference, int customerID, ASafeLog log, AConnection db) {
			if (inference == null) {
				return new LogicalGlueResult {
					Error = "No Logical Glue data found.",
				};
			} // if

			decimal? minScore = 0;
			decimal? maxScore = 0;
			inference = inference ?? new Inference {
				Error = new InferenceError(),
				ModelOutputs = new SortedDictionary<ModelNames, ModelOutput>()
			};

			try {
                if (inference != null && inference.Bucket.HasValue) {
					List<I_Grade> allGrades = db.Fill<I_Grade>("SELECT * FROM I_Grade", CommandSpecies.Text);

					int gradeID = (int)inference.Bucket.Value;

					maxScore = allGrades.First(x => x.GradeID == gradeID).UpperBound;

					if (gradeID > 1)
						minScore = allGrades.First(x => x.GradeID == (gradeID - 1)).UpperBound;
				} // if
			} catch (Exception ex) {
				log.Error(ex, "Failed to retrieve min max grade scores for bucket {0}", inference.Bucket);
			} // try
			
			try {
				var result = new LogicalGlueResult {
					Error = inference.Error.Message,
					Date = inference.ReceivedTime,
					Bucket = inference.Bucket,
					BucketStr = inference.Bucket.HasValue ? inference.Bucket.ToString() : string.Empty,
					MonthlyRepayment = inference.MonthlyRepayment,
					UniqueID = inference.UniqueID,
					FLScore = inference.ModelOutputs.ContainsKey(ModelNames.FuzzyLogic)
						? inference.ModelOutputs[ModelNames.FuzzyLogic].Grade.Score
						: null,
					NNScore = inference.Score,
					IsTryout = inference.IsTryOut,
				};

				var b = (maxScore - minScore) ?? 0;
				var a = (result.NNScore - minScore) ?? 0;

				result.BucketPercent = b == 0 ? 0 : a / b;

				result.IsHardReject = (inference.Etl != null) && (inference.Etl.Code == EtlCode.HardReject);
				result.ScoreIsReliable = inference.ModelOutputs.ContainsKey(ModelNames.NeuralNetwork) &&
					inference.ModelOutputs[ModelNames.NeuralNetwork].Error.IsEmpty;
				return result;
			} catch (Exception ex) {
				log.Warn(ex, "Failed loading lg data for customer {0}", customerID);
				return new LogicalGlueResult {
					Error = "Failed loading logical glue data",
				};
			} // try
		} // FromInference
	} // class LogicalGlueResult
} // namespace

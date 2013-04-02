using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using EZBob.DatabaseLib.Model.Database;
using System.Globalization;

namespace EzBob.Web.Areas.Underwriter.Models
{
    public class Score
    {
        public string Medal { get; set; }
        public double Points { get; set; }
        public double Result { get; set; }
    }

    public class MedalCharacteristic
    {
        public string CustomerCharacteristic { get; set; }
        public decimal WeightUsed { get; set; }
        public decimal ACParameters { get; set; }
        public decimal MaxPoss { get; set; }
        public decimal PointsObtainedPercent { get { return GetPointsObtainedPercent(); } }

        private decimal GetPointsObtainedPercent()
        {
            return MaxPoss == 0 ? 0 : Math.Round((ACParameters/MaxPoss*100),1);
        }
    }

    public class MedalCalculators
    {
        public Score Score =new Score();
        public IList<MedalCharacteristic> MedalCharacteristics = new List<MedalCharacteristic>();
        public decimal TotalWeightUsed { get; set; }
        public decimal TotalACParameters { get; set; }
        public decimal TotalMaxPoss { get; set; }
        public decimal TotalPointsObtainedPercent { get; set; }

        public MedalCalculators(EZBob.DatabaseLib.Model.Database.Customer customer)
        {
            if (customer.ScoringResults.Count == 0) return ;
            var maxdate = customer.ScoringResults.ToList().Max(s => s.ScoreDate);
            var scoringResult = customer.ScoringResults.ToList().SingleOrDefault(s => s.ScoreDate == maxdate);
            BuildScore(scoringResult);
            BuildCharecteristic(scoringResult);  
            GetTotal();
        }
        
        private void BuildScore(ScoringResult scoringResult)
        {
            Score = new Score
            {
                Medal = scoringResult.Medal,
                Points = scoringResult.ScorePoints,
                Result = scoringResult.ScoreResult
            };
        }

        private void BuildCharecteristic(ScoringResult scoringResult)
        {
            string[] splitACDescription = scoringResult.ACDescription.Split(';');
            string[] splitWeight = scoringResult.Weights.Split(';');
            string[] splitACParameters = scoringResult.ACParameters.Split(';');
            string[] splitMaxPoss = scoringResult.MAXPossiblePoints.Split(';');

            string pattern = @"([0-9]*\.[0-9]+|[0-9]+)";
            var regex = new Regex(pattern);

            
            for (var i = 0; i <= splitACDescription.Length - 1; i++)
            {
                MedalCharacteristics.Add(new MedalCharacteristic
                {
                    CustomerCharacteristic = splitACDescription[i],
                    WeightUsed =  Convert.ToDecimal(splitWeight[i], CultureInfo.InvariantCulture),
                    ACParameters = Convert.ToDecimal(regex.Match(splitACParameters[i]).ToString(), CultureInfo.InvariantCulture),
                    MaxPoss =  Convert.ToDecimal(splitMaxPoss[i], CultureInfo.InvariantCulture)
                });
            }
        }

        private void GetTotal ()
        {
            TotalACParameters = MedalCharacteristics.Sum(m => m.ACParameters);
            TotalMaxPoss = MedalCharacteristics.Sum(m => m.MaxPoss);
            TotalWeightUsed = MedalCharacteristics.Sum(m=>m.WeightUsed);
            TotalPointsObtainedPercent = TotalMaxPoss == 0 ? 0 : Math.Round((TotalACParameters / TotalMaxPoss * 100), 1);       
        }
    }
}
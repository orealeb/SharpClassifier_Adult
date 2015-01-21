using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier
{
    public class Classification<TKey>
    {
        public Classification()
        {
            Probabilities = new Dictionary<TKey, Probability<TKey>>();
        }

        public Dictionary<TKey, Probability<TKey>> Probabilities { get; set; }

        public Probability<TKey> MostProbableClass
        {
            get
            {
                return
                    Probabilities
                        .Values
                        .OrderByDescending(prob => prob.Value)
                        .First();
            }
        }

        public void AddProbability(Probability<TKey> probability)
        {
            Probabilities.Add(probability.Key, probability);
        }

        public Probability<TKey> GetProbability(TKey key)
        {
            return Probabilities[key];
        }

        public void Normalize(double min, double max)
        {
            double probabilitySum = Probabilities.Values.Sum(prob => prob.Value);
            if (probabilitySum > 0)
            {
                Probabilities.Values.ToList().ForEach(prob => Math.Max(min, Math.Min(max, prob.Value /= probabilitySum)));
            }         
        }
    }
}

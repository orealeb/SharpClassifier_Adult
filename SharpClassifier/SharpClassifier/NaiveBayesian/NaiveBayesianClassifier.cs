using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier.NaiveBayesian
{
    // http://cul.codeplex.com/SourceControl/changeset/view/c8a093ae4eb0#Utilities%2fClassifier%2fNaiveBayes%2fNaiveBayes.cs
    public class NaiveBayesianClassifier<TKey, TToken> : BaseClassifier<TKey, TToken>
    {
        public NaiveBayesianClassifier() : base()
        {
            MinTokenProbability = 0;
            MaxTokenProbability = 1;
            MinTokenCount = 0;
            ClassificationTokenLimit = Int32.MaxValue;
        }

        public double MinTokenProbability { get; set; }
        public double MaxTokenProbability { get; set; }
        public int MinTokenCount { get; set; }
        public int ClassificationTokenLimit { get; set; }     

        public Classification<TKey> ClassifyToken(TToken token)
        {
            Classification<TKey> classification = new Classification<TKey>();
            foreach (var tokenClass in Classes.Values)
            {
                int count = tokenClass.CountOfToken(token);
                double probability = (double)count / tokenClass.TrainingSetCount;
                probability = Math.Max(MinTokenProbability, Math.Min(MaxTokenProbability, probability));
                classification.AddProbability(new Probability<TKey> { Key = tokenClass.Key, Value = probability, Count = count });
            }

            classification.Normalize(MinTokenProbability, MaxTokenProbability);

            return
                classification;
        }

        public void UpdateClassWeightFromTestsCount()
        {
            int totalTrainingSets = Classes.Values.Sum(cls => cls.TrainingSetCount);
            foreach (Class<TKey, TToken> tokenClass in Classes.Values)
            {
                tokenClass.ClassWeight = (double)tokenClass.TrainingSetCount / totalTrainingSets;
            }
        }

        public override Classification<TKey> ClassifyTokens(IEnumerable<TToken> tokens)
        {
            Classification<TKey> classification = new Classification<TKey>();

            tokens = tokens.Distinct();

            IEnumerable<Classification<TKey>> classifications = GetClassifications(tokens);

            foreach (var tokenClass in Classes.Values)
            {
                double initialPosteriori = tokenClass.ClassWeight;

                Probability<TKey> prob =
                    new Probability<TKey>
                    {
                        Key = tokenClass.Key,
                        Value = initialPosteriori
                    };

                prob.Value *= GetTopClasssificationsProbability(tokenClass, classifications, this);

                classification.AddProbability(prob);
            }

            return
                classification;
        }

        public double GetTopClasssificationsProbability(Class<TKey, TToken> tokenClass, IEnumerable<Classification<TKey>> classifications, NaiveBayesianClassifier<TKey, TToken> classifier)
        {
            double value = 1;
            List<Classification<TKey>> limitedClassifications;

            // Filter out unusual words (if wanted)
            classifications = classifications.Where(c => c.Probabilities.Sum(prob => prob.Value.Count) >= classifier.MinTokenCount);

            if (classifier.ClassificationTokenLimit < classifications.Count())
            {
                var sortedClassifications = classifications.OrderByDescending(prob => Math.Abs(prob.Probabilities[tokenClass.Key].Value - 0.5));
                limitedClassifications = sortedClassifications.Take(classifier.ClassificationTokenLimit).ToList();
            }
            else
            {
                limitedClassifications = classifications.ToList();
            }

            limitedClassifications
                .ForEach(c => value *= c.Probabilities[tokenClass.Key].Value);

            return value;
        }

        public IEnumerable<Classification<TKey>> GetClassifications(IEnumerable<TToken> tokens)
        {
            return tokens.Select(ClassifyToken);
        }     
    }
}

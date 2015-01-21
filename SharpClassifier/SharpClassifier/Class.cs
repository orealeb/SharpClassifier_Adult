using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier
{
    public class Class<TKey, TToken>
    {
        public Class(TKey key)
        {
            Key = key;
            TokenCounts = new Dictionary<TToken, int>();
            ClassWeight = 1;
        }

        public TKey Key { get; set; }
        public int TrainingSetCount { get; private set; }
        public double ClassWeight { get; set; }
        public Dictionary<TToken, int> TokenCounts { get; set; }

        public void AddClassTrainingSet(IEnumerable<TToken> tokens)
        {
            TrainingSetCount++;

            tokens = tokens.Distinct();
            foreach (TToken token in tokens)
            {
                IncrementToken(token);
            }
        }

        public void IncrementToken(TToken token)
        {
            lock (TokenCounts)
            {
                if (TokenCounts.ContainsKey(token))
                {
                    TokenCounts[token]++;
                }
                else
                {
                    TokenCounts.Add(token, 1);
                }
            }
        }

        public int CountOfToken(TToken token)
        {
            return TokenCounts.ContainsKey(token) ? TokenCounts[token] : 0;
        }

        public double ProbabilityOfToken(TToken token)
        {
            return CountOfToken(token) / TrainingSetCount;
        }

        public override string ToString()
        {
            return Key.ToString();
        }       
    }
}

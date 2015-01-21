using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier
{    
    public abstract class BaseClassifier<TKey, TToken> : IClassifier<TKey, TToken>
    {
        public BaseClassifier()
        {
            Classes = new Dictionary<TKey, Class<TKey, TToken>>();
        }

        public Dictionary<TKey, Class<TKey, TToken>> Classes { get; set; }

        public Class<TKey, TToken> GetOrCreateTokenClass(TKey key)
        {
            if (Classes.ContainsKey(key) == false)
            {
                Classes.Add(key, new Class<TKey, TToken>(key));
            }

            return Classes[key];
        }

        public Class<TKey, TToken> AddTokenClass(TKey key, IEnumerable<IEnumerable<TToken>> trainingSets = null)
        {
            Class<TKey, TToken> tokenSet = new Class<TKey, TToken>(key);

            if (trainingSets != null)
            {
                foreach (IEnumerable<TToken> trainingSet in trainingSets)
                {
                    tokenSet.AddClassTrainingSet(trainingSet);
                }
            }

            Classes.Add(key, tokenSet);
            return tokenSet;
        }

        public abstract Classification<TKey> ClassifyTokens(IEnumerable<TToken> tokens);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier
{
    public interface IClassifier<TKey, TToken>
    {
        Dictionary<TKey, Class<TKey, TToken>> Classes { get; set; }
        Classification<TKey> ClassifyTokens(IEnumerable<TToken> tokens);
        Class<TKey, TToken> GetOrCreateTokenClass(TKey key);
        Class<TKey, TToken> AddTokenClass(TKey key, IEnumerable<IEnumerable<TToken>> trainingSets = null);
    }
}

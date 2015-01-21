using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpClassifier
{
    public class Probability<TKey>
    {
        public TKey Key { get; set; }
        public double Value { get; set; }
        public int Count { get; set; }

        public override string ToString()
        {
            return string.Format("{0}, {1}", Key, Value);
        }
    }
}

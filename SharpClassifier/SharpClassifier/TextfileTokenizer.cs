using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;

namespace SharpClassifier
{
    public static class TextfileTokenizer
    {
        private static Regex _tokenizer = new Regex(@"[\w]*", RegexOptions.Singleline);

        public static IEnumerable<string> Tokenize(string filename, bool includeMultiples = false)
        {
            string file = File.ReadAllText(filename);
            Match matchResult = _tokenizer.Match(file);
            Dictionary<string, int> counter = new Dictionary<string, int>();

            while (matchResult.Success)
            {
                if (matchResult.Groups[0].Success)
                {
                    string token = matchResult.Groups[0].Value.Trim();

                    if (string.IsNullOrWhiteSpace(token) == false)
                    {
                        if (counter.ContainsKey(token) == false)
                        {
                            counter.Add(token, 1);
                            yield return token;
                        }
                        else
                        {
                            counter[token]++;
                        }
                    }
                }

                matchResult = matchResult.NextMatch();
            }

            // If a token is present multiple times, a special version is returned to indicate that it's very common.
            if (includeMultiples)
            {
                foreach (KeyValuePair<string, int> keyValuePair in counter)
                {
                    if (keyValuePair.Value > 5)
                    {
                        yield return keyValuePair.Key + "*";
                    }
                    
                    if (keyValuePair.Value > 10)
                    {
                        yield return keyValuePair.Key + "**";
                    }
                }
            }
        }
    }
}

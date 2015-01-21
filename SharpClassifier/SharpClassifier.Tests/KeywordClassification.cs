using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using SharpClassifier.NaiveBayesian;

namespace SharpClassifier.Tests
{
    [TestClass]
    public class KeywordClassification
    {
        private static List<string[]> _keywordFile;
        public const string TestDataPath = @"..\..\..\Data\";

        [TestMethod]
        public void Can_create_classifier()
        {
            Create_classifier("Species");
        }

        [TestMethod]
        public void Can_classify_species()
        {
            TestClassification("Species");
        }

        [TestMethod]
        public void Can_classify_type()
        {
            TestClassification("Type");
        }

        [TestMethod]
        public void Can_classify_syfte()
        {
            TestClassification("Syfte");
        }

        [TestMethod]
        public void Can_classify_bolag()
        {
            TestClassification("Bolag");
        }

        [TestMethod]
        public void Can_classify_typ()
        {
            TestClassification("Typ");
        }

        private void TestClassification(string categoryGroupName)
        {
            IClassifier<string, string> classifier = Create_classifier(categoryGroupName);
            List<string[]> _rows = GetRows(categoryGroupName);
            int hits = 0;
            int misses = 0;
            Debug.WriteLine("Classifying " + categoryGroupName);
            foreach (string[] row in _rows)
            {
                string expectedTokenClassName = row[1];
                string[] keywords = row[2].Split(' ');
                string tokenClassName = classifier.ClassifyTokens(keywords).MostProbableClass.Key;

                if (tokenClassName == expectedTokenClassName)
                {
                    hits++;
                }
                else
                {
                    misses++;
                    Debug.WriteLine("Miss: \"{0}\", src={1}, gen={2}", row[2], expectedTokenClassName, tokenClassName);
                }
            }

            Debug.WriteLine("DONE hits={0}, misses={1}, hitrate={2:0.00%}\n", hits, misses, (double)hits / (hits + misses));
        }

        // Species              Dog                     dog jackets...
        private NaiveBayesianClassifier<string, string> Create_classifier(string categoryGroupName, int trainLimit = Int32.MaxValue)
        {
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            List<string[]> _rows = GetRows(categoryGroupName);

            _rows.ForEach(row =>
                {
                    string tokenClassName = row[1];
                    Class<string, string> tokenClass = classifier.GetOrCreateTokenClass(tokenClassName);
                    string[] keywords = row[2].Split(' ');
                    tokenClass.AddClassTrainingSet(keywords);
                });

            classifier.UpdateClassWeightFromTestsCount();
            return classifier;
        }

        private List<string[]> GetRows(string categoryGroupName)
        {
            if (_keywordFile == null)
            {
                _keywordFile =
                    File
                        .ReadAllLines(TestDataPath + "Keywords.txt", Encoding.Default)
                        .Where(s => string.IsNullOrWhiteSpace(s) == false)
                        .Skip(1)
                        .Select(s => Regex.Replace(s, @"\s{2,}", "\t", RegexOptions.Singleline))
                        .Select(s => s.Split('\t'))
                        .ToList();
            }

            return
                _keywordFile
                    .Where(row => row[0] == categoryGroupName)
                    .ToList();
        }
    }
}

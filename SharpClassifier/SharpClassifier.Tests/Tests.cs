using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpClassifier.NaiveBayesian;

namespace SharpClassifier.Tests
{
    [TestClass]
    public class Tests
    {
        private int _hits = 0;
        private int _misses = 0;
        public const string TestDataPath = @"..\..\..\Data\";
        public const int TakeLimit = 50000;

        [TestMethod]
        public void Can_create_NaiveBayesianClassifier()
        {
            new NaiveBayesianClassifier<string, string>();
        }

        [TestMethod]
        public void Can_populate_NaiveBayesianClassifier()
        {
            IClassifier<string, string> classifier = Create_simple_classifier();

            Assert.AreEqual(classifier.Classes["a"].TokenCounts["a"], 1);
            Assert.AreEqual(classifier.Classes["a"].TokenCounts["b"], 2);

            Assert.AreEqual(classifier.Classes["b"].TokenCounts["a"], 1);
            Assert.AreEqual(classifier.Classes["b"].TokenCounts["f"], 1);
        }

        [TestMethod]
        public void Can_compute_probability()
        {
            NaiveBayesianClassifier<string, string> classifier = Create_simple_classifier();
            var classification = classifier.ClassifyToken("a");
            Assert.AreEqual(0.5, classification.Probabilities["a"].Value);
            Assert.AreEqual(0.5, classification.Probabilities["b"].Value);

            classification = classifier.ClassifyToken("b");
            //Assert.AreEqual(classification[0].Value, 1.0);
            //Assert.AreEqual(classification[1].Value, 0.0);
        }

        [TestMethod]
        public void Can_create_play_tennis_classifier()
        {
            Create_play_tennis_classifier();
        }

        [TestMethod]
        public void Tennis_values_gets_correct_bias()
        {
            IClassifier<string, string> classifier = Create_play_tennis_classifier();
            var classification = classifier.ClassifyTokens(new List<string> { });
            Assert.AreEqual(9.0 / 14, classification.Probabilities["yes"].Value);
            Assert.AreEqual(5.0 / 14, classification.Probabilities["no"].Value);
        }

        [TestMethod]
        public void Can_classify_strong_values()
        {
            NaiveBayesianClassifier<string, string> classifier = (NaiveBayesianClassifier<string, string>)Create_play_tennis_classifier();
            //var classification = classifier.ClassifyTokens(new List<string> { "Strong" });
            var classification = classifier.ClassifyToken("Strong");
            AssertInRange(3.0 / 9, classification.Probabilities["yes"].Value);
            AssertInRange(3.0 / 5, classification.Probabilities["no"].Value);
        }

        [TestMethod]
        public void Can_classify_tokenized_email()
        {
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            var set = classifier.AddTokenClass("ham");
            set.AddClassTrainingSet(TextfileTokenizer.Tokenize(TestDataPath + @"ham\easy_ham\00001.7c53336b37003a9286aba55d2945844c"));
        }

        [TestMethod]
        public void Can_classify_full()
        {
            IClassifier<string, string> classifier = Create_play_tennis_classifier();
            var classification = classifier.ClassifyTokens(new List<string> { "Sunny", "Cool", "High", "Strong" });
            AssertInRange(0.0053, classification.Probabilities["yes"].Value);
            AssertInRange(0.0206, classification.Probabilities["no"].Value);
        }

        [TestMethod]
        public void Classify_small_span_and_ham()
        {
            // http://raza-rizvi.blogspot.com/2010/06/text-classifier-using-naive-bayes.html
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            classifier.MinTokenCount = 0;
            //classifier.ClassificationTokenLimit = 15;
            var ham = classifier.AddTokenClass("ham");
            var spam = classifier.AddTokenClass("spam");

            Debug.WriteLine("Training...");

            string trainHamPath = @"ham\easy_ham_2\";
            string trainSpamPath = @"spam\spam_2\";

            string classifyHamPath = @"ham";
            string classifySpamPath = @"spam";  

            //string hamPath = @"ham";
            //string spamPath = @"spam";

            TrainOnFiles(ham, TestDataPath + trainHamPath);
            TrainOnFiles(spam, TestDataPath + trainSpamPath);

            classifier.UpdateClassWeightFromTestsCount();

            Debug.WriteLine(
                "Training Done, {0} hams ({1} tokens), {2} spams ({3} tokens)",
                ham.TrainingSetCount,
                ham.TokenCounts.Count,
                spam.TrainingSetCount,
                spam.TokenCounts.Count);

            Debug.WriteLine("Classifying...");
            _hits = 0;
            _misses = 0;
            CategorizeFiles(classifier, ham, TestDataPath + classifyHamPath);
            CategorizeFiles(classifier, spam, TestDataPath + classifySpamPath);
            Console.WriteLine("Done, {0} hits, {1} misses, {2:0.00%} hit percent", _hits, _misses, (double)_hits / (_hits + _misses));
        }

        private void TrainOnFiles(Class<string, string> tokenClass, string path)
        {
            Parallel.ForEach(
                Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Take(TakeLimit),
                file =>
                {
                    tokenClass
                        .AddClassTrainingSet(TextfileTokenizer.Tokenize(file));
                });
        }

        private void CategorizeFiles(NaiveBayesianClassifier<string, string> classifier, Class<string, string> expected, string path)
        {
            Parallel.ForEach(
                Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).Take(TakeLimit),
                file => ClassifyFile(classifier, expected, file));
        }

        private void ClassifyFile(NaiveBayesianClassifier<string, string> classifier, Class<string, string> expected, string file)
        {
            Classification<string> classification = classifier.ClassifyTokens(TextfileTokenizer.Tokenize(file));
            if (classification.MostProbableClass.Key == expected.Key)
            {
                _hits++;
                //Debug.WriteLine("Hit: " + file);                
            }
            else
            {
                _misses++;
                Debug.WriteLine("Miss: {0} (hits={1}, misses={2}, hitrate={3:0.00%})", file, _hits, _misses, (double)_hits / (_hits + _misses));
            }
        }

        public void AssertInRange(double expected, double actual, double range = 0.1)
        {
            double delta = Math.Abs(expected - actual);
            if (delta > range)
            {
                Assert.AreEqual(expected, actual);
            }
        }

        private static NaiveBayesianClassifier<string, string> Create_simple_classifier()
        {
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            classifier.AddTokenClass("a", new List<List<string>> { new List<string> { "a", "b", "b" }, new List<string> { "b" } });
            classifier.AddTokenClass("b", new List<List<string>> { new List<string> { "a", "f", "f" }, new List<string> { "z" } });
            return classifier;
        }

        private static IClassifier<string, string> Create_play_tennis_classifier()
        {
            // http://www.google.se/url?sa=t&rct=j&q=bayesian%2Bclassifier&source=web&cd=3&ved=0CDMQFjAC&url=http%3A%2F%2Fwww.cs.bham.ac.uk%2F~axk%2FML_Bayes05.ppt&ei=2NmvTqPmH4Si4gTD6oWcAQ&usg=AFQjCNG6c4gs-RKz6ap5V18rqmXyFZFRPQ
            string data = @"
Day3	Overcast	Hot	High	Weak	Yes
Day4	Rain	Mild	High	Weak	Yes
Day5	Rain	Cool	Normal	Weak	Yes
Day1	Sunny	Hot	High	Weak	No
Day2	Sunny	Hot	High	Strong	No
Day6	Rain	Cool	Normal	Strong	No
Day7	Overcast	Cool	Normal	Strong	Yes
Day8	Sunny	Mild	High	Weak	No
Day9	Sunny	Cool	Normal	Weak	Yes
Day10	Rain	Mild	Normal	Weak	Yes
Day11	Sunny	Mild	Normal	Strong	Yes
Day12	Overcast	Mild	High	Strong	Yes
Day13	Overcast	Hot	Normal	Weak	Yes
Day14	Rain	Mild	High	Strong	No";

            List<string> yes = data.Split('\n').Select(s => s.Trim()).Where(s => s.EndsWith("Yes")).ToList();
            List<string> no = data.Split('\n').Select(s => s.Trim()).Where(s => s.EndsWith("No")).ToList();
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            classifier.AddTokenClass("yes", yes.Select(s => s.Split('\t').Select(s2 => s2.Trim()).Where(s2 => s2 != "")));
            classifier.AddTokenClass("no", no.Select(s => s.Split('\t').Select(s2 => s2.Trim()).Where(s2 => s2 != "")));
            classifier.UpdateClassWeightFromTestsCount();
            return classifier;
        }
    }
}

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpClassifier.NaiveBayesian;


namespace SharpClassifier
{
    public class RunSharpClassifier
    {
        private int _hits = 0;
        private int _misses = 0;
        public const string TestDataPath = @"..\..\..\Data\";
        public const int TakeLimit = 50000;


        public RunSharpClassifier()
        {

            Do_Classify();

        }


        public void Do_Classify()
        {
            IClassifier<string, string> classifier = Create_play_tennis_classifier();
            var classification = classifier.ClassifyTokens(new List<string> { "Sunny", "Cool", "High", "Strong" });



            // AssertInRange(0.0053, classification.Probabilities["yes"].Value);
            //  AssertInRange(0.0206, classification.Probabilities["no"].Value);
        }



        private IClassifier<string, string> Create_play_tennis_classifier()
        {

            //https://archive.ics.uci.edu/ml/machine-learning-databases/iris/iris.data
            #region trainingdata

            string trainingdata = File.ReadAllText(@"..\..\..\Data\adult.csv");

            #endregion


            List<string> less_50k = trainingdata.Split('\n').Select(s => s.Trim()).Where(s => s.EndsWith(">50K")).ToList();
            List<string> greater_equal_50k = trainingdata.Split('\n').Select(s => s.Trim()).Where(s => s.EndsWith("<=50K")).ToList();
            //List<string> Iris_virginica = trainingdata.Split('\n').Select(s => s.Trim()).Where(s => s.EndsWith("Iris-virginica")).ToList();
            NaiveBayesianClassifier<string, string> classifier = new NaiveBayesianClassifier<string, string>();
            var less_50k_var = classifier.AddTokenClass(">50K", less_50k.Select(s => s.Split(',').Select(s2 => s2.Trim()).Where(s2 => s2 != "")));
            var greater_equal_50k_var = classifier.AddTokenClass("<=50K", greater_equal_50k.Select(s => s.Split(',').Select(s2 => s2.Trim()).Where(s2 => s2 != "")));
            // var vir = classifier.AddTokenClass("Iris-virginica", Iris_virginica.Select(s => s.Split('\t').Select(s2 => s2.Trim()).Where(s2 => s2 != "")));
            classifier.UpdateClassWeightFromTestsCount();




            Console.WriteLine(
               "Training Done, {0} >50K ({1} tokens), {2} <=50K ({3} tokens))",
               less_50k_var.TrainingSetCount,
               less_50k_var.TokenCounts.Count,
               greater_equal_50k_var.TrainingSetCount,
               greater_equal_50k_var.TokenCounts.Count);

            Console.WriteLine("Classifying...");
            _hits = 0;
            _misses = 0;

            using (StreamReader reader = new StreamReader(@"..\..\..\Data\adult.test.csv"))
            {
                string line; int linenum = 0;
                while ((line = reader.ReadLine()) != null)
                {
                    // Do something with the line.
                    string[] tokens = line.Split(',');
                    var classification = classifier.ClassifyTokens(new List<string> { tokens[0], tokens[1], tokens[2], tokens[3], tokens[4], tokens[5], 
                                                                        tokens[6], tokens[7], tokens[8], tokens[9], 
                                                                    tokens[10], tokens[11], tokens[12], tokens[13]});

                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(@"..\..\..\Data\sharpclassifier_adult.csv", true))
                    {
                        file.WriteLine(classification.MostProbableClass.Key);
                    }

                    if (classification.MostProbableClass.Key.Equals(tokens[14]))
                    {
                        _hits++;
                        //Console.WriteLine("Hit: " + file);                
                    }
                    else
                    {
                        _misses++;
                        Console.WriteLine("Miss: {0} (hits={1}, misses={2}, hitrate={3:0.00%})", linenum, _hits, _misses, (double)_hits / (_hits + _misses));
                    }
                    linenum++;

                }

            }




            //  Console.WriteLine(((double)9.0 / 14).ToString() + " and classification prob: " + classification.Probabilities["yes"].Value);
            //  Console.WriteLine(((double)5.0 / 14).ToString() + " and classification prob: " + classification.Probabilities["no"].Value);

            //if (classification.MostProbableClass.Key == expected.Key)
            //{
            //    _hits++;
            //    //Debug.WriteLine("Hit: " + file);                
            //}
            //else
            //{
            //    _misses++;
            //    Debug.WriteLine("Miss: {0} (hits={1}, misses={2}, hitrate={3:0.00%})", file, _hits, _misses, (double)_hits / (_hits + _misses));
            //}


            //Console.WriteLine("Done, {0} hits, {1} misses, {2:0.00%} hit percent", _hits, _misses, (double)_hits / (_hits + _misses));






            return classifier;
        }








    }
}
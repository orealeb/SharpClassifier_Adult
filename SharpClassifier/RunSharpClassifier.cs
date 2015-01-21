using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using SharpClassifier.NaiveBayesian;

public class RunSharpClassifier
{
    public RunSharpClassifier()
	{


        Do_Classify();

	}


    public void Do_Classify()
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
            Console.WriteLine("Miss: {0} (hits={1}, misses={2}, hitrate={3:0.00%})", file, _hits, _misses, (double)_hits / (_hits + _misses));
        }
    }
}

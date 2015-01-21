# README #

SharpClassifier is developed at [http://sharpclassifier.codeplex.com/](http://sharpclassifier.codeplex.com/).

"Project Description
C "Classifier" is an AI software component that tries to classify instances from given evidence (if shiny then diamond). A famous example is classifying email spam, separating it from ham.

SharpClassifier currently only contains a single classifier - A Bayesian Naive Classifier. Most Bayesian Naive Classifiers for C# you'll find out there only handles two classes (spam/ham), but this implementation supports any number of classses."

A few configurations has been made so SharpClassifier easily works with more than just two classes. However, some values are still hard-coded, so it works only for the iris dataset. This can be easily modified: all you have to do is change the classes in the code to the classes used in the new data set.

To run:

* Make sure iris.txt is in the Data folder
* Open the solution file on Visual Studio and run
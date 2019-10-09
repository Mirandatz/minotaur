# MINOTAUR
 
**M**ulti-object**I**ve ge**N**etic alg**O**rithm for consis**T**ent and  interpret**A**ble m**U**lti-label **R**ules is a .NET Core implementation of genetic algorithm that generates a collection of rule-based classification models, each offering a different compromise between predictive power and interpretability.

The default fittest selection algorithm is [NSGA-II](http://www.dmi.unict.it/mpavone/nc-cs/materiale/NSGA-II.pdf).
The default metrics being optimized are F-Score (in the training dataset), number of rules in the model and the average "feature-space volume" covered by the model's rules.


# How to compile
 - [Download](https://dotnet.microsoft.com/download/dotnet-core/3.0) and install the .NET Core SDK (the project requires version 3.0 or greater)
 - Clone this repository
 - [Build the solution](https://docs.microsoft.com/en-us/dotnet/core/tools/dotnet-build) (e.g. on Windows, open the PowerShell inside the repository and run the command `dotnet build .\Minotaur\Minotaur.sln -c Release`
 - If everything went well, a file `minotaur.exe` was created in the directory `minotaur\Minotaur\Minotaur\bin\x64\Release\netcoreapp3.0`

# How to run
- After building the solution, MINOTAUR can be run by executing the generated `minotaur.exe` and providing it with the required command line arguments, separated by spaces.

## Command line arguments
- This readme file may be out of date; to see the current command line arguments, check [this file](https://github.com/Mirandatz/minotaur/blob/master/Minotaur/Minotaur/ProgramSettings.cs).
- Most of the command line arguments have "reasonable default values", but its recommended to tune some of the arguments to improve the performance of the algorithm (e.g. by enabling caching results) and / or to improve the generated classifiers

### Mandatory Command Line Arguments
Both the train and the test datasets must be provided to MINOTAUR.
The train dataset is used to compute the fitness of the genetic algorithm individuals (e.g. fscore).
The test dataset is used to generate the final report of the algorithm (e.g. fscore on test dataset) and
'status report messages'.
Therefore, the following command line arguments are mandatory
- \-\-train-data={path}
- \-\-train-labels={path}
- \-\-train-data={path}
- \-\-test-labels={path}
- \-\-output-directory={path}

### Input files format
- The input files must be in the 'csv' format.
- Columns must be separated by a ","
- Rows must be separated by a 'endline' character (depends on the operating system)
- All values must be finite floating point values (read the Current Limitations section)

## Running an example
- Build the solution
- Clone the pre-processed [minotaur.datasets repository](https://github.com/Mirandatz/minotaur.datasets) 
- Assuming that the current directory is the same as the generated MINOTAUR executable, and that you are using the Windows PowerShell,  the following command will run MINOTAUR with default values for most of its arguments 
`.\Minotaur.exe --train-data="C:\Source\minotaur.datasets\iris\ready-for-minotaur\fold-0\train-data.csv" --train-labels="C:\Source\minotaur.datasets\iris\ready-for-minotaur\fold-0\train-labels.csv" --test-data="C:\Source\minotaur.datasets\iris\ready-for-minotaur\fold-0\test-data.csv" --test-labels="C:\Source\minotaur.datasets\iris\ready-for-minotaur\fold-0\test-labels.csv" --output-directory="./"`
- After the algorithm has finished, it will create two files in the current directory :
  - `final-population-individuals.txt`  contains the the generated classification models, described in a human readable format. Each individual is separated by a line of "=====".
  - `final-population-fitnesses.txt` contains the generated classification models fitnesses on the test dataset. The default metrics used to compute the fitness are [fscore, rule-count, average-rule-volume]. To understand why some values are negative, read the **Current limitations** section.


# Current limitations
- The current implementation only support multi-label datasets; therefore single-label datasets (like Iris) must have the labels "one-hot-encoded", [like this](https://github.com/Mirandatz/minotaur.datasets/tree/master/iris/ready-for-minotaur/fold-0)
- The current implementation only support continuous features; if the dataset contains categorical features they won't be parsed correctly. In case they are successfully parsed, they will be treated as continuous features, which means the results will probably be nonsensical.
- The current implementation only supports the maximization of the fitness values; therefore objectives that must be minimized (e.g. model-size) are multiplied by -1. 
namespace Minotaur {
	using System;
	using System.ComponentModel.DataAnnotations;
	using McMaster.Extensions.CommandLineUtils;

	public sealed class ProgramSettings {

		/// <summary>
		/// I'm using this unelegant approach 
		/// because the CommandLineUtils API is somewhat... Limited.
		/// Either that or I couldn't find the appropriate method to call.
		/// </summary>
		public int OnExecute() {
			return Program.Run(this);
		}

		[Required]
		[FileExists]
		[Option(ShortName = "",
			LongName = "train-data",
			Description = "Path to the .csv file containg the training data."
			)]
		public string TrainDataFilename { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(ShortName = "",
			LongName = "train-labels",
			Description = "Path to the .csv file containg the training labels."
			)]
		public string TrainLabelsFilename { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(ShortName = "",
			LongName = "test-data",
			Description = "Path to the.csv file containg the test data."
			)]
		public string TestDataFilename { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(ShortName = "",
			LongName = "test-labels",
			Description = "Path to the .csv file containg the test labels."
			)]
		public string TestLabelsFilename { get; } = string.Empty;

		//[Required]
		//[FileExists]
		//[Option(ShortName = "",
		//	LongName = "feature-types",
		//	Description =
		//	"Path to the text file containg the types of the features. " +
		//	"A feature may be 'continuous' or 'categorical'. " +
		//	"The file must contain one feature type per line. " +
		//	"The file must also contain exactly N lines, " +
		//	"being N the number of columns in train-data and test-data."
		//	)]
		//public string FeatureTypesFilename { get; } = string.Empty;

		[Required]
		[Option(ShortName = "",
			LongName = "output-directory",
			Description = "Directory to write the output files."
			)]
		[DirectoryExists]
		public string OutputDirectory { get; } = string.Empty;

		[Option(ShortName = "",
			LongName = "max-generations",
			Description =
			"The maximum number of generations (iterations) to run. " +
			"Must be greater than or equal to min-generations."
			)]
		[Range(1, int.MaxValue)]
		public int MaximumGenerations { get; } = 2000;

		[Option(ShortName = "",
			LongName = "hyperrectangle-cache-size",
			Description =
			"The maximum number of entries in the cache of hyperrectangle creator. " +
			"If set to 0, effectively disables caching."
			)]
		[Range(0, int.MaxValue)]
		public int HyperRectangleCacheSize { get; } = 0;

		[Option(ShortName = "",
			LongName = "rule-coverage-cache-size",
			Description =
			"The maximum number of entries in the cache of rule coverage computer. " +
			"If set to 0, effectively disables caching."
			)]
		[Range(0, int.MaxValue)]
		public int RuleCoverageCacheSize { get; } = 0;

		[Option(ShortName = "",
			LongName = "individual-fitness-cache-size",
			Description =
			"The maximum number of entries in the cache of individual fitness . " +
			"If set to 0, effectively disables caching."
			)]
		[Range(0, int.MaxValue)]
		public int FitnessCacheSize { get; } = 0;

		[Option(ShortName = "", LongName = "population-size", Description = "The number of individuals in the initial and final populations.")]
		[Range(1, int.MaxValue)]
		public int PopulationSize { get; } = 200;

		[Option(ShortName = "", LongName = "mutants-per-generation",
			Description =
			"How many mutants should be generated each generation."
			)]
		[Range(1, int.MaxValue)]
		public int MutantsPerGeneration { get; } = 100;

		[Option(CommandOptionType.MultipleValue, ShortName = "", LongName = "fitness-metrics",
			Description =
			"The metrics to use as fitness during the training phase."
			)]
		[AllowedValues("fscore", "model-size", "average-rule-volume", "rule-count")]
		public string[] MetricNames { get; } = new string[] { "fscore", "rule-count" };

		[Option(ShortName = "", LongName = "fittest-selection", Description = "The fittest selection strategy.")]
		[AllowedValues("nsga2", "lexicographic")]
		public string SelectionAlgorithm { get; } = "nsga2";

		[Option(ShortName = "", LongName = "individual-mutation-add-rule-weight",
			Description =
			"The probability, when mutating a individual, of adding a new rule to it."
			)]
		[Range(0, int.MaxValue)]
		public int IndividualMutationAddRuleWeight { get; } = 5;

		[Option(ShortName = "", LongName = "individual-mutation-modify-rule-weight",
			Description =
			"The probability, when mutating a individual, of modifying a rule that it contains."
			)]
		[Range(0, int.MaxValue)]
		public int IndividualMutationModifyRuleWeight { get; } = 20;

		[Option(ShortName = "", LongName = "individual-mutation-remove-rule-weight",
			Description =
			"The probability, when mutating a individual, of removing a rule that it contains."
			)]
		[Range(0, int.MaxValue)]
		public int IndividualMutationRemoveRuleWeight { get; } = 10;

		[Option(ShortName = "", LongName = "max-failed-mutations-per-generation",
			Description = "When trying to mutate a individual, the mutant generated may not be consistent." +
			"This option defines how many times the mutation may fail during a single generation." +
			"If this number is reached, the evolutionary process stops."
			)]
		[Range(0, int.MaxValue)]
		public int MaximumFailedMutationAttemptsPerGeneration { get; } = 2000;

		//[Required]
		//[Option(ShortName = "", LongName = "rule-mutation-remove-test-probability",
		//	Description =
		//	"The probability, when mutating a rule, of adding a new test to it."
		//	)]
		//[Range(0f, 1f)]
		//public float RuleMutationRemoveTestProbability { get; }

		//[Required]
		//[Option(ShortName = "", LongName = "rule-mutation-modify-test-probability",
		//	Description =
		//	"The probability, when mutating a rule, of modifying a test that it contains."
		//	)]
		//[Range(0f, 1f)]
		//public float RuleMutationModifyTestProbability { get; }

		//[Required]
		//[Option(ShortName = "", LongName = "rule-mutation-modify-consequent-probability",
		//	Description =
		//	"The probability, when mutating a rule, of modifying it's consequent (the labels it predicts)."
		//	)]
		//[Range(0f, 1f)]
		//public float RuleMutationModifyConsequentProbability { get; }

		//[Required]
		//[Option(CommandOptionType.MultipleValue, ShortName = "", LongName = "test-metrics",
		//	Description =
		//	"The metrics to use as fitness during the testing phase."
		//	)]
		//[AllowedValues("fscore", "precision", "recall", "model-size", "prediction-explanation-size", "prediction-ratio")]
		//public string[] TestMetricNames { get; }

		//[DirectoryExists]
		//[Option(ShortName = "", LongName = "initial-population",
		//	Description =
		//	"Path to the directory containing number of .json files with individuals." +
		//	"The directory must contain exactly 'population-size' files."
		//	)]
		//public string InitialPopulationDirectory { get; }

		//[Option(ShortName = "", LongName = "min-generations",
		//	Description =
		//	"The minimum number of generations to run." +
		//	"Must be less than or equal to max-generations.")]
		//[Range(1, int.MaxValue)]
		//public int MinimumGenerations { get; } = 1;

		//[Option(ShortName = "", LongName = "conservative-mutation",
		//	Description =
		//	"If true, the before mutating a individual, a clone of the individual is made." +
		//	"If false, the mutation doesn't keep a copy of the original individual in the population."
		//	)]
		//public bool ConservativeMutation { get; }

		//[Required]
		//[Option(ShortName = "", LongName = "breeding-probability",
		//	Description =
		//	"This value is the ratio of the population that will be breed." +
		//	"If the population size is 100 and this value is 0.5, then 50 children will" +
		//	"be generated."
		//	)]
		//[Range(0, float.MaxValue)]
		//public float BreedingProbability { get; }

		//public int ChildrenPerGeneration {
		//	get {
		//		return (int) System.Math.Ceiling(PopulationSize * BreedingProbability);
		//	}
		//}

		//[Required]
		//[Option(ShortName = "", LongName = "max-failed-mutations-per-individual",
		//	Description =
		//	"When trying to mutate a individual, the mutant generated may not be consistent." +
		//	"This option defines how many times we should try to mutate a individual." +
		//	"If this number is reached, another individual is selected for mutation."
		//	)]
		//[Range(0, int.MaxValue)]
		//public int MaximumMutationFailedAttemptsPerIndividual { get; }
	}
}

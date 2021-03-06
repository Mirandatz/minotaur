namespace Minotaur {
	using System;
	using System.ComponentModel.DataAnnotations;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Classification;

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
		[Option(
			ShortName = "", LongName = "train-data",
			Description = "Path to the .csv file containg the training data. The .csv file must be headerless")]
		public string TrainDataPath { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(
			ShortName = "", LongName = "train-labels",
			Description = "Path to the .csv file containg the training labels. The .csv file must be headerless.")]
		public string TrainLabelsPath { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(
			ShortName = "", LongName = "test-data",
			Description = "Path to the.csv file containg the test data. The .csv file must be headerless")]
		public string TestDataPath { get; } = string.Empty;

		[Required]
		[FileExists]
		[Option(
			ShortName = "", LongName = "test-labels",
			Description = "Path to the .csv file containg the test labels. The .csv file must be headerless")]
		public string TestLabelsPath { get; } = string.Empty;

		[Required]
		[Option(
			ShortName = "", LongName = "output-dir",
			Description = "Output directory.")]
		[DirectoryExists]
		public string OutputDirectory { get; } = string.Empty;

		[Option(ShortName = "", LongName = "save-models",
			Description = "Whether the models should be saved to disk.")]
		public bool SaveModels { get; } = false;

		[Option(ShortName = "", LongName = "save-train-predictions",
			Description = "Whether the predictions for the train dataset should be saved to disk.")]
		public bool SaveTrainPredictions { get; }

		[Option(ShortName = "", LongName = "save-test-predictions",
			Description = "Whether the predictions for the test dataset should be saved to disk.")]
		public bool SaveTestPredictions { get; }

		//[Required]
		//[Option(
		//	ShortName = "", LongName = "classification-type",
		//	Description = "Describes whether the dataset is a single-label or multi-label.")]
		//[AllowedValues("singlelabel", "multilabel")]
		public ClassificationType ClassificationType { get; } = ClassificationType.MultiLabel;

		[Required]
		[Option(
			ShortName = "", LongName = "max-generations",
			Description =
			"The maximum number of generations (iterations) to run. ")]
		[Range(2, int.MaxValue)]
		public int MaximumGenerations { get; }

		[Required]
		[Option(
			ShortName = "", LongName = "population-size",
			Description = "The number of individuals in the initial and final populations.")]
		[Range(5, int.MaxValue)]
		public int PopulationSize { get; }

		[Required]
		[Option(
			ShortName = "", LongName = "mutants-per-generation",
			Description = "How many mutants should be generated each generation.")]
		[Range(1, int.MaxValue)]
		public int MutantsPerGeneration { get; }

		[Required]
		[Option(CommandOptionType.MultipleValue,
			ShortName = "", LongName = "fitness-metrics",
			Description = "The metrics to use as fitness during the training phase.")]
		[AllowedValues("fscore", "rule-count")]
		public string[] MetricNames { get; } = null!;

		[Required]
		[Option(
			ShortName = "", LongName = "fittest-selection",
			Description = "The fittest selection strategy.")]
		[AllowedValues("nsga2", "lexicographic")]
		public string SelectionAlgorithm { get; } = null!;

		[Required]
		[Option(
			ShortName = "", LongName = "individual-mutation-add-rule-weight",
			Description = "The relative weight, when mutating a individual, of adding a new rule to it.")]
		[Range(0, int.MaxValue)]
		public int IndividualMutationAddRuleWeight { get; } // = 5;

		[Required]
		[Option(
			ShortName = "", LongName = "individual-mutation-modify-rule-weight",
			Description = "The relative weight, when mutating a individual, of modifying a rule that it contains.")]
		[Range(0, int.MaxValue)]
		public int IndividualMutationModifyRuleWeight { get; } // = 20;

		[Required]
		[Option(
			ShortName = "", LongName = "individual-mutation-remove-rule-weight",
			Description = "The relative weight, when mutating a individual, of removing a rule that it contains.")]
		[Range(0, int.MaxValue)]
		public int IndividualMutationRemoveRuleWeight { get; } // = 10;

		[Required]
		[Option(
			ShortName = "", LongName = "max-failed-mutations-per-generation",
			Description = "When trying to mutate a individual, the mutant generated may not be consistent." +
			"This option defines how many times the mutation may fail during a single generation." +
			"If this number is reached, the evolutionary process stops.")]
		[Range(0, int.MaxValue)]
		public int MaximumFailedMutationAttemptsPerGeneration { get; } // = 2000;

		[Required]
		[Option(ShortName = "", LongName = "minotaur-hyperparameter-t",
			Description = "After a consistent hyperrectangle is found using the " +
			"Constrained Feature-Space Box-Enlargement algorithm, " +
			"one must create a rule inside such hyperrectangle. " +
			"This paramater is used to specifiy how many instances such rule should " +
			"try to cover.")]
		[Range(1, int.MaxValue)]
		public int TargetNumberOfInstancesToCoverDuringRuleCreationg { get; }

		//[Option(ShortName = "", LongName = "rule-consequent-threshold",
		//	Description = "The consequent of a rule (i.e. the labels it predicts) are " +
		//	"generated by averaging the labels of the dataset instances it covers and checking" +
		//	"if they are above a certain threshold, defined by this parameter.")]
		//[Range(0f, 1f)]
		public float RuleConsequentThreshold { get; } = 0.5f;

		[Option(ShortName = "", LongName = "skip-expensive-sanity-checks",
			Description = "Whether sanity checks should be performed or not. " +
			"This is a debug feature; enabling it may cause degrade the performance.")]
		public bool SkipExpensiveSanityChecks { get; }
	}
}

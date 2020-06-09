namespace Minotaur {
	using System;
	using System.Collections.Generic;
	using System.IO;
	using System.Text.Json;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.EvolutionaryAlgorithms;
	using Minotaur.EvolutionaryAlgorithms.Metrics;
	using Minotaur.EvolutionaryAlgorithms.Population;
	using Minotaur.EvolutionaryAlgorithms.Selection;
	using Minotaur.Math.Dimensions;
	using Minotaur.Output;
	using Minotaur.Random;
	using Minotaur.Theseus;
	using Minotaur.Theseus.Evolution;
	using Minotaur.Theseus.IndividualCreation;
	using Minotaur.Theseus.Mutation;
	using Minotaur.Theseus.RuleCreation;

	public static class Program {

		public static int Main(string[] args) {
			if (args.Length == 1 && args[0] == "--lazy-dev-switch")
				args = LazyDevArguments();

			return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static string[] LazyDevArguments() {
			if (Directory.Exists(path: "C:/Source/minotaur/temp"))
				Directory.Delete(path: "C:/Source/minotaur/temp", recursive: true);

			Directory.CreateDirectory(path: "C:/Source/minotaur/temp");
			return new string[] {
				"--train-data=C:/Source/default-rule/datasets/synthetic0/folds/1/train-data.csv",
				"--train-labels=C:/Source/default-rule/datasets/synthetic0/folds/1/train-labels.csv",
				"--test-data=C:/Source/default-rule/datasets/synthetic0/folds/1/test-data.csv",
				"--test-labels=C:/Source/default-rule/datasets/synthetic0/folds/1/test-labels.csv",

				"--output-dir=C:/Source/minotaur/temp",

				"--fitness-metrics=fscore",
				"--fitness-metrics=rule-count",
				"--fittest-selection=nsga2",

				"--save-models",

				"--individual-mutation-add-rule-weight=5",
				"--individual-mutation-modify-rule-weight=20",
				"--individual-mutation-remove-rule-weight=10",

				"--population-size=30",
				"--mutants-per-generation=10",
				"--max-failed-mutations-per-generation=2000",


				"--max-generations=200",

				"--minotaur-hyperparameter-t=100",

				"--skip-expensive-sanity-checks"
			};
		}

		public static int Run(ProgramSettings settings) {
			ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
			
			PrintSettings(settings);

			(var trainDataset, var testDataset) = DatasetLoader.LoadDatasets(
				trainDataFilename: settings.TrainDataPath,
				trainLabelsFilename: settings.TrainLabelsPath,
				testDataFilename: settings.TestDataPath,
				testLabelsFilename: settings.TestLabelsPath,
				classificationType: settings.ClassificationType);

			Console.WriteLine();
			PrintTrainDatasetInformation(trainDataset);
			Console.WriteLine();

			var hyperRectangleCoverageComputer = new HyperRectangleCoverageComputer(dataset: trainDataset);

			var featureTestIntervalConveter = new FeatureTestDimensionIntervalConverter(trainDataset);

			var ruleAntecedentHyperRectangleConverter = new RuleAntecedentHyperRectangleConverter(featureTestIntervalConveter);

			var seedSelector = new CFSBESeedFinder(
				ruleConverter: ruleAntecedentHyperRectangleConverter,
				coverageComputer: hyperRectangleCoverageComputer);

			var antecedentCreator = new AntecedentCreator(ruleAntecedentHyperRectangleConverter: ruleAntecedentHyperRectangleConverter);

			var consequentCreator = settings.ClassificationType switch
			{
				ClassificationType.SingleLabel => (IConsequentCreator) new SingleLabelConsequentCreator(dataset: trainDataset),
				ClassificationType.MultiLabel => (IConsequentCreator) new MultiLabelConsequentCreator(dataset: trainDataset, threshold: settings.RuleConsequentThreshold),
				_ => throw CommonExceptions.UnknownClassificationType,
			};

			var hyperRectangleIntersector = new HyperRectangleIntersector(trainDataset);
			var nonIntersectingHyperRectangleCreator = new NonIntersectingRectangleCreator(hyperRectangleIntersector);

			var ruleCreator = new CoverageAwareRuleCreator(
				seedSelector: seedSelector,
				boxConverter: ruleAntecedentHyperRectangleConverter,
				boxCreator: nonIntersectingHyperRectangleCreator,
				coverageComputer: hyperRectangleCoverageComputer,
				antecedentCreator: antecedentCreator,
				consequentCreator: consequentCreator,
				hyperRectangleIntersector: hyperRectangleIntersector,
				targetNumberOfInstancesToCover: settings.TargetNumberOfInstancesToCoverDuringRuleCreationg,
				runExpensiveSanityChecks: settings.SkipExpensiveSanityChecks);

			var individualMutationChooser = BiasedOptionChooser<IndividualMutationType>.Create(
				new Dictionary<IndividualMutationType, int>() {
					[IndividualMutationType.AddRule] = settings.IndividualMutationAddRuleWeight,
					[IndividualMutationType.ModifyRule] = settings.IndividualMutationModifyRuleWeight,
					[IndividualMutationType.RemoveRule] = settings.IndividualMutationRemoveRuleWeight
				});

			var ruleSwappingindividualMutator = new RuleSwappingIndividualMutator(
				mutationChooser: individualMutationChooser,
				ruleCreator: ruleCreator);

			var populationMutator = new PopulationMutator(
				individualMutator: ruleSwappingindividualMutator,
				mutantsPerGeneration: settings.MutantsPerGeneration,
				maximumFailedAttemptsPerGeneration: settings.MaximumFailedMutationAttemptsPerGeneration);

			var trainMetrics = IMetricParser.ParseMetrics(
				dataset: trainDataset,
				metricsNames: settings.MetricNames,
				classificationType: settings.ClassificationType);

			var trainFitnessEvaluator = new FitnessEvaluator(trainMetrics);

			var testMetrics = IMetricParser.ParseMetrics(
				dataset: testDataset,
				metricsNames: settings.MetricNames,
				classificationType: settings.ClassificationType);

			var testFitnessEvaluator = new FitnessEvaluator(testMetrics);

			var fittestIdentifier = IFittestIdentifierParser.Parse(
				name: settings.SelectionAlgorithm,
				fittestCount: settings.PopulationSize);

			var individualCreator = new SingleRuleIndividualCreator(ruleCreator: ruleCreator);

			var initialPopulation = CreateInitialPopulation(
				individualCreator: individualCreator,
				settings: settings);

			var modelSerializer = new ModelSerializer();

			var populationFitnessSerializer = new PopulationFitnessSerializer(
				trainFitnessEvaluator: trainFitnessEvaluator,
				testFitnessEvaluator: testFitnessEvaluator);

			var trainPredictionsSerializer = new PredictionsSerializer(dataset: trainDataset);
			var testPredictionsSerializer = new PredictionsSerializer(dataset: testDataset);

			var persistentOutputManager = new PersistentOutputManager(
				outputDirectory: settings.OutputDirectory,
				saveModels: settings.SaveModels,
				saveTrainPredictions: settings.SaveTrainPredictions,
				saveTestPredictions: settings.SaveTestPredictions,
				modelSerializer: modelSerializer,
				populationFitnessSerializer: populationFitnessSerializer,
				trainPredictionsSerializer: trainPredictionsSerializer,
				testPredictionsSerializer: testPredictionsSerializer);

			//var consistencyChecker = new RuleConsistencyChecker(
			//	ruleAntecedentHyperRectangleConverterconverter: ruleAntecedentHyperRectangleConverter,
			//	hyperRectangleIntersector: hyperRectangleIntersector);

			var evolutionEngine = new EvolutionEngine(
				maximumNumberOfGenerations: settings.MaximumGenerations,
				fitnessEvaluator: trainFitnessEvaluator,
				populationMutator: populationMutator,
				fittestIdentifier: fittestIdentifier);

			var lastGenerationSummary = evolutionEngine.Run(initialPopulation);
			if (lastGenerationSummary.GenerationNumber == settings.MaximumGenerations)
				Console.WriteLine($"Evolution cycle stopped. Reason: maximum number of generations reached.");
			else
				Console.WriteLine($"Evolution cycle stopped. Reason: maximum number of generations reached.");

			persistentOutputManager.SaveWhatMustBeSaved(population: lastGenerationSummary.Population);

			Console.WriteLine("Done.");

			return 0;
		}

		private static void PrintSettings(ProgramSettings settings) {
			Console.WriteLine("Running MINOTAUR with settings:");

			var serializationOptions = new JsonSerializerOptions() {
				WriteIndented = true
			};

			var serialized = JsonSerializer.Serialize(settings, serializationOptions);
			Console.WriteLine(serialized);
			Console.WriteLine();
		}

		private static void PrintTrainDatasetInformation(Dataset trainDataset) {
			Console.WriteLine();
			Console.WriteLine($"Train dataset information");
			Console.WriteLine($"Train dataset instance count {trainDataset.InstanceCount}");
			Console.WriteLine($"Train dataset feature count {trainDataset.FeatureCount}");
			Console.WriteLine($"Train dataset class count {trainDataset.ClassCount}");
		}

		private static Individual[] CreateInitialPopulation(IIndividualCreator individualCreator, ProgramSettings settings) {
			Console.Write("Creating initial population...");

			var population = new Individual[settings.PopulationSize];

			Parallel.For(0, population.Length, i => {
				population[i] = individualCreator.CreateFirstGenerationIndividual();
			});

			Console.WriteLine(" Done.");

			return population;
		}
	}
}

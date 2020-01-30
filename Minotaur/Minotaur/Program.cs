namespace Minotaur {
	using System;
	using System.Collections.Generic;
	using System.Text.Json;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Classification;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Metrics;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Math.Dimensions;
	using Minotaur.Output;
	using Minotaur.Profiling;
	using Minotaur.Random;
	using Minotaur.Theseus;
	using Minotaur.Theseus.Evolution;
	using Minotaur.Theseus.IndividualCreation;
	using Minotaur.Theseus.IndividualMutation;
	using Minotaur.Theseus.RuleCreation;

	public static class Program {

		public static int Main(string[] args) {
			if (args.Length == 1 && args[0] == "--lazy-dev-switch")
				args = LazyDevArguments();

			return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static string[] LazyDevArguments() {
			return new string[] {
				"--train-data=c:/source/minotaur-gecco2020/datasets/yeast/folds/0/train-data.csv",
				"--train-labels=c:/source/minotaur-gecco2020/datasets/yeast/folds/0/train-labels.csv",
				"--test-data=c:/source/minotaur-gecco2020/datasets/yeast/folds/0/test-data.csv",
				"--test-labels=c:/source/minotaur-gecco2020/datasets/yeast/folds/0/test-labels.csv",

				"--classification-type=multilabel",

				"--output-directory=C:/Source/minotaur.output/",

				"--fitness-metrics=fscore",
				"--fitness-metrics=rule-count",
				"--fittest-selection=nsga2",

				"--population-size=30",
				"--mutants-per-generation=10",

				"--max-generations=200",

				"--cfsbe-target-instance-coverage=100",

				"--rule-consequent-threshold=0.5",
				"--expensive-sanity-checks=false"
			};
		}

		public static int Run(ProgramSettings settings) {
			ThreadPool.SetMaxThreads(Environment.ProcessorCount, Environment.ProcessorCount);
			SanityChecker.PerformChecks = bool.Parse(settings.ExpensiveSanityChecks);

			PrintSettings(settings);

			(var trainDataset, var testDataset) = DatasetLoader.LoadDatasets(
				trainDataFilename: settings.TrainDataFilename,
				trainLabelsFilename: settings.TrainLabelsFilename,
				testDataFilename: settings.TestDataFilename,
				testLabelsFilename: settings.TestLabelsFilename,
				classificationType: settings.ClassificationType);

			Console.WriteLine();
			PrintTrainDatasetInformation(trainDataset);
			Console.WriteLine();

			var hyperRectangleCoverageComputer = new HyperRectangleCoverageComputer(
				dataset: trainDataset,
				cache: new NullCache<HyperRectangle, DatasetCoverage>());

			var featureTestIntervalConveter = new FeatureTestDimensionIntervalConverter(trainDataset);

			var ruleAntecedentHyperRectangleConverter = new RuleAntecedentHyperRectangleConverter(featureTestIntervalConveter);

			var seedSelector = new SeedFinder(
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
				targetNumberOfInstancesToCover: settings.ConstrainedFeatureSpaceBoxEnlargementTargetNumberOfInstances);

			var individualMutationChooser = BiasedOptionChooser<IndividualMutationType>.Create(
				new Dictionary<IndividualMutationType, int>() {
					[IndividualMutationType.AddRule] = settings.IndividualMutationAddRuleWeight,
					[IndividualMutationType.ModifyRule] = settings.IndividualMutationModifyRuleWeight,
					[IndividualMutationType.RemoveRule] = settings.IndividualMutationRemoveRuleWeight
				});

			var ruleSwappingindividualMutator = new RuleSwappingIndividualMutator(
				mutationChooser: individualMutationChooser,
				ruleCreator: ruleCreator);

			var populationMutator = new PopulationMutatorMk2(
				individualMutator: ruleSwappingindividualMutator,
				mutantsPerGeneration: settings.MutantsPerGeneration,
				maximumFailedAttemptsPerGeneration: settings.MaximumFailedMutationAttemptsPerGeneration);

			var trainMetrics = IMetricParser.ParseMetrics(
				dataset: trainDataset,
				metricsNames: settings.MetricNames,
				classificationType: settings.ClassificationType);

			var trainFitnessEvaluator = new FitnessEvaluatorMk2(trainMetrics);

			var testMetrics = IMetricParser.ParseMetrics(
				dataset: testDataset,
				metricsNames: settings.MetricNames,
				classificationType: settings.ClassificationType);

			var testFitnessEvaluator = new FitnessEvaluatorMk2(testMetrics);

			var fittestIdentifier = IFittestIdentifierParser.Parse(
				name: settings.SelectionAlgorithm,
				fittestCount: settings.PopulationSize);

			var individualCreator = new SingleRuleIndividualCreator(ruleCreator: ruleCreator);

			var initialPopulation = CreateInitialPopulation(
				individualCreator: individualCreator,
				settings: settings);

			var consistencyChecker = new RuleConsistencyChecker(
				ruleAntecedentHyperRectangleConverterconverter: ruleAntecedentHyperRectangleConverter,
				hyperRectangleIntersector: hyperRectangleIntersector);

			CheckInitialPopulationConsistency(consistencyChecker, initialPopulation);

			var stdoutLogger = new BasicStdoutLogger(testDatasetFitnessEvaluator: testFitnessEvaluator);

			var fileLogger = new AdvancedFileLogger(
					outputDirectory: settings.OutputDirectory,
					trainDatasetFitnessEvaluator: trainFitnessEvaluator,
					testDatasetFitnessEvaluator: testFitnessEvaluator);

			var evolutionEngine = new EvolutionEngineMk2(
				maximumNumberOfGenerations: settings.MaximumGenerations,
				fitnessEvaluator: trainFitnessEvaluator,
				populationMutator: populationMutator,
				fittestIdentifier: fittestIdentifier,
				stdoutLogger: stdoutLogger,
				fileLogger: fileLogger);

			var lastGenerationResult = evolutionEngine.Run(initialPopulation);
			if (lastGenerationResult.GenerationNumber == settings.MaximumGenerations)
				Console.WriteLine($"Evolution cycle stopped. Reason: maximum number of generations reached.");
			else
				Console.WriteLine($"Evolution cycle stopped. Reason: maximum number of generations reached.");

			PrintTicks();

			Console.Write("Writing output to disk... ");
			fileLogger.WriteToDisk();
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
			Console.WriteLine("Train dataset information");
			Console.WriteLine($"Train dataset instance count {trainDataset.InstanceCount}");
			Console.WriteLine($"Train dataset feature count {trainDataset.FeatureCount}");
			Console.WriteLine($"Train dataset class count {trainDataset.ClassCount}");
			Console.WriteLine($"Train dataset volume {VolumeComputer.ComputeDatasetVolume(trainDataset)}");
		}

		private static Individual[] CreateInitialPopulation(IIndividualCreator individualCreator, ProgramSettings settings) {
			var statusReportPrefix = "Creating initial population: ";
			var statusReport = $"" +
				$"{statusReportPrefix} " +
				$"0 / {settings.PopulationSize}";

			Console.Write(statusReport);

			var population = new Individual[settings.PopulationSize];
			var created = 0L;

			Parallel.For(0, population.Length, i => {
				population[i] = individualCreator.CreateFirstGenerationIndividual();

				statusReport = $"" +
				$"\r{statusReportPrefix} " +
				$"{Interlocked.Increment(ref created)} / {settings.PopulationSize}";

				Console.Write(statusReport);
			});

			Console.WriteLine(" Done.");

			return population;
		}

		private static void CheckInitialPopulationConsistency(RuleConsistencyChecker consistencyChecker, Individual[] population) {
			Console.Write("Checking if the population is consistent... ");

			Parallel.For(0, population.Length, i => {
				var individual = population[i];
				var isConsistent = consistencyChecker.IsConsistent(individual);
				if (!isConsistent) {
					throw new InvalidOperationException();
				}
			});

			Console.WriteLine("Yep, it is.");
		}

		private static void PrintTicks() {
			Console.WriteLine();
			Console.WriteLine($"Fitness evaluation ticks: {Timers.FitnessEvaluationTicks}");
			Console.WriteLine($"CFSBE ticks: {Timers.CFSBETicks}");
		}
	}
}

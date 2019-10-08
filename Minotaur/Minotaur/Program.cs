namespace Minotaur {
	using System;
	using System.Collections.Generic;
	using System.Text.Json;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Metrics;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Minotaur.Theseus;
	using Minotaur.Theseus.IndividualBreeding;
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
				"--train-data=C:/Source/minotaur.datasets/iris/ready-for-minotaur/fold-0/train-data.csv",
				"--train-labels=C:/Source/minotaur.datasets/iris/ready-for-minotaur/fold-0/train-labels.csv",
				"--test-data=C:/Source/minotaur.datasets/iris/ready-for-minotaur/fold-0/test-data.csv",
				"--test-labels=C:/Source/minotaur.datasets/iris/ready-for-minotaur/fold-0/test-labels.csv",
				//"--feature-types=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-feature-types.csv",

				"--output-directory=C:/Source/minotaur-output/",

				"--cfsbe-target-instance-coverage=50",

				"--rule-consequent-threshold=0.5",
			};
		}

		public static int Run(ProgramSettings settings) {
			PrintSettings(settings);

			(var trainDataset, var testDataset) = DatasetLoader.LoadDatasets(
				trainDataFilename: settings.TrainDataFilename,
				trainLabelsFilename: settings.TrainLabelsFilename,
				testDataFilename: settings.TestDataFilename,
				testLabelsFilename: settings.TestLabelsFilename);

			Console.WriteLine();
			PrintTrainDatasetInformation(trainDataset);
			Console.WriteLine();

			var hyperRectangleCoverageCache = IConcurrentCacheSelector.Create<HyperRectangle, DatasetCoverage>(capacity: settings.RuleCoverageCacheSize);

			var hyperRectangleCoverageComputer = new HyperRectangleCoverageComputer(
				dataset: trainDataset,
				cache: hyperRectangleCoverageCache);

			var featureTestIntervalConveter = new FeatureTestDimensionIntervalConverter(trainDataset);

			var ruleAntecedentHyperRectangleConverter = new RuleAntecedentHyperRectangleConverter(featureTestIntervalConveter);

			var seedSelector = new SeedFinder(
				ruleConverter: ruleAntecedentHyperRectangleConverter,
				coverageComputer: hyperRectangleCoverageComputer);

			var antecedentCreator = new InstanceCoveringRuleAntecedentCreator(ruleAntecedentHyperRectangleConverter: ruleAntecedentHyperRectangleConverter);

			var consequentCreator = new InstanceLabelsAveragingRuleConsequentCreator(
				dataset: trainDataset,
				threshold: settings.RuleConsequentThreshold);

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

			var populationMutator = new PopulationMutator(
				individualMutator: ruleSwappingindividualMutator,
				mutantsPerGeneration: settings.MutantsPerGeneration,
				maximumFailedAttemptsPerGeneration: settings.MaximumFailedMutationAttemptsPerGeneration);

			var trainMetrics = MetricsSelector.SelectMetrics(
				dataset: trainDataset,
				metricsNames: settings.MetricNames);

			var trainFitnessCache = IConcurrentCacheSelector.Create<Individual, Fitness>(
				capacity: settings.FitnessCacheSize);

			var trainFitnessEvaluator = new FitnessEvaluator(
			  metrics: trainMetrics,
			  cache: trainFitnessCache);

			var testMetrics = MetricsSelector.SelectMetrics(
				dataset: testDataset,
				metricsNames: settings.MetricNames);

			var testFitnessCache = IConcurrentCacheSelector.Create<Individual, Fitness>(
				capacity: settings.FitnessCacheSize);

			var testFitnessEvaluator = new FitnessEvaluator(
				metrics: testMetrics,
				cache: testFitnessCache);

			var fitnessReportMaker = new FitnessReportMaker(
				trainDatasetFitnessEvaluator: trainFitnessEvaluator,
				testDatasetFitnessEvaluator: testFitnessEvaluator);

			var fittestSelector = IFittestSelectorCreator.Create(
				fittestSelectorName: settings.SelectionAlgorithm,
				fittestCount: settings.PopulationSize,
				fitnessEvaluator: trainFitnessEvaluator);

			var individualCreator = new SingleRuleIndividualCreator(ruleCreator: ruleCreator);

			var initialPopulation = CreateInitialPopulation(
				individualCreator: individualCreator,
				settings: settings);

			var consistencyChecker = new RuleConsistencyChecker(
				ruleAntecedentHyperRectangleConverterconverter: ruleAntecedentHyperRectangleConverter,
				hyperRectangleIntersector: hyperRectangleIntersector);

			CheckInitialPopulationConsistency(consistencyChecker, initialPopulation);

			var evolutionEngine = new EvolutionEngine(
				populationMutator: populationMutator,
				fitnessReportMaker: fitnessReportMaker,
				fittestSelector: fittestSelector,
				consistencyChecker: consistencyChecker,
				maximumGenerations: settings.MaximumGenerations);

			var evolutionReport = evolutionEngine.Run(initialPopulation);
			Console.WriteLine($"Evolution stoped. Reason: {evolutionReport.ReasonForStoppingEvolution}");

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
				population[i] = individualCreator.Create();

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
	}
}

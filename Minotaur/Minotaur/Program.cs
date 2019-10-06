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
				"--train-data=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-1-train-data.csv",
				"--train-labels=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-1-train-labels.csv",
				"--test-data=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-1-test-data.csv",
				"--test-labels=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-1-test-labels.csv",
				"--feature-types=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-feature-types.csv",

				//"--train-data=C:/Source/dataset-making/train-data.csv",
				//"--train-labels=C:/Source/dataset-making/train-labels.csv",

				//"--test-data=C:/Source/dataset-making/test-data.csv",
				//"--test-labels=C:/Source/dataset-making/test-labels.csv",

				//"--feature-types=C:/Source/dataset-making/feature-types.csv",

				//"--output-directory=C:/Source/minotaur/temp/",

				"--fitness-metrics=fscore",
				"--fitness-metrics=model-size",
				//"--fitness-metrics=average-rule-volume",

				"--max-generations=300",
				"--max-failed-mutations-per-generation=20000",

				"--population-size=100",
				"--mutants-per-generation=200",

				$"--hyperrectangle-cache-size={1024*32}",
				$"--rule-coverage-cache-size={1024*32}",
				$"--individual-fitness-cache-size={1024}",

				"--fittest-selection=nsga2",

				//"--rule-mutation-add-test-weight=10",
				//"--rule-mutation-remove-test-weight=0.5",
				//"--rule-mutation-modify-test-weight=80",
				//"--rule-mutation-modify-consequent-weight=20",

				"--individual-mutation-add-rule-weight=5",
				"--individual-mutation-modify-rule-weight=20",
				"--individual-mutation-remove-rule-weight=10",
			};
		}

		public static int Run(ProgramSettings settings) {
			PrintSettings(settings);

			(var trainDataset, var testDataset) = DatasetLoader.LoadDatasets(
				trainDataFilename: settings.TrainDataFilename,
				trainLabelsFilename: settings.TrainLabelsFilename,
				testDataFilename: settings.TestDataFilename,
				testLabelsFilename: settings.TestLabelsFilename,
				featureTypesFilename: settings.FeatureTypesFilename);

			Console.WriteLine();
			PrintTrainDatasetInformation(trainDataset);
			Console.WriteLine();

			//var dimensionIntervalCreator = new DimensionIntervalCreator(dataset: trainDataset);

			//var hyperRectangleCreatorCache = IConcurrentCacheSelector.Create<Rule, HyperRectangle>(
			//	capacity: settings.HyperRectangleCacheSize);

			//var hyperRectangleCreator = new HyperRectangleCreator(
			//  dimensionIntervalCreator: dimensionIntervalCreator,
			//  cache: hyperRectangleCreatorCache);

			var hyperRectangleCoverageCache = IConcurrentCacheSelector.Create<HyperRectangle, DatasetCoverage>(
				capacity: 32 * 1024);

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
				threshold: 0.5f);

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
				targetNumberOfInstancesToCover: 250);

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

			var individualBreeder = new IndividualBreeder(
				dataset: trainDataset,
				ruleConsistencyChecker: consistencyChecker);

			var populationBreeder = new PopulationBreeder(
				individualBreeder: individualBreeder,
				childrenPerGeneration: 0);

			var evolutionEngine = new EvolutionEngine(
				populationBreeder: populationBreeder,
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

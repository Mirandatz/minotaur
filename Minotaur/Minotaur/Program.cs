namespace Minotaur {
	using System;
	using System.Collections.Generic;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms;
	using Minotaur.GeneticAlgorithms.Metrics;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.GeneticAlgorithms.Selection;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Minotaur.Theseus;
	using Minotaur.Theseus.IndividualCreation;
	using Minotaur.Theseus.IndividualMutation;
	using Minotaur.Theseus.RuleCreation;
	using Minotaur.Theseus.TestCreation;
	using Newtonsoft.Json;

	public static class Program {

		public static int Main(string[] args) {
			if (args.Length == 1 && args[0] == "--lazy-dev-switch")
				args = LazyDevArguments();

			return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static string[] LazyDevArguments() {
			return new string[] {
				//"--train-data=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-0-train-data.csv",
				//"--train-labels=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-0-train-labels.csv",
				//"--test-data=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-0-test-data.csv",
				//"--test-labels=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-fold-0-test-labels.csv",
				//"--feature-types=C:/Source/geneal.datasets/ready-for-darwin/emotions/emotions-feature-types.csv",

				"--train-data=C:/Source/dataset-making/train-data.csv",
				"--train-labels=C:/Source/dataset-making/train-labels.csv",

				"--test-data=C:/Source/dataset-making/test-data.csv",
				"--test-labels=C:/Source/dataset-making/test-labels.csv",

				"--feature-types=C:/Source/dataset-making/feature-types.csv",

				//"--output-directory=C:/Source/minotaur/temp/",

				"--fitness-metrics=fscore",
				"--fitness-metrics=rule-count",
				//"--fitness-metrics=average-rule-volume",

				"--max-generations=2000",
				"--max-failed-mutations-per-generation=2000",

				"--population-size=50",
				"--mutants-per-generation=100",
				"--maximum-initial-rule-count=50",

				$"--hyperrectangle-cache-size={1024*32}",
				$"--rule-coverage-cache-size={1024*32}",
				$"--individual-fitness-cache-size={1024}",

				"--fittest-selection=nsga2",

				//"--rule-mutation-add-test-weight=10",
				//"--rule-mutation-remove-test-weight=0.5",
				//"--rule-mutation-modify-test-weight=80",
				//"--rule-mutation-modify-consequent-weight=20",

				"--individual-mutation-add-rule-weight=900",
				"--individual-mutation-modify-rule-weight=30",
				"--individual-mutation-remove-rule-weight=2",
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

			var dimensionIntervalCreator = new DimensionIntervalCreator(dataset: trainDataset);

			var ruleCoverageCache = IConcurrentCacheSelector.Create<Rule, RuleCoverage>(
				capacity: settings.RuleCoverageCacheSize);

			var ruleCoverageComputer = new RuleCoverageComputer(
				dataset: trainDataset,
				cache: ruleCoverageCache);

			var hyperRectangleCreatorCache = IConcurrentCacheSelector.Create<Rule, HyperRectangle>(
				capacity: settings.HyperRectangleCacheSize);

			var hyperRectangleCreator = new HyperRectangleCreator(
			  dimensionIntervalCreator: dimensionIntervalCreator,
			  cache: hyperRectangleCreatorCache);

			var seedSelector = new SeedSelector(
				hyperRectangleCreator: hyperRectangleCreator,
				ruleCoverageComputer: ruleCoverageComputer);

			var ruleCreator = new MinimalRuleCreator(
				dataset: trainDataset,
				seedSelector: seedSelector,
				hyperRectangleCreator: hyperRectangleCreator,
				maximumRatioOfNullFeatureTest: 0.9f,
				probabilityOfGeneratingNullTest: 0.2f);

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

			var consistencyChecker = new RuleConsistencyChecker(hyperRectangleCreator);

			CheckInitialPopulationConsistency(consistencyChecker, initialPopulation);

			var evolutionEngine = new EvolutionEngine(
				populationMutator: populationMutator,
				fitnessReportMaker: fitnessReportMaker,
				fittestSelector: fittestSelector,
				consistencyChecker: consistencyChecker,
				maximumGenerations: settings.MaximumGenerations);

			var d1 = new ContinuousDimensionInterval(
				dimensionIndex: 0,
				start: DimensionBound.CreateStart(float.NegativeInfinity),
				end: DimensionBound.CreateEnd(-1.9858872f));

			var d2 = new ContinuousDimensionInterval(
				dimensionIndex: 1,
				start: DimensionBound.CreateStart(0.008649528f),
				end: DimensionBound.CreateEnd(0.052685183f));

			var box = new HyperRectangle(new IDimensionInterval[] { d1, d2 });
			var seed = new float[] { 0.6426971f, 0.052685183f };
			var order = NaturalRange.CreateShuffled(0, 2);
			while (order[0] != 0)
				order = NaturalRange.CreateShuffled(0, 2);

			//var d1 = new ContinuousDimensionInterval(
			//	dimensionIndex: 0,
			//	start: DimensionBound.CreateStart(0),
			//	end: DimensionBound.CreateEnd(2));

			//var d2 = new ContinuousDimensionInterval(
			//	dimensionIndex: 1,
			//	start: DimensionBound.CreateStart(0),
			//	end: DimensionBound.CreateEnd(2f));

			//var box = new HyperRectangle(new IDimensionInterval[] { d1, d2 });
			//var seed = new float[] { 3, 3 };
			//var order = NaturalRange.CreateShuffled(0, 2);
			//while (order[0] != 1)
			//	order = NaturalRange.CreateShuffled(0, 2);


			var wtf = hyperRectangleCreator.CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: new HyperRectangle[] { box },
				dimensionExpansionOrder: order);

			_ = evolutionEngine.Run(initialPopulation);

			return 0;
		}

		private static void PrintSettings(ProgramSettings settings) {
			Console.WriteLine("Running MINOTAUR with settings:");
			var serialized = JsonConvert.SerializeObject(settings, Formatting.Indented);
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

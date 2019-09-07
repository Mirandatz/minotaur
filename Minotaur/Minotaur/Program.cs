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
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Minotaur.Theseus;
	using Newtonsoft.Json;

	public static class Program {

		public static int Main(string[] args) {
			if (args.Length == 1 && args[0] == "--lazy-dev-switch")
				args = LazyDevArguments();

			return CommandLineApplication.Execute<ProgramSettings>(args);
		}

		private static string[] LazyDevArguments() {
			return new string[] {
				"--train-data=C:/Source/geneal.datasets/ready-for-darwin/birds/birds-fold-0-train-data.csv",
				"--train-labels=C:/Source/geneal.datasets/ready-for-darwin/birds/birds-fold-0-train-labels.csv",
				"--test-data=C:/Source/geneal.datasets/ready-for-darwin/birds/birds-fold-0-test-data.csv",
				"--test-labels=C:/Source/geneal.datasets/ready-for-darwin/birds/birds-fold-0-test-labels.csv",
				"--feature-types=C:/Source/geneal.datasets/ready-for-darwin/birds/birds-feature-types.csv",

				"--output-directory=C:/Source/minotaur/temp/",

				"--fitness-metrics=fscore",
				"--fitness-metrics=model-size",

				"--max-generations=500",
				"--max-failed-mutations-per-generation=500",

				"--population-size=50",
				"--maximum-initial-rule-count=50",

				"--hyperrectangle-cache-size=50000",
				"--rule-coverage-cache-size=50000",
				"--individual-fitness-cache-size=1000",

				"--fittest-selection=nsga2",

				"--mutation-probability=1.0",

				//"--rule-mutation-add-test-weight=10",
				//"--rule-mutation-remove-test-weight=0.5",
				//"--rule-mutation-modify-test-weight=80",
				//"--rule-mutation-modify-consequent-weight=20",

				"--individual-mutation-add-rule-weight=15",
				"--individual-mutation-modify-rule-weight=80",
				"--individual-mutation-remove-rule-weight=5",
			};
		}

		public static int Run(ProgramSettings settings) {
			PrintSettings(settings);

			(var trainDataset, var testDataset) = LoadDatasets(settings);

			var dimensionIntervalCreator = new DimensionIntervalCreator(dataset: trainDataset);

			IConcurrentCache<Rule, RuleCoverage> ruleCoverageCache;
			if (settings.RuleCoverageCacheSize == 0) {
				ruleCoverageCache = new NullCache<Rule, RuleCoverage>();
			} else {
				ruleCoverageCache = new ConcurrentLruCache<Rule, RuleCoverage>(capacity: settings.RuleCoverageCacheSize);
			}

			var ruleCoverageComputer = new RuleCoverageComputer(
				dataset: trainDataset,
				cache: ruleCoverageCache);

			IConcurrentCache<Rule, HyperRectangle> hyperRectangleCreatorCache;
			if (settings.HyperRectangleCacheSize == 0) {
				hyperRectangleCreatorCache = new NullCache<Rule, HyperRectangle>();
			} else {
				hyperRectangleCreatorCache = new ConcurrentLruCache<Rule, HyperRectangle>(
					capacity: settings.HyperRectangleCacheSize);
			}

			var hyperRectangleCreator = new HyperRectangleCreator(
				dimensionIntervalCreator: dimensionIntervalCreator,
				cache: hyperRectangleCreatorCache);

			var seedSelector = new SeedSelector(
				hyperRectangleCreator: hyperRectangleCreator,
				ruleCoverageComputer: ruleCoverageComputer);

			var testCreator = new TestCreator(dataset: trainDataset);

			var ruleCreator = new RuleCreator(
				seedSelector: seedSelector,
				testCreator: testCreator,
				hyperRectangleCreator: hyperRectangleCreator);

			var individualCreator = new IndividualCreator(
				ruleCreator: ruleCreator,
				maximumInitialRuleCount: settings.MaximumInitialRuleCount);

			var initialPopulation = CreateInitialPopulation(
				individualCreator: individualCreator,
				settings: settings);

			var individualMutationChooser = BiasedOptionChooser<IndividualMutationType>.Create(
				new Dictionary<IndividualMutationType, int>() {
					[IndividualMutationType.AddRule] = settings.IndividualMutationAddRuleWeight,
					[IndividualMutationType.ModifyRule] = settings.IndividualMutationModifyRuleWeight,
					[IndividualMutationType.RemoveRule] = settings.IndividualMutationRemoveRuleWeight
				});

			var individualMutator = new IndividualMutator(
				mutationChooser: individualMutationChooser,
				ruleCreator: ruleCreator);

			var populationMutator = new PopulationMutator(
				individualMutator: individualMutator,
				mutantsPerGeneration: settings.MutantsPerGeneration,
				maximumFailedAttemptsPerGeneration: settings.MaximumFailedMutationAttemptsPerGeneration);

			var metrics = MetricsCreator.CreateFromMetricNames(
				dataset: trainDataset,
				metricsNames: settings.MetricNames);

			IConcurrentCache<Individual, Fitness> fitnessCache;
			if (settings.FitnessCacheSize == 0) {
				fitnessCache = new NullCache<Individual, Fitness>();
			} else {
				fitnessCache = new ConcurrentLruCache<Individual, Fitness>(capacity: settings.FitnessCacheSize);
			}

			var fitnessEvaluator = new FitnessEvaluator(
				metrics: metrics,
				cache: fitnessCache);

			var fittestSelector = CreateFittestSelector(
				fitnessEvaluator: fitnessEvaluator,
				settings: settings);

			var evolutionEngine = new EvolutionEngine(
				populationMutator: populationMutator,
				fittestSelector: fittestSelector,
				maximumGenerations: settings.MaximumGenerations);

			var evolutionReport = evolutionEngine.Run(initialPopulation);
			PrintFinalReport(
				trainDatasetFitnessEvaluator: fitnessEvaluator,
				testDataset: testDataset,
				settings: settings,
				report: evolutionReport);

			return 0;
		}

		private static void PrintSettings(ProgramSettings settings) {
			Console.WriteLine("Running MINOTAUR with settings:");
			var serialized = JsonConvert.SerializeObject(settings, Formatting.Indented);
			Console.WriteLine(serialized);
			Console.WriteLine();
		}

		private static (Dataset TrainDataset, Dataset TestDataset) LoadDatasets(ProgramSettings settings) {
			Console.Write("Started loading datasets...");

			var trainData = Task.Run(() => DatasetLoader.LoadData(settings.TrainDataFilename));
			var trainLabels = Task.Run(() => DatasetLoader.LoadLabels(settings.TrainLabelsFilename));

			var testData = Task.Run(() => DatasetLoader.LoadData(settings.TestDataFilename));
			var testLabels = Task.Run(() => DatasetLoader.LoadLabels(settings.TestLabelsFilename));

			var featureTypes = Task.Run(() => DatasetLoader.LoadFeatureTypes(settings.FeatureTypesFilename));

			Task.WaitAll(
				trainData,
				trainLabels,
				testData,
				testLabels,
				featureTypes);

			Console.WriteLine(" Done.");

			var trainDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes.Result,
				mutableData: trainData.Result,
				mutableLabels: trainLabels.Result);

			var testDataset = Dataset.CreateFromMutableObjects(
				mutableFeatureTypes: featureTypes.Result,
				mutableData: testData.Result,
				mutableLabels: testLabels.Result);

			Console.Write("Checking if TrainDataset and TestDataset are compatible...");

			if (trainDataset.FeatureCount != testDataset.FeatureCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same feature count");
			if (trainDataset.ClassCount != testDataset.ClassCount)
				throw new InvalidOperationException(nameof(trainDataset) + " and " + nameof(testDataset) + " must have the same class count");

			for (int i = 0; i < trainDataset.FeatureCount; i++) {
				var trainFeatureType = trainDataset.GetFeatureType(i);
				var testFeatureType = testDataset.GetFeatureType(i);

				switch (trainFeatureType) {

				case FeatureType.Categorical:
				if (testFeatureType == FeatureType.Categorical ||
					testFeatureType == FeatureType.CategoricalButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.Continuous:
				if (testFeatureType == FeatureType.Continuous ||
					testFeatureType == FeatureType.ContinuousButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.CategoricalButTriviallyValued:
				if (testFeatureType == FeatureType.Categorical ||
					testFeatureType == FeatureType.CategoricalButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				case FeatureType.ContinuousButTriviallyValued:
				if (testFeatureType == FeatureType.Continuous ||
					testFeatureType == FeatureType.ContinuousButTriviallyValued) {
					continue;
				} else {
					throw new InvalidOperationException(
						nameof(trainDataset) + " and " +
						nameof(testDataset) + " must have the same feature types.");
				}

				default:
				throw new InvalidOperationException("Unknown / unsupported value for {nameof(FeatureType)}.");
				}
			}

			Console.WriteLine(" Ok.");

			return (TrainDataset: trainDataset, TestDataset: testDataset);
		}

		private static Individual[] CreateInitialPopulation(IndividualCreator individualCreator, ProgramSettings settings) {

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

		private static IFittestSelector CreateFittestSelector(FitnessEvaluator fitnessEvaluator, ProgramSettings settings) {
			switch (settings.SelectionAlgorithm) {
			case "nsga2":
			return new NSGA2(
				fitnessEvaluator: fitnessEvaluator,
				fittestCount: settings.PopulationSize);

			case "lexicographic":
			throw new NotImplementedException();

			default:
			throw new ArgumentException($"Unknown selection algorithm: {settings.SelectionAlgorithm}");
			}
		}

		private static void PrintFinalReport(
			FitnessEvaluator trainDatasetFitnessEvaluator,
			Dataset testDataset,
			ProgramSettings settings,
			EvolutionReport report
			) {

			Console.WriteLine($"Evolution Engine stoped. Reason: {report.ReasonForStoppingEvolution}.");
			Console.WriteLine("Computing metrics of final population on train dataset...");
			var trainFitnesses = trainDatasetFitnessEvaluator.Evaluate(report.FinalPopulation);
			Console.WriteLine(FitnessReportMaker.MakeReport(trainFitnesses));

			var testsMetrics = MetricsCreator.CreateFromMetricNames(
				dataset: testDataset,
				metricsNames: settings.MetricNames);

			var testFitnessEvaluator = new FitnessEvaluator(
				metrics: testsMetrics,
				cache: new NullCache<Individual, Fitness>());

			Console.WriteLine("Computing metrics of final population on test dataset...");
			var testFitnesses = testFitnessEvaluator.Evaluate(report.FinalPopulation);
			Console.WriteLine(FitnessReportMaker.MakeReport(testFitnesses));
		}
	}
}

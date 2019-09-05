namespace Minotaur {
	using System;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using McMaster.Extensions.CommandLineUtils;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math.Dimensions;
	using Minotaur.Random;
	using Minotaur.Theseus;

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

				"--max-generations=2000",
				"--max-failed-mutations-per-generation=500",

				"--population-size=200",
				"--maximum-initial-rule-count=200",

				"--hyperrectangle-cache-size=50000",
				"--rule-coverage-cache-size=50000",

				"--fittest-selection=nsga2",
				"--mutation-probability=1.0",

				"--rule-mutation-remove-test-probability=0.12",
				"--rule-mutation-modify-test-probability=0.80",
				"--rule-mutation-modify-consequent-probability=0.08",

				"--add-rule-probability=0.1",
				"--modify-rule-probability=0.8",
				"--remove-rule-probability=0.1"
			};
		}

		public static int Run(ProgramSettings settings) {
			(var trainDataset, var testDataset) = LoadDatasets(settings);

			var dimensionIntervalCreator = new DimensionIntervalCreator(dataset: trainDataset);

			IConcurrentCache<Rule, HyperRectangle> hyperRectangleCreatorCache;
			if (settings.HyperRectangleCacheSize == 0) {
				hyperRectangleCreatorCache = new NullCache<Rule, HyperRectangle>();
			} else {
				hyperRectangleCreatorCache = new ConcurrentLruCache<Rule, HyperRectangle>(
					capacity: settings.HyperRectangleCacheSize);
			}

			IConcurrentCache<Rule, RuleCoverage> ruleCoverageCache;
			if (settings.RuleCoverageCacheSize == 0) {
				ruleCoverageCache = new NullCache<Rule, RuleCoverage>();
			} else {
				ruleCoverageCache = new ConcurrentLruCache<Rule, RuleCoverage>(capacity: settings.RuleCoverageCacheSize);
			}

			var ruleCoverageComputer = new RuleCoverageComputer(
				dataset: trainDataset,
				cache: ruleCoverageCache);

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

			throw new NotImplementedException();
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
			Console.Write("Creating initial population");

			var population = new Individual[settings.PopulationSize];

			Parallel.For(0, population.Length, i => {
				population[i] = individualCreator.Create();
				Console.Write(".");
			});

			Console.WriteLine(" Done.");

			return population;
		}
	}
}

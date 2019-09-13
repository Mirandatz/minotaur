namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Minotaur.Theseus.TestCreation;

	public sealed class OverfittingRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _hyperRectangleCreator;
		private readonly OverfittingTestCreator _testCreator;

		public OverfittingRuleCreator(
			SeedSelector seedSelector,
			OverfittingTestCreator testCreator,
			HyperRectangleCreator hyperRectangleCreator
			) {
			_seedSelector = seedSelector ?? throw new ArgumentNullException(nameof(seedSelector));
			_testCreator = testCreator ?? throw new ArgumentNullException(nameof(testCreator));
			_hyperRectangleCreator = hyperRectangleCreator ?? throw new ArgumentNullException(nameof(hyperRectangleCreator));

			Dataset = testCreator.Dataset;
		}

		public bool TryCreateRule(Array<Rule> existingRules, out Rule rule) {
			if (existingRules is null)
				throw new ArgumentNullException(nameof(existingRules));

			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				datasetInstanceIndex: out var seedIndex);

			if (!seedFound) {
				rule = null;
				return false;
			}

			var seed = Dataset.GetInstanceData(seedIndex);
			var hyperRectangles = CreateHyperRectangles(existingRules: existingRules);

			var secureRectangle = CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: hyperRectangles);

			if (!secureRectangle.Contains(seed))
				throw new InvalidOperationException();

			var tests = CreateTests(secureRectangle);
			var labels = Dataset.GetInstanceLabels(seedIndex);

			rule = new Rule(
				tests: tests,
				predictedLabels: labels);

			if (!rule.Covers(seed))
				throw new InvalidOperationException();

			return true;
		}

		private HyperRectangle[] CreateHyperRectangles(Array<Rule> existingRules) {
			var hyperRectangles = new HyperRectangle[existingRules.Length];

			Parallel.For(0, hyperRectangles.Length, i => {
				var currentRule = existingRules[i];
				var hyperRectangle = _hyperRectangleCreator.FromRule(currentRule);
				hyperRectangles[i] = hyperRectangle;
			});

			return hyperRectangles;
		}

		private HyperRectangle CreateLargestNonIntersectingHyperRectangle(Array<float> seed, HyperRectangle[] existingRectangles) {
			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
							inclusiveStart: 0,
							exclusiveEnd: Dataset.FeatureCount);

			return _hyperRectangleCreator.CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: existingRectangles,
				dimensionExpansionOrder: dimensionExpansionOrder);
		}

		private IFeatureTest[] CreateTests(HyperRectangle secureRectangle) {
			var dimension = secureRectangle.Dimensions;
			var tests = new IFeatureTest[dimension.Length];

			for (int i = 0; i < tests.Length; i++)
				tests[i] = _testCreator.FromDimensionInterval(dimension[i]);

			return tests;
		}
	}
}
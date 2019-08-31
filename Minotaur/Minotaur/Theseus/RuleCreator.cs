namespace Minotaur.Theseus {
	using System;
	using System.Threading.Tasks;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Random = Random.ThreadStaticRandom;

	public sealed class RuleCreator {

		public readonly Dataset Dataset;
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _hyperRectangleCreator;
		private readonly HyperRectangleEnlarger _hyperRectangleExpander;
		private readonly TestCreator _testCreator;

		public RuleCreator(
			Dataset dataset,
			SeedSelector seedSelector,
			TestCreator testCreator,
			HyperRectangleCreator hyperRectangleCreator,
			HyperRectangleEnlarger hyperRectangleEnlarger
			) {
			Dataset = dataset ?? throw new ArgumentNullException(nameof(dataset));
			_seedSelector = seedSelector ?? throw new ArgumentNullException(nameof(seedSelector));
			_testCreator = testCreator ?? throw new ArgumentNullException(nameof(testCreator));
			_hyperRectangleCreator = hyperRectangleCreator ?? throw new ArgumentNullException(nameof(hyperRectangleCreator));
			_hyperRectangleExpander = hyperRectangleEnlarger ?? throw new ArgumentNullException(nameof(hyperRectangleEnlarger));
		}

		public bool TryCreateRule(Array<Rule> existingRules, out Rule rule) {
			if (existingRules is null)
				throw new ArgumentNullException(nameof(existingRules));

			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				out var seed);

			if (!seedFound) {
				rule = null;
				return false;
			}

			var hyperRectangles = CreateHyperRectangles(existingRules: existingRules);

			var secureRectangle = EnlargeRectangle(
				seed: seed,
				hyperRectangles: hyperRectangles);

			var tests = CreateTests(secureRectangle);
			var labels = Random.Bools(count: Dataset.ClassCount);

			rule = new Rule(
				tests: tests,
				predictedLabels: labels);

			return true;
		}

		private IFeatureTest[] CreateTests(HyperRectangle secureRectangle) {
			var dimension = secureRectangle.Dimensions;
			var tests = new IFeatureTest[dimension.Length];

			for (int i = 0; i < tests.Length; i++)
				tests[i] = _testCreator.FromDimensionInterval(dimension[i]);

			return tests;
		}

		private HyperRectangle EnlargeRectangle(HyperRectangle seed, HyperRectangle[] hyperRectangles) {
			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
							inclusiveStart: 0,
							exclusiveEnd: Dataset.FeatureCount);

			return _hyperRectangleExpander.Enlarge(
				target: seed,
				others: hyperRectangles,
				dimensionExpansionOrder: dimensionExpansionOrder);
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
	}
}

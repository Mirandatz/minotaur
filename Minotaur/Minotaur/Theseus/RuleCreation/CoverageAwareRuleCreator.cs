namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Minotaur.Math.Dimensions;
	using Minotaur.Theseus.TestCreation;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly MaximalTestCreator _testCreator;
		private readonly int _minimumInstancesToCover;

		public CoverageAwareRuleCreator(SeedSelector seedSelector, HyperRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer, MaximalTestCreator testCreator, int minimumInstancesToCover) {
			Dataset = _seedSelector.Dataset;
			_seedSelector = seedSelector;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
			_testCreator = testCreator;
			_minimumInstancesToCover = minimumInstancesToCover;
		}

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule rule) {
			var seedFound = _seedSelector.TryFindSeed(
				existingRules: existingRules,
				datasetInstanceIndex: out var seedIndex);

			if (!seedFound) {
				rule = null!;
				return false;
			}

			var seed = Dataset.GetInstanceData(seedIndex);
			var existingRectangles = _boxCreator.FromRules(existingRules);

			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var secureRectangle = _boxCreator.CreateLargestNonIntersectingHyperRectangle(
				seed: seed,
				existingRectangles: existingRectangles,
				dimensionExpansionOrder: dimensionExpansionOrder);

			if (!secureRectangle.Contains(seed))
				throw new InvalidOperationException();

			var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);

			var coveredInstancesIndices = secureRectangleCoverage.IndicesOfCoveredInstances.ToArray();

			// @Consideration: maybe we could try finding another seed?
			if (coveredInstancesIndices.Length < _minimumInstancesToCover) {
				rule = null!;
				return false;
			}

			var coveredInstancesDistancesToSeed = Dataset.ComputeDistances(
				targetInstanceIndex: seedIndex,
				otherInstancesIndices: coveredInstancesIndices);

			Array.Sort(
				keys: coveredInstancesIndices,
				items: coveredInstancesDistancesToSeed);

			throw new NotImplementedException();
		}

		private Rule FromHyperRectangle(HyperRectangle hyperRectangle) {
			throw new NotImplementedException();
		}
	}
}

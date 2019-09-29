namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; }
		private readonly SeedSelector _seedSelector;
		private readonly HyperRectangleCreator _boxCreator;
		private readonly HyperRectangleCoverageComputer _coverageComputer;
		private readonly int _minimumInstancesToCover;

		public CoverageAwareRuleCreator(Dataset dataset, SeedSelector seedSelector, HyperRectangleCreator boxCreator, HyperRectangleCoverageComputer coverageComputer, int minimumInstancesToCover) {
			Dataset = dataset;
			_seedSelector = seedSelector;
			_boxCreator = boxCreator;
			_coverageComputer = coverageComputer;
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
	}
}

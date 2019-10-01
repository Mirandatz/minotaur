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
		private readonly InstanceCoveringRuleAntecedentCreator _antecedentCreator;
		private readonly InstanceLabelsAveragingRuleConsequentCreator _consequentCreator;
		private readonly int _minimumInstancesToCover;

		public CoverageAwareRuleCreator(SeedSelector seedSelector, HyperRectangleCreator hyperRectangleCreator, HyperRectangleCoverageComputer coverageComputer, InstanceCoveringRuleAntecedentCreator antecedentCreator, InstanceLabelsAveragingRuleConsequentCreator consequentCreator, int minimumInstancesToCover) {
			_seedSelector = seedSelector;
			_boxCreator = hyperRectangleCreator;
			_coverageComputer = coverageComputer;
			_antecedentCreator = antecedentCreator;
			_consequentCreator = consequentCreator;
			_minimumInstancesToCover = minimumInstancesToCover;
			Dataset = _seedSelector.Dataset;
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

			// @Sanity check
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

			var relevantInstances = coveredInstancesIndices
				.AsSpan()
				.Slice(start: 0, length: _minimumInstancesToCover);

			var ruleAntecedent = _antecedentCreator.CreateAntecedent(
				seed: seed,
				nearestInstancesIndices: relevantInstances);

			var ruleConsequent = _consequentCreator.CreateConsequent(relevantInstances);

			rule = new Rule(
				tests: ruleAntecedent,
				predictedLabels: ruleConsequent);

			return true;
		}
	}
}

namespace Minotaur.Theseus.RuleCreation {
	using System;
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.Collections;
	using Minotaur.Collections.Dataset;
	using Minotaur.GeneticAlgorithms.Population;
	using Minotaur.Math;
	using Random = Random.ThreadStaticRandom;

	public sealed class CoverageAwareRuleCreator: IRuleCreator {
		public Dataset Dataset { get; } = null!
		private readonly SeedFinder _seedSelector = null!;
		private readonly RuleAntecedentHyperRectangleConverter _boxConverter = null!;
		private readonly NonIntersectingRectangleCreator _boxCreator = null!;
		private readonly HyperRectangleCoverageComputer _coverageComputer = null!;
		private readonly InstanceCoveringRuleAntecedentCreator _antecedentCreator = null!;
		private readonly InstanceLabelsAveragingRuleConsequentCreator _consequentCreator = null!;
		private readonly int _minimumInstancesToCover;

		public bool TryCreateRule(Array<Rule> existingRules, [MaybeNullWhen(false)] out Rule rule) {

			var seedsIndices = _seedSelector.FindSeedsIndices(existingRules);

			if (seedsIndices.IsEmpty) {
				rule = default!;
				return false;
			}

			// This may fail just because we chose a bad seed or even because
			// we chose a bad dimension expansion order

			var boxes = _boxConverter.FromRules(existingRules);
			var dimensionExpansionOrder = NaturalRange.CreateShuffled(
				inclusiveStart: 0,
				exclusiveEnd: Dataset.FeatureCount);

			var seedIndex = Random.Choice(seedsIndices);
			var secureRectangle = _boxCreator.CreateLargestNonIntersectingRectangle(
				seedIndex: seedIndex,
				existingHyperRectangles: boxes,
				dimensionExpansionOrder: dimensionExpansionOrder);

			throw new NotImplementedException();

			//var secureRectangle = _boxCreator.CreateLargestNonIntersectingHyperRectangle(
			//	seed: seed,
			//	existingRectangles: existingRectangles,
			//	dimensionExpansionOrder: dimensionExpansionOrder);

			//// @Sanity check
			//if (!secureRectangle.Contains(seed))
			//	throw new InvalidOperationException();

			//var secureRectangleCoverage = _coverageComputer.ComputeCoverage(secureRectangle);

			//var coveredInstancesIndices = secureRectangleCoverage.IndicesOfCoveredInstances.ToArray();

			//// @Consideration: maybe we could try finding another seed?
			//if (coveredInstancesIndices.Length < _minimumInstancesToCover) {
			//	rule = null!;
			//	return false;
			//}

			//var coveredInstancesDistancesToSeed = Dataset.ComputeDistances(
			//	targetInstanceIndex: seedIndex,
			//	otherInstancesIndices: coveredInstancesIndices);

			//Array.Sort(
			//	keys: coveredInstancesDistancesToSeed,
			//	items: coveredInstancesIndices);

			//var relevantInstances = coveredInstancesIndices
			//	.AsSpan()
			//	.Slice(start: 0, length: _minimumInstancesToCover);

			//var ruleAntecedent = _antecedentCreator.CreateAntecedent(
			//	seed: seed,
			//	nearestInstancesIndices: relevantInstances);

			//var ruleConsequent = _consequentCreator.CreateConsequent(relevantInstances);

			//rule = new Rule(
			//	tests: ruleAntecedent,
			//	predictedLabels: ruleConsequent);

			//return true;
		}
	}
}

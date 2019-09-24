namespace Minotaur.Theseus.IndividualMutation {
	using System.Diagnostics.CodeAnalysis;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RepeatingIndividualMutator: IIndividualMutator {

		private readonly RuleSwappingIndividualMutator _individualMutator;
		private const int MaximumMutationCycles = 5;

		public RepeatingIndividualMutator(RuleSwappingIndividualMutator individualMutator) {
			_individualMutator = individualMutator;
		}

		public bool TryMutate(Individual original, [MaybeNullWhen(false)]  out Individual mutated) {
			if (!_individualMutator.TryMutate(original, out var temp)) {
				mutated = default!;
				return false;
			}

			var previous = temp;
			for (int i = 0; i < MaximumMutationCycles; i++) {
				if (_individualMutator.TryMutate(original: previous, out var current)) {
					previous = current;
				} else {
					mutated = previous;
					return true;
				}
			}

			mutated = previous;
			return true;
		}
	}
}

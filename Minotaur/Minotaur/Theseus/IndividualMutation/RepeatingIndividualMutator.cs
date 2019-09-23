namespace Minotaur.Theseus.IndividualMutation {
	using System;
	using Minotaur.GeneticAlgorithms.Population;

	public sealed class RepeatingIndividualMutator: IIndividualMutator {

		private readonly RuleSwappingIndividualMutator _individualMutator;
		private const int MaximumMutationCycles = 5;

		public RepeatingIndividualMutator(RuleSwappingIndividualMutator individualMutator) {
			_individualMutator = individualMutator ?? throw new ArgumentNullException(nameof(individualMutator));
		}

		public bool TryMutate(Individual original,  out Individual mutated) {
			throw new NotImplementedException();

			//var sucess = _individualMutator.TryMutate(original, out var temp);
			//if (!sucess) {
			//	mutated = default;
			//	return false;
			//}

			//var previous = temp;
			//for (int i = 0; i < MaximumMutationCycles; i++) {

			//	sucess = _individualMutator.TryMutate(
			//		original: previous,
			//		out var current);

			//	if (sucess) {
			//		previous = current;
			//	} else {
			//		mutated = previous;
			//		return true;
			//	}
			//}

			//mutated = previous;
			//return true;
		}
	}
}

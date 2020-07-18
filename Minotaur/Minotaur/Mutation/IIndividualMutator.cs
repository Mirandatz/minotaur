namespace Minotaur.Mutation {
	using Minotaur.Classification;

	public interface IIndividualMutator {

		ConsistentModel? TryMutate(ConsistentModel model);
	}
}

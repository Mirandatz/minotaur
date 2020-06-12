namespace Minotaur.Theseus.RuleCreation {
	using System;
	using Minotaur.Classification.Rules;

	public interface IConsequentCreator {
		Consequent CreateConsequent(ReadOnlySpan<int> indicesOfDatasetInstances);
	}
}

namespace Minotaur.Population {
	using System;
	using Minotaur.Collections;

	public interface IIndividual: IEquatable<IIndividual> {
		Array<IRule> Rules { get; }
		Array<bool> Predict(ReadOnlySpan<float> instance);
		Matrix<bool> Predict(Dataset dataset);
	}
}

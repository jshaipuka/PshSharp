namespace Psh.Coevolution;

public class FloatRegFitPredictionIndividual : PredictionGAIndividual
{
    protected internal static int _sampleSize = 8;
    private int[] _sampleIndices;

    protected internal PushGP _solutionGA;

    public FloatRegFitPredictionIndividual()
    {
        // The sample test cases used for fitness prediction.
        _sampleIndices = new int[_sampleSize];
        _solutionGA = null;
    }

    public FloatRegFitPredictionIndividual(PushGP inSolutionGA)
    {
        _sampleIndices = new int[_sampleSize];
        _solutionGA = inSolutionGA;
    }

    public FloatRegFitPredictionIndividual(PushGP inSolutionGA, int[] inSamples)
    {
        _sampleIndices = new int[_sampleSize];
        for (var i = 0; i < _sampleSize; i++) _sampleIndices[i] = inSamples[i];
        _solutionGA = inSolutionGA;
    }

    /// <summary>Gets the given sample index</summary>
    /// <param name="inIndex" />
    /// <returns>sample index</returns>
    public virtual int GetSampleIndex(int inIndex)
    {
        return _sampleIndices[inIndex];
    }

    /// <summary>Sets one of the sample indices to a new sample index.</summary>
    /// <param name="index" />
    /// <param name="sample" />
    public virtual void SetSampleIndex(int inIndex, int inSample)
    {
        _sampleIndices[inIndex] = inSample;
    }

    public virtual void SetSampleIndicesAndSolutionGA(PushGP inSolutionGA, int[] inSamples)
    {
        _sampleIndices = new int[_sampleSize];
        for (var i = 0; i < _sampleSize; i++) _sampleIndices[i] = inSamples[i];
        _solutionGA = inSolutionGA;
    }

    public override float PredictSolutionFitness(PushGPIndividual pgpIndividual)
    {
        var errors = new List<float>();
        for (var n = 0; n < _sampleSize; n++)
        {
            var test = _solutionGA._testCases[_sampleIndices[n]];
            var e = _solutionGA.EvaluateTestCase(pgpIndividual, test._input, test._output);
            errors.Add(e);
        }

        return AbsoluteAverageOfErrors(errors);
    }

    public override GAIndividual Clone()
    {
        return new FloatRegFitPredictionIndividual(_solutionGA, _sampleIndices);
    }

    public override string ToString()
    {
        var str = "Prediction Indices: [ ";
        foreach (var i in _sampleIndices) str += i + " ";
        str += "]";
        return str;
    }

    public virtual bool EqualPredictors(GAIndividual inB)
    {
        return _sampleIndices.OrderBy(x => x)
            .SequenceEqual(((FloatRegFitPredictionIndividual)inB)._sampleIndices.OrderBy(x => x));
        // int[] a = CopyArray(_sampleIndices);
        // int[] b = CopyArray(((Psh.Coevolution.FloatRegFitPredictionIndividual)inB)._sampleIndices);
        // /*
        // a = Arrays.copyOf(_sampleIndices, _sampleSize);
        // b = Arrays.copyOf(((FloatRegFitPredictionIndividual)inB)._sampleIndices, _sampleSize);
        // */
        // Arrays.Sort(a);
        // Arrays.Sort(b);
        // if (Arrays.Equals(a, b))
        // {
        //   return true;
        // }
        // return false;
    }

    private int[] CopyArray(int[] inArray)
    {
        var newArray = new int[inArray.Length];
        for (var i = 0; i < inArray.Length; i++) newArray[i] = inArray[i];
        return newArray;
    }
}
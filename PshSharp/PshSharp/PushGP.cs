/*
* Copyright 2009-2010 Jon Klein
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
*    http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,

* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System.Diagnostics;

namespace Psh;

/// <summary>The Push Genetic Programming core class.</summary>
public abstract class PushGP : GA
{
    protected internal float _averageSize;

    protected internal int _bestSize;

    protected internal int _executionLimit;

    protected internal float _fairMutationRange;

    protected internal int _finalSimplifications;

    public Interpreter _interpreter;

    protected internal int _maxPointsInProgram;

    protected internal int _maxRandomCodeSize;

    protected internal float _nodeSelectionLeafProbability;

    protected internal string _nodeSelectionMode;

    protected internal int _nodeSelectionTournamentSize;

    protected internal int _reportSimplifications;

    protected internal int _reproductionSimplifications;

    protected internal float _simplificationPercent;

    protected internal float _simplifyFlattenPercent;

    protected internal string _targetFunctionString;

    protected internal bool _useFairMutation;

    /// <exception cref="System.Exception" />
    protected internal override void InitFromParameters()
    {
        // Default parameters to be used when optional parameters are not
        // given.
        var defaultFairMutationRange = 0.3f;
        var defaultsimplifyFlattenPercent = 20f;
        var defaultInterpreterClass = "Psh.Interpreter";
        var defaultInputPusherClass = "Psh.InputPusher";
        var defaultTargetFunctionString = string.Empty;
        float defaultNodeSelectionLeafProbability = 10;
        var defaultNodeSelectionTournamentSize = 2;
        // Limits
        _maxRandomCodeSize = (int)GetFloatParam("max-random-code-size");
        _executionLimit = (int)GetFloatParam("execution-limit");
        _maxPointsInProgram = (int)GetFloatParam("max-points-in-program");
        // Fair mutation parameters
        _useFairMutation = "fair".Equals(GetParam("mutation-mode", true));
        _fairMutationRange = GetFloatParam("fair-mutation-range", true);
        if (float.IsNaN(_fairMutationRange)) _fairMutationRange = defaultFairMutationRange;
        // Node selection parameters
        _nodeSelectionMode = GetParam("node-selection-mode", true);
        if (_nodeSelectionMode != null)
        {
            if (!_nodeSelectionMode.Equals("unbiased") && !_nodeSelectionMode.Equals("leaf-probability") &&
                !_nodeSelectionMode.Equals("size-tournament"))
                throw new Exception("node-selection-mode must be set to unbiased,\n" +
                                    "leaf-probability, or size-tournament. Currently set to " + _nodeSelectionMode);
            _nodeSelectionLeafProbability = GetFloatParam("node-selection-leaf-probability", true);
            if (float.IsNaN(_nodeSelectionLeafProbability))
                _nodeSelectionLeafProbability = defaultNodeSelectionLeafProbability;
            _nodeSelectionTournamentSize = (int)GetFloatParam("node-selection-tournament-size", true);
            if (float.IsNaN(GetFloatParam("node-selection-tournament-size", true)))
                _nodeSelectionTournamentSize = defaultNodeSelectionTournamentSize;
        }
        else
        {
            _nodeSelectionMode = "unbiased";
        }

        // Simplification parameters
        _simplificationPercent = GetFloatParam("simplification-percent");
        _simplifyFlattenPercent = GetFloatParam("simplify-flatten-percent", true);
        if (float.IsNaN(_simplifyFlattenPercent)) _simplifyFlattenPercent = defaultsimplifyFlattenPercent;
        _reproductionSimplifications = (int)GetFloatParam("reproduction-simplifications");
        _reportSimplifications = (int)GetFloatParam("report-simplifications");
        _finalSimplifications = (int)GetFloatParam("final-simplifications");
        // ERC parameters
        int minRandomInt;
        var defaultMinRandomInt = -10;
        int maxRandomInt;
        var defaultMaxRandomInt = 10;
        int randomIntResolution;
        var defaultRandomIntResolution = 1;
        if (float.IsNaN(GetFloatParam("min-random-integer", true)))
            minRandomInt = defaultMinRandomInt;
        else
            minRandomInt = (int)GetFloatParam("min-random-integer", true);
        if (float.IsNaN(GetFloatParam("max-random-integer", true)))
            maxRandomInt = defaultMaxRandomInt;
        else
            maxRandomInt = (int)GetFloatParam("max-random-integer", true);
        if (float.IsNaN(GetFloatParam("random-integer-resolution", true)))
            randomIntResolution = defaultRandomIntResolution;
        else
            randomIntResolution = (int)GetFloatParam("random-integer-resolution", true);
        float minRandomFloat;
        var defaultMinRandomFloat = -10.0f;
        float maxRandomFloat;
        var defaultMaxRandomFloat = 10.0f;
        float randomFloatResolution;
        var defaultRandomFloatResolution = 0.01f;
        if (float.IsNaN(GetFloatParam("min-random-float", true)))
            minRandomFloat = defaultMinRandomFloat;
        else
            minRandomFloat = GetFloatParam("min-random-float", true);
        if (float.IsNaN(GetFloatParam("max-random-float", true)))
            maxRandomFloat = defaultMaxRandomFloat;
        else
            maxRandomFloat = GetFloatParam("max-random-float", true);
        if (float.IsNaN(GetFloatParam("random-float-resolution", true)))
            randomFloatResolution = defaultRandomFloatResolution;
        else
            randomFloatResolution = GetFloatParam("random-float-resolution", true);
        // Setup our custom interpreter class based on the params we're given
        var interpreterClass = GetParam("interpreter-class", true);
        if (interpreterClass == null) interpreterClass = defaultInterpreterClass;
        var iclass = Type.GetType(interpreterClass);
        var iObject = Activator.CreateInstance(iclass);
        if (!(iObject is Interpreter)) throw new Exception("interpreter-class must inherit from class Interpreter");
        _interpreter = (Interpreter)iObject;
        _interpreter.randProgram.SetInstructions(_interpreter, new Program(GetParam("instruction-set")));
        _interpreter.randInt.min = minRandomInt;
        _interpreter.randInt.max = maxRandomInt;
        _interpreter.randInt.resolution = randomIntResolution;
        _interpreter.randFloat.min = minRandomFloat;
        _interpreter.randFloat.max = maxRandomFloat;
        _interpreter.randFloat.resolution = randomFloatResolution;
        _interpreter.randCode.maxSize = _maxRandomCodeSize;
        _interpreter.execS._maxPointsInProgram = _maxPointsInProgram;
        // Frame mode and input pusher class
        // string framemode = GetParam("push-frame-mode", true);
        var inputpusherClass = GetParam("inputpusher-class", true);
        if (inputpusherClass == null) inputpusherClass = defaultInputPusherClass;
        iclass = Type.GetType(inputpusherClass);
        iObject = Activator.CreateInstance(iclass);
        if (!(iObject is InputPusher)) throw new Exception("inputpusher-class must inherit from class InputPusher");
        _interpreter.SetInputPusher((InputPusher)iObject);
        // Initialize the interpreter
        InitInterpreter(_interpreter);
        // if (framemode != null && framemode.Equals("pushstacks")) {
        //   _interpreter.SetUseFrames(true);
        // }
        // Target function string
        _targetFunctionString = GetParam("target-function-string", true);
        if (_targetFunctionString == null) _targetFunctionString = defaultTargetFunctionString;
        // Init the GA
        base.InitFromParameters();
        // Print important parameters
        Print("  Important Parameters\n");
        Print(" ======================\n");
        if (!_targetFunctionString.Equals(string.Empty)) Print("Target Function: " + _targetFunctionString + "\n\n");
        Print("Population Size: " + (int)GetFloatParam("population-size") + "\n");
        Print("Generations: " + _maxGenerations + "\n");
        Print("Execution Limit: " + _executionLimit + "\n\n");
        Print("Crossover Percent: " + _crossoverPercent + "\n");
        Print("Mutation Percent: " + _mutationPercent + "\n");
        Print("Simplification Percent: " + _simplificationPercent + "\n");
        Print("Clone Percent: " + (100 - _crossoverPercent - _mutationPercent - _simplificationPercent) + "\n\n");
        Print("Tournament Size: " + _tournamentSize + "\n");
        if (_trivialGeographyRadius != 0) Print("Trivial Geography Radius: " + _trivialGeographyRadius + "\n");
        Print("Node Selection Mode: " + _nodeSelectionMode);
        Print("\n");
        Print("Instructions: " + _interpreter.randProgram.GetInstructionsString(_interpreter) + "\n");
        Print("\n");
    }

    protected internal override void InitIndividual(GAIndividual inIndividual)
    {
        var i = (PushGPIndividual)inIndividual;
        var randomCodeSize = Rng.Next(_maxRandomCodeSize) + 2;
        var p = _interpreter.randProgram.RandomCode(randomCodeSize);
        i.SetProgram(p);
    }

    /// <exception cref="System.Exception" />
    protected internal override void BeginGeneration()
    {
        _averageSize = 0;
    }

    protected internal override void EndGeneration()
    {
        _averageSize /= _populations[0].Length;
    }

    protected internal override void Evaluate()
    {
        float totalFitness = 0;
        _bestMeanFitness = float.MaxValue;
        for (var n = 0; n < _populations[_currentPopulation].Length; n++)
        {
            var i = _populations[_currentPopulation][n];
            EvaluateIndividual(i);
            totalFitness += i.GetFitness();
            // fitness is minimized.
            if (i.GetFitness() < _bestMeanFitness)
            {
                _bestMeanFitness = i.GetFitness();
                _bestIndividual = n;
                _bestSize = ((PushGPIndividual)i)._program.ProgramSize();
                _bestErrors = i.GetErrors();
            }
        }

        _populationMeanFitness = totalFitness / _populations[_currentPopulation].Length;
    }

    protected internal override void EvaluateIndividual(GAIndividual inIndividual)
    {
        EvaluateIndividual(inIndividual, false);
    }

    protected internal virtual void EvaluateIndividual(GAIndividual inIndividual, bool duringSimplify)
    {
        var errors = new List<float>();
        if (!duringSimplify) _averageSize += ((PushGPIndividual)inIndividual)._program.ProgramSize();
        var sw = Stopwatch.StartNew();
        for (var n = 0; n < _testCases.Count; n++)
        {
            var test = _testCases[n];
            var e = EvaluateTestCase(inIndividual, test._input, test._output);
            errors.Add(e);
        }

        // XXX We measure the time but don't do anything with it.
        var t = sw.ElapsedMilliseconds;
        sw.Stop();
        inIndividual.SetFitness(AbsoluteAverageOfErrors(errors));
        inIndividual.SetErrors(errors);
    }

    // System.out.println("Evaluated individual in " + t + " msec: fitness "
    // + inIndividual.GetFitness());
    /// <exception cref="System.Exception" />
    protected internal abstract void InitInterpreter(Interpreter inInterpreter);

    protected internal override string Report()
    {
        var report = base.Report();
        if (double.IsInfinity(_populationMeanFitness)) _populationMeanFitness = double.MaxValue;
        report += ";; Best Program:\n  " + _populations[_currentPopulation][_bestIndividual] + "\n\n";
        report += ";; Best Program Fitness (mean): " + _bestMeanFitness + "\n";
        if (_testCases.Count == _bestErrors.Count)
        {
            report += ";; Best Program Errors: (";
            for (var i = 0; i < _testCases.Count; i++)
            {
                if (i != 0) report += " ";
                report += "(" + _testCases[i]._input + " ";
                report += Math.Abs(_bestErrors[i]) + ")";
            }

            report += ")\n";
        }

        report += ";; Best Program Size: " + _bestSize + "\n\n";
        report += ";; Mean Fitness: " + _populationMeanFitness + "\n";
        report += ";; Mean Program Size: " + _averageSize + "\n";
        var simplified = Autosimplify((PushGPIndividual)_populations[_currentPopulation][_bestIndividual],
            _reportSimplifications);
        report += ";; Number of Evaluations Thus Far: " + _interpreter.GetEvaluationExecutions() + "\n";
        // string mem = (Runtime.GetRuntime().TotalMemory() / 10000000.0f).ToString();
        // report += ";; Memory usage: " + mem + "\n\n";
        report += ";; Partial Simplification (may beat best):\n  ";
        report += simplified._program + "\n";
        report += ";; Partial Simplification Size: ";
        report += simplified._program.ProgramSize() + "\n\n";
        return report;
    }

    protected internal override string FinalReport()
    {
        var report = string.Empty;
        report += base.FinalReport();
        if (!_targetFunctionString.Equals(string.Empty))
            report += ">> Target Function: " + _targetFunctionString + "\n\n";
        var simplified = Autosimplify((PushGPIndividual)_populations[_currentPopulation][_bestIndividual],
            _finalSimplifications);
        // Note: The number of evaluations here will likely be higher than that
        // given during the last generational report, since evaluations made
        // during simplification count towards the total number of
        // simplifications.
        report += ">> Number of Evaluations: " + _interpreter.GetEvaluationExecutions() + "\n";
        report += ">> Best Program: " + _populations[_currentPopulation][_bestIndividual] + "\n";
        report += ">> Fitness (mean): " + _bestMeanFitness + "\n";
        if (_testCases.Count == _bestErrors.Count)
        {
            report += ">> Errors: (";
            for (var i = 0; i < _testCases.Count; i++)
            {
                if (i != 0) report += " ";
                report += "(" + _testCases[i]._input + " ";
                report += Math.Abs(_bestErrors[i]) + ")";
            }

            report += ")\n";
        }

        report += ">> Size: " + _bestSize + "\n\n";
        report += "<<<<<<<<<< After Simplification >>>>>>>>>>\n";
        report += ">> Best Program: ";
        report += simplified._program + "\n";
        report += ">> Size: ";
        report += simplified._program.ProgramSize() + "\n\n";
        return report;
    }

    public virtual string GetTargetFunctionString()
    {
        return _targetFunctionString;
    }

    protected internal virtual PushGPIndividual Autosimplify(PushGPIndividual inIndividual, int steps)
    {
        var simplest = (PushGPIndividual)inIndividual.Clone();
        var trial = (PushGPIndividual)inIndividual.Clone();
        EvaluateIndividual(simplest, true);
        var bestError = simplest.GetFitness();
        var madeSimpler = false;
        for (var i = 0; i < steps; i++)
        {
            madeSimpler = false;
            float method = Rng.Next(100);
            if (trial._program.ProgramSize() <= 0) break;
            if (method < _simplifyFlattenPercent)
            {
                // Flatten random thing
                var pointIndex = Rng.Next(trial._program.ProgramSize());
                var point = trial._program.Subtree(pointIndex);
                if (point is Program)
                {
                    trial._program.Flatten(pointIndex);
                    madeSimpler = true;
                }
            }
            else
            {
                // Remove small number of random things
                var numberToRemove = Rng.Next(3) + 1;
                for (var j = 0; j < numberToRemove; j++)
                {
                    var trialSize = trial._program.ProgramSize();
                    if (trialSize > 0)
                    {
                        var pointIndex = Rng.Next(trialSize);
                        trial._program.ReplaceSubtree(pointIndex, new Program());
                        trial._program.Flatten(pointIndex);
                        madeSimpler = true;
                    }
                }
            }

            if (madeSimpler)
            {
                EvaluateIndividual(trial, true);
                if (trial.GetFitness() <= bestError)
                {
                    simplest = (PushGPIndividual)trial.Clone();
                    bestError = trial.GetFitness();
                }
            }

            trial = (PushGPIndividual)simplest.Clone();
        }

        return simplest;
    }

    protected internal override void Reproduce()
    {
        var nextPopulation = _currentPopulation == 0 ? 1 : 0;
        for (var n = 0; n < _populations[_currentPopulation].Length; n++)
        {
            float method = Rng.Next(100);
            GAIndividual next;
            if (method < _mutationPercent)
            {
                next = ReproduceByMutation(n);
            }
            else
            {
                if (method < _crossoverPercent + _mutationPercent)
                {
                    next = ReproduceByCrossover(n);
                }
                else
                {
                    if (method < _crossoverPercent + _mutationPercent + _simplificationPercent)
                        next = ReproduceBySimplification(n);
                    else
                        next = ReproduceByClone(n);
                }
            }

            _populations[nextPopulation][n] = next;
        }
    }

    protected internal override GAIndividual ReproduceByCrossover(int inIndex)
    {
        var a = (PushGPIndividual)ReproduceByClone(inIndex);
        var b = (PushGPIndividual)TournamentSelect(_tournamentSize, inIndex);
        if (a._program.ProgramSize() <= 0) return b;
        if (b._program.ProgramSize() <= 0) return a;
        var aindex = ReproductionNodeSelection(a);
        var bindex = ReproductionNodeSelection(b);
        if (a._program.ProgramSize() + b._program.SubtreeSize(bindex) - a._program.SubtreeSize(aindex) <=
            _maxPointsInProgram) a._program.ReplaceSubtree(aindex, b._program.Subtree(bindex));
        return a;
    }

    protected internal override GAIndividual ReproduceByMutation(int inIndex)
    {
        var i = (PushGPIndividual)ReproduceByClone(inIndex);
        var totalsize = i._program.ProgramSize();
        var which = ReproductionNodeSelection(i);
        var oldsize = i._program.SubtreeSize(which);
        var newsize = 0;
        if (_useFairMutation)
        {
            var range = (int)Math.Max(1, _fairMutationRange * oldsize);
            newsize = Math.Max(1, oldsize + Rng.Next(2 * range) - range);
        }
        else
        {
            newsize = Rng.Next(_maxRandomCodeSize);
        }

        object newtree;
        if (newsize == 1)
            newtree = _interpreter.randProgram.RandomAtom();
        else
            newtree = _interpreter.randProgram.RandomCode(newsize);
        if (newsize + totalsize - oldsize <= _maxPointsInProgram) i._program.ReplaceSubtree(which, newtree);
        return i;
    }

    /// <summary>Selects a node to use during crossover or mutation.</summary>
    /// <remarks>
    ///     Selects a node to use during crossover or mutation. The selection
    ///     mechanism depends on the global parameter _nodeSelectionMode.
    /// </remarks>
    /// <param name="inInd">= Individual to select node from.</param>
    /// <returns>Index of the node to use for reproduction.</returns>
    protected internal virtual int ReproductionNodeSelection(PushGPIndividual inInd)
    {
        var totalSize = inInd._program.ProgramSize();
        var selectedNode = 0;
        if (totalSize <= 1)
        {
            selectedNode = 0;
        }
        else
        {
            if (_nodeSelectionMode.Equals("unbiased"))
            {
                selectedNode = Rng.Next(totalSize);
            }
            else
            {
                if (_nodeSelectionMode.Equals("leaf-probability"))
                {
                    // TODO Implement. Currently runs unbiased
                    // note: if there aren't any internal nodes, must select leaf, and
                    // if no leaf, must select internal
                    selectedNode = Rng.Next(totalSize);
                }
                else
                {
                    // size-tournament
                    var maxSize = -1;
                    selectedNode = 0;
                    for (var j = 0; j < _nodeSelectionTournamentSize; j++)
                    {
                        var nextwhich = Rng.Next(totalSize);
                        var nextwhichsize = inInd._program.SubtreeSize(nextwhich);
                        if (nextwhichsize > maxSize)
                        {
                            selectedNode = nextwhich;
                            maxSize = nextwhichsize;
                        }
                    }
                }
            }
        }

        return selectedNode;
    }

    protected internal virtual GAIndividual ReproduceBySimplification(int inIndex)
    {
        var i = (PushGPIndividual)ReproduceByClone(inIndex);
        i = Autosimplify(i, _reproductionSimplifications);
        return i;
    }

    public virtual void RunTestProgram(Program p, int inTestCaseIndex)
    {
        var i = new PushGPIndividual(p);
        var test = _testCases[inTestCaseIndex];
        Console.Out.WriteLine("Executing program: " + p);
        EvaluateTestCase(i, test._input, test._output);
        Console.Out.WriteLine(_interpreter);
    }
}
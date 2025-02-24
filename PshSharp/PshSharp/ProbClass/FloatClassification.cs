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

namespace Psh.ProbClass;

public class FloatClassification : PushGP
{
    internal float _currentInput;

    internal int _inputCount;

    /// <exception cref="System.Exception" />
    protected internal override void InitFromParameters()
    {
        base.InitFromParameters();
        var cases = GetParam("test-cases");
        var caselist = new Program(cases);
        _inputCount = ((Program)caselist.DeepPeek(0)).Size() - 1;
        for (var i = 0; i < caselist.Size(); i++)
        {
            var p = (Program)caselist.DeepPeek(i);
            if (p.Size() < 2) throw new Exception("Not enough entries for fitness case \"" + p + "\"");
            if (p.Size() != _inputCount + 1)
                throw new Exception("Wrong number of inputs for fitness case \"" + p + "\"");
            var @in = Convert.ToSingle(p.DeepPeek(0).ToString());
            var @out = Convert.ToSingle(p.DeepPeek(1).ToString());
            Print(";; Fitness case #" + i + " input: " + @in + " output: " + @out + "\n");
            _testCases.Add(new GATestCase(@in, @out));
        }
    }

    protected internal override void InitInterpreter(Interpreter inInterpreter)
    {
    }

    public override float EvaluateTestCase(GAIndividual inIndividual, object inInput, object inOutput)
    {
        _interpreter.ClearStacks();
        _currentInput = (float)inInput;
        var stack = _interpreter.FloatStack();
        stack.Push(_currentInput);
        _interpreter.Execute(((PushGPIndividual)inIndividual)._program, _executionLimit);
        var result = stack.Top();
        // System.out.println( _interpreter + " " + result );
        return result - (float)inOutput;
    }
}
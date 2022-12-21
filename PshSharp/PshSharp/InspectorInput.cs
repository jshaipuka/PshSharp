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

using System.Text.RegularExpressions;
using SharpenMinimal;

namespace Psh;

/// <summary>A utility class to help read PshInspector input files.</summary>
public class InspectorInput
{
    internal int _executionLimit;

    internal Interpreter _interpreter;
    internal Program _program;

    /// <summary>Constructs an InspectorInput from a filename string</summary>
    /// <param name="inFilename">The file to input from.</param>
    /// <exception cref="System.Exception" />
    public InspectorInput(string inFilename)
    {
        InitInspectorInput(inFilename);
    }

    /// <summary>Initializes an InspectorInput.</summary>
    /// <remarks>
    ///     Initializes an InspectorInput. The file should be organized as follows:
    ///     1 Program to execute
    ///     2 Execution limit
    ///     3 Integer, float, or boolean inputs
    ///     4 Available instructions. This is only used for creating random code
    ///     with code.rand or exec.rand
    /// </remarks>
    /// <param name="inFile">The file to input from.</param>
    /// <exception cref="System.Exception" />
    private void InitInspectorInput(string inFile)
    {
        _interpreter = new Interpreter();
        // Read fileString
        var fileString = Params.ReadFileString(inFile);
        // Get programString
        var indexNewline = fileString.IndexOf("\n");
        var programString = Extensions.Trim(Runtime.Substring(fileString, 0, indexNewline));
        fileString = Runtime.Substring(fileString, indexNewline + 1);
        // Get _executionLimit
        indexNewline = fileString.IndexOf("\n");
        if (indexNewline != -1)
        {
            var limitString = Extensions.Trim(Runtime.Substring(fileString, 0, indexNewline));
            _executionLimit = Convert.ToInt32(limitString);
            fileString = Runtime.Substring(fileString, indexNewline + 1);
        }
        else
        {
            // If here, no inputs to be pushed were included
            _executionLimit = Convert.ToInt32(fileString);
            fileString = string.Empty;
        }

        // Get inputs and push them onto correct stacks. If fileString = ""
        // at this point, then can still do the following with correct result.
        indexNewline = fileString.IndexOf("\n");
        if (indexNewline != -1)
        {
            var inputsString = Extensions.Trim(Runtime.Substring(fileString, 0, indexNewline));
            fileString = Runtime.Substring(fileString, indexNewline + 1);
            // Parse the inputs and load them into the interpreter
            ParseAndLoadInputs(inputsString);
        }
        else
        {
            ParseAndLoadInputs(fileString);
            fileString = string.Empty;
        }

        // Get the available instructions for random code generation
        indexNewline = fileString.IndexOf("\n");
        if (!Extensions.Trim(fileString).Equals(string.Empty))
            _interpreter.randProgram.SetInstructions(_interpreter, new Program(Extensions.Trim(fileString)));
        // Check for input.inN instructions
        CheckForInputIn(programString);
        // Add random integer and float parameters
        _interpreter.randInt.min = -10;
        _interpreter.randInt.max = 10;
        _interpreter.randInt.resolution = 1;
        _interpreter.randFloat.min = -10.0f;
        _interpreter.randFloat.max = 10.0f;
        _interpreter.randFloat.resolution = 0.01f;
        _interpreter.randCode.maxSize = 50;
        // Load the program
        _program = new Program(programString);
        _interpreter.LoadProgram(_program);
    }

    // Initializes program
    /// <summary>Returns the initialized interpreter</summary>
    /// <returns>The initialized interpreter</returns>
    public virtual Interpreter GetInterpreter()
    {
        return _interpreter;
    }

    public virtual Program GetProgram()
    {
        return _program;
    }

    /// <summary>Returns the execution limit</summary>
    /// <returns>The execution limit</returns>
    public virtual int GetExecutionLimit()
    {
        return _executionLimit;
    }

    /// <exception cref="System.Exception" />
    private void ParseAndLoadInputs(string input)
    {
        var inputTokens = Regex.Split(input, "\\s+");
        foreach (var token in inputTokens)
        {
            if (token.Equals(string.Empty)) continue;

            if (token.Equals("true"))
            {
                _interpreter.BoolStack().Push(true);
                _interpreter.InputStack().Push(true);
            }
            else if (token.Equals("false"))
            {
                _interpreter.BoolStack().Push(false);
                _interpreter.InputStack().Push(false);
            }
            else if (token.Matches("((-|\\+)?[0-9]+(\\.[0-9]+)?)+"))
            {
                if (token.Contains('.'))
                {
                    _interpreter.FloatStack().Push(float.Parse(token));
                    _interpreter.InputStack().Push(float.Parse(token));
                }
                else
                {
                    _interpreter.IntStack().Push(Convert.ToInt32(token));
                    _interpreter.InputStack().Push(Convert.ToInt32(token));
                }
            }
            else
            {
                throw new Exception($"Inputs must be of type int, float, or boolean. \"{token}\" is none of these.");
            }
        }
    }

    private void CheckForInputIn(string programString)
    {
        var added = string.Empty;
        var numstr = string.Empty;
        var index = 0;
        var numindex = 0;
        var spaceindex = 0;
        var parenindex = 0;
        var endindex = 0;
        while (true)
        {
            index = programString.IndexOf("input.in");
            if (index == -1) break;
            // System.out.println(programString + "    " + index);
            numindex = index + 8;
            if (!char.IsDigit(programString[numindex]))
            {
                programString = Runtime.Substring(programString, numindex);
                continue;
            }

            spaceindex = programString.IndexOf(' ', numindex);
            parenindex = programString.IndexOf(')', numindex);
            if (spaceindex == -1)
            {
                endindex = parenindex;
            }
            else
            {
                if (parenindex == -1)
                    endindex = spaceindex;
                else
                    endindex = Math.Min(spaceindex, parenindex);
            }

            numstr = Runtime.Substring(programString, numindex, endindex);
            // Check for doubles in added
            if (added.IndexOf(" " + numstr + " ") == -1)
            {
                added = added + " " + numstr + " ";
                _interpreter.DefineInstruction("input.in" + numstr, new InputInN(Convert.ToInt32(numstr)));
            }

            programString = Runtime.Substring(programString, numindex);
        }
    }
}
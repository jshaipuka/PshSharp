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

using System.Text;
using System.Text.RegularExpressions;

namespace Psh;

/// <summary>A Push program.</summary>
public class Program : ObjectStack
{
    /// <summary>Constructs an empty Program.</summary>
    public Program()
    {
    }

    /// <summary>Constructs a copy of an existing Program.</summary>
    /// <param name="inOther">The Push program to copy.</param>
    public Program(Program inOther)
    {
        inOther.CopyTo(this);
    }

    /// <summary>Constructs a Push program by parsing a String.</summary>
    /// <param name="inString">The Push program string to parse.</param>
    /// <exception cref="System.Exception" />
    public Program(string inString)
    {
        Parse(inString);
    }

    /// <summary>Sets this program to the parsed program string.</summary>
    /// <param name="inString">The Push program string to parse.</param>
    /// <returns>The point size of the new program.</returns>
    /// <exception cref="System.Exception" />
    public int Parse(string inString)
    {
        // XXX We could do something much better here.
        Clear();
        inString = inString.Replace("(", " ( ");
        inString = inString.Replace(")", " ) ");
        var tokens = Regex.Split(inString, "\\s+");
        Parse(tokens, 0);
        return ProgramSize();
    }

    /// <exception cref="System.Exception" />
    private int Parse(string[] inTokens, int inStart)
    {
        var first = inStart == 0;
        for (var n = inStart; n < inTokens.Length; n++)
        {
            var token = inTokens[n];
            if (!token.Equals(string.Empty))
            {
                if (token.Equals("("))
                {
                    // Found an open paren -- begin a recursive Parse, though
                    // the very first
                    // token in the list is a special case -- no need to create
                    // a sub-program
                    if (!first)
                    {
                        var p = new Program();
                        n = p.Parse(inTokens, n + 1);
                        Push(p);
                    }
                }
                else
                {
                    if (token.Equals(")"))
                        // End of the program -- return the advanced token index to
                        // the caller
                        return n;

                    if (char.IsLetter(token[0]))
                        Push(token);
                    else
                        // This makes printing stacks very ugly. For now, will store
                        // program instructions as strings, as was done before.
                        /*
                          Instruction i = _interpreter._instructions.get(token);
                          if (i != null)
                          push(i);
                          else
                          push(token);
                          */
                        try
                        {
                            object number;
                            if (token.IndexOf('.') != -1)
                                number = float.Parse(token);
                            else
                                number = Convert.ToInt32(token);
                            Push(number);
                        }
                        catch (FormatException fe)
                        {
                            throw new Exception("Could not convert '" + token + "' to number.", fe);
                        }
                }

                first = false;
            }
        }

        // If we're here, there was no closing brace for one of the programs
        throw new Exception("No closing brace found for program");
    }

    /// <summary>Returns the size of the program and all subprograms.</summary>
    /// <returns>The size of the program.</returns>
    public int ProgramSize()
    {
        var size = _size;
        for (var n = 0; n < _size; n++)
        {
            var o = this[n];
            if (o is Program) size += ((Program)o).ProgramSize();
        }

        return size;
    }

    /// <summary>Returns the size of a subtree.</summary>
    /// <param name="inIndex">The index of the requested subtree.</param>
    /// <returns>The size of the subtree.</returns>
    public int SubtreeSize(int inIndex)
    {
        var sub = Subtree(inIndex);
        if (sub == null) return 0;
        if (sub is Program) return ((Program)sub).ProgramSize();
        return 1;
    }

    /// <summary>Returns a subtree of the program.</summary>
    /// <param name="inIndex">The index of the requested subtree.</param>
    /// <returns>The program subtree.</returns>
    public object Subtree(int inIndex)
    {
        if (inIndex < _size) return this[inIndex];

        var startIndex = _size;
        for (var n = 0; n < _size; n++)
        {
            var o = this[n];
            if (o is Program)
            {
                var sub = (Program)o;
                var length = sub.ProgramSize();
                if (inIndex - startIndex < length) return sub.Subtree(inIndex - startIndex);
                startIndex += length;
            }
        }

        return null;
    }

    /// <summary>Replaces a subtree of this Program with a new object.</summary>
    /// <param name="inIndex">The index of the subtree to replace.</param>
    /// <param name="inReplacement">The replacement for the subtree</param>
    /// <returns>True if a replacement was made (the index was valid).</returns>
    public bool ReplaceSubtree(int inIndex, object inReplacement)
    {
        if (_size == 0)
        {
            Push(inReplacement);
            return true;
        }

        if (inIndex < _size)
        {
            this[inIndex] = Cloneforprogram(inReplacement);
            return true;
        }

        var startIndex = _size;
        for (var n = 0; n < _size; n++)
        {
            var o = this[n];
            if (o is Program)
            {
                var sub = (Program)o;
                var length = sub.ProgramSize();
                if (inIndex - startIndex < length)
                    return sub.ReplaceSubtree(inIndex - startIndex, inReplacement);
                startIndex += length;
            }
        }

        return false;
    }

    public void Flatten(int inIndex)
    {
        if (inIndex < _size)
        {
            // If here, the index to be flattened is in this program. So, push
            // the rest of the program onto a new program, and replace this with
            // that new program.
            var replacement = new Program(this);
            Clear();
            for (var i = 0; i < replacement._size; i++)
                if (inIndex == i)
                {
                    if (replacement[i] is Program)
                    {
                        var p = (Program)replacement[i];
                        for (var j = 0; j < p._size; j++) Push(p[j]);
                    }
                    else
                    {
                        Push(replacement[i]);
                    }
                }
                else
                {
                    Push(replacement[i]);
                }
        }
        else
        {
            var startIndex = _size;
            for (var n = 0; n < _size; n++)
            {
                var o = this[n];
                if (o is Program)
                {
                    var sub = (Program)o;
                    var length = sub.ProgramSize();
                    if (inIndex - startIndex < length)
                    {
                        sub.Flatten(inIndex - startIndex);
                        break;
                    }

                    startIndex += length;
                }
            }
        }
    }

    /// <summary>Copies this program to another.</summary>
    /// <param name="inOther">The program to receive the copy of this program</param>
    public void CopyTo(Program inOther)
    {
        for (var n = 0; n < _size; n++) inOther.Push(this[n]);
    }

    /// <summary>Creates a copy of an object suitable for adding to a Push Program.</summary>
    /// <remarks>
    ///     Creates a copy of an object suitable for adding to a Push Program. Java's
    ///     clone() is unfortunately useless for this task.
    /// </remarks>
    private object Cloneforprogram(object inObject)
    {
        // Java clone() is useless :(
        if (inObject is string
            || inObject is int
            || inObject is float
            || inObject is Instruction
            || inObject is bool)
            return inObject;
        if (inObject is Program) return new Program((Program)inObject);
        throw new Exception("Not sure how to clone program piece " + inObject);
    }


    public override string ToString()
    {
        var result = new StringBuilder("(");
        for (var n = 0; n < _size; n++)
        {
            if (n != 0) result.Append(" ");
            if (this[n] is float)
                result.Append(((float)this[n]).ToString("0.0##############"));
            else
                result.Append(this[n]);
        }

        result.Append(")");
        return result.ToString();
    }

    public override void Push(object inValue)
    {
        if (inValue == null)
            throw new ArgumentNullException("inValue (cannot have null in programs)");
        base.Push(inValue);
    }
}
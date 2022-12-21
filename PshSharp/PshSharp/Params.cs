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

using SharpenMinimal;

namespace Psh;

/// <summary>A utility class for reading PushGP params.</summary>
public class Params
{
    /// <exception cref="System.Exception" />
    public static Dictionary<string, string> ReadFromFile(string inFilepath)
    {
        var map = new Dictionary<string, string>();
        return Read(ReadFileString(inFilepath), map, inFilepath);
    }

    /// <exception cref="System.Exception" />
    public static Dictionary<string, string> Read(string inParams)
    {
        var map = new Dictionary<string, string>();
        return Read(inParams, map, null);
    }

    /// <exception cref="System.Exception" />
    public static Dictionary<string, string> Read(string inParams, Dictionary<string, string> inMap, string inFilepath)
    {
        var linenumber = 0;
        var filename = "<string>";
        try
        {
            var reader = new StringReader(inParams);
            string line;
            string parent;
            if (inFilepath == null)
            {
                parent = null;
                filename = "<string>";
            }
            else
            {
                // parent = inFile.GetParent();
                parent = Path.GetDirectoryName(inFilepath);
                filename = Path.GetFileName(inFilepath);
            }

            while ((line = reader.ReadLine()) != null)
            {
                linenumber += 1;
                var comment = line.IndexOf('#', 0);
                if (comment != -1) line = Runtime.Substring(line, 0, comment);
                if (line.StartsWith("include"))
                {
                    var startIndex = "include".Length;
                    var includefile = Extensions.Trim(Runtime.Substring(line, startIndex, line.Length));
                    try
                    {
                        var f = Path.Combine(parent, includefile);
                        Read(ReadFileString(f), inMap, f);
                    }
                    catch (IncludeException)
                    {
                        // A previous include exception should bubble up to the
                        // top
                        throw;
                    }
                    catch (Exception)
                    {
                        // Any other exception should generate an error message
                        throw new IncludeException("Error including file \"" + includefile + "\" at line " +
                                                   linenumber + " of file \"" + filename + "\"");
                    }
                }
                else
                {
                    var split = line.IndexOf('=', 0);
                    if (split != -1)
                    {
                        var name = Extensions.Trim(Runtime.Substring(line, 0, split));
                        var value = Extensions.Trim(Runtime.Substring(line, split + 1, line.Length));
                        while (value.EndsWith("\\"))
                        {
                            value = Runtime.Substring(value, 0, value.Length - 1);
                            line = reader.ReadLine();
                            if (line == null) break;
                            linenumber++;
                            value += Extensions.Trim(line);
                        }

                        inMap[name] = value;
                    }
                }
            }
        }
        catch (IncludeException)
        {
            // A previous include exception should bubble up to the top
            throw;
        }
        catch (Exception)
        {
            // Any other exception should generate an error message
            throw new IncludeException("Error at line " + linenumber + " of parameter file \"" + filename + "\"");
        }

        return inMap;
    }

    /// <summary>Utility function to read a file in its entirety to a string.</summary>
    /// <param name="inPath">The file path to be read.</param>
    /// <returns>The contents of a file represented as a string.</returns>
    /// <exception cref="System.Exception" />
    internal static string ReadFileString(string inPath)
    {
        return File.ReadAllText(inPath);
    }
}
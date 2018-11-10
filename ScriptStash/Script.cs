using System;
using System.Collections.Generic;
using System.IO;

namespace ScriptStash
{
    /// <summary>
    /// The class hold in-memory scripts lines and allow line manipulation operations.
    /// </summary>
    public class Script
    {
        #region variables
        /// <summary>
        /// Store the script lines.
        /// </summary>
        private List<string> _lines;

        /// <summary>
        /// Store the file path + name.
        /// </summary>
        private string _fileLocation = string.Empty;

        /// <summary>
        /// Stire script load pattern.
        /// </summary>
        private string _pattern = string.Empty;

        /// <summary>
        /// Inner tokens dictionary.
        /// </summary>
        private Tokens _tokens = null;
        #endregion

        #region properties
        /// <summary>
        /// Lines indexer property.
        /// </summary>
        /// <param name="index">Integer location of the line in the script (use zero based index).</param>
        /// <returns>a string with the line text.</returns>
        public string this[int index]
        {
            get
            {
                if ((index >= 0) && (index < _lines.Count))
                {
                    return _lines[index];
                }
                return null;
            }
            set
            {
                if ((index >= 0) && (index < _lines.Count))
                {
                    _lines[index] = value;
                }
            }
        }

        /// <summary>
        /// A get property to all script lines list.
        /// </summary>
        public List<string> Lines
        {
            get { return _lines; }
        }

        /// <summary>
        /// A get property to all script text lines.
        /// </summary>
        public string Text
        {
            get
            {
                string result = string.Empty;

                try
                {
                    if (_lines != null)
                    {
                        if (_lines.Count > 0)
                        {
                            for (int i = 0; i < _lines.Count; i++)
                            {
                                result += _lines[i] + ((i+1 < _lines.Count) ? Environment.NewLine : string.Empty);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"ScriptStash failed to assemble script '{_fileLocation}' text.", ex);
                }
                
                return result;
            }
        }

        /// <summary>
        /// A get property of the script line count.
        /// </summary>
        public int LineCount
        {
            get
            {
                if (_lines != null)
                {
                    return _lines.Count;
                }
                return 0;
            }
        }

        /// <summary>
        /// A get property of loaded script file extension (file suffix).
        /// </summary>
        public string FileExtension
        {
            get { return Path.GetExtension(_fileLocation);  }
        }

        /// <summary>
        /// A get property of loaded sript file name.
        /// </summary>
        public string FileName
        {
            get { return Path.GetFileName(_fileLocation); }
        }

        /// <summary>
        /// A get property for script directory name.
        /// </summary>
        public string FileDirectory
        {
            get { return Path.GetDirectoryName(_fileLocation);  }
        }

        /// <summary>
        /// A get property for script full path.
        /// </summary>
        public string FilePath
        {
            get { return _fileLocation; }
        }

        /// <summary>
        /// A get property for file name pattern that was used to load.
        /// </summary>
        public string Pattern
        {
            get { return _pattern; }
        }

        /// <summary>
        /// Inner string key-values dictionary for token-driven scripting.
        /// </summary>
        public Tokens Tokens
        {
            get { return _tokens; }
        }
        #endregion

        #region Constructors
        /// <summary>
        /// Script class constructor that loads the script file.
        /// </summary>
        /// <param name="fileLocation">The script file path+name.</param>
        public Script(string fileLocation)
        {
            _fileLocation = fileLocation;
            Load();
        }

        /// <summary>
        /// Script class constructor with tokens directory that loads the script file.
        /// </summary>
        /// <param name="fileLocation">The script file path+name.</param>
        /// <param name="tokens">reference to outer tokens dictionary memory. Reference ScriptStash.Tokens if the shared tokens directory is needed.</param>
        public Script(string fileLocation, ref Tokens tokens) : this (fileLocation)
        {
            _tokens = tokens;
        }
        #endregion

        #region hidden functionality
        /// <summary>
        /// Loads the script lines from given location at construction time.
        /// </summary>
        private void Load()
        {
            try
            {
                _lines = new List<string>(File.ReadAllLines(_fileLocation));
            }
            catch (Exception ex)
            {
                throw new Exception($"ScriptStash failed loading : '{_fileLocation}' !", ex);
            }
        }
        #endregion

        #region visible functionality
        #region index lookups
        /// <summary>
        /// Find the first index of line that match passed string condition.
        /// Returns -1 if an item that matches the conditions is not found.
        /// </summary>
        /// <param name="condition">A predicate function over string</param>
        /// <returns>A zero-based index of the first line that match condition.</returns>
        public int FirstLineIndexWhere(Predicate<string> condition)
        {
            return _lines.FindIndex(condition);
        }

        /// <summary>
        /// Find the last index of line that match passed string condition.
        /// Returns -1 if an item that matches the conditions is not found.
        /// </summary>
        /// <param name="condition">A predicate function over string</param>
        /// <returns>A zero-based index of the last line that match condition.</returns>
        public int LastLineIndexWhere(Predicate<string> condition)
        {
            return _lines.FindLastIndex(condition);
        }

        /// <summary>
        /// Find all line indexes that match passed string condition.
        /// Returns empty List if no match found.
        /// </summary>
        /// <param name="condition">A predicate function over string</param>
        /// <returns>A zero-based indexes list of found lines that match condition.</returns>
        public List<int> LinesIndexWhere(Predicate<string> condition)
        {
            int index = 0;
            int tempIndex = -1;
            List<int> result = new List<int>();

            do
            {
                tempIndex = _lines.FindIndex(index, condition);
                if (tempIndex > -1)
                {
                    result.Add(tempIndex);
                    index = tempIndex + 1;
                }
            } while (tempIndex > -1);

            return result;
        }
        #endregion index lookups

        #region line operations
        /// <summary>
        /// Add new line to the end of the script lines.
        /// </summary>
        /// <param name="line">The new line text.</param>
        public void AppendLine(string line)
        {
            _lines.Add(line);
        }

        /// <summary>
        /// Inserts given text line at a given index in the script lines.
        /// lines from given index are moved by 1 location and the new line inserted at the index position.
        /// To add new line to the start, use position 0.
        /// If index > lines count, the appendLine method is used.
        /// </summary>
        /// <exception cref="ArgumentException">throws exception if position less then zero or the line missing.</exception>
        /// <param name="line">The new line text.</param>
        /// <param name="index">Script line position (zero based index). </param>
        public void InsertLineAtIndex(string line, int index)
        {
            // missing line not allowed
            if (line == null)
            {
                throw (new ArgumentException("Line can't be NULL !", "line"));
            }

            // Illegal position not alowed.
            if (index < 0)
            {
                throw (new ArgumentException("Line position can't be less then 0 !", "index"));
            }
            
            if (index > (_lines.Count - 1))
            {
                AppendLine(line);
                return;
            }

            // Move one place ahead every line and insert the new line in givven position.
            _lines.Insert(index, line);
        }

        /// <summary>
        /// Remove script line at a given position.
        /// </summary>
        /// <param name="index">Zero based line index.</param>
        public void RemoveLineAtIndex(int index)
        {
            // Illegal position not alowed.
            if (index < 0)
            {
                throw (new ArgumentException("Line position can't be less then 0 !", "index"));
            }
            if (index > (_lines.Count - 1))
            {
                throw (new ArgumentException("Line position can't be grather then script lines count (using zero based index) !", "index"));
            }

            // overwrite line index and move each line one index up
            _lines.RemoveAt(index);
        }

        /// <summary>
        /// Inserts given text line before the first line match the passed string condition.
        /// </summary>
        /// <param name="line">Line string to insert.</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The line number that was inserted if condition match. If no match found returns (-1).</returns>
        public int InsertLineOnFirstWhere(string line, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int insertIndex = this.FirstLineIndexWhere(condition);

            if( insertIndex != -1)
            {
                this.InsertLineAtIndex(line, insertIndex);
            }

            return insertIndex;
        }

        /// <summary>
        /// Inserts given text line before the last line that match the passed string condition.
        /// </summary>
        /// <param name="line">A predicate function over string</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The line number that was inserted if condition match. If no match found returns (-1).</returns>
        public int InsertLineOnLastWhere(string line, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int insertIndex = this.LastLineIndexWhere(condition);

            if( insertIndex != -1)
            {
                this.InsertLineAtIndex(line, insertIndex);
            }

            return insertIndex;
        }

        /// <summary>
        /// Inserts given text line before every line who match the passed string condition.
        /// </summary>
        /// <param name="line">The line text tobe inserted.</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The inserted indexes of line numbers with condition match. If no match found returns zero length list.</returns>
        public List<int> InsertLineWhere(string line, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            List<int> insertIndexes = this.LinesIndexWhere(condition);

            for( int i=0; i < insertIndexes.Count; i++)
            {
                this.InsertLineAtIndex(line, insertIndexes[i]);
                // move all line indexes after the i position one index line up.
                if (i < insertIndexes.Count - 1 )
                {
                    for (int j = i + 1; j < insertIndexes.Count; j++)
                    {
                        insertIndexes[j]++;
                    } 
                }
            }
            
            return insertIndexes;
        }

        /// <summary>
        /// Replace given text line at a given index in the script lines.
        /// If index > lines count, the appendLine method is used.
        /// </summary>
        /// <param name="replaceLine">The new line text.</param>
        /// <param name="index">Script line position (zero based index).</param>
        public void ReplaceLineAtIndex(string replaceLine, int index)
        {
            // missing line not allowed
            if (replaceLine == null)
            {
                throw (new ArgumentException("Line can't be NULL !", "line"));
            }

            // Illegal position not alowed.
            if (index < 0)
            {
                throw (new ArgumentException("Line position can't be less then 0 !", "index"));
            }

            if (index > (_lines.Count - 1))
            {
                AppendLine(replaceLine);
                return;
            }

            _lines[index] = replaceLine;
        }

        /// <summary>
        /// Replace first line match the passed string condition with the given text line.
        /// </summary>
        /// <param name="replaceLine">The replacement line text.</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The line number that was replaced if condition match. If no match found returns (-1).</returns>
        public int ReplaceLineOnFirstWhere(string replaceLine, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int replaceIndex = this.FirstLineIndexWhere(condition);

            if(replaceIndex != -1)
            {
                this.ReplaceLineAtIndex(replaceLine, replaceIndex);
            }

            return replaceIndex;
        }

        /// <summary>
        /// Replace last line match the passed string condition with the given text line.
        /// </summary>
        /// <param name="replaceLine">The replacement line text.</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The line number that was replaced if condition match. If no match found returns (-1).</returns>
        public int ReplaceLineOnLastWhere(string replaceLine, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int replaceIndex = this.LastLineIndexWhere(condition);

            if (replaceIndex != -1)
            {
                this.ReplaceLineAtIndex(replaceLine, replaceIndex);
            }

            return replaceIndex;
        }

        /// <summary>
        /// Replace every line matches the passed string condition with the given text line.
        /// </summary>
        /// <param name="replaceLine">The replacement line text.</param>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The replaced indexes of line numbers with condition match. If no match found returns zero length list.</returns>
        public List<int> ReplaceLineWhere(string replaceLine, Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            List<int> replaceIndexes = this.LinesIndexWhere(condition);

            foreach (int index in replaceIndexes)
            {
                this.ReplaceLineAtIndex(replaceLine, index);
            }

            return replaceIndexes;
        }

        /// <summary>
        /// Remove the first found line that match given string condition.
        /// </summary>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The removed index line (returns -1 if no line has a match).</returns>
        public int RemoveLineOnFirstWhere(Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int removeIndex = this.FirstLineIndexWhere(condition);

            if( removeIndex != -1)
            {
                _lines.RemoveAt(removeIndex);
            }

            return removeIndex;
        }

        /// <summary>
        /// Remove the last found line that match given string condition.
        /// </summary>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The removed index line (returns -1 if no line has a match).</returns>
        public int RemoveLineOnLastWhere(Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            int removeIndex = this.LastLineIndexWhere(condition);

            if (removeIndex != -1)
            {
                _lines.RemoveAt(removeIndex);
            }

            return removeIndex;
        }

        /// <summary>
        /// Remove the first found line that match given string condition.
        /// </summary>
        /// <param name="condition">A predicate function over string.</param>
        /// <returns>The removed index lines list (value -1 used to note that no line has a match).</returns>
        public List<int> RemoveLineWhere(Predicate<string> condition)
        {
            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            List<int> removeIndexes = this.LinesIndexWhere(condition);
            int offset = 0;

            removeIndexes.Sort();
            foreach (int index in removeIndexes)
            {
                _lines.RemoveAt(index - offset);
                offset++;
            }

            return removeIndexes;
        }

        #endregion line operations

        #region file operations
        /// <summary>
        /// Save current script text to file. All file lines are overwritten.
        /// </summary>
        public void Save()
        {
            try
            {
                using (StreamWriter txtWriter = new StreamWriter(_fileLocation, false))
                {
                    for (int i = 0; i < _lines.Count; i++)
                    {
                        txtWriter.WriteLine(_lines[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ScriptStash failed saving : '{_fileLocation}' !", ex);
            }
        }

        /// <summary>
        /// Save current script to a new given location (file path + name).
        /// </summary>
        /// <param name="newfileLocation">The new target path location.</param>
        public void SaveAs(string newfileLocation)
        {
            if( (newfileLocation == null) || newfileLocation.Equals(string.Empty) )
            {
                throw new ArgumentException("new-file-Location cant be empty.", "newfileLocation");
            }
            else
            {
                _fileLocation = newfileLocation;
                this.Save();
            }            
        }

        /// <summary>
        /// Reloads the script from last known location. The script lines are overwritten.
        /// </summary>
        public void Reload()
        {
            if (File.Exists(_fileLocation) == false)
            {
                throw new IOException($"Script.Reload operation has failed. File missing at '{_fileLocation}' !");
            }

            _lines.Clear();
            Load();
        }

        /// <summary>
        /// Reloads the script from given location. The script lines are overwritten.
        /// </summary>
        /// <param name="fileDirectory">a given file location (dictiopnary path).</param>
        public void Reload(string fileDirectory)
        {
            string newLocation = string.Empty;

            if ((fileDirectory == null) || (fileDirectory.Equals(string.Empty)))
            {
                throw new MissingFieldException("Script.Reload operation has failed. file directory missing.", "fileDirectory");
            }
            else
            {
                fileDirectory = fileDirectory.TrimEnd(new char[] { '\\' });
            }

            if( Directory.Exists(fileDirectory) == false)
            {
                throw new IOException($"Script.Reload operation has failed. Directory missing at '{fileDirectory}' !");
            }

            if (fileDirectory.Equals(_fileLocation) == false)
            {
                newLocation = fileDirectory + @"\" + this.FileName;

                if(File.Exists(newLocation) == false)
                {
                    throw new IOException($"Script.Reload operation has failed. File missing at '{newLocation}' !");
                }
                else
                {
                    _fileLocation = newLocation;
                }
            }

            this.Reload();
        }

        #endregion file operations

        #region inline inject operations
        /// <summary>
        /// Replace script tokens (parameters) from an external dictionary of key-value pairs.
        /// </summary>
        /// <exception cref="ArgumentException"> throws exception if parameters dictionary is missing.</exception>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <returns>Script text with tokens value injected in it.</returns>
        public string InjectTokens(Dictionary<string, string> externalTokens)
        {
            if (externalTokens == null) throw new ArgumentException("Missing dictionary of parameters at ScriptStash script inject tokens.", "parameters");

            // get all script text (if empty returns as string.Empty)
            string convText = this.Text;

            // loop and replace tokens with values
            if (externalTokens.Keys.Count > 0)
            {
                if (convText.Equals(string.Empty) == false)
                {
                    foreach (var param in externalTokens)
                    {
                        convText = convText.Replace(param.Key, param.Value);
                    }
                }
            }

            return convText;
        }

        /// <summary>
        /// Replace script tokens (parameters) from an external tokens dictionary (equal implementation of dictionary key-value pairs).
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <returns>Script text with tokens value injected in it.</returns>
        public string InjectTokens(Tokens externalTokens)
        {
            return this.InjectTokens(externalTokens as Dictionary<string, string>);
        }

        /// <summary>
        /// Replace script tokens (parameters) from an external of tokens set.
        /// </summary>
        /// <param name="tokensSet">List of tokens set.</param>
        /// <returns>List of script text transformations with tokens value injected in it from the tokens set.</returns>
        public List<string> InjectTokens(List<Tokens> tokensSet)
        {
            List<string> resultSet = new List<string>();

            // missing tokens set is not allowed
            if ((tokensSet==null) || (tokensSet.Count==0))
            {
                throw (new ArgumentException("Script.InjectTokens has failed. Tokens set can't be NULL/empty or not exist !", "tokensSet"));
            }

            foreach (var tokens in tokensSet)
            {
                resultSet.Add(this.InjectTokens(tokens as Dictionary<string, string>));
            }            

            return resultSet;
        }

        /// <summary>
        /// Replace script tokens (parameters) from an tokens file format (json/xml/csv) loaded as a Script
        /// </summary>
        /// <param name="tokensScript">A Script with tokens files loaded into it.</param>
        /// <returns>Script text with tokens value injected in it.</returns>
        public string InjectTokens(Script tokensScript)
        {
            string result = string.Empty;
            Tokens tokens = null;

            // missing tokens is not allowed
            if (tokensScript == null)
            {
                throw (new ArgumentException("Script.InjectTokens has failed. Tokens set can't be NULL/empty or not exist !", "tokensScript"));
            }

            switch (tokensScript.FileExtension)
            {
                case ".json":
                    tokens = Tokens.LoadJsonText(tokensScript.Text);
                    break;
                case ".xml":
                    tokens = Tokens.LoadXmlText(tokensScript.Text);
                    break;
                case ".csv":
                    tokens = Tokens.LoadCsvText(tokensScript.Text);
                    break;
                default:
                    throw new FormatException($"Fail using Script.InjectTokens(Script). File type ({tokensScript.FileName}) not supported. Please use only flat JSON,XML or CSV file formats as injection token files.");
            }

            if(tokens != null)
            {
                result = this.InjectTokens(tokens as Dictionary<string, string>);
            }

            return result;
        }

        /// <summary>
        /// Replace script tokens (parameters) from an external of tokens (as) scripts set.
        /// </summary>
        /// <param name="tokensScripts">List of tokens set (loaded as Scripts).</param>
        /// <returns>List of script text transformations with tokens value injected in it from the Scripts set.</returns>
        public List<string> InjectTokens(List<Script> tokensScripts)
        {
            List<string> results = null;

            // missing tokens set is not allowed
            if ((tokensScripts == null) || (tokensScripts.Count == 0))
            {
                throw (new ArgumentException("Script.InjectTokens has failed. Tokens-scripts list can't be NULL/empty or not exist !", "tokensScripts"));
            }
            else
            {
                results = new List<string>();
                foreach (Script tscrip in tokensScripts)
                {
                    results.Add(this.InjectTokens(tscrip));
                }
            }
            
            return results;
        }

        /// <summary>
        /// Replace script tokens (parameters) from ScriptStash inner dictionary of key-value pairs.
        /// </summary>
        /// <returns>Script text with tokens value injected in it.</returns>
        public string InjectTokens()
        {
            // get all script text (if empty returns as string.Empty)
            string convText = this.Text;

            // loop and replace tokens with values
            if ((_tokens != null) && (_tokens?.Keys.Count > 0))
            {
                if (convText.Equals(string.Empty) == false)
                {
                    foreach (var param in _tokens)
                    {
                        convText = convText.Replace(param.Key, param.Value);
                    }
                }
            }

            return convText;
        }
        #endregion inline inject operations
        #endregion
    }
}
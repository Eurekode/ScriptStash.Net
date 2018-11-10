using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ScriptStash
{
    /// <summary>
    /// A class for stashing in-memory scripts text for run-time reuse (load once and use many times).
    /// the Stash is also the root point to script line manipulation, tokens/parameters injections and save & reload operations. 
    /// </summary>ReLoadOnLineMatch
    public class Stash
    {
        #region enumarations
        /// <summary>
        /// Stash fill modes, used by the fill stash commands.
        /// Values : { APPEND, REFRESH }
        /// </summary>
        public enum FillStachMode { APPEND, REFRESH };
        #endregion enumarations

        #region variables
        /// <summary>
        /// Collection of inner script objects, stored with file name as the key.
        /// </summary>
        private Dictionary<string, Script> _scripts;

        /// <summary>
        /// Collection of inner tokens, stored as token (parameter) name as key with its dictionery string value.
        /// </summary>
        private Tokens _tokens;

        #endregion

        #region properties

        /// <summary>
        /// A get indexer for script object by its name as a key. If not found the return value is null.
        /// </summary>
        /// <param name="scriptName">A key script name. (Should be unique name !).</param>
        /// <returns>Script object.</returns>
        public Script this[string scriptName]
        {
            get
            {
                if (_scripts.ContainsKey(scriptName))
                {
                    return _scripts[scriptName];
                }
                else return null;
            }
        }

        /// <summary>
        /// A get property for all scripts as List<>
        /// </summary>
        public List<Script> Scripts
        {
            get { return _scripts.Values.ToList(); }
        }

        /// <summary>
        /// A get propery for the number of currently stashed scripts.
        /// </summary>
        public int ScriptsCount
        {
            get { return _scripts.Count; }
        }

        /// <summary>
        /// Inner string key-values dictionary for ScriptStash common dictionary usage.
        /// The tokens dictionary is shared by all ScriptStash auto-loaded scripts.
        /// </summary>
        public Tokens Tokens
        {
            get { return _tokens; }
        }

        #endregion properties

        #region Constructors
        /// <summary>
        /// Empty constructor.
        /// { Note : Use the FillStash(..) method to fill the stash. }
        /// </summary>
        public Stash()
        {
            _scripts = new Dictionary<string, Script>();
            _tokens = new Tokens();
        }

        /// <summary>
        /// Init constructor. Filles the script stash with scripts files.
        /// </summary>
        /// <param name="scriptsFolder">The scripts main location/folder.</param>
        /// <param name="pattern">The scripts file pattern (e.g. "*.vbs").</param>
        public Stash(string scriptsFolder, string pattern) : this()
        {
            // after invoke of default constructor, to init dictionaries, the stash is filled...
            FillStash(scriptsFolder, pattern, FillStachMode.APPEND);
        }

        /// <summary>
        /// Init constructor. Filles the script stash with scripts files.
        /// </summary>
        /// <param name="scriptsFolder">The scripts main location/folder.</param>
        /// <param name="patterns">Scripts file patterns list (e.g. '*.json', '*.sql', ...).</param>
        public Stash(string scriptsFolder, List<string> patterns) : this()
        {
            // after invoke of default constructor, to init dictionaries, the stash is filled...
            FillStash(scriptsFolder, patterns, FillStachMode.APPEND);
        }

        /// <summary>
        /// Init constructor. Filles the script stash with scripts files.
        /// </summary>
        /// <param name="scriptsFolders">The scripts files locations list.</param>
        /// <param name="pattern">The scripts file pattern (e.g. "*.sql").</param>
        public Stash(List<string> scriptsFolders, string pattern) : this()
        {
            // after invoke of default constructor, to init dictionaries, the stash is filled...
            FillStash(scriptsFolders, pattern, FillStachMode.APPEND);
        }

        /// <summary>
        /// Init constructor. Filles the script stash with scripts files.
        /// </summary>
        /// <param name="scriptsFolders">The scripts files locations list.</param>
        /// <param name="patterns">Scripts file patterns list (e.g. '*.vbs', '*.xml', ...).</param>
        public Stash(List<string> scriptsFolders, List<string> patterns) : this()
        {
            // after invoke of default constructor, to init dictionaries, the stash is filled...
            FillStash(scriptsFolders, patterns, FillStachMode.APPEND);
        }
        #endregion

        #region public functionality
        #region fill stash operations
        /// <summary>
        /// Fill the stash with all files from fixed file pattern (e.g. '*.sql') that can be found under given directory folder. <br />
        /// Example: <br />
        /// @code
        ///     Stash stash = new Stash();
        ///     stash.FillStash("C:\sql_scripts", "*.sql", Stash.FillStachMode.APPEND);
        /// @endcode
        /// </summary>
        /// <param name="scriptsFolder">The scripts files location.</param>
        /// <param name="pattern">The scripts files pattern (e.g. '*.vbs').</param>
        /// <param name="mode" cref="FillStachMode">APPEND - adds only new files to stash. REFRESH - adds all files to stash (existing files are reloaded from disk).</param>
        public void FillStash(string scriptsFolder, string pattern, FillStachMode mode)
        {
            if ( (scriptsFolder == null) || (scriptsFolder.Equals(string.Empty)) )
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Scripts folder missing.", "scriptsFolder");
            }

            if ((pattern == null) || (pattern.Equals(string.Empty)))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Pattern missing.", "pattern");
            }

            try
            {
                string[] filePaths = Directory.GetFiles(scriptsFolder, pattern, SearchOption.AllDirectories);
                List<string> fileNameKeys = null;

                if (filePaths.Length > 0)
                {
                    fileNameKeys = filePaths.Select(name => Path.GetFileName(name)).ToList();
                }

                foreach (var fileLoc in filePaths)
                {
                    string fileNameKey = Path.GetFileName(fileLoc);
                    
                    switch (mode)
                    {
                        case FillStachMode.APPEND:
                            if (_scripts.ContainsKey(fileNameKey) == false)
                            {
                                _scripts.Add(fileNameKey, new Script(fileLoc, ref _tokens));
                            }
                            break;
                        case FillStachMode.REFRESH:
                            if (_scripts.ContainsKey(fileNameKey) == true)
                            {
                                _scripts[fileNameKey].Reload(Path.GetDirectoryName(fileLoc));
                            }
                            else
                            {
                                _scripts.Add(fileNameKey, new Script(fileLoc, ref _tokens));
                            }
                            break;
                        default:
                            break;
                    }
                }

                // check for removed files no longer at scripts-folder location (only on refresh mode).
                if ((mode == FillStachMode.REFRESH) && (fileNameKeys != null))
                {
                    foreach (var scriptName in _scripts.Keys)
                    {
                        if (fileNameKeys.Contains(scriptName) == false)
                        {
                            _scripts.Remove(scriptName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Fill ScriptStash failed. Using folder location : '{scriptsFolder}', and pattern file : '{pattern}'. Fill mode set to '{mode.ToString()}'.", ex);
            }
        }

        /// <summary>
        /// Fill the stash with all files that has one of the given file patterns under the given directory folder.
        /// Example: <br />
        /// @code
        ///     var patterns = new List<string>() { "*.T01", "*.T02", "*.T03" };
        ///     Stash stash = new Stash();
        ///     stash.FillStash("C:\tax_files", patterns, Stash.FillStachMode.APPEND);
        /// @endcode
        /// </summary>
        /// <param name="scriptsFolder">The scripts files location.</param>
        /// <param name="patterns">Scripts file patterns list (e.g. '*.vbs', '*.sql', ...).</param>
        /// <param name="mode" cref="FillStachMode">APPEND - adds only new files to stash. REFRESH - adds all files to stash (existing files are reloaded from disk).</param>
        public void FillStash(string scriptsFolder, List<string> patterns, FillStachMode mode)
        {
            if ((scriptsFolder == null) || (scriptsFolder.Equals(string.Empty)))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Scripts folder missing.", "scriptsFolder");
            }

            if ((patterns == null) || (patterns.Count == 0))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Patterns missing.", "patterns");
            }

            foreach (string pattern in patterns)
            {
                this.FillStash(scriptsFolder, pattern, mode);
            }
        }

        /// <summary>
        /// Fill the stash with all files that has given pattern, found under one of the given directories list.
        /// Duplicate file names will be ignored. In such case, only first file is loaded on APPEND mode, and only last file is loaded on REFRESH mode.
        /// Example: <br />
        /// @code
        ///     var folders = new List<string>() { "C:\tax_files1", "C:\tax_files2" };
        ///     Stash stash = new Stash();
        ///     stash.FillStash(folders, "*.T??", Stash.FillStachMode.APPEND);
        /// @endcode
        /// </summary>
        /// <param name="scriptsFolders">The scripts files locations list.</param>
        /// <param name="pattern">The scripts files pattern (e.g. '*.sql').</param>
        /// <param name="mode" cref="FillStachMode">APPEND - adds only new files to stash. REFRESH - adds all files to stash (existing files are reloaded from disk).</param>
        public void FillStash(List<string> scriptsFolders, string pattern, FillStachMode mode)
        {
            if ((scriptsFolders == null) || (scriptsFolders.Count == 0))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Scripts folder missing.", "scriptsFolders");
            }

            if ((pattern == null) || (pattern.Equals(string.Empty)))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Pattern missing.", "pattern");
            }

            foreach (string folder in scriptsFolders)
            {
                this.FillStash(folder, pattern, mode);
            }
        }

        /// <summary>
        /// Fill the stash with all files that has one of the given patterns, and can be found under one of the given directories list.
        /// Duplicate file names will be ignored. In such case, only first file is loaded on APPEND mode, and only last file is loaded on REFRESH mode.
        /// Example: <br />
        /// @code
        ///     var patterns = new List<string>() { "*.T01", "*.T02", "*.T03" };
        ///     var folders = new List<string>() { "C:\tax_files1", "C:\tax_files2" };
        ///     Stash stash = new Stash();
        ///     stash.FillStash(folders, patterns, Stash.FillStachMode.APPEND);
        /// @endcode
        /// </summary>
        /// <param name="scriptsFolders">The scripts files locations list.</param>
        /// <param name="patterns">Scripts file patterns list (e.g. '*.vbs', '*.sql', ...).</param>
        /// <param name="mode" cref="FillStachMode">APPEND - adds only new files to stash. REFRESH - adds all files to stash (existing files are reloaded from disk).</param>
        public void FillStash(List<string> scriptsFolders, List<string> patterns, FillStachMode mode)
        {
            if ((scriptsFolders == null) || (scriptsFolders.Count == 0))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Scripts folder missing.", "scriptsFolders");
            }

            if ((patterns == null) || (patterns.Count == 0))
            {
                throw new ArgumentException("Stash.FillStash operation has failed. Patterns missing.", "patterns");
            }

            foreach (string folder in scriptsFolders)
            {
                foreach (string pattern in patterns)
                {
                    this.FillStash(folder, pattern, mode);
                }                    
            }
        }

        #endregion fill stash operations

        #region clear & add operations
        /// <summary>
        /// clears the stashed scripts dictionary.
        /// </summary>
        public void ClearScriptsStash()
        {
            _scripts.Clear();
        }

        /// <summary>
        /// Add new script object to stach.
        /// Example: <br />
        /// @code
        ///     Script sql_script = new Script("C:\sqls\script1.pl");
        ///     Stash sqlStash = new Stash();
        ///     sqlStash.FillStash("C:\sql_scripts", "*.sql");
        ///     sqlStash.Add(sql_script);
        /// @endcode
        /// </summary>
        /// <param name="script">script object.</param>
        public void Add(Script script)
        {
            if (script == null)
            {
                throw new ArgumentException("ScriptStash.Add operation has failed. NULL script object was used.", "script");
            }

            try
            {
                _scripts.Add(script.FileName, script);
            }
            catch (Exception ex)
            {
                throw new Exception($"ScriptStash.Add operation has failed. Using script obejct loaded from file : '{script.FileName}'.", ex);
            }
        }

        /// <summary>
        /// Add new script from file directory into the stash.  <br />
        /// Example: <br />
        /// @code
        ///     string sql_file = "C:\sqls\script1.pl";
        ///     Stash sqlStash = new Stash();
        ///     sqlStash.FillStash("C:\sql_scripts", "*.sql");
        ///     sqlStash.Add(sql_file);
        /// @endcode
        /// </summary>
        /// <param name="scriptLocation">The script file path+name</param>
        /// <param name="useShareTokens">TRUE - Use stash shared tokens dictionary. FALSE - Do not use it.</param>
        public void Add(string scriptLocation, bool useShareTokens = true)
        {
            if (File.Exists(scriptLocation) == false)
            {
                throw new FileNotFoundException("ScriptStash.Add operation has failed. Source script file not found/reached.", scriptLocation);
            }

            try
            {
                if (useShareTokens)
                {
                    _scripts.Add(Path.GetFileName(scriptLocation), new Script(scriptLocation, ref _tokens));
                }
                else
                {
                    _scripts.Add(Path.GetFileName(scriptLocation), new Script(scriptLocation));
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"ScriptStash.Add operation has failed. Using script location : '{scriptLocation}', and useShareTokens set to : '{useShareTokens}'.", ex);
            }
        }
        #endregion clear & add operations

        #region file operations
        /// <summary>
        /// Save all stash scripts. Overwrite each file at its loaded path location.
        /// </summary>
        public void SaveAll()
        {
            foreach(Script script in _scripts.Values)
            {
                script.Save();
            }
        }

        /// <summary>
        /// Save all stash scripts to a given target folder.
        /// Last saved location will be current position to all the stash files.
        /// </summary>
        /// <param name="targetFolder">target folder full path.</param>
        public void SaveToFolder(string targetFolder)
        {
            if ((targetFolder == null) || (targetFolder.Equals(string.Empty)))
            {
                throw new ArgumentException("ScriptStash.SaveToFolder operation has failed. NULL/Empty new folder path was used.", "newFolder");
            }

            if(Directory.Exists(targetFolder) == false)
            {
                throw new IOException($"ScriptStash.SaveToFolder operation has failed. Folder: '{targetFolder}' not found.");
            }

            foreach (Script script in _scripts.Values)
            {
                string newFileLocation = targetFolder.TrimEnd(new char[] { '\\' }) + "\\" + script.FileName;
                script.SaveAs(newFileLocation);
            }
        }

        /// <summary>
        /// Reload all stash script files from origian lication. Any not-saved script changes will be overwriten.  
        /// </summary>
        public void ReLoadAll()
        {
            foreach (Script script in _scripts.Values)
            {
                script.Reload();
            }
        }

        /// <summary>
        /// Save only stash scripts that match given script level condition.
        /// Last saved location will be current position to all the stash files.
        /// Example: <br />
        /// @code
        ///     Stash textStash = new Stash("C:\text_files", "*.txt");
        ///     string markLine = "A Mark Line !!";
        ///       :
        ///     textStash.SaveOnScriptMatch(script => script.Text.Contains(markLine)); 
        /// @endcode
        /// </summary>
        /// <param name="condition">A Script level condition.</param>
        public void SaveOnScriptMatch(Predicate<Script> condition)
        {
            if( condition==null )
            {
                throw new ArgumentException("ScriptStash.SaveOnScriptMatch operation has failed. Match condition missing.", "Condition");
            }

            foreach (Script script in _scripts.Values)
            {
                if (condition(script))
                {
                    script.Save();
                }
            }
        }

        /// <summary>
        /// Save only stash scripts that match given script level condition, into a given target folder.
        /// Last saved location will be current position to all the stash files.
        /// </summary>
        /// <param name="targetFolder">Target folder (full path).</param>
        /// <param name="condition">A Script level condition.</param>
        public void SaveToFolderOnScriptMatch(string targetFolder, Predicate<Script> condition)
        {
            if ((targetFolder == null) || (targetFolder.Equals(string.Empty)))
            {
                throw new ArgumentException("ScriptStash.SaveToFolderOnScriptMatch operation has failed. NULL/Empty new folder path was used.", "newFolder");
            }

            if (Directory.Exists(targetFolder) == false)
            {
                throw new IOException($"ScriptStash.SaveToFolderOnScriptMatch operation has failed. Folder: '{targetFolder}' not found.");
            }

            if (condition == null)
            {
                throw new ArgumentException("ScriptStash.SaveToFolderOnScriptMatch operation has failed. Match condition missing.", "Condition");
            }

            foreach (Script script in _scripts.Values)
            {
                if (condition(script))
                {
                    string newFileLocation = targetFolder.TrimEnd(new char[] { '\\' }) + "\\" + script.FileName;
                    script.SaveAs(newFileLocation);
                }                
            }
        }

        /// <summary>
        /// Reload, from origin path, only stash script files that match given script level condition.
        /// Any not-saved script changes will be overwriten.
        /// </summary>
        /// <param name="condition">A Script level condition.</param>
        public void ReLoadOnScriptMatch(Predicate<Script> condition)
        {
            if (condition == null)
            {
                throw new ArgumentException("ScriptStash.ReLoadOnScriptMatch operation has failed. Match condition missing.", "Condition");
            }

            foreach (Script script in _scripts.Values)
            {
                if (condition(script))
                {
                    script.Reload();
                }                
            }
        }

        /// <summary>
        /// Save only stash scripts that has one or more line matchs to given line level condition.
        /// Last saved location will be current position to all the stash files.
        /// </summary>
        /// <param name="condition">A line string level condition.</param>
        public void SaveOnLineMatch(Predicate<string> condition)
        {
            if (condition == null)
            {
                throw new ArgumentException("ScriptStash.SaveOnScriptMatch operation has failed. Match condition missing.", "Condition");
            }

            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    script.Save();
                }
            }
        }

        /// <summary>
        /// Save only stash scripts that has one or more line matchs to given script level condition, into a given target folder.
        /// Last saved location will be current position to all the stash files.
        /// </summary>
        /// <param name="targetFolder">Target folder (full path).</param>
        /// <param name="condition">A line string level condition.</param>
        public void SaveToFolderOnLineMatch(string targetFolder, Predicate<string> condition)
        {
            if ((targetFolder == null) || (targetFolder.Equals(string.Empty)))
            {
                throw new ArgumentException("ScriptStash.SaveToFolder operation has failed. NULL/Empty new folder path was used.", "newFolder");
            }

            if (Directory.Exists(targetFolder) == false)
            {
                throw new IOException($"ScriptStash.SaveToFolder operation has failed. Folder: '{targetFolder}' not found.");
            }

            if (condition == null)
            {
                throw new ArgumentException("ScriptStash.SaveOnScriptMatch operation has failed. Match condition missing.", "Condition");
            }

            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    string newFileLocation = targetFolder.TrimEnd(new char[] { '\\' }) + "\\" + script.FileName;
                    script.SaveAs(newFileLocation);
                }
            }
        }

        /// <summary>
        /// Reload, from origin path, only stash script files that has one or more line matchs to given script level condition.
        /// Any not-saved script changes will be overwriten.
        /// </summary>
        /// <param name="condition">A line string level condition.</param>
        public void ReLoadOnLineMatch(Predicate<string> condition)
        {
            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    script.Reload();
                }
            }
        }
        #endregion file operations

        #region multi inject operations
        /// <summary>
        /// Replace script tokens (parameters) from external dictionary of string key-value pairs apply to all scripts.
        /// Example: <br />
        /// @code
        ///     // sql files format at folder `C:\sql_files`, is :
        ///     //      ``SELECT [CSV_FIELDS] FROM USERS WHERE[USERS_CONDITION]``
        ///     
        ///     string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
        ///     string sql_condition_str = "USER_GENDER = 'MALE'";
        ///     Stash stash = new Stash("C:\sql_files", "*.sql");
        ///     Dictionary<string, string> tokensDict = new Dictionary<string, string>();
        ///     tokensDict.Add("[CSV_FIELDS]", sql_fields_list);
        ///     tokensDict.Add("[USERS_CONDITION]", sql_condition_str);
        ///     Dictionary<string, string> queries = stash.InjectTokens(tokensDict);
        /// @endcode
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokens(Dictionary<string, string> externalTokens)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            foreach (Script script in _scripts.Values)
            {
                convDict.Add(script.FileName, script.InjectTokens(externalTokens));
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from external dictionary of tokens dictionaty apply to all scripts.
        /// Example: <br />
        /// @code
        ///     // sql files format at folder `C:\sql_files`, is :
        ///     //      ``SELECT [CSV_FIELDS] FROM USERS WHERE[USERS_CONDITION]``
        ///     
        ///     string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
        ///     string sql_condition_str = "USER_GENDER = 'FEMALE'";
        ///     Stash stash = new Stash("C:\sql_files", "*.sql");
        ///     Tokens tokensDict = new Tokens();
        ///     tokensDict["[CSV_FIELDS]"] = sql_fields_list;
        ///     tokensDict["[USERS_CONDITION]"] = sql_condition_str;
        ///     Dictionary<string, string> queries = stash.InjectTokens(tokensDict); 
        /// @endcode
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokens(Tokens externalTokens)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            foreach (Script script in _scripts.Values)
            {
                convDict.Add(script.FileName, script.InjectTokens(externalTokens));
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from ScriptStash inner dictionary of key-value pairs apply to all scripts.
        /// Example: <br />
        /// @code
        ///     // sql files format at folder `C:\sql_files`, is :
        ///     //      ``SELECT [CSV_FIELDS] FROM USERS WHERE[USERS_CONDITION]``
        ///     
        ///     string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\", USER_AGE AS \"Age\"";
        ///     string sql_condition_str = "USER_AGE > 17";
        ///     Stash stash = new Stash("C:\sql_files", "*.sql");
        ///     stash.Tokens["[CSV_FIELDS]"] = sql_fields_list;
        ///     stash.Tokens["[USERS_CONDITION]"] = sql_condition_str;
        ///     Dictionary<string, string> queries = stash.InjectTokens();
        /// @endcode
        /// </summary>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokens()
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            foreach(Script script in _scripts.Values)
            {
                convDict.Add(script.FileName, script.InjectTokens());
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from external dictionary of string key-value pairs apply to scripts that meet given condition.
        /// 
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <param name="condition">A predicate function over Script.</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnScriptMatch(Dictionary<string, string> externalTokens, Predicate<Script> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if(condition(script))
                {
                    convDict.Add(script.FileName, script.InjectTokens(externalTokens));
                }                
            }

            return convDict;
        }

        /// <summary>
        ///  Replace script tokens (parameters) from external dictionary of tokens dictionaty apply to scripts that meet given condition.
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <param name="condition">A predicate function over Script.</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnScriptMatch(Tokens externalTokens, Predicate<Script> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if (condition(script))
                {
                    convDict.Add(script.FileName, script.InjectTokens(externalTokens));
                }
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from ScriptStash inner dictionary of key-value pairs apply to scripts that meet given condition.
        /// </summary>
        /// <param name="condition">A predicate function over Script.</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnScriptMatch(Predicate<Script> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if (condition(script))
                {
                    convDict.Add(script.FileName, script.InjectTokens());
                }
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from external dictionary of string key-value pairs apply to scripts that has at least one line that meet given condition.
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <param name="condition">A predicate function over script line (string).</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnLineMatch(Dictionary<string, string> externalTokens, Predicate<string> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    convDict.Add(script.FileName, script.InjectTokens(externalTokens));
                }
            }

            return convDict;
        }

        /// <summary>
        ///  Replace script tokens (parameters) from external dictionary of tokens dictionaty apply to scripts that has at least one line that meet given condition.
        /// </summary>
        /// <param name="externalTokens">Parameters dictionary [parameter name as key, parameter string as value].</param>
        /// <param name="condition">A predicate function over Script.</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnLineMatch(Tokens externalTokens, Predicate<string> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    convDict.Add(script.FileName, script.InjectTokens(externalTokens));
                }
            }

            return convDict;
        }

        /// <summary>
        /// Replace script tokens (parameters) from ScriptStash inner dictionary of key-value pairs apply to scripts that has at least one line that meet given condition.
        /// Example: <br />
        /// @code
        ///     // sql files format at folder `C:\sql_files`, is :
        ///     //      ``SELECT [CSV_FIELDS] FROM USERS WHERE[USERS_CONDITION]``
        ///     
        ///     string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\", USER_AGE AS \"Age\"";
        ///     string sql_condition_str = "USER_AGE > 22";
        ///     Stash stash = new Stash("C:\sql_files", "*.sql");
        ///     stash.Tokens["[CSV_FIELDS]"] = sql_fields_list;
        ///     stash.Tokens["[USERS_CONDITION]"] = sql_condition_str;
        ///     Dictionary<string, string> queries = stash.InjectTokensOnLineMatch(line => line.Contains("FROM"));
        /// @endcode
        /// </summary>
        /// <param name="condition">A predicate function over Script.</param>
        /// <returns>Text dictionary with script names as keys and converted text as values.</returns>
        public Dictionary<string, string> InjectTokensOnLineMatch(Predicate<string> condition)
        {
            Dictionary<string, string> convDict = new Dictionary<string, string>();

            // missing condition not allowed
            if (condition == null)
            {
                throw (new ArgumentException("Condition predicate can't be NULL !", "condition"));
            }

            foreach (Script script in _scripts.Values)
            {
                if (script.Lines.FindIndex(condition) >= 0)
                {
                    convDict.Add(script.FileName, script.InjectTokens());
                }
            }

            return convDict;
        }
        #endregion multi inject operations
        #endregion public functionality
    }
}

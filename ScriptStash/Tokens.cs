using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
using System.Text.Json;
#elif NET40_OR_GREATER
using System.Web.Script.Serialization;
#endif

namespace ScriptStash
{
    /// <summary>
    /// This class inherites Dictionary of strings and replace the indexer functionality
    /// to support string tokens, key-value, dictionary storage.
    /// </summary>
    public class Tokens : Dictionary<string, string>
    {
#region properties
        /// <summary>
        /// hides inherited indexr property.
        /// Get value by key if exist. Do not fail whan key not exist (return empty string).
        /// Set by overwrite a known existing key with value. Or, if key not exist, add its
        /// key-value to the dictionary.
        /// </summary>
        /// <param name="key">the token key name.</param>
        /// <returns>String value</returns>
        public new string this[string key]
        {
            get
            {
                string result = string.Empty;

                if (base.ContainsKey(key))
                {
                    result = base[key];
                }

                return result;
            }

            set
            {
                if (base.ContainsKey(key))
                {
                    base[key] = value;
                }
                else
                {
                    base.Add(key, value);
                }
            }
        }

#endregion properties

#region Constructors
        /// <summary>
        /// Default empty constructor.
        /// </summary>
        public Tokens() : base() { }

        /// <summary>
        /// Copy constructor from base class instance.
        /// </summary>
        /// <param name="dictionary">Source dictionary of key:values to build the new tokens object.</param>
        public Tokens(Dictionary<string, string> dictionary) : base(dictionary) { }

        /// <summary>
        /// Copy constructor from other Tokens class instance.
        /// </summary>
        /// <param name="tokens">Source tokens of key:values to build the new tokens object.</param>
        public Tokens(Tokens tokens) : base(tokens as Dictionary<string, string>) { }
#endregion Constructors

#region public methods
#region operators
        /// <summary>
        /// Append new token key:value pairs from another tokens instance.
        /// </summary>
        /// <param name="tokens">Tokens to be appended into target Tokens object.</param>
        /// <returns>Number of tokens added by the append operation.</returns>
        public int Append(Tokens tokens)
        {
            int result = 0;

            // append to current instance
            foreach (var item in tokens)
            {
                if (this.Keys.Contains(item.Key) && !this[item.Key].Equals(item.Value)) { result++; }
                this[item.Key] = item.Value;
            }

            return result;
        }
#endregion operators

#region json Serialization/Deserialization factory
        /// <summary>
        /// Static JSON parsing factory tool. Source file format should be simple flat key:value JSON string. <br />
        /// Example:                        <br />
        /// @code
        /// {
        ///     "key1" : "value1",
        ///     "key2" : "value2",
        ///     "key3" : "value3"           <br />
        /// }
        /// @endcode
        /// </summary>
        /// <param name="json">The json string syntax.</param>
        /// <returns>Tokens object with the key:value pairs, found in text.</returns>
        public static Tokens LoadJsonText(string json)
        {
            Tokens tokens = null;

            if ((json == null) || (json.Equals(string.Empty)) )
            {
                throw (new ArgumentException("Tokens.LoadJsonText has failed. Json text string can't be NULL/empty or not exist ! Use correct json syntax.", "json"));
            }

            // convert json string
            try
            {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
                tokens = JsonSerializer.Deserialize<Tokens>(json);
#elif NET40_OR_GREATER
                var jss = new JavaScriptSerializer();
                tokens = jss.Deserialize<Tokens>(json);
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadJsonText has failed. Please check input json syntax.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Static factory deserialization tool. Source file format should be simple flat key:value json file. <br />
        /// Example:                        <br />
        /// @code
        /// {
        ///     "key1" : "value1",
        ///     "key2" : "value2",
        ///     "key3" : "value3"
        /// }
        /// @endcode
        /// </summary>
        /// <param name="file">Full path of json source file name.</param>
        /// <returns>Tokens object with the file key:value pairs.</returns>
        public static Tokens LoadJson(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.LoadJson has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            // load json file
            try
            {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
                tokens = JsonSerializer.Deserialize<Tokens>(File.ReadAllText(file));
#elif NET40_OR_GREATER
                var jss = new JavaScriptSerializer();
                tokens = jss.Deserialize<Tokens>(File.ReadAllText(file));
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadJson has failed. Input file at : '{file}'.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Static factory deserialization tool. Source file format should be a file with array of key:value json objects. <br />
        /// Example:                            <br />
        /// @code
        /// [                                   
        ///     {                               
        ///         "key1" : "value1",          
        ///         "key2" : "value2",          
        ///         "key3" : "value3"           
        ///     },
        ///     {                               
        ///         "key1" : "val_A",           
        ///         "key2" : "val_B",           
        ///         "key3" : "val_C"            
        ///     }                               
        /// ]
        /// @endcode
        /// </summary>
        /// <param name="file">Full path of json source file name.</param>
        /// <returns>Tokens object list, each with the file key:value pairs.</returns>
        public static List<Tokens> LoadJsonArray(string file)
        {
            List<Tokens> tokensArray = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.LoadJsonArray has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            // load json file
            try
            {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
                tokensArray = JsonSerializer.Deserialize<List<Tokens>>(File.ReadAllText(file));
#elif NET40_OR_GREATER
                var jss = new JavaScriptSerializer();
                tokensArray = jss.Deserialize<List<Tokens>>(File.ReadAllText(file));
#endif

            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadJsonArray has failed. Input file at : '{file}'.", ex);
            }

            return tokensArray;
        }

        /// <summary>
        /// Append new token key:value pairs from json file.
        /// </summary>
        /// <param name="file">Full path of json source file name.</param>
        /// <returns>Total count of added or updated token key values.</returns>
        public int AppendJson(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.AppendJson has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            try
            {
                // load new tokens file
                tokens = Tokens.LoadJson(file);
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadJson has failed. Input file at : '{file}'.", ex);
            }

            // append to current instance and return append count
            return this.Append(tokens);
        }

        /// <summary>
        /// Convert current key:value dictionaty pairs into a json format string.
        /// </summary>
        /// <returns>Json string.</returns>
        public string ToJson()
        {
            string jsonText = string.Empty;

            try
            {
#if NETCOREAPP3_0_OR_GREATER || NETSTANDARD2_0_OR_GREATER
                jsonText = JsonSerializer.Serialize(this);
#elif NET40_OR_GREATER
                var jss = new JavaScriptSerializer();
                jsonText = jss.Serialize(this);
#endif
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.ToJson has failed. inner error : '{ex.Message}'.", ex);
            }

            return jsonText;        
        }

        /// <summary>
        /// Export serialization tool. Target file save inner key:value pairs into json file.
        /// </summary>
        /// <param name="file">Full path of json target file name.</param>
        public void SaveJson(string file)
        {
            // missing file not allowed
            if ((file == null) ||  (file.Equals(string.Empty)))
            {
                throw (new ArgumentException("Tokens.SaveJson has failed. File can't be NULL/empty ! Use full path file name.", "file"));
            }
            // save json file
            try
            {
                File.WriteAllText(file, this.ToJson());
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.SaveJson has failed. inner error : '{ex.Message}'.", ex);
            }
        }
#endregion json Serialization/Deserialization factory

#region xml Serialization/Deserialization factory
        /// <summary>
        /// Static XML parsing factory tool. Source file format should be simple flat key:value XML file. <br />
        /// Example:                    <br />
        /// @code
        /// <root>                     
        ///     <key1>value1</key1>     
        ///     <key2>value2</key2>     
        ///     <key3>value3</key3>     
        /// </root>
        /// @endcode
        /// </summary>
        /// <param name="xml">The XML string syntax.</param>
        /// <returns>Tokens object with the key:value pairs, found in text.</returns>
        public static Tokens LoadXmlText(string xml)
        {
            Tokens tokens = null;

            if ((xml == null) || (xml.Equals(string.Empty)))
            {
                throw (new ArgumentException("Tokens.LoadXmlText has failed. XML text string can't be NULL/empty or not exist ! Use correct XML syntax.", "xml"));
            }

            // convert json string
            try
            {
                XElement xmlRoot = XElement.Parse(xml);
                tokens = new Tokens(xmlRoot.Elements().ToDictionary(key => key.Name.ToString(), val => val.Value));
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadXmlText has failed. Please check input XML syntax.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Static factory deserialization tool. Source file format should be simple flat key:value xml file. <br />
        /// Example:                    <br />
        /// @code
        /// <root>                      
        ///     <key1>value1</key1>     
        ///     <key2>value2</key2>     
        ///     <key3>value3</key3>     
        /// </root>
        /// @endcode
        /// </summary>
        /// <param name="file">Full path of XML source file name.</param>
        /// <returns>Tokens object with the file key:value pairs.</returns>
        public static Tokens LoadXml(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.LoadXml has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }
            // load xml file
            try
            {
                XElement xmlRoot = XElement.Load(file);
                tokens = new Tokens(xmlRoot.Elements().ToDictionary(key => key.Name.ToString(), val => val.Value));
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadXml has failed. Input file at : '{file}'.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Static factory deserialization tool. Source file format should be a file with array of key:value json objects. <br />
        /// Example:                        <br />
        /// @code
        /// <root>                          
        ///     <tokens>                    
        ///         <key1>value1</key1>     
        ///         <key2>value2</key2>     
        ///         <key3>value3</key3>     
        ///     </tokens>                   
        ///     <tokens>                    
        ///         <key1>value1</key1>     
        ///         <key2>value2</key2>     
        ///         <key3>value3</key3>     
        ///     </tokens>                   
        /// </root>
        /// @endcode
        /// </summary>
        /// <param name="file">Full path of XML source file name.</param>
        /// <returns>Tokens object List. Each with the file key:value pairs.</returns>
        public static List<Tokens> LoadXmlArray(string file)
        {
            List<Tokens> tokensArray = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.LoadXmlArray has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            // load xml file
            try
            {
                XElement xmlRoot = XElement.Load(file);
                tokensArray = xmlRoot.Elements().Select(tokns => new Tokens( 
                        tokns.Elements().ToDictionary(key => key.Name.ToString(), val => val.Value)
                    )).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadXmlArray has failed. Input file at : '{file}'.", ex);
            }

            return tokensArray;
        }

        /// <summary>
        /// Append new token key:value pairs from XML file.
        /// </summary>
        /// <param name="file">Full path of XML source file name.</param>
        /// <returns>Total count of added or updated token key values.</returns>
        public int AppendXml(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.AppendXml has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            try
            {
                // load new tokens file
                tokens = Tokens.LoadXml(file);
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadXml has failed. Input file at : '{file}'.", ex);
            }

            // append to current instance and return append count
            return this.Append(tokens);
        }

        /// <summary>
        /// Convert current key:value dictionaty pairs into a xml format string.
        /// </summary>
        /// <returns>Json string.</returns>
        public string ToXml()
        {
            XElement xmlRoot = null;
            string xmlText = string.Empty;

            try
            {
                xmlRoot = new XElement("root", this.Select(el => new XElement(el.Key, el.Value)));
                xmlText = xmlRoot.ToString();
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.ToXml has failed. inner error : '{ex.Message}'.", ex);
            }

            return xmlText;
        }

        /// <summary>
        /// Export serialization tool. Target file save inner key:value pairs into xml file.
        /// </summary>
        /// <param name="file">Full path of XML target file name.</param>
        public void SaveXml(string file)
        {
            // missing file not allowed
            if ((file == null) || (file.Equals(string.Empty)))
            {
                throw (new ArgumentException("Tokens.SaveXml has failed. File can't be NULL/empty ! Use full path file name.", "file"));
            }
            // save xml file
            try
            {
                File.WriteAllText(file, this.ToXml());
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.SaveXml has failed. inner error : '{ex.Message}'.", ex);
            }
        }

#endregion xml Serialization/Deserialization factory

#region csv Serialization/Deserialization factory
        /// <summary>
        /// Static CSV parsing factory tool. Source file format should be simple flat key:value CSV file. <br />
        /// Example:                <br />
        /// @code
        ///     "key1","value1"     
        ///     "key2","value2"     
        ///     "key3","value3"     
        /// @endcode    
        /// </summary>
        /// <param name="csv">The CSV string syntax.</param>
        /// <returns>Tokens object with the key:value pairs, found in text.</returns>
        public static Tokens LoadCsvText(string csv)
        {
            Tokens tokens = null;

            if ((csv == null) || (csv.Equals(string.Empty)))
            {
                throw (new ArgumentException("Tokens.LoadCsvText has failed. CSV text string can't be NULL/empty or not exist ! Use correct CSV syntax.", "csv"));
            }

            // convert CSV string
            try
            {
                IEnumerable<string> csvLines = csv.Split('\n').Select(line => line.Trim()).ToList();
                tokens = new Tokens(csvLines.Select(line => line.Split(',')).ToDictionary(line => line[0].Replace("\"", ""), line => line[1].Replace("\"", "")));
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadCsvText has failed. Please check input CSV syntax.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Static factory deserialization tool. Source file format should be simple flat key:value CSV file. <br />
        /// Example:                <br />
        /// @code
        ///     "key1","value1"     
        ///     "key2","value2"     
        ///     "key3","value3"     
        /// @endcode
        /// </summary>
        /// <param name="file">Full path of CSV source file name.</param>
        /// <returns>Tokens object with the file key:value pairs.</returns>
        public static Tokens LoadCsv(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.LoadCsv has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }
            // load xml file
            try
            {
                IEnumerable<string> csvLines = File.ReadLines(file);
                tokens = new Tokens(csvLines.Select(line => line.Split(',')).ToDictionary(line => line[0].Replace("\"", "") , line => line[1].Replace("\"", "") ));
                
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadCsv has failed. Input file at : '{file}'.", ex);
            }

            return tokens;
        }

        /// <summary>
        /// Append new token key:value pairs from CSV file.
        /// </summary>
        /// <param name="file">Full path of CSV source file name.</param>
        /// <returns>Total count of added or updated token key values.</returns>
        public int AppendCsv(string file)
        {
            Tokens tokens = null;

            // missing file not allowed
            if ((file == null) ||
                (file.Equals(string.Empty)) ||
                (File.Exists(file) == false))
            {
                throw (new ArgumentException("Tokens.AppendCsv has failed. File can't be NULL/empty or not exist ! Use full path file name.", "file"));
            }

            try
            {
                // load new tokens file
                tokens = Tokens.LoadCsv(file);
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.LoadCsv has failed. Input file at : '{file}'.", ex);
            }

            // append to current instance and return append count
            return this.Append(tokens);
        }

        /// <summary>
        /// Convert current key:value dictionaty pairs into a CSV format string.
        /// </summary>
        /// <returns>CSV string.</returns>
        public string ToCsv()
        {
            string csvText = string.Empty;

            try
            {
                csvText = string.Join(Environment.NewLine, this.Select(pair => "\"" + pair.Key + "\",\"" + pair.Value + "\""));
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.ToCsv has failed. inner error : '{ex.Message}'.", ex);
            }

            return csvText;
        }

        /// <summary>
        /// Export serialization tool. Target file save inner key:value pairs into CSV file.
        /// </summary>
        /// <param name="file">Full path of CSV target file name.</param>
        public void SaveCsv(string file)
        {
            // missing file not allowed
            if ((file == null) || (file.Equals(string.Empty)))
            {
                throw (new ArgumentException("Tokens.SaveCsv has failed. File can't be NULL/empty ! Use full path file name.", "file"));
            }
            // save csv file
            try
            {
                File.WriteAllText(file, this.ToCsv());
            }
            catch (Exception ex)
            {
                throw new Exception($"Tokens.SaveCsv has failed. inner error : '{ex.Message}'.", ex);
            }
        }

#endregion csv Serialization/Deserialization factory
#endregion public methods
    }
}

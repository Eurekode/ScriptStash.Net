using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptStash;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ScriptStashUnitTests
{
    [TestClass()]
    public class StashTests
    {
        private const string SCRIPTS_FOLDER = @"\scripts";
        private const string SCRIPTS_FOLDER2 = @"\scripts_2";
        private const string SCRIPTS_FOLDER3 = @"\scripts_3";
        private const string SQL_PATTERN = "*.sql";
        private const string VBS_PATTERN = "*.vbs";
        private const string TXT_PATTERN = "*.txt";
        private const string REST_PATTERN = "*.rest";
        private const string ALL_PATTERN = "*.*";
        private const string TestFileName1 = "file1.txt";
        private const string TestFileName2 = "file2.txt";
        private const string TestFileName3 = "file3.txt";
        private const string PERL_SEND_MAIL_SAMPLE = "perlMailSendExample.pl";
        private const string SQL_GET_ALL_USERS_ADDRESS_SAMPLE = "sqlGetAllUsersAddressWhere.sql";
        private const string SQL_GET_ALL_USERS_PHONE_SAMPLE = "sqlGetAllUsersPhoneWhere.sql";
        private const string SQL_GET_ALL_USERS_SAMPLE = "sqlGetAllUsersWhere.sql";
        private const string JSON_TOKENS_SCRIPT_FILE = "sql_tokens_script.json";
        private const string JSON_TOKENS_SET_FILE = "sql_tokens_set.json";
        private const string REST_REQUEST_FILE = "metaweather_request.rest";
        private const string XML_TEST_SET = "test_set.xml";
        private const string TOKEN_CSV_FIELDS = "[CSV_FIELDS]";
        private const string TOKEN_USERS_CONDITION = "[USERS_CONDITION]";
        private string _scriptsLocation1;
        private string _scriptsLocation2;
        private string _scriptsLocation3;
        private string _TestFileDelPath1;
        private string _TestFileDelPath2;
        private string _TestFileDelPath3;

        public StashTests()
        {
            string execDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _scriptsLocation1 = execDirectory + SCRIPTS_FOLDER;
            _scriptsLocation2 = execDirectory + SCRIPTS_FOLDER2;
            _scriptsLocation3 = execDirectory + SCRIPTS_FOLDER3;
            _TestFileDelPath1 = _scriptsLocation3 + "\\" + TestFileName1;
            _TestFileDelPath2 = _scriptsLocation3 + "\\" + TestFileName2;
            _TestFileDelPath3 = _scriptsLocation3 + "\\" + TestFileName3;
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Sanity")]
        public void ScriptStashEmptyConstructorTest()
        {
            Stash stash = new Stash();
            Assert.AreEqual(stash.ScriptsCount, 0);
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Sanity")]
        public void ScriptStashConstructorTest()
        {
            Stash stash = new Stash(_scriptsLocation1, SQL_PATTERN);
            Assert.AreEqual(stash.ScriptsCount, 3);
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Sanity")]
        public void ScriptStashFillPatternsConstructorTest()
        {
            List<string> patterns = new List<string>() { SQL_PATTERN, VBS_PATTERN};
            Stash stash = new Stash(_scriptsLocation2, patterns);
            Assert.AreEqual(stash.ScriptsCount, 5);
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Sanity")]
        public void ScriptStashFillFoldersConstructorTest()
        {
            List<string> folders = new List<string>() { _scriptsLocation1, _scriptsLocation2 };
            Stash stash = new Stash(folders, SQL_PATTERN);
            Assert.AreEqual(stash.ScriptsCount, 6);
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Sanity")]
        public void ScriptStashFillFoldersPatternsConstructorTest()
        {
            List<string> folders = new List<string>() { _scriptsLocation1, _scriptsLocation2 };
            List<string> patterns = new List<string>() { SQL_PATTERN, VBS_PATTERN };
            Stash stash = new Stash(folders, patterns);
            Assert.AreEqual(stash.ScriptsCount, 8);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashTest()
        {
            Stash stash = new Stash();
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 3);
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 3);
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.REFRESH);
            Assert.AreEqual(stash.ScriptsCount, 3);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashDifferentPatternsTest()
        {
            var patterns = new List<string>() { SQL_PATTERN, VBS_PATTERN };
            Stash stash = new Stash();
            stash.FillStash(_scriptsLocation1, patterns, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 4);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashDifferentFoldersTest()
        {
            var locations = new List<string>() { _scriptsLocation1, _scriptsLocation1 + "_2" };
            Stash stash = new Stash();
            stash.FillStash(locations, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 6);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashDifferentFoldersPatternsTest()
        {
            var locations = new List<string>() { _scriptsLocation1, _scriptsLocation2 };
            var patterns = new List<string>() { SQL_PATTERN, VBS_PATTERN };
            Stash stash = new Stash();
            stash.FillStash(locations, patterns, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 8);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashDuplicateFilesAppendTest()
        {
            string dupFileName = "vbsScriptSample.vbs";
            var locations = new List<string>() { _scriptsLocation1, _scriptsLocation1 + "_2" };
            Stash stash = new Stash();
            stash.FillStash(locations, VBS_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 2);
            int findIndex = stash[dupFileName].FirstLineIndexWhere(line => line.Contains("Duplicate"));
            Assert.AreEqual(findIndex, -1);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void FillStashDuplicateFilesRefreshTest()
        {
            string dupFileName = "vbsScriptSample.vbs";
            var locations = new List<string>() { _scriptsLocation1, _scriptsLocation1 + "_2" };
            Stash stash = new Stash();
            stash.FillStash(locations, VBS_PATTERN, Stash.FillStachMode.REFRESH);
            Assert.AreEqual(stash.ScriptsCount, 2);
            int findIndex = stash[dupFileName].FirstLineIndexWhere(line => line.Contains("Duplicate"));
            Assert.IsTrue(findIndex > -1);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void ClearScriptsStashTest()
        {
            Stash stash = new Stash();
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 3);
            stash.ClearScriptsStash();
            Assert.AreEqual(stash.ScriptsCount, 0);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void AddScriptTest()
        {
            Stash stash = new Stash();
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 3);
            
            Script perlCode = new Script(_scriptsLocation1 + "\\" + PERL_SEND_MAIL_SAMPLE);
            stash.Add(perlCode);
            Assert.AreEqual(stash.ScriptsCount, 4);
        }

        [TestMethod()]
        [TestCategory("Stash Class")]
        public void AddFileTest()
        {
            Stash stash = new Stash();
            stash.FillStash(_scriptsLocation1, SQL_PATTERN, Stash.FillStachMode.APPEND);
            Assert.AreEqual(stash.ScriptsCount, 3);

            stash.Add(_scriptsLocation1 + @"\" + PERL_SEND_MAIL_SAMPLE);
            Assert.AreEqual(stash.ScriptsCount, 4);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensExternalDictionaryTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'MALE'";
            Stash stash = new Stash(_scriptsLocation1, SQL_PATTERN);
            Dictionary<string, string> tokensDict = new Dictionary<string, string>();
            tokensDict.Add(TOKEN_CSV_FIELDS, sql_fields_list);
            tokensDict.Add(TOKEN_USERS_CONDITION, sql_condition_str);

            Dictionary<string, string> queries = stash.InjectTokens(tokensDict);

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensExternalTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'FEMALE'";
            Stash stash = new Stash(_scriptsLocation1, SQL_PATTERN);
            Tokens tokensDict = new Tokens();
            tokensDict[TOKEN_CSV_FIELDS] = sql_fields_list;
            tokensDict[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokens(tokensDict);

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensInnerTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\", USER_AGE AS \"Age\"";
            string sql_condition_str = "USER_AGE > 17";
            Stash stash = new Stash(_scriptsLocation1, SQL_PATTERN);
            stash.Tokens[TOKEN_CSV_FIELDS] = sql_fields_list;
            stash.Tokens[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokens();

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensScriptConditionedExternalDictionaryTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'MALE'";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            Dictionary<string, string> tokensDict = new Dictionary<string, string>();
            tokensDict.Add(TOKEN_CSV_FIELDS, sql_fields_list);
            tokensDict.Add(TOKEN_USERS_CONDITION, sql_condition_str);

            Dictionary<string, string> queries = stash.InjectTokensOnScriptMatch(tokensDict, script => script.FileExtension.Equals(".sql"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensScriptConditionedExternalTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'FEMALE'";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            Tokens tokensDict = new Tokens();
            tokensDict[TOKEN_CSV_FIELDS] = sql_fields_list;
            tokensDict[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokensOnScriptMatch(tokensDict, script => script.FileName.StartsWith("sqlGet"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensScriptConditionedInnerTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\", USER_AGE AS \"Age\"";
            string sql_condition_str = "USER_AGE > 17";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            stash.Tokens[TOKEN_CSV_FIELDS] = sql_fields_list;
            stash.Tokens[TOKEN_USERS_CONDITION] = "USER_AGE < 100";
            stash.Tokens[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokensOnScriptMatch(script => script.FileExtension.Equals(".sql"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensLineConditionedExternalDictionaryTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'MALE'";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            Dictionary<string, string> tokensDict = new Dictionary<string, string>();
            tokensDict.Add(TOKEN_CSV_FIELDS, sql_fields_list);
            tokensDict.Add(TOKEN_USERS_CONDITION, sql_condition_str);

            Dictionary<string, string> queries = stash.InjectTokensOnLineMatch(tokensDict, line => line.StartsWith("SELECT"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensLineConditionedExternalTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\"";
            string sql_condition_str = "USER_GENDER = 'FEMALE'";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            Tokens tokensDict = new Tokens();
            tokensDict[TOKEN_CSV_FIELDS] = sql_fields_list;
            tokensDict[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokensOnLineMatch(tokensDict, line => line.Contains("FROM"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensLineConditionedInnerTokensTest()
        {
            string sql_fields_list = "USER_NAME AS \"Name\", USER_COUNTRY AS \"Country\", USER_AGE AS \"Age\"";
            string sql_condition_str = "USER_AGE > 22";
            Stash stash = new Stash(_scriptsLocation1, ALL_PATTERN);
            Assert.AreNotEqual(stash.ScriptsCount, 3);

            stash.Tokens[TOKEN_CSV_FIELDS] = sql_fields_list;
            stash.Tokens[TOKEN_USERS_CONDITION] = "USER_AGE < 400";
            stash.Tokens[TOKEN_USERS_CONDITION] = sql_condition_str;

            Dictionary<string, string> queries = stash.InjectTokensOnLineMatch(line => line.Contains("FROM"));

            Assert.AreEqual(queries.Count, 3);
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_ADDRESS_SAMPLE].Contains(sql_condition_str));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_fields_list));
            Assert.IsTrue(queries[SQL_GET_ALL_USERS_PHONE_SAMPLE].Contains(sql_condition_str));

            int countLines = queries[SQL_GET_ALL_USERS_SAMPLE].Count(c => c == '\n') + 1;
            Assert.AreEqual(countLines, 3);
        }


        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectJsonTokensArray()
        {
            List<Tokens> tokensSet = Tokens.LoadJsonArray(_scriptsLocation1 + "\\" + JSON_TOKENS_SET_FILE);
            Stash sqlScripts = new Stash(_scriptsLocation1, SQL_PATTERN);
            List<string> injectedSqls = sqlScripts[SQL_GET_ALL_USERS_PHONE_SAMPLE].InjectTokens(tokensSet);
            Assert.AreEqual(injectedSqls.Count, 3);
            Assert.IsTrue(injectedSqls[0].Contains("WHERE USER.AGE > 17"));
            Assert.IsTrue(injectedSqls[1].Contains("WHERE USER.CITY='Paris'"));
            Assert.IsTrue(injectedSqls[2].Contains("WHERE (USER.CITY='London' OR USER.CITY='NY')"));
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectXmlTokensArray()
        {
            List<Tokens> tokensSet = Tokens.LoadXmlArray(_scriptsLocation1 + "\\" + XML_TEST_SET);
            Stash restFulScripts = new Stash(_scriptsLocation1, REST_PATTERN);
            List<string> injectedRestQueries = restFulScripts[REST_REQUEST_FILE].InjectTokens(tokensSet);
            Assert.AreEqual(injectedRestQueries.Count, 2);
            Assert.IsTrue(injectedRestQueries[0].Equals("https://www.metaweather.com/api/location/44418/2013/4/27/"));
            Assert.IsTrue(injectedRestQueries[1].Equals("https://www.metaweather.com/api/location/2487956/2013/4/30/"));
        }

        [TestMethod()]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void InjectTokensListScripts()
        {
            Stash requestSet = new Stash(_scriptsLocation1, "metaweather_request?.csv");
            Stash restFulScripts = new Stash(_scriptsLocation1, "metaweather*.rest");
            List<string> injectedRestQueries = restFulScripts[REST_REQUEST_FILE].InjectTokens(requestSet.Scripts);
            Assert.AreEqual(injectedRestQueries.Count, 2);
            Assert.IsTrue(injectedRestQueries[0].Equals("https://www.metaweather.com/api/location/44418/2013/4/27/"));
            Assert.IsTrue(injectedRestQueries[1].Equals("https://www.metaweather.com/api/location/2487956/2013/4/30/"));
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveAllTest()
        {
            string testLine = "Test Line 12345 !@#$%";
            // Load 1st stash of text files. Each file with 5 line count in it (prepared case).
            Stash textStash1 = new Stash(_scriptsLocation2, TXT_PATTERN);
            int stashSize = textStash1.ScriptsCount;
            // Add 6th test line to each stash file. Check that all stash files has now 6 lines, and save it all.
            textStash1.Scripts.ForEach(script => script.AppendLine(testLine)); 
            Assert.AreEqual(textStash1.Scripts.Count(script => script.LineCount == 6), stashSize);
            textStash1.SaveAll();
            // Load 2st stash of same text files. check that each has 6 lines, and that each has the test line in the 6th line.
            Stash textStash2 = new Stash(_scriptsLocation2, TXT_PATTERN);
            Assert.AreEqual(textStash2.Scripts.Count(script => script.LineCount == 6), stashSize);
            Assert.AreEqual(textStash2.Scripts.Count(script => script.LastLineIndexWhere(line => line.Equals(testLine)) == 5 ), stashSize);
            // Return to starting point by removing the test line from each text file. Save it all and check that all files has again 5 lines and does not contains the test line.
            textStash2.Scripts.ForEach(script => script.RemoveLineWhere(line => line.Equals(testLine)));
            textStash2.SaveAll();
            Assert.AreEqual(textStash2.Scripts.Count(script => script.LineCount == 5), stashSize);
            Assert.AreEqual(textStash2.Scripts.Count(script => script.FirstLineIndexWhere(line => line.Equals(testLine)) >= 0), 0);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveToFolderTest()
        {
            Stash textStash1 = new Stash(_scriptsLocation2, TXT_PATTERN);
            int stashSize = textStash1.ScriptsCount;
            // Save test files to empty directory
            textStash1.SaveToFolder(_scriptsLocation3);
            // Load again and check file count stays the same.
            Stash textStash2 = new Stash(_scriptsLocation3, TXT_PATTERN);
            Assert.AreEqual(textStash2.ScriptsCount, stashSize);
            // Clear test directory for next time
            DirectoryInfo dirInfo = new DirectoryInfo(_scriptsLocation3);
            foreach(FileInfo file in dirInfo.GetFiles())
            {
                if(file.Extension.Equals(".txt"))
                {
                    file.Delete();
                }
            }
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void ReloadAllTest()
        {
            string newline = "new line !@#";
            Stash textStash = new Stash(_scriptsLocation2, TXT_PATTERN);
            List<int> lineCountsOrig = textStash.Scripts.Select(script => script.LineCount).ToList();
            textStash.Scripts.ForEach(script => script.InsertLineAtIndex(newline, 1));
            List<int> lineCountsChanged1 = textStash.Scripts.Select(script => script.LineCount).ToList();
            Assert.AreNotEqual(lineCountsOrig.Sum(), lineCountsChanged1.Sum());
            textStash.ReLoadAll();
            List<int> lineCountsChanged2 = textStash.Scripts.Select(script => script.LineCount).ToList();
            Assert.AreEqual(lineCountsOrig.Sum(), lineCountsChanged2.Sum());
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveOnScriptMatchTest()
        {
            string markLine = "A Mark Line !!";            
            // load zero files state.
            Stash textStash0 = new Stash(_scriptsLocation2, TXT_PATTERN);
            // load change files stash and change 2 files from the 3 files
            Stash textStash1 = new Stash(_scriptsLocation2, TXT_PATTERN);
            textStash1[TestFileName1].AppendLine(markLine);
            textStash1[TestFileName2].AppendLine(markLine);
            // save only script files that contains the mark line text.
            textStash1.SaveOnScriptMatch(script => script.Text.Contains(markLine));
            // load test stash and check that only the 2 of the files were saved with the mark line. 
            Stash textStash2 = new Stash(_scriptsLocation2, TXT_PATTERN);
            Assert.IsTrue(textStash2[TestFileName1].Text.Contains(markLine));
            Assert.IsTrue(textStash2[TestFileName2].Text.Contains(markLine));
            Assert.IsFalse(textStash2[TestFileName3].Text.Contains(markLine));
            // reset test files to zero state (for next time).
            textStash0.SaveAll();
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveToFolderOnScriptMatchTest()
        {
            string markLine = "!! A Mark Line Here !!";
            // load change files stash and change 2 files from the 3 files
            Stash textStash = new Stash(_scriptsLocation2, TXT_PATTERN);
            textStash[TestFileName1].AppendLine(markLine);
            textStash[TestFileName2].AppendLine(markLine);
            // save only script files that contains the mark line text.
            textStash.SaveToFolderOnScriptMatch(_scriptsLocation3, script => script.Text.Contains(markLine));
            // check at new target folder that only the 2 changed files were saved.
            Assert.IsTrue(File.Exists(_TestFileDelPath1));
            Assert.IsTrue(File.Exists(_TestFileDelPath2));
            Assert.IsFalse(File.Exists(_TestFileDelPath3));
            // reset target folder. clear test files for next time
            if (File.Exists(_TestFileDelPath1)) { File.Delete(_TestFileDelPath1); }
            if (File.Exists(_TestFileDelPath2)) { File.Delete(_TestFileDelPath2); }
            if (File.Exists(_TestFileDelPath3)) { File.Delete(_TestFileDelPath3); }
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void ReLoadOnScriptMatchTest()
        {
            // load change files stash and change 2 files from the 3 files
            Stash textStash = new Stash(_scriptsLocation2, TXT_PATTERN);
            // remove line from 3 text scripts
            int lineCountOld1 = textStash[TestFileName1].LineCount;
            int lineCountOld2 = textStash[TestFileName2].LineCount;
            int lineCountOld3 = textStash[TestFileName3].LineCount;
            textStash[TestFileName1].RemoveLineAtIndex(1);
            textStash[TestFileName2].RemoveLineAtIndex(1);
            textStash[TestFileName3].RemoveLineAtIndex(1);
            Assert.AreEqual(textStash[TestFileName1].LineCount + 1, lineCountOld1);
            Assert.AreEqual(textStash[TestFileName2].LineCount + 1, lineCountOld2);
            Assert.AreEqual(textStash[TestFileName3].LineCount + 1, lineCountOld3);
            // reload only 1 changed file. check original lines numer returned only to this file.
            textStash.ReLoadOnScriptMatch(script => script.FileName == TestFileName2);
            Assert.AreEqual(textStash[TestFileName1].LineCount + 1, lineCountOld1);
            Assert.AreEqual(textStash[TestFileName2].LineCount , lineCountOld2);
            Assert.AreEqual(textStash[TestFileName3].LineCount + 1, lineCountOld3);
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveOnLineMatchTest()
        {
            string markLine = "A Mark Line !!";
            // load zero files state.
            Stash textStash0 = new Stash(_scriptsLocation2, TXT_PATTERN);
            // load change files stash and change 2 files from the 3 files
            Stash textStash1 = new Stash(_scriptsLocation2, TXT_PATTERN);
            textStash1[TestFileName1].AppendLine(markLine);
            textStash1[TestFileName2].AppendLine(markLine);
            // save only script files that contains the mark line text.
            textStash1.SaveOnLineMatch(line => line.Contains(markLine));
            // load test stash and check that only the 2 of the files were saved with the mark line. 
            Stash textStash2 = new Stash(_scriptsLocation2, TXT_PATTERN);
            Assert.IsTrue(textStash2[TestFileName1].Text.Contains(markLine));
            Assert.IsTrue(textStash2[TestFileName2].Text.Contains(markLine));
            Assert.IsFalse(textStash2[TestFileName3].Text.Contains(markLine));
            // reset test files to zero state (for next time).
            textStash0.SaveAll();
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void SaveToFolderOnLineMatchTest()
        {
            string markLine = "!! A Mark Line Here !!";
            // load change files stash and change 2 files from the 3 files
            Stash textStash = new Stash(_scriptsLocation2, TXT_PATTERN);
            textStash[TestFileName1].AppendLine(markLine);
            textStash[TestFileName2].AppendLine(markLine);
            // save only script files that contains the mark line text.
            textStash.SaveToFolderOnLineMatch(_scriptsLocation3, line => line.Contains(markLine));
            // check at new target folder that only the 2 changed files were saved.
            Assert.IsTrue(File.Exists(_TestFileDelPath1));
            Assert.IsTrue(File.Exists(_TestFileDelPath2));
            Assert.IsFalse(File.Exists(_TestFileDelPath3));
            // reset target folder. clear test files for next time
            if (File.Exists(_TestFileDelPath1)) { File.Delete(_TestFileDelPath1); }
            if (File.Exists(_TestFileDelPath2)) { File.Delete(_TestFileDelPath2); }
            if (File.Exists(_TestFileDelPath3)) { File.Delete(_TestFileDelPath3); }
        }

        [TestMethod]
        [TestCategory("Stash Class"), TestCategory("Token Inject")]
        public void ReLoadOnLineMatchTest()
        {
            string markSuffix = "###!!###";
            // load change files stash and change 2 files from the 3 files
            Stash textStash = new Stash(_scriptsLocation2, TXT_PATTERN);
            // remove line from 3 text scripts
            int lineCountOld1 = textStash[TestFileName1].LineCount;
            int lineCountOld2 = textStash[TestFileName2].LineCount;
            int lineCountOld3 = textStash[TestFileName3].LineCount;
            textStash[TestFileName1].RemoveLineAtIndex(1);
            textStash[TestFileName2].RemoveLineAtIndex(1);
            textStash[TestFileName2].Lines[0] += markSuffix;
            textStash[TestFileName3].RemoveLineAtIndex(1);
            Assert.AreEqual(textStash[TestFileName1].LineCount + 1, lineCountOld1);
            Assert.AreEqual(textStash[TestFileName2].LineCount + 1, lineCountOld2);
            Assert.AreEqual(textStash[TestFileName3].LineCount + 1, lineCountOld3);
            // reload only 1 changed file. check original lines numer returned only to this file.
            textStash.ReLoadOnLineMatch(line => line.EndsWith(markSuffix));
            Assert.AreEqual(textStash[TestFileName1].LineCount + 1, lineCountOld1);
            Assert.AreEqual(textStash[TestFileName2].LineCount, lineCountOld2);
            Assert.AreEqual(textStash[TestFileName3].LineCount + 1, lineCountOld3);
        }
    }
}
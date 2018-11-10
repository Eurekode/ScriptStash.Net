using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptStash;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ScriptStashUnitTests
{
    [TestClass()]
    public class ScriptTests
    {
        private const string SCRIPTS_FOLDER = @"\scripts";
        private const string COMMIT_BY_SHA_URL = "github_api_commit_by_sha.rest";
        private const string JQUERY_EXAMPLE_FILE = "jQueryHideCallbackExample.html";
        private const string PERL_SEND_MAIL_SAMPLE = "perlMailSendExample.pl";
        private const string VBS_MOC_SAMPLE = "vbsScriptSample.vbs";
        private const string REST_TOKEN_USER = "{user}";
        private const string REST_TOKEN_REPO = "{repo name}";
        private const string REST_TOKEN_SHA  = "{sha}";
        private const string JAVASCRIPT_CODE_PLACEHOLDER = @"// JAVASCRIPT ACTION CODE PLACEHOLDER";
        private const string JAVASCRIPT_CODE_MESSAGE = "\t\t\talert(\"The paragraph is now hidden\");";
        private const string PERL_REMARK_LINE = "## END of code...";
        private const string PERL_MAIL_BODY_LINE = "  body_str => \"Happy birthday to you!\\n\",";
        private const string VBS_ERR_HANDLING_L1 = "If Err.Number <> 0 Then      'error handling:";
        private const string VBS_ERR_HANDLING_L2 = "  WScript.Echo Err.Number & \" Srce: \" & Err.Source & \" Desc: \" &  Err.Description";
        private const string VBS_ERR_HANDLING_L3 = "  Err.Clear";
        private const string VBS_ERR_HANDLING_L4 = "End If";
        private const string VBS_ERR_HANDLING_L5 = "On Error goto 0";
        private const string REST_TARGET_URL = "https://api.github.com/repos/Eurekode/ScriptStash/git/commits/b3adc256d77f120010d8a33e1dcd6543fdb001ae";
        private const string SQL_GET_ALL_USERS_ADDRESS_SAMPLE = "sqlGetAllUsersAddressWhere.sql";
        private const string SQL_GET_ALL_USERS_PHONE_SAMPLE = "sqlGetAllUsersPhoneWhere.sql";
        private const string SQL_GET_ALL_USERS_SAMPLE = "sqlGetAllUsersWhere.sql";
        private const string JSON_TOKENS_SCRIPT_FILE = "sql_tokens_script.json";
        private const string JSON_TOKENS_SET_FILE = "sql_tokens_set.json";
        private const string REST_REQUEST_FILE = "metaweather_request.rest";
        private const string XML_TEST_SET = "test_set.xml";
        private const string CSV_WEATHER_REQUEST1 = "metaweather_request1.csv";
        private const string CSV_WEATHER_REQUEST2 = "metaweather_request2.csv";
        private string _scriptsLocation;

        public ScriptTests()
        {
            string execDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _scriptsLocation = execDirectory + SCRIPTS_FOLDER;
        }

        private Tokens PrepareGitApiCommitsTokens()
        {
            Tokens tokenParams = new Tokens();
            tokenParams[REST_TOKEN_USER] = "Eurekode";
            tokenParams[REST_TOKEN_REPO] = "ScriptStash";
            tokenParams[REST_TOKEN_SHA] = "b3adc256d77f120010d8a33e1dcd6543fdb001ae";
            return tokenParams;
        }

        [TestMethod()]
        [TestCategory("Script Class"), TestCategory("Sanity")]
        public void ScriptLoadTest()
        {
            Script restScript = new Script(_scriptsLocation + "\\" + COMMIT_BY_SHA_URL);
            Assert.IsTrue(restScript.LineCount > 0);
            Assert.IsTrue(restScript.FileName.Equals(COMMIT_BY_SHA_URL));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void ScriptLoadWithTokensRefTest()
        {
            Tokens tokenParams = PrepareGitApiCommitsTokens();
            Script restScript = new Script(_scriptsLocation + "\\" + COMMIT_BY_SHA_URL, ref tokenParams);
            Assert.IsFalse(restScript.Tokens == null);
            Assert.IsTrue(restScript.Tokens.Count == 3);
            Assert.AreEqual(restScript.Tokens[REST_TOKEN_REPO], "ScriptStash");
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void FirstLineIndexWhereTest()
        {
            Script javaScript = new Script(_scriptsLocation + "\\" + JQUERY_EXAMPLE_FILE);
            int lineIndex = javaScript.FirstLineIndexWhere(line => line.Contains(JAVASCRIPT_CODE_PLACEHOLDER));
            Assert.AreEqual(lineIndex, 7);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void LastLineIndexWhereTest()
        {
            Script javaScript = new Script(_scriptsLocation + "\\" + JQUERY_EXAMPLE_FILE);
            int lineIndex = javaScript.LastLineIndexWhere(line => line.Contains("html"));
            Assert.AreEqual(lineIndex, 19);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void LinesIndexWhereTest()
        {
            Script javaScript = new Script(_scriptsLocation + "\\" + JQUERY_EXAMPLE_FILE);
            List<int> scriptTagIndexLines = javaScript.LinesIndexWhere(value => value.Contains("script"));
            List<int> zuluTagIndexLines = javaScript.LinesIndexWhere(line => line.Equals("zulu"));
            List<int> htmlTagIndexLines = javaScript.LinesIndexWhere(line => line.Contains("html"));
            Assert.AreEqual(scriptTagIndexLines.Count, 3);
            Assert.AreEqual(zuluTagIndexLines.Count, 0);
            Assert.AreEqual(htmlTagIndexLines.Count, 2);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void AppendLineTest()
        {
            int lastLineIndex1, lastLineIndex2;
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            lastLineIndex1 = perlCode.LineCount;
            perlCode.AppendLine(PERL_REMARK_LINE);
            lastLineIndex2 = perlCode.LineCount;
            Assert.IsTrue(lastLineIndex2 - lastLineIndex1 == 1);
            Assert.IsTrue(perlCode[lastLineIndex2-1].Contains("END"));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InsertLineAtIndexTest()
        {
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            perlCode.InsertLineAtIndex(PERL_MAIL_BODY_LINE, 17);
            Assert.AreEqual(perlCode[17], PERL_MAIL_BODY_LINE);
            Assert.AreEqual(perlCode[18], ");");
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void RemoveLineAtIndexTest()
        {
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            perlCode.RemoveLineAtIndex(3);
            Assert.IsFalse(perlCode.Text.Contains("https:"));
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void ReplaceLineAtIndexTest()
        {
            string newComment = @"\t' NEW COMMENT LINE !";
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            vbsCode.ReplaceLineAtIndex(newComment, 3);
            Assert.AreEqual(vbsCode[3], newComment);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InsertLineOnFirstWhereTest()
        {
            Regex insertRule = new Regex(@"^\);");
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            perlCode.InsertLineOnFirstWhere(PERL_MAIL_BODY_LINE, line => insertRule.IsMatch(line));
            Assert.AreEqual(perlCode[17], PERL_MAIL_BODY_LINE);
            Assert.AreEqual(perlCode[18], ");");
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InsertLineOnLastWhereTest()
        {
            Regex insertRule = new Regex(@"^\);");
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            int newLineIndex = perlCode.InsertLineOnLastWhere(PERL_MAIL_BODY_LINE, line => insertRule.IsMatch(line));
            Assert.AreEqual(perlCode[newLineIndex], PERL_MAIL_BODY_LINE);
            Assert.AreEqual(perlCode[newLineIndex+1], ");");
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InsertLineWhereTest()
        {
            // insert rule is every 'End Sub' or 'End Function' in the vbs script...
            Regex insertRule = new Regex(@"((End)|(end))\s((Sub)|(sub)|(function)|(Function))");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            vbsCode.InsertLineWhere(VBS_ERR_HANDLING_L5, line => insertRule.IsMatch(line));
            List<int> OnErrorZeroLines = vbsCode.LinesIndexWhere(line => line.Equals(VBS_ERR_HANDLING_L5));
            Assert.AreEqual(OnErrorZeroLines.Count , 3);
            Assert.IsTrue(insertRule.IsMatch(vbsCode[OnErrorZeroLines[0] + 1]));
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void RemoveLineOnFirstWhereTest()
        {
            Regex removeRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            int removedIndex = vbsCode.RemoveLineOnFirstWhere(line => removeRule.IsMatch(line));
            Assert.AreEqual(removedIndex, 3);
            Assert.AreEqual(vbsCode.LineCount, 19);
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void RemoveLineOnLastWhereTest()
        {
            Regex removeRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            int removedIndex = vbsCode.RemoveLineOnLastWhere(line => removeRule.IsMatch(line));
            Assert.AreEqual(removedIndex, 17);
            Assert.AreEqual(vbsCode.LineCount, 19);
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void RemoveLineWhereTest()
        {
            Regex removeRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            List<int> removedIndexs = vbsCode.RemoveLineWhere(line => removeRule.IsMatch(line));
            Assert.AreEqual(removedIndexs.Count, 3);
            Assert.AreEqual(vbsCode.LineCount, 17);
            Assert.AreEqual(vbsCode.LinesIndexWhere(line => removeRule.IsMatch(line)).Count(), 0);
            Assert.IsTrue(vbsCode[3].Contains(":"));
            Assert.IsTrue(vbsCode[10].EndsWith("true"));
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void ReplaceLineOnFirstWhereTest()
        {
            string newComment = @"\t' NEW COMMENT LINE !";
            Regex replaceRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            int replacedLineIndex = vbsCode.ReplaceLineOnFirstWhere(newComment, line => replaceRule.IsMatch(line));
            Assert.AreEqual(replacedLineIndex, 3);
            Assert.AreEqual(vbsCode[replacedLineIndex], newComment);
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void ReplaceLineOnLastWhereTest()
        {
            string newComment = @"\t' NEW COMMENT LINE !";
            Regex replaceRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            int replacedLineIndex = vbsCode.ReplaceLineOnLastWhere(newComment, line => replaceRule.IsMatch(line));
            Assert.AreEqual(replacedLineIndex, 17);
            Assert.AreEqual(vbsCode[replacedLineIndex], newComment);
        }

        [TestMethod]
        [TestCategory("Script Class")]
        public void ReplaceLineWhereTest()
        {
            string newComment = @"\t' NEW COMMENT LINE !";
            Regex replaceRule = new Regex(@"(code goes here)");
            Script vbsCode = new Script(_scriptsLocation + "\\" + VBS_MOC_SAMPLE);
            List<int> replacedLineIndexs = vbsCode.ReplaceLineWhere(newComment, line => replaceRule.IsMatch(line));
            List<int> checkRelacedIndexs = vbsCode.LinesIndexWhere(line => line.Equals(newComment));            
            Assert.AreEqual(replacedLineIndexs.Count, 3);
            Assert.AreEqual(replacedLineIndexs.Sum(num => num * num), checkRelacedIndexs.Sum(num => num * num));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void SaveTest()
        {
            string oldVariable = @"$message";
            string newVariable = @"$MyMessage";
            Regex findRule = new Regex(@"\" + oldVariable);
            Regex afterReplaceRule = new Regex(@"\" + newVariable);
            Script perlCode = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            List<int> selectedIndxs = perlCode.LinesIndexWhere(line => findRule.IsMatch(line));

            perlCode.LinesIndexWhere(line => findRule.IsMatch(line)).ForEach(indx => 
            {
                perlCode[indx] = perlCode[indx].Replace(oldVariable, newVariable);
            });

            perlCode.Save();
            Script perlCodeReloaded = new Script(_scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE);
            List<int> checkList = perlCodeReloaded.LinesIndexWhere(line => afterReplaceRule.IsMatch(line));
            Assert.AreEqual(checkList.Count, 2);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void SaveAsTest()
        {
            string origFilePath = _scriptsLocation + "\\" + PERL_SEND_MAIL_SAMPLE;
            string copyFilePath = _scriptsLocation + "\\" + "copy_of_" + PERL_SEND_MAIL_SAMPLE;
            Script perlCode = new Script(origFilePath);
            // make sure copy file not exist from last debug
            if( File.Exists(copyFilePath) ) { File.Delete(copyFilePath); }
            perlCode.SaveAs(copyFilePath);
            Assert.IsTrue(File.Exists(copyFilePath));
            // clear for next time
            File.Delete(copyFilePath);
            Assert.IsFalse(File.Exists(copyFilePath));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void ReloadTest()
        {
            string newParagrph = @"     <p>A new paragraph Added here.</p>";
            Script javaScript = new Script(_scriptsLocation + "\\" + JQUERY_EXAMPLE_FILE);
            int lineCount = javaScript.LineCount;
            javaScript.InsertLineAtIndex(newParagrph, 18);
            Assert.AreEqual(javaScript.LineCount, lineCount + 1);
            javaScript.Reload();
            Assert.AreEqual(javaScript.LineCount, lineCount);
        }

        [TestMethod()]
        [TestCategory("Script Class"), TestCategory("Token Inject")]
        public void InjectTokensTest()
        {
            Script restScript = new Script(_scriptsLocation + "\\" + COMMIT_BY_SHA_URL);
            Tokens tokenParams = PrepareGitApiCommitsTokens();
            string resultUrl = restScript.InjectTokens(tokenParams);
            Assert.AreEqual(resultUrl, REST_TARGET_URL);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InjectinnerRefTokensTest()
        {
            Tokens tokenParams = PrepareGitApiCommitsTokens();
            Script restScript = new Script(_scriptsLocation + "\\" + COMMIT_BY_SHA_URL, ref tokenParams);
            string resultUrl = restScript.InjectTokens();
            Assert.AreEqual(resultUrl, REST_TARGET_URL);
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InjectTokensScriptTest()
        {
            Script sqlTokensScript = new Script(_scriptsLocation + "\\" + JSON_TOKENS_SCRIPT_FILE);
            Script sqlScript = new Script(_scriptsLocation + "\\" + SQL_GET_ALL_USERS_SAMPLE);
            string sql = sqlScript.InjectTokens(sqlTokensScript);
            Assert.IsTrue(sql.Contains("SELECT name, family, age, city, phone"));
            Assert.IsTrue(sql.Contains("WHERE age > 17"));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InjectTokensListScripts()
        {
            Stash requestSet = new Stash(_scriptsLocation, "metaweather_request?.csv");
            Script restFulScript = new Script(_scriptsLocation + "\\" + REST_REQUEST_FILE);
            List<string> injectedRestQueries = restFulScript.InjectTokens(requestSet.Scripts);
            Assert.AreEqual(injectedRestQueries.Count, 2);
            Assert.IsTrue(injectedRestQueries[0].Equals("https://www.metaweather.com/api/location/44418/2013/4/27/"));
            Assert.IsTrue(injectedRestQueries[1].Equals("https://www.metaweather.com/api/location/2487956/2013/4/30/"));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InjectJsonTokensArray()
        {
            List<Tokens> tokensSet= Tokens.LoadJsonArray(_scriptsLocation + "\\" + JSON_TOKENS_SET_FILE);
            Script sqlScript = new Script(_scriptsLocation + "\\" + SQL_GET_ALL_USERS_PHONE_SAMPLE);
            List<string> injectedSqls = sqlScript.InjectTokens(tokensSet);
            Assert.AreEqual(injectedSqls.Count, 3);
            Assert.IsTrue(injectedSqls[0].Contains("WHERE USER.AGE > 17"));
            Assert.IsTrue(injectedSqls[1].Contains("WHERE USER.CITY='Paris'"));
            Assert.IsTrue(injectedSqls[2].Contains("WHERE (USER.CITY='London' OR USER.CITY='NY')"));
        }

        [TestMethod()]
        [TestCategory("Script Class")]
        public void InjectXmlTokensArray()
        {
            List<Tokens> tokensSet = Tokens.LoadXmlArray(_scriptsLocation + "\\" + XML_TEST_SET);
            Script restFulScript = new Script(_scriptsLocation + "\\" + REST_REQUEST_FILE);
            List<string> injectedRestQueries = restFulScript.InjectTokens(tokensSet);
            Assert.AreEqual(injectedRestQueries.Count, 2);
            Assert.IsTrue(injectedRestQueries[0].Equals("https://www.metaweather.com/api/location/44418/2013/4/27/"));
            Assert.IsTrue(injectedRestQueries[1].Equals("https://www.metaweather.com/api/location/2487956/2013/4/30/"));
        }
    }
}
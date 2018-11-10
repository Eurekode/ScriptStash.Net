using Microsoft.VisualStudio.TestTools.UnitTesting;
using ScriptStash;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ScriptStashUnitTests
{
    [TestClass()]
    public class TokensTests
    {
        private const string SCRIPTS_FOLDER = @"\scripts";
        private const string JSON_TOKENS_FILE = "json_tokens.json";
        private const string JSON_TOKENS_SET_FILE = "sql_tokens_set.json";
        private const string JSON_APPEND_TOKENS_FILE = "‏‏json_append_tokens.json";
        private const string XML_TOKENS_FILE = "token_config.xml";
        private const string XML_APPEND_TOKENS_FILE = "test.xml";
        private const string CSV_TOKENS_FILE = "csv_tokens.csv";
        private const string CSV_APPEND_TOKENS_FILE = "‏‏csv_append_tokens.csv";
        private const string REST_REQUEST_FILE = "metaweather_request.rest";
        private const string XML_TEST_SET = "test_set.xml";

        private string _scriptsLocation;

        public TokensTests()
        {
            string execDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            _scriptsLocation = execDirectory + SCRIPTS_FOLDER;
        }

        [TestMethod()]
        [TestCategory("Tokens Class"), TestCategory("Sanity")]
        public void TokensTest()
        {
            Tokens tokens = new Tokens();
            string key1 = "[KEY1]";
            string key2 = "[KEY2]";
            string testVal1 = "val_A";
            string testVal2 = "val_B";
            string testVal3 = "val_C";
            tokens.Add(key1, testVal1);
            tokens[key2] = testVal2;
            tokens[key1] = testVal3;
            Assert.AreEqual(tokens[key1], testVal3);
            Assert.AreNotEqual(tokens[key1], testVal1);
            Assert.IsTrue(tokens.ContainsKey(key2));
            Assert.AreEqual(tokens[key2], testVal2);
            Assert.IsTrue(tokens.Count == 2);
        }

        [TestMethod()]
        [TestCategory("Tokens Class"), TestCategory("Sanity")]
        public void AppendTest()
        {
            Tokens tokens1 = new Tokens();
            Tokens tokens2 = new Tokens();
            tokens1["A"] = "1";
            tokens1["B"] = "2";
            tokens1["C"] = "3";
            tokens1["D"] = "4";
            tokens2["C"] = "3";
            tokens2["D"] = "40";
            tokens2["E"] = "50";
            tokens1.Append(tokens2);
            Assert.AreEqual(tokens1.Count, 5);
            Assert.AreEqual(tokens1["C"], "3");
            Assert.AreEqual(tokens1["D"], "40");
            Assert.AreEqual(tokens1["E"], "50");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadJsonTextTest()
        {
            string json = "{ \"key1\": \"value1\", \"key2\": \"value2\", \"key3\": \"value3\" }";
            Tokens tokens = Tokens.LoadJsonText(json);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value2");
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadJsonTest()
        {
            Tokens tokens = Tokens.LoadJson(_scriptsLocation + "\\" + JSON_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadJsonArrayTest()
        {
            List<Tokens> tokensArray = Tokens.LoadJsonArray(_scriptsLocation + "\\" + JSON_TOKENS_SET_FILE);
            Assert.AreEqual(tokensArray.Count, 3);
            Assert.AreEqual(tokensArray[0]["[USERS_CONDITION]"], "USER.AGE > 17");
            Assert.AreEqual(tokensArray[1]["[CSV_FIELDS]"], "USER.NAME, USER.FAMILY");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void AppendJsonTest()
        {
            Tokens tokens = Tokens.LoadJson(_scriptsLocation + "\\" + JSON_TOKENS_FILE);
            tokens.AppendJson(_scriptsLocation + "\\" + JSON_APPEND_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 4);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value20");
            Assert.IsTrue(tokens["key3"] == "value3");
            Assert.IsTrue(tokens["key4"] == "value4");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void ToJsonTest()
        {
            Tokens tokens = new Tokens { { "A", "1" }, { "B", "2" }, { "C", "3" } };
            string jsonString = tokens.ToJson();
            Assert.IsTrue(jsonString.StartsWith("{"));
            Assert.IsTrue(jsonString.Contains("\"A\":\"1\","));
            Assert.IsTrue(jsonString.Contains("\"B\":\"2\","));
            Assert.IsTrue(jsonString.Contains("\"C\":\"3\""));
            Assert.IsTrue(jsonString.EndsWith("}"));
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void SaveJsonTest()
        {
            string jsonFileName = _scriptsLocation + "\\temp_test.json";
            Tokens tokens = new Tokens { { "Key_A", "Val_A" }, { "Key_B", "Val_B" }, { "Key_C", "Val_c" } };
            tokens.SaveJson(jsonFileName);

            Tokens loadTokens = Tokens.LoadJson(jsonFileName);
            Assert.AreEqual(loadTokens.Count, 3);
            Assert.AreEqual(loadTokens["Key_A"], "Val_A");
            Assert.AreEqual(loadTokens["Key_B"], "Val_B");
            Assert.AreEqual(loadTokens["Key_C"], "Val_c");

            File.Delete(jsonFileName);
            Assert.IsFalse(File.Exists(jsonFileName));
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadXmlTextTest()
        {
            string xml = "<root><key1>value1</key1><key2>value2</key2><key3>value3</key3></root>";
            Tokens tokens = Tokens.LoadXmlText(xml);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value2");
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadXmlTest()
        {
            Tokens tokens = Tokens.LoadXml(_scriptsLocation + "\\" + XML_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadXmlArrayTest()
        {
            List<Tokens> tokensArray = Tokens.LoadXmlArray(_scriptsLocation + "\\" + XML_TEST_SET);
            Assert.AreEqual(tokensArray.Count, 2);
            Assert.AreEqual(tokensArray[0]["WEOID"], "44418");
            Assert.AreEqual(tokensArray[1]["MM"], "4");
            Assert.AreEqual(tokensArray[1]["DD"], "30");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void AppendXmlTest()
        {
            Tokens tokens = Tokens.LoadXml(_scriptsLocation + "\\" + XML_TOKENS_FILE);
            tokens.AppendXml(_scriptsLocation + "\\" + XML_APPEND_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 4);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value20");
            Assert.IsTrue(tokens["key3"] == "value3");
            Assert.IsTrue(tokens["key4"] == "value4");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void ToXmlTest()
        {
            Tokens tokens = new Tokens { { "A", "1" }, { "B", "2" }, { "C", "3" } };
            string jsonString = tokens.ToXml();
            Assert.IsTrue(jsonString.StartsWith("<root>"));
            Assert.IsTrue(jsonString.Contains("<A>1</A>"));
            Assert.IsTrue(jsonString.Contains("<B>2</B>"));
            Assert.IsTrue(jsonString.Contains("<C>3</C>"));
            Assert.IsTrue(jsonString.EndsWith("</root>"));
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void SaveXmlTest()
        {
            string xmlFileName = _scriptsLocation + "\\temp_test.xml";
            Tokens tokens = new Tokens { { "Key_A", "Val_A" }, { "Key_B", "Val_B" }, { "Key_C", "Val_c" } };
            tokens.SaveXml(xmlFileName);

            Tokens loadTokens = Tokens.LoadXml(xmlFileName);
            Assert.AreEqual(loadTokens.Count, 3);
            Assert.AreEqual(loadTokens["Key_A"], "Val_A");
            Assert.AreEqual(loadTokens["Key_B"], "Val_B");
            Assert.AreEqual(loadTokens["Key_C"], "Val_c");

            File.Delete(xmlFileName);
            Assert.IsFalse(File.Exists(xmlFileName));
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadCsvTextTest()
        {
            string csv = "\"key1\",\"value1\"\n\"key2\",\"value2\"\n\"key3\",\"value3\"";
            Tokens tokens = Tokens.LoadCsvText(csv);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value2");
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void LoadCsvTest()
        {
            Tokens tokens = Tokens.LoadCsv(_scriptsLocation + "\\" + CSV_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 3);
            Assert.IsTrue(tokens["key3"] == "value3");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void AppendCsvTest()
        {
            Tokens tokens = Tokens.LoadCsv(_scriptsLocation + "\\" + CSV_TOKENS_FILE);
            tokens.AppendCsv(_scriptsLocation + "\\" + CSV_APPEND_TOKENS_FILE);
            Assert.AreEqual(tokens.Count, 4);
            Assert.IsTrue(tokens["key1"] == "value1");
            Assert.IsTrue(tokens["key2"] == "value20");
            Assert.IsTrue(tokens["key3"] == "value3");
            Assert.IsTrue(tokens["key4"] == "value4");
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void ToCsvTest()
        {
            Tokens tokens = new Tokens { { "A", "1" }, { "B", "2" }, { "C", "3" } };
            string xmlString = tokens.ToCsv();
            Assert.IsTrue(xmlString.Contains($"\"A\",\"1\"{Environment.NewLine}"));
            Assert.IsTrue(xmlString.Contains($"\"B\",\"2\"{Environment.NewLine}"));
            Assert.IsTrue(xmlString.Contains("\"C\",\"3\""));
        }

        [TestMethod()]
        [TestCategory("Tokens Class")]
        public void SaveCsvTest()
        {
            string csvFileName = _scriptsLocation + "\\temp_test.csv";
            Tokens tokens = new Tokens { { "Key_A", "Val_A" }, { "Key_B", "Val_B" }, { "Key_C", "Val_c" } };
            tokens.SaveCsv(csvFileName);

            Tokens loadTokens = Tokens.LoadCsv(csvFileName);
            Assert.AreEqual(loadTokens.Count, 3);
            Assert.AreEqual(loadTokens["Key_A"], "Val_A");
            Assert.AreEqual(loadTokens["Key_B"], "Val_B");
            Assert.AreEqual(loadTokens["Key_C"], "Val_c");

            File.Delete(csvFileName);
            Assert.IsFalse(File.Exists(csvFileName));
        }

    }
}
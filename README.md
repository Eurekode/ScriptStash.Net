# ScriptStash ![ScriptStash Logo](/ScriptStash/package/scriptstash_nuget_logo.png)
[![Version](https://img.shields.io/badge/nuget-v2.0.0-brightgreen.svg?logo=appveyor&longCache=true&style=plastic)](https://www.nuget.org/packages/ScriptStash.Net)
## What is ...
**ScriptStash** is a C# code loader for text/script files manipulation. It loads each script file once. Keeping it stored in-memory, ready to be used for any applicative reuse over and over. Also, it can be used for token-driven scripting (e.g. place in script token for SELECT-SQL table name, changing the query target table dynamically at run-time).

## How it works ...
**ScriptStash** loads with **System.IO.File** each script at its destination folder and store it in-memory while your solution application running. **ScriptStash** can store dictionary key-value pairs for token injection to every stored tokenized script file. So, it can return the original file loaded, or a transformed text with all needed tokens replaced with values from given dictionary.

## Usage ...
Usage examples:
- Maintain a query SQL scripts directory for a web-application with database storage.
- Keep reusable shell scripts for 3rd party ALM tool. Load and execute them from yours C# application utility.
- Store scripts from any scripting language (e.g. PowerShell, VBScript, JavaScript, etc.) for execution from C# app.
- Handle installation scripts batch, to be deployed on several environments with different inner setting per environment.
- Maintain test template scripts, with data-driven cases to be injected to each test template before execution.

## Code Example ...
``` C#
  string scriptsFolder = "c:\project\scripts";
  string pattern = "*.sql";
  string genSelectAllQueryName = "select_all_query.sql";
  string genTemplateTableQuery = "template_table_query.sql";
  :
  :
  Stash mySqlStash = new Stash(scriptsFolder, pattern);
  string sqlText1 = mySqlStash[genSelectAllQueryName].Text;
  :
  mySqlStash.Tokens["<table-name>"] = "USERS";
  mySqlStash.Tokens["<where-clause>"] = "WHERE USER.City='Tel-Aviv";
  string sqlText2 = mySqlStash[genTemplateTableQuery].InjectTokens();
``` 
**[!]** For more indepth example please chek next Gist : [Stash example - SQL queries](https://gist.github.com/Eurekode/50e20e5df20afc8435b2a9a5dcc6fa2c)

## Documentation
Check out **ScriptStash** documentation pages [here](https://eurekode.github.io/script_stash_documentations/index.html).
  
## License
This project is MIT licensed. For more information, see [license](LICENSE).

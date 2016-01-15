using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;

namespace SqlEtl.Helpers
{
    public abstract class ParseArgs
    {
        internal static IEnumerable<FlagTokens> GetTokens(string[] args)
        {
            var rest = new StringCollection();
            FlagTokens tok;
            foreach (var arg in args)
            {
                if (arg.StartsWith("/"))
                {
                    tok = new FlagTokens
                    {
                        Flag = arg,
                        Args = null
                    };
                    var flagArgsStart = arg.IndexOf(':');
                    if (flagArgsStart > 0)
                    {
                        tok.Flag = arg.Substring(0, flagArgsStart);
                        tok.Args = new[] { arg.Substring(flagArgsStart + 1) };
                    }
                    yield return tok;
                }
                else
                {
                    rest.Add(arg);
                }
            }
            if (rest.Count <= 0) yield break;
            tok = new FlagTokens
            {
                Flag = null,
                Args = new string[rest.Count]
            };
            rest.CopyTo(tok.Args, 0);
            yield return tok;
        }

        internal static void LogError(string message, params object[] args)
        {
            Console.WriteLine(message);
        }

        internal class FlagTokens
        {
            internal string[] Args;
            internal string Flag;
            internal int NumberArgs => Args?.Length ?? 0;
        }
    }

    public class Arguments : ParseArgs
    {
        private readonly StringDictionary _argValues;

        public Arguments(string[] args)
        {
            _argValues = new StringDictionary();
            var noArgs = true;
            foreach (var tok in GetTokens(args))
            {
                noArgs = false;
                if (tok.Flag == null)
                {
                }
                else
                {
                    switch (tok.Flag.ToLowerInvariant())
                    {
                        case "/?":
                        case "/h":
                            _argValues["Help"] = "1";
                            break;
                        case "/source":
                            {
                                _argValues["Source"] = tok.Args[0];
                                break;
                            }
                        case "/destination":
                            {
                                _argValues["Destination"] = tok.Args[0];
                                break;
                            }
                        case "/batchsize":
                            {
                                _argValues["BatchSize"] = tok.Args[0];
                                break;
                            }
                        case "/resumeonerror":
                            {
                                _argValues["ResumeOnError"] = tok.Args[0];
                                break;
                            }
                        case "/skip":
                            {
                                _argValues["Skip"] = tok.Args[0];
                                break;
                            }
                        case "/createobjects":
                            {
                                _argValues["CreateObjects"] = tok.Args[0];
                                break;
                            }
                        case "/retrycount":
                            {
                                _argValues["RetryCount"] = tok.Args[0];
                                break;
                            }
                        case "/retryinterval":
                            {
                                _argValues["RetryInterval"] = tok.Args[0];
                                break;
                            }
                        case "/customkeydefinitions":
                            {
                                _argValues["CustomKeyDefinitions"] = tok.Args[0];
                                break;
                            }
                    }
                }
            }
            if (noArgs)
            {
                Console.WriteLine(@"Enter valid arguments.");
            }
        }

        public string this[string argName] => (_argValues[argName]);

        public static void PrintHelp()
        {
            Console.WriteLine(@"Ez ETL Utility Version " + Assembly.GetExecutingAssembly().GetName().Version);
            Console.WriteLine(@"Copyright (c) Awemind, LLC., All rights reserved.");
            Console.WriteLine();
            Console.WriteLine(@"Usage: SqlEtl.exe <parameters>");
            Console.WriteLine(@"   Example: /source:""source"" /destination:""destinatin"" /batchsize:50 /resumeonerror:true");
            Console.WriteLine(@" Parameters:");
            Console.WriteLine(@"  /source");
            Console.WriteLine(@"    Connection string for data source");
            Console.WriteLine(@"  /destination");
            Console.WriteLine(@"    Connection string for destination");
            Console.WriteLine(@"  /resumeonerror");
            Console.WriteLine(@"    resume if any error occurs due to missing primary keys");
            Console.WriteLine(@"  /batchsize");
            Console.WriteLine(@"    number of rows per transfer");
            Console.WriteLine(@"  /CreateObjects");
            Console.WriteLine(@"    destination is empty database, so create all tables, indexes, storedproc and views on destination");
            Console.WriteLine(@"  /skip");
            Console.WriteLine(@"    comma separated ist of tables needs to be skipped");
            Console.WriteLine(@"  /retrycount");
            Console.WriteLine(@"    try number of times to retry on any failure");
            Console.WriteLine(@"  /retryinterval");
            Console.WriteLine(@"    seconds to be waited before retrying any failure");
            Console.WriteLine(@"  /customkeydefinitions");
            Console.WriteLine(@"    moving data needs clustered index but if none is presented you can fake the keys here for example tablename~key1,key2 in a separate file and provide full path to the file here");
            Console.WriteLine(@"  /?");
            Console.WriteLine(@"  /h");
            Console.WriteLine(@"    Displays this help text");
        }
    }
}
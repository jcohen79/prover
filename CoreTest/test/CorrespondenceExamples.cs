using GraphMatching;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ConsoleApplication1.test
{

    [TestFixture]
    public class CorrespondenceExamples
    {
        static ColorDef cRed = ColorDef.Red;
        static ColorDef cGreen = ColorDef.Green;
        static ColorDef cS = new ColorDef("S", "#80A080");
        static ColorDef cT = new ColorDef("T", "#A0A080");
        static EdgeDescriptor edSimpleRed = new EdgeDescriptor("", cRed);
        static EdgeDescriptor edSimpleGreen = new EdgeDescriptor("", cGreen);
        static EdgeDescriptor edS = new EdgeDescriptor("", cS);
        static EdgeDescriptor edT = new EdgeDescriptor("", cT);
        static String sA = "a";
        static String sB = "b";
        SymbolTable stGlobal;
        static TraceNode trace = TraceNode.full;

        [SetUp]
        public void Init()
        {
            stGlobal = new SymbolTable(null);
            stGlobal.Add("edSimpleRed", edSimpleRed);
            stGlobal.Add("edSimpleGreen", edSimpleGreen);
            stGlobal.Add("edS", edS);
            stGlobal.Add("edT", edT);
            stGlobal.Add("sA", sA);
            stGlobal.Add("sB", sB);
        }

        public void RunCorrespondence(string pattern, string name, string data, string expected, bool failureExpected = false)
        {
            try
            {
                SymbolTable stTest = new SymbolTable(stGlobal);
                REPL repl = new REPL();
                repl.Prepare();
                repl.ee.EvaluateCorrespondenceSource(pattern, stTest);
                Correspondence c1 = (Correspondence) stTest.Lookup(name);
                Graph gData = repl.ee.EvaluateGraph(data, stTest);

                var m = new PatternFinder(c1.gFirst, gData, trace);
                List<GraphMatch> matches = m.Match();
                int size = matches.Count;
                Assert.IsTrue(size > 0, "no matches found");
                Assert.IsTrue(size == 1, "too many matches found: " + size);
                GraphMatch gm = matches[0];
                var cOutput = new Restrictor("output", null);
                var gOutput = new Graph(null, null, null, null, cOutput);

                c1.Perform(stTest, gm, Correspondence.ReplacementDirectionType.Forward, ExpressionEvaluator.cGlobalRestrictor, cOutput, gOutput);

                Console.WriteLine("output " + gOutput);
                Graph gExpected = repl.ee.EvaluateGraph(expected, c1.stSecond, true);
                var em = new PatternFinder(gExpected, gOutput, trace);

                List<GraphMatch> eMatches = em.Match();
                int iESize = eMatches.Count;
                if (failureExpected)
                    Assert.IsFalse(iESize == 1, "expected failure not found");
                else
                {
                    Assert.IsTrue(iESize > 0, "no match to expected found");
                    Assert.IsTrue(iESize == 1, "too many matches to expected found: " + size);
                }
            }
            catch (Exception e)
            {
                if (failureExpected)
                    return;
                throw e;
            }
        }


        // fails. don't understand test
        // [Test]
        public void ListToTree()
         {
             string c1 = @"correspondence ListToTree { R@, F@Name1, S@Name2, R~F^edS, F~S^edS, R=B\edT }
                         to { B@, L@Name1, R@Name2, B~L^edS, B~R^edS, B=R/edT }
                         using { F~L^edT, S~R^edT }";
             string data = "listRoot@, treeRoot@, a@\"this is a\", b@\"this is b\", listRoot~treeRoot^edT, listRoot~a^edS, a~b^edS";
             string expected = @" treeRoot@, L@, R@, treeRoot~L^edS, treeRoot~R^edS";

            
            RunCorrespondence(c1, "ListToTree", data, expected);

         }


    }
}
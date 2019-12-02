using GraphMatching;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace ConsoleApplication1.test
{

    [TestFixture]
    [Category("Graph")]
    public class GraphMatcherTest
    {
        static ColorDef cRed = ColorDef.Red;
        static ColorDef cGreen = ColorDef.Green;
        static ColorDef cS = new ColorDef("S", "#80A080");
        static ColorDef cT = new ColorDef("T", "#80A0A0");
        static ColorDef cU = new ColorDef("U", "#A0A080");
        static EdgeDescriptor edSimpleRed = new EdgeDescriptor("", cRed);
        static EdgeDescriptor edSimpleGreen = new EdgeDescriptor("", cGreen);
        static EdgeDescriptor edS = new EdgeDescriptor("", cS);
        static EdgeDescriptor edT = new EdgeDescriptor("", cT);
        static EdgeDescriptor edU = new EdgeDescriptor("", cU);
        static String sA = "a";
        static String sB = "b";
        SymbolTable stGlobal;
         //static Vertex vSymbol1 = new Vertex("symbol1", new ValueLiteral ("pay"));

        void Add(SymbolTable st, Named obj)
        {
            st.Add(obj.Name, obj);
        }

        [SetUp]
        public void Init()
        {
            stGlobal = new SymbolTable(null);
            stGlobal.Add("edSimpleRed", edSimpleRed);
            stGlobal.Add("edSimpleGreen", edSimpleGreen);
            stGlobal.Add("edS", edS);
            stGlobal.Add("edT", edT);
            stGlobal.Add("edU", edU);
            stGlobal.Add("sA", sA);
            stGlobal.Add("sB", sB);
            stGlobal.Add("symbol1", new ValueLiteral("pay"));
        }

        public void PerformMatches(string pattern, string data, string expected, bool failureExpected = false, bool fDuplicateAllowed = false)
        {
            try
            {
                REPL repl = new REPL();
                repl.Prepare();
                SymbolTable stTest = new SymbolTable(stGlobal);
                Graph gP = repl.ee.EvaluateGraph(pattern, stTest, fDuplicateAllowed);
                Graph gD = repl.ee.EvaluateGraph(data, stTest, fDuplicateAllowed);

                var m = new PatternFinder(gP, gD, TraceNode.full);
                List<GraphMatch> matches = m.Match();

                List<GraphMatch> gmlExpected = repl.ee.EvaluateGraphMatchList(expected, stTest);
                var gmlc = new GraphMatchListComparison("actual", matches, "expected", gmlExpected);

                if (failureExpected)
                {
                    if (gmlc.Same)
                        Assert.IsFalse(gmlc.Same, "expected failure not found");
                }
                else if (!gmlc.Same)
                    Assert.IsTrue(gmlc.Same, gmlc.Reason);
            }
            catch (Exception e)
            {
                if (failureExpected)
                    return;
                System.Console.WriteLine(e.Message + "\n" + e.StackTrace);
                throw e;
            }
        }


        [Test]
         public void V2E1()
         {
             PerformMatches("a@, b@, a~b", "c@, d@, c~d", "{ a{c}, b{d}, a~b{c~d} }");
         }

         [Test]
         public void V2E2C1()
         {
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a~b{c~d}, b~a{d~c} }," +
                            "{ a{d}, b{c}, a~b{d~c}, b~a{c~d} }");
         }

         [Test]
         public void V2E2C2()
         {
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleGreen",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleGreen",
                            "{ a{c}, b{d}, a~b{c~d}, b~a{d~c} }");
         }

         [Test]
         public void V2V3E2C2()
         {
             PerformMatches("a@, b@, a~b^edSimpleRed",
                            "c@, d@, e@, c~d^edSimpleRed, d~e^edSimpleGreen",
                            "{ a{c}, b{d}, a~b{c~d} }");
         }

         [Test]
         public void V2V3E2P1()
         {
             PerformMatches("A@, B@\"pay\", A~B^edSimpleRed",
                            "a@, b@\"pay\", c@\"load\", a~b^edSimpleRed, a~c^edSimpleRed",
                            "{ A{a}, B{b}, A~B{a~b} }");
         }
        
         [Test]
         public void V2V3E2P2()
         {
             PerformMatches("A@, B@symbol1, A~B^edSimpleRed",
                            "a@, b@symbol1, c@\"load\", a~b^edSimpleRed, a~c^edSimpleRed",
                            "{ A{a}, B{b}, A~B{a~b} }");
         }
        
         [Test]
         public void V3Loop()
         {
             PerformMatches("a@, b@, c@, a~b^edSimpleRed, b~c^edSimpleRed, c~a^edSimpleRed",
                            "d@, e@, f@, d~e^edSimpleRed, e~f^edSimpleRed, f~d^edSimpleRed",
                            "{ a{d}, b{e}, c{f}, a~b{d~e}, b~c{e~f}, c~a{f~d} }," +
                            "{ a{e}, b{f}, c{d}, a~b{e~f}, b~c{f~d}, c~a{d~e} }," +
                            "{ a{f}, b{d}, c{e}, a~b{f~d}, b~c{d~e}, c~a{e~f} }");
         }

         [Test]
         public void V3Loop1()
         {
             PerformMatches(
                 "a@, b@, c@, a~b^edSimpleRed, b~c^edSimpleRed, c~a^edSimpleGreen",
                 "d@, e@, f@, d~e^edSimpleRed, e~f^edSimpleRed, f~d^edSimpleGreen",
                 "{ a{d}, b{e}, c{f}, a~b{d~e}, b~c{e~f}, c~a{f~d} }");
         }

         [Test]
         public void V4V5E3C2()
         {
             PerformMatches("a@, b@, c@, d@, a~b^edSimpleRed, b~c^edSimpleRed, c~d^edSimpleRed",
                            "e@, f@, g@, h@, i@, e~f^edSimpleRed, f~g^edSimpleRed, g~h^edSimpleRed, h~i^edSimpleGreen",
                            "{ a{e}, b{f}, c{g}, d{h}, a~b{e~f}, b~c{f~g}, c~d{g~h} }");
         }

         [Test]
         public void V4V4E3C1()
         {
             PerformMatches("a@, b@, c@, k@, a~b^edSimpleRed, b~c^edSimpleRed, b~k^edSimpleRed",
                            "e@, f@, g@, h@, j@, e~f^edSimpleRed, f~g^edSimpleRed, f~j^edSimpleRed",
                            "{ a{e}, b{f}, c{g}, k{j}, a~b{e~f}, b~c{f~g}, b~k{f~j} }," +
                            "{ a{e}, b{f}, c{j}, k{g}, a~b{e~f}, b~c{f~j}, b~k{f~g} }");
         }

         [Test]
         public void V6V6E5C1()
         {
             PerformMatches("a@, b@, c@, d@, k@, l@, a~b^edSimpleRed, b~c^edSimpleRed, c~d^edSimpleRed, b~k^edSimpleRed, k~l^edSimpleRed",
                            "e@, f@, g@, h@, j@, m@, e~f^edSimpleRed, f~g^edSimpleRed, g~h^edSimpleRed, f~j^edSimpleRed, j~m^edSimpleRed",
                            "{ a{e}, b{f}, c{g}, d{h}, k{j}, l{m}, a~b{e~f}, b~c{f~g}, c~d{g~h}, b~k{f~j}, k~l{j~m} }," +
                            "{ a{e}, b{f}, c{j}, d{m}, k{g}, l{h}, a~b{e~f}, b~c{f~j}, c~d{j~m}, b~k{f~g}, k~l{g~h} }");
         }

         [Test]
         public void V4V12E12C1()
         {
             PerformMatches("A@, B@, C@, D@, C~A^edS, C~B^edS, D~C^edS",
                            "a@, b@, c@, d@, e@, f@, g@, h@, i@, j@, k@, l@, m@, c~a^edS, c~b^edS, d~c^edS, d~f^edS, e~d^edS, e~g^edS, f~e^edS, f~m^edS, g~h^edS, g~i^edS, m~k^edS, m~j^edS",
                            "{ A{a}, B{b}, C{c}, D{d}, C~A{c~a}, C~B{c~b}, D~C{d~c} }," +
                            "{ A{b}, B{a}, C{c}, D{d}, C~A{c~b}, C~B{c~a}, D~C{d~c} }," +
                            "{ A{i}, B{h}, C{g}, D{e}, C~A{g~i}, C~B{g~h}, D~C{e~g} }," +
                            "{ A{h}, B{i}, C{g}, D{e}, C~A{g~h}, C~B{g~i}, D~C{e~g} }," +
                            "{ A{j}, B{k}, C{m}, D{f}, C~A{m~j}, C~B{m~k}, D~C{f~m} }," +
                            "{ A{k}, B{j}, C{m}, D{f}, C~A{m~k}, C~B{m~j}, D~C{f~m} }," +
                            "{ A{c}, B{f}, C{d}, D{e}, C~A{d~c}, C~B{d~f}, D~C{e~d} }," +
                            "{ A{f}, B{c}, C{d}, D{e}, C~A{d~f}, C~B{d~c}, D~C{e~d} }," +
                            "{ A{m}, B{e}, C{f}, D{d}, C~A{f~m}, C~B{f~e}, D~C{d~f} }," +
                            "{ A{e}, B{m}, C{f}, D{d}, C~A{f~e}, C~B{f~m}, D~C{d~f} }," +
                            "{ A{g}, B{d}, C{e}, D{f}, C~A{e~g}, C~B{e~d}, D~C{f~e} }," +
                            "{ A{d}, B{g}, C{e}, D{f}, C~A{e~d}, C~B{e~g}, D~C{f~e} }");
         }

         [Test]
         public void V3V3E4C2()
         {
             PerformMatches("A@, B@, C@, A~B^edS, B~C^edS, A~B^edT, B~C^edT",
                            "a@, b@, c@, a~b^edS, b~c^edS, a~b^edT, b~c^edT",
                            "{ A{a}, B{b}, C{c}, A~B^edS{a~b^edS}, B~C^edS{b~c^edS}, A~B^edT{a~b^edT}, B~C^edT{b~c^edT} }");
          }

         [Test]
         public void V4V5E5C1()
         {
             PerformMatches("A@, B@, C@, D@, A~B^edS, A~C^edS, A~D^edS, B~C^edS, C~D^edS",
                            "a@, b@, c@, d@, e@, a~b^edS, a~c^edS, a~d^edS, a~e^edS, b~c^edS, c~d^edS, d~e^edS",
                            "{ A{a}, B{b}, C{c}, D{d}, A~B^edS{a~b^edS}, A~C^edS{a~c^edS}, A~D^edS{a~d^edS}, B~C^edS{b~c^edS}, C~D^edS{c~d^edS}}," +
                            "{ A{a}, B{c}, C{d}, D{e}, A~B^edS{a~c^edS}, A~C^edS{a~d^edS}, A~D^edS{a~e^edS}, B~C^edS{c~d^edS}, C~D^edS{d~e^edS}}");
         }

         [Test]
         public void V4V4E4C2()
         {
             PerformMatches("A@, B@, C@, D@, A~B^edT, B~C^edS, D~B^edS",
                            "a@, b@, c@, d@, a~b^edT, b~c^edS, b~d^edS, c~b^edS, d~b^edS",
                            "{ A{a}, B{b}, C{c}, D{d}, A~B^edT{a~b^edT}, B~C^edS{b~c^edS}, D~B^edS{d~b^edS}}," +
                            "{ A{a}, B{b}, C{d}, D{c}, A~B^edT{a~b^edT}, B~C^edS{b~d^edS}, D~B^edS{c~b^edS}}");
         }

         [Test]
         public void V2SE1()
         {
             PerformMatches("A@, b@, A~b", "a@, b@, a~b", "{ A{a}, b{b}, A~b{a~b} }", false, true);
         }

         [Test]
         public void V4V4E4C2S2()
         {
             PerformMatches("A@, B@, C@, D@, x@, y@, A~B^edT, B~C^edS, D~B^edS, B~x^edU, D~y^edU",
                            "a@, b@, c@, d@, x@, y@, a~b^edT, b~c^edS, b~d^edS, c~b^edS, d~b^edS, b~x^edU, d~y^edU",
                            "{ A{a}, B{b}, C{c}, D{d}, x{x}, y{y}, A~B^edT{a~b^edT}, B~C^edS{b~c^edS}, D~B^edS{d~b^edS}, B~x^edU{b~x^edU}, D~y^edU{d~y^edU}}", false, true);
         }

         [Test]
         public void V2E2C1_Mismatches()
         {
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{cX}, b{d}, a~b{c~d}, b~a{d~c} }, { a{d}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b1{d}, a~b{c~d}, b~a{d~c} }, { a{d}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a1~b{c~d}, b~a{d~c} }, { a{d}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a~b1{c~d}, b~a{d~c} }, { a{d}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a~c{c~d}, b~a{d~c} }, { a{d}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a~b{c~d}, b~a{d~c} }, { a{d1}, b{c}, a~b{d~c}, b~a{c~d} }",
                            true);
             PerformMatches("a@, b@, a~b^edSimpleRed, b~a^edSimpleRed",
                            "c@, d@, c~d^edSimpleRed, d~c^edSimpleRed",
                            "{ a{c}, b{d}, a~b{c~d}, b~a{d~c} }, { a{d}, a{c}, a~b{d~c}, b~a{c~d} }",
                            true);
         }
    }
}
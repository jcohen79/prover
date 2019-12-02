using GrammarDLL;
using GraphMatching;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace reslab.test
{

    /// <summary>
    /// Semantic resolution for puzzle
    /// </summary>
    public class PsmPuzzle : Psm
    {
        bool fHas3To4;

        Pso pso1;
        Pso pso2;
        Pso pso3;
        Pso pso4;
        Pso pso5;
        static Pso psoNone = new Pso("none");

        static bool E(List<Pso> rgpsoArgs)
        {
            if (psoNone == rgpsoArgs[0])
                return false;
            return rgpsoArgs[0] == rgpsoArgs[1];
        }

        bool D(List<Pso> rgpsoArgs)
        {
            Pso a = rgpsoArgs[0];
            Pso b = rgpsoArgs[1];
            if (psoNone == a || psoNone == b)
                return false;
            if (a == pso1)
                return b == pso2 || b == pso4;
            if (a == pso2)
                return b == pso3;
            if (a == pso3)
                if (fHas3To4)
                    return b == pso4 || b == pso5;
                else
                    return b == pso5;
            if (a == pso4)
                return b == pso5;
            if (a == pso5)
                return false;
            Debug.Assert(false);
            return false;
        }

        bool B(List<Pso> rgpsoArgs)
        {
            Pso a = rgpsoArgs[0];
            Pso b = rgpsoArgs[1];
            if (psoNone == a || psoNone == b)
                return false;
            if (a == pso1)
                return b == pso2 || b == pso3 || b == pso4 || b == pso5;
            if (a == pso2)
                return b == pso3 || b == pso5;
            if (a == pso3)
                if (fHas3To4)
                    return b == pso4 || b == pso5;
                else
                    return b == pso5;
            if (a == pso4)
                return b == pso5;
            if (a == pso5)
                return false;
            Debug.Assert(false);
            return false;
        }

        bool U(List<Pso> rgpsoArgs)
        {
            if (psoNone == rgpsoArgs[0] || psoNone == rgpsoArgs[1])
                return false;
            if (B(rgpsoArgs))
                return false;
            if (E(rgpsoArgs))
                return false;
            List<Pso> rgRev = new List<Pso>();
            rgRev.Add(rgpsoArgs[1]);
            rgRev.Add(rgpsoArgs[0]);
            if (B(rgRev))
                return false;
            return true;
        }

        bool S1(List<Pso> rgpsoArgs)
        {
            return rgpsoArgs[0] == pso1;
        }

        bool S2(List<Pso> rgpsoArgs)
        {
            return rgpsoArgs[0] == pso2;
        }

        bool S3(List<Pso> rgpsoArgs)
        {
            return rgpsoArgs[0] == pso3;
        }

        bool S4(List<Pso> rgpsoArgs)
        {
            return rgpsoArgs[0] == pso4;
        }

        bool S5(List<Pso> rgpsoArgs)
        {
            return rgpsoArgs[0] == pso5;
        }

        Pso qS11(List<Pso> rgpsoArgs)
        {
            return pso1;
        }

        Pso qS21(List<Pso> rgpsoArgs)
        {
            return pso2;
        }

        Pso qS31(List<Pso> rgpsoArgs)
        {
            return pso3;
        }

        Pso qS41(List<Pso> rgpsoArgs)
        {
            return pso4;
        }

        Pso qS51(List<Pso> rgpsoArgs)
        {
            return pso5;
        }

        Pso qB1(List<Pso> rgpsoArgs)
        {
            Pso psoY = rgpsoArgs[0];
            Pso psoX = rgpsoArgs[1];
            if (psoY == pso1)
                return psoNone;
            if (psoY == pso2 || psoY == pso4)
                return psoNone;
            if (psoY == pso3)
            {
                if (psoX == pso1)
                    return pso2;
                return psoNone;
            }
            if (psoY == pso5)
            {
                if (psoX == pso1 || psoX == pso2)
                    return pso3;
                return psoNone;
            }
            return psoNone;
        }

        /// <summary>
        /// Suppose the existence of x,y such that U(x,y)
        /// </summary>
        /// <param name="rgpsoArgs"></param>
        /// <returns></returns>
        Pso qCx1(List<Pso> rgpsoArgs)
        {
            return pso2;
        }

        Pso qCy1(List<Pso> rgpsoArgs)
        {
            return pso3;
        }

        public PsmPuzzle(bool fHas3To4) : base("puzzle" + (fHas3To4 ? "With" : "Without"))
        {
            this.fHas3To4 = fHas3To4;
            pso1 = psoRegObject(new Pso("1"));
            pso2 = psoRegObject(new Pso("2"));
            pso3 = psoRegObject(new Pso("3"));
            pso4 = psoRegObject(new Pso("4"));
            pso5 = psoRegObject(new Pso("5"));
            RegPredicate("E", E);
            RegPredicate("B", B);
            RegPredicate("D", D);
            RegPredicate("U", U);
            RegPredicate("S1", S1);
            RegPredicate("S2", S2);
            RegPredicate("S3", S3);
            RegPredicate("S4", S4);
            RegPredicate("S5", S5);
            RegFunction("qB1", qB1);
            RegFunction("qS11", qS11);
            RegFunction("qS21", qS21);
            RegFunction("qS31", qS31);
            RegFunction("qS41", qS41);
            RegFunction("qS51", qS51);
            RegFunction("qCx1", qCx1);
            RegFunction("qCy1", qCy1);
        }
    }


    [TestFixture]
    [Category("Proof Examples")]
    class PuzzleTest
    {
        LogRes logRes = new LogRes();
        string stPuzzleBase = @"
\forall x \forall y (B(x,y) \lor B(y,x) \lor U(x,y) \lor E(x,y)),  
\forall x \forall y (E(x,y) \rightarrow \neg U(x,y)),  
\forall x \forall y (E(x,y) \rightarrow \neg B(x,y)),  
\forall x \forall y (U(x,y) \rightarrow U(y,x)),  
\forall x \forall y (U(x,y) \rightarrow \neg E(y,x)),  
\forall x \forall y (U(x,y) \rightarrow \neg B(x,y)),
\forall x \forall y (B(x,y) \rightarrow \neg U(x,y)),  
\forall x \forall y (B(x,y) \rightarrow \neg B(y,x)),  
\forall x \forall y (B(x,y) \rightarrow \neg E(y,x)),  
\forall x \forall y \forall z (B(x,y) \land B(y,z) \rightarrow B(x,z)),  

\forall x E(x,x),  
\forall x \forall y (E(x,y) \rightarrow E(y,x)),  
\forall x \forall y \forall z (E(x,y) \land E(y,z) \rightarrow E(x,z)),  

\forall x (S1(x) \lor S2(x) \lor S3(x) \lor S4(x) \lor S5(x)),
\forall x \forall y (B(x,y) \leftrightarrow D(x,y) \lor (\exists z@qB (B(x,z) \land D(z,y)))),

\forall x \forall y (S1(x) \rightarrow (D(x,y) \leftrightarrow S2(y) \lor S4(y))),
\forall x \forall y (S2(x) \rightarrow (D(x,y) \leftrightarrow S3(y))),
\forall x \forall y (S3(x) \rightarrow (D(x,y) \leftrightarrow S5(y))),
\forall x \forall y (S4(x) \rightarrow (D(x,y) \leftrightarrow S5(y))),
\forall x \forall y (S5(x) \rightarrow \neg D(x,y)),
\forall x \forall y (S1(y) \rightarrow \neg D(x,y)),

\forall x \forall y (S1(x) \rightarrow (S1(y) \leftrightarrow  E(x,y))),  
\forall x \forall y (S2(x) \rightarrow (S2(y) \leftrightarrow  E(x,y))),  
\forall x \forall y (S3(x) \rightarrow (S3(y) \leftrightarrow  E(x,y))),  
\forall x \forall y (S4(x) \rightarrow (S4(y) \leftrightarrow  E(x,y))),  
\forall x \forall y (S5(x) \rightarrow (S5(y) \leftrightarrow  E(x,y))),  

\forall x (S1(x) \rightarrow \neg S2(x)),  
\forall x (S1(x) \rightarrow \neg S3(x)),  
\forall x (S1(x) \rightarrow \neg S4(x)),  
\forall x (S1(x) \rightarrow \neg S5(x)),  
\forall x (S2(x) \rightarrow \neg S1(x)),  
\forall x (S2(x) \rightarrow \neg S3(x)),  
\forall x (S2(x) \rightarrow \neg S4(x)),  
\forall x (S2(x) \rightarrow \neg S5(x)),  
\forall x (S3(x) \rightarrow \neg S1(x)),  
\forall x (S3(x) \rightarrow \neg S2(x)),  
\forall x (S3(x) \rightarrow \neg S4(x)),  
\forall x (S3(x) \rightarrow \neg S5(x)),  
\forall x (S4(x) \rightarrow \neg S1(x)),  
\forall x (S4(x) \rightarrow \neg S2(x)),  
\forall x (S4(x) \rightarrow \neg S3(x)),  
\forall x (S4(x) \rightarrow \neg S5(x)),  
\forall x (S5(x) \rightarrow \neg S1(x)),  
\forall x (S5(x) \rightarrow \neg S2(x)),  
\forall x (S5(x) \rightarrow \neg S3(x)),  
\forall x (S5(x) \rightarrow \neg S4(x)),

\exists x@qS1 S1(x),
\exists x@qS2 S2(x),
\exists x@qS3 S3(x),
\exists x@qS4 S4(x),
\exists x@qS5 S5(x)";



        // [Test]
        public void ProveParsedPuzzlesWithU()
        {
            LogRes logRes = new LogRes();
            Psm psm = new PsmPuzzle(false);
            SkoTest.ProveParsed(stPuzzleBase + @"
\Rightarrow
\exists x \exists y U(x, y)
", imd: psm, irr: logRes);
        }

       //  [Test]
        public void ProveParsedPuzzlesWithoutU()
        {
            LogRes logRes = new LogRes();
            Psm psm = new PsmPuzzle(true);
            SkoTest.ProveParsed(stPuzzleBase + @"
, \forall x \forall y (S3(x) \land S4(y) \rightarrow B(x, y))
\Rightarrow
\neg \exists x@qCx \exists y@qCy U(x, y)
", imd: psm, irr: logRes);
        }

        public class PsmSimple : Psm
        {
            Pso pso1;
            Pso pso2;

            static bool E(List<Pso> rgpsoArgs)
            {
                return rgpsoArgs[0] == rgpsoArgs[1];
            }

            bool S1(List<Pso> rgpsoArgs)
            {
                return rgpsoArgs[0] == pso1;
            }

            bool S2(List<Pso> rgpsoArgs)
            {
                return rgpsoArgs[0] == pso2;
            }

            Pso qy1(List<Pso> rgpsoArgs)
            {
                return pso1;
            }


            public PsmSimple() : base("Simple")
            {
                pso1 = psoRegObject(new Pso("1"));
                pso2 = psoRegObject(new Pso("2"));
                RegPredicate("S1", S1);
                RegPredicate("S2", S2);
                RegFunction("qy1", qy1);
            }
        }
    }
}

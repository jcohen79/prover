#define VALIDATE

using GrammarDLL;
using NUnit.Framework;

namespace reslab.test
{
    [TestFixture]
    [Category("Clause")]
    class CknTest
    {
        Ckn cknRoot;
        LParse lparse;
        Res resNull = null;

        Asc ascFromStr(string stClause)
        {
            return Asc.ascFromLsx(lparse.lsxParse(stClause));
        }

        void Init(string stClause)
        {
            lparse = new LParse();
            Sko.AddSyms(lparse, new ExpressionEvaluatorGrammar());
            SatTest.UsualVariables(lparse);
            cknRoot = Ckn.cknRoot(ascFromStr(stClause));
        }

        void Add (string stClause)
        {
            cknRoot.AddAsc(ascFromStr(stClause));
#if VALIDATE
            cknRoot.Validate(null);
#endif
        }

        void fSubsumed(string stClause)
        {
            Assert.IsNotNull(cknRoot.ascSubsumes(ascFromStr(stClause), true, resNull));
        }

        void fNotSubsumed(string stClause)
        {
            Assert.IsNull(cknRoot.ascSubsumes(ascFromStr(stClause), true, resNull));
        }

        void fSubsumesOld(string stClause)
        {
            Assert.IsNotNull(cknRoot.ascSubsumes(ascFromStr(stClause), false, resNull));
        }

        void fNotSubsumesOld(string stClause)
        {
            Assert.IsNull(cknRoot.ascSubsumes(ascFromStr(stClause), false, resNull));
        }

        [Test]
        public void CknSimple()
        {
            Init("(nil (A c b))");
            Add("(nil (A b c))");
            Add("(nil (A c d))");
            Add("(nil (B c d))");
            Add("(nil (B x d))");
            Add("(nil (B y x))");

            fSubsumesOld("(nil (A x d))");

            fSubsumed("(nil (A c d))");
            fNotSubsumed("(nil (A c e))");
            fSubsumesOld("(nil (A c d))");
            fNotSubsumesOld("(nil (A c e))");

            fSubsumed("(nil (B (B a c) d))");
            fNotSubsumed("(nil (A (B a c) e))");
            fNotSubsumesOld("(nil (A x e))");
        }

        [Test]
        public void CknTwo()
        {
            Init("(nil (A c b) (C a b))");
            Add("(nil (A b c) (B c d))");
            Add("(nil (A c d) (B a d))");
            Add("(nil (B c d) (B y x))");
            Add("(nil (B x d) (A c d))");
            Add("(nil (B y x) (C a b))");

            fSubsumed("(nil (B (B a c) d) (A c d))");

            fSubsumed("(nil (A c b) (C a b))");
            fSubsumed("(nil (B c d))");
            fNotSubsumed("(nil (A c e)  (C a b))");
            fNotSubsumed("(nil (B c e))");
            fSubsumesOld("(nil (A c d) (B a d))");
            fNotSubsumesOld("(nil (A c e) (C a b))");

            fSubsumesOld("(nil (A x d) (B a d))");
            fSubsumesOld("(nil (A c x) (B a x))");
            fNotSubsumesOld("(nil (A x b) (C x b))");   // different values for x
            fNotSubsumed("(nil (B (B a c) d) (C c c))");
            fNotSubsumesOld("(nil (A x e) (B x d))");

        }
    }
}

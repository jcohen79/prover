using NUnit.Framework;


namespace reslab.test
{
    [TestFixture]
    [Category("Clause")]
    class PvmTest
    {
        static Vbv vbvOrigin = new Vbv(null);

        public void TestPvs(string stLeft, string stRight,
                            sbyte nVblId, ushort nValuePosn,
                            string stExpected)
        {
            Cbd cbd = new Cbd();
            Asc ascA = cbd.ascBuild(stLeft);
            Asc ascB = cbd.ascBuild(stRight);

            Pvm pvm = new Pvm();
            Pvb pvbA = new Pva(ascA, vbvOrigin).Add(pvm);
            Pvb pvbB = new Pva(ascB, vbvOrigin).Add(pvm);
            pvbB.mofOutput = new Mof(ascB, Vie.nIdxA);

            Pvb pvsAB = new Pvs(pvbA, pvbB, nVblId, nValuePosn, vbvOrigin, null).Add(pvm);
            Assert.IsTrue(pvm.fPerform());
            Assert.AreEqual(pvsAB.ascResult, cbd.ascBuild(stExpected));
        }

        [Test]
        public void Pv2s()
        {
            TestPvs("(()  (a b x) (d e f))",
                    "(((g h))  (i j k))",
                    0, 4,
                    "(() (a b (i j k)) (d e f))");
            TestPvs("(()  (a b x) (d x y))",
                    "(((g h))  (i j z))",
                    0, 4,
                    "(( )  (a b (i j @0)) (d (i j @0) @1))");
            TestPvs("(()  (a b x) (d x y))",
                    "(()  (i j z))",
                    0, 2,
                    "(()  (a b (i j @0)) (d (i j @0) @1))");
        }

        public void TestPvc(uint nLeftMask, string stLeft, 
                            uint nRightMask, string stRight,
                            string stExpected)
        {
            Cbd cbd = new Cbd();
            Asc ascA = cbd.ascBuild(stLeft);
            Asc ascB = cbd.ascBuild(stRight);

            Pvm pvm = new Pvm();
            Mrs mrs = new Mrs(pvm);
            mrs.mesLeft.pvbLatest = new Pva(ascA, vbvOrigin).Add(pvm);
            mrs.mesRight.pvbLatest = new Pva(ascB, vbvOrigin).Add(pvm);
            Asc ascInferred = new Asc(null, null);    // why null?

            Pvc pvrAB = new Pvc(mrs, ascInferred, vbvOrigin);
            pvrAB.nMaskA = nLeftMask;
            pvrAB.nMaskB = nRightMask;
            pvrAB.Add(pvm);
            Assert.IsTrue(pvm.fPerform());
            Assert.AreEqual(pvrAB.ascResult, cbd.ascBuild(stExpected));
        }

        [Test]
        public void Pv3c()
        {
            TestPvc(1, "(()  (Pa) (Q))",
                    1, "(((Pa) (Pb))  (Pc))",
                    "(((Pb)) (Q) (Pc))");
            TestPvc(2, "(((c e))  (a b x) (d x y))", 
                    1, "(((a b y))  (i j z))",
                    "(((c e)) (d @0 @1) (i j @2))");
        }

        void TestPvp(string stA, string stB, ushort nReplaceAt, ushort nReplaceFrom, ushort nReplaceWith, string stExpected)
        {
            Cbd cbd = new Cbd();
            Asc ascA = cbd.ascBuild(stA);
            Asc ascB = cbd.ascBuild(stB);

            Pvm pvm = new Pvm();
            Pvb pvbA = new Pva(ascA, vbvOrigin).Add(pvm);
            Pvb pvbB = new Pva(ascB, vbvOrigin).Add(pvm);
            pvbB.mofOutput = new Mof(null, null);

            Pvb pvrAB = new Pvp(pvbA, pvbB, nReplaceAt, nReplaceFrom, nReplaceWith, vbvOrigin).Add(pvm);
            Assert.IsTrue(pvm.fPerform());
            Assert.AreEqual(pvrAB.ascResult, cbd.ascBuild(stExpected));

        }

        [Test,Ignore("broken")]
        public void Pv5p()
        {
            TestPvp("(((c e))  (a  x (f1 x)) (d x y))",
                    "(((a b y)) (= (f1 x) (f2 e y))  (i j z))",
                    6, 6, 8,
                    "(((c e) (a b @0)) (a @1 (f2 e @0)) (d @1 @2) (i j @3))");
        }

        [Test]
        public void Pv6p1()
        {
            TestPvp("(nil  (= a b))",
                    "(nil  (= b c))",
                    4, 3, 4,
                    "(nil (= a c))");
            TestPvp("(nil  (= (f  x (f  y e)) (f  (f x y) e))))",
                    "(nil  (=  (f  (f  z w) e) (f z w)))",
                    8, 3, 8,
                    "(nil (= (f  x (f  y e)) (f x y)))");
        }

        [Test]
        public void Pv6p2()
        {
            TestPvp("((a)  (= a b))",
                    "((b)  (= b c))",
                    5, 4, 5,
                    "((a b) (= a c))");
        }
    }
}

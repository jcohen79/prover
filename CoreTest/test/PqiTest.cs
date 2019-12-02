using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphMatching;
using NUnit.Framework;
using System.Diagnostics;

namespace reslab.test
{
    public class Rbt : Bid, Rib
    {
        public Ipr iprSize;

        public Ipr iprComplexity()
        {
            return iprSize;
        }

        public Lsx lsxTo(Asy asy)
        {
            throw new NotImplementedException();
        }
    }

    public class BasT : Bas
    {
        public BasT(Gnb gnp, bool fRight, Bab basPrev, Ipr iprSize) : base(gnp, fRight, basPrev, iprSize)
        {
        }
    }

    public class Tpc
    {
        public Bas basRight;
        public Bas basLeft;
        public Pqi pqi;
        GnpR gnp = new GnpR(null, null);
        public int nTotal = 0;

        public Tpc()
        {
            basRight = bastMake();
            basLeft = bastMake();
            pqi = Pqi.pqiMake(true, basRight, basLeft, null);
        }

        BasT bastMake()
        {
            return new BasT(gnp, true, null, Npr.nprOnly);
        }

        public void AddLeft(Ipr iprSize = null)
        {
            Rbt rbt = new Rbt();
            rbt.iprSize = iprSize;
            pqi.basLeft.AddToBas(rbt);
        }

        public void AddRight(Ipr iprSize = null)
        {
            Rbt rbt = new Rbt();
            rbt.iprSize = iprSize;
            pqi.basRight.AddToBas(rbt);
        }

        public bool fGet()
        {
            Bel belLeft;
            Bel belRight;
            if (!pqi.fGetPairFromPqi(out belLeft, out belRight))
            {
                return false;
            }
            Debug.WriteLine("fGet " + ((Rbt)belLeft.ribVal).iprSize + " " + ((Rbt)belRight.ribVal).iprSize);
            nTotal++;
            return true;
        }

        public int nCount()
        {
            int nNum = 0;
            while (fGet())
                nNum++;
            Assert.IsFalse(fGet());
            return nNum;
        }

        public static void TestSeq (params int[] rgSeq)
        {
            Tpc tpc = new Tpc();
            int nLeft = 0;
            int nRight = 0;
            for (int i = 0; i < rgSeq.Length; i++)
            {
                int nEntry = rgSeq[i];
                if (nEntry < 0)
                {
                    for (int j = 0; j < -nEntry; j++)
                        tpc.AddLeft();
                    nLeft -= nEntry;
                }
                else if (nEntry > 0)
                {
                    for (int j = 0; j < nEntry; j++)
                        tpc.AddRight();
                    nRight += nEntry;
                }
                else
                    tpc.fGet();
            }
            tpc.nCount();
            Assert.AreEqual(nLeft * nRight, tpc.nTotal);
        }

    }


    public class Tpz
    {
        public int nTotal = 0;
        Res res = new Res();
        GnpR gnp;
        int nNum = 0;

        public Tpz()
        {
            gnp = new GnpR(res, null);
        }

        public void AddLeft(int nSize)
        {
            sbyte[] rgnStep = new sbyte[1+nSize];
            rgnStep[0] = (sbyte)nNum++;
            Asc ascStep = new Asc(rgnStep, null);
            gnp.AddToFiltered(false, ascStep);
        }

        public void AddRight(int nSize)
        {
            sbyte[] rgnStep = new sbyte[1+nSize];
            rgnStep[0] = (sbyte) (nNum++);
            Asc ascStep = new Asc(rgnStep, null);
            gnp.AddToFiltered(true, ascStep);
        }

        public bool fGet()
        {
            Asc ascLeft;
            Asc ascRight;
            if (!gnp.fGetNextPair(out ascLeft, out ascRight))
            {
                return false;
            }
           // Debug.WriteLine("fGet " + ascLeft.rgnTree.Length-1 + " " + ascRight.rgnTree.Length-1);
            nTotal++;
            return true;
        }

        public int nCount()
        {
            int nNum = 0;
            while (fGet())
                nNum++;
            Assert.IsFalse(fGet());
            return nNum;
        }

        public static void TestSeq (params int[] rgSeq)
        {
            Tpc tpc = new Tpc();
            int nLeft = 0;
            int nRight = 0;
            for (int i = 0; i < rgSeq.Length; i++)
            {
                int nEntry = rgSeq[i];
                if (nEntry < 0)
                {
                    for (int j = 0; j < -nEntry; j++)
                        tpc.AddLeft();
                    nLeft -= nEntry;
                }
                else if (nEntry > 0)
                {
                    for (int j = 0; j < nEntry; j++)
                        tpc.AddRight();
                    nRight += nEntry;
                }
                else
                    tpc.fGet();
            }
            tpc.nCount();
            Assert.AreEqual(nLeft * nRight, tpc.nTotal);
        }
        public static void TestSeqSized (params int[] rgSeq)
        {
            Tpz tpc = new Tpz();
            int nLeft = 0;
            int nRight = 0;
            int nPresent = 0;
            for (int i = 0; i < rgSeq.Length; i++)
            {
                int nEntry = rgSeq[i];
                if (nEntry < 0)
                {
                    tpc.AddLeft(-nEntry);
                    nLeft++;
                    nPresent += nRight;
                }
                else if (nEntry > 0)
                {
                    tpc.AddRight(nEntry);
                    nRight++;
                    nPresent += nLeft;
                }
                else
                {
                    tpc.fGet();
                    nPresent--;
                }
#if false
                while (nPresent > 0)
                {
                    tpc.fGet();
                    nPresent--;
                }
#endif
            }
            tpc.nCount();
            Assert.AreEqual(nLeft * nRight, tpc.nTotal);
        }
    }


    [TestFixture]
    [Category("Clause")]
    class PqiTest
    {
        [Test]
        public void PqiStart ()
        {
            Tpc tpc = new Tpc();
            Assert.IsFalse(tpc.fGet());
            Assert.IsFalse(tpc.fGet());

            tpc.AddLeft();
            Assert.IsFalse(tpc.fGet());

            tpc.AddRight();
            Assert.IsTrue(tpc.fGet());
            Assert.IsFalse(tpc.fGet());
            Assert.IsFalse(tpc.fGet());

            tpc.AddLeft();
            tpc.nCount();
            Assert.AreEqual(tpc.nTotal, 2);

            tpc.AddRight();
            tpc.nCount();
            Assert.AreEqual(tpc.nTotal, 4);

            tpc.AddRight();
            tpc.nCount();
            Assert.AreEqual(tpc.nTotal, 6);
        }

        [Test]
        public void PqiSeq()
        {
            Tpc.TestSeq(1, 2, -1, 3);
            Tpc.TestSeq(2, 2);
        }

        [Test]
        public void PqiRnd()
        {
            Random r = new Random(100);
            for (int i = 3; i < 40; i++)
            {
                int nLen = r.Next(i);
                int[] rgnCase = new int[nLen];

                Debug.Write("\nTpc.TestSeq(");
                for (int j = 0; j < nLen; j++)
                {
                    int n1 = r.Next(3) - 1;
                    int n2 = r.Next(3) - 1;
                    int n3 = r.Next(3) - 1;
                    int n4 = r.Next(3) - 1;
                    int n = n1 + n2 + n3 + n4;
                    rgnCase[j] = n;
                    if (j == 0)
                        Debug.Write(" " + n);
                    else
                        Debug.Write(", " + n);
                }
                Debug.WriteLine(");");
                Tpc.TestSeq(rgnCase);
            }
        }

        [Test]
        public void PqiSized1 ()
        {
            Tpz.TestSeqSized(-3, 2, 9, -9, -1, -6, 1, 8, 0, 0, -2, 5, 7, 6, -10, 5, 3, 0);

            Tpz.TestSeqSized(7, 2, 9, 5, -4, -3, 1, 8, -8, -5, 6, 5, -9, 7, -3, 7, 5, -8, -9, -9, -8, 0, 7, 3, -8, 2, 2, -6, 3, -8, 4, -3, 3, 8, -3, -2, 5, 3, -3, 4, -9, -4, -10, 7, 8, 2, -4, 6, -2, -3);

            Tpz.TestSeqSized(2, -1, 0, 0, -17, 0, 0, 5, 0, 0, 8, 0, 8, 0, 0, 7, 0, 8, 0, 8);
            Tpz.TestSeqSized(2, -1, 0, 0, 0);
            Tpz.TestSeqSized(-1, 2, 0, 0, 0);
            Tpz.TestSeqSized(1, 2, -3, -5, -4);
            Tpz.TestSeqSized(1, 2, -3, 0, 0, -5, -4);
            /*

            Tpc tpc = new Tpc();
            Assert.IsFalse(tpc.fGet());
            Assert.IsFalse(tpc.fGet());

            Ipr ipr1 = Prn.prnObtain(1);
            Ipr ipr2 = Prn.prnObtain(2);
            Ipr ipr3 = Prn.prnObtain(3);
            Ipr ipr4 = Prn.prnObtain(4);
            Ipr ipr5 = Prn.prnObtain(5);

            tpc.AddLeft(ipr1);
            tpc.AddLeft(ipr2);
            tpc.AddRight(ipr3);
            tpc.AddRight(ipr5);
            Assert.IsTrue(tpc.fGet());  // 1,3
            Assert.IsTrue(tpc.fGet());  // 1,5
            Assert.IsTrue(tpc.fGet());  // 2,3

            Assert.IsTrue(tpc.fGet());  // 2,5
            tpc.AddRight(ipr4);
            Assert.IsTrue(tpc.fGet());  // 1,4
            Assert.IsTrue(tpc.fGet());  // 2,4

            Assert.IsFalse(tpc.fGet());
            */
        }

        [Test]
        public void PqiSizedRnd()
        {
            Random r = new Random(100);
            for (int i = 1; i < 100; i++)
            {
                int nLen = i+5; //  r.Next(i);
                int[] rgnCase = new int[nLen];

                Debug.Write("\nTpz.TestSeqSized(");
                for (int j = 0; j < nLen; j++)
                {
                    int n = r.Next(20) - 10;
                    rgnCase[j] = n;
                    if (j == 0)
                        Debug.Write(" " + n);
                    else
                        Debug.Write(", " + n);
                }
                Debug.WriteLine(");");
                Tpz.TestSeqSized(rgnCase);
            }
        }


    }
}

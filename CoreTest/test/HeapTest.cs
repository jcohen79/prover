using NUnit.Framework;
using System;

namespace reslab.test
{

    public class Tcel : Icmp
    {
        public int nValue;

        public Tcel (int nValue)
        {
            this.nValue = nValue;
        }

        public Ipr iprPriority()
        {
            return Prn.prnObtain(nValue);
        }

        public override string ToString()
        {
            return "<" + nValue + ">";
        }
    }

    [TestFixture]
    [Category("Clause")]
    class HeapTest
    {
        void Add(Heap<Tcel> heap, int nVal)
        {
            heap.Add(new Tcel(nVal));
            heap.Validate();
        }

        void CheckRest(Heap<Tcel> heap, int nExpectedSum)
        {
            Tcel tPrev = null;
            int k = 0;
            int nFound = 0;
            while (!heap.fEmpty())
            {
                Tcel tNext = heap.tGet();
                nFound += tNext.nValue;
                if (tPrev != null)
                    Assert.IsTrue(tPrev.nValue <= tNext.nValue);
                tPrev = tNext;
                k++;
                heap.Validate();
            }
            Assert.AreEqual(nExpectedSum, nFound);
        }

        [Test]
        public void HeapCheck1()
        {
            Heap<Tcel> heap = new Heap<Tcel>();
            Add(heap, 3);
            Add(heap, 1);
            Add(heap, 2);
            CheckRest(heap, 6);
        }

        [Test]
        public void HeapCheck2()
        {
            for (int j = 1; j < 20; j++)
            {
                int nExpected = 0;
                Random r = new Random(123456);
                Heap<Tcel> heap = new Heap<Tcel>();
                for (int i = 0; i < j*j; i++)
                {
                    int iR = r.Next(2000);
                    Add(heap, iR);
                    nExpected += iR;
                }
                CheckRest(heap, nExpected);
            }
        }
        [Test]
        public void HeapCheck3()
        {
            for (int n = 0; n < 1000; n++)
                HeapCheck3Run(n);
        }
        void HeapCheck3Run(int n)
        {
            Random r = new Random(12345);
            Heap<Tcel> heap = new Heap<Tcel>();
            int nExpected = 0;
            for (int i = 0; i < n; i++)
            {
                switch (r.Next(4))
                {
                    case 0:
                        {
                            int iR = r.Next(2000);
                            Add(heap, iR);
                            nExpected += iR;
                        }
                        break;
                    case 1:
                        {
                            if (heap.nSize() <= 0)
                                break;
                            Tcel tVal = heap.tGet();
                            nExpected -= tVal.nValue;
                        }
                        break;
                    case 2:
                        {
                            if (heap.nSize() <= 0)
                                break;
                            int iR = r.Next(heap.nSize());
                            Tcel tChanged = heap.tPeekAt(iR);
                            int nChange = r.Next(100);
                            tChanged.nValue -= nChange;
                            nExpected -= nChange;
                            heap.MoveUp(iR);
                        }
                        break;
                    case 3:
                        {
                            if (heap.nSize() <= 0)
                                break;
                            int iR = r.Next(heap.nSize());
                            Tcel tChanged = heap.tPeekAt(iR);
                            int nChange = r.Next(100);
                            tChanged.nValue += nChange;
                            nExpected += nChange;
                            heap.HeapifyBack(iR);
                        }
                        break;
                }
            }
            CheckRest(heap, nExpected);
        }
    }
}
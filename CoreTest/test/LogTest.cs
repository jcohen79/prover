using NUnit.Framework;
using System;


namespace reslab.test
{
    public class Tra : Ika
    {
        int nPosn;
        LogTest lto;

        public Tra (LogTest lto, int nPosn)
        {
            this.lto = lto;
            this.nPosn = nPosn;
        }

        public bool fPerform(object objLeft, object objRight, Object objData, Tcd tcdEvent)
        {
            lto.nTestState |= 1 << nPosn;
            return false;
        }
    }

    public class Nis : Bid
    {
        public readonly string stValue;

        public Nis(string stValue)
        {
            this.stValue = stValue;
        }
    }


    [TestFixture]
    [Category("Clause")]
    public class LogTest
    {
        public int nTestState;

        Tra[] rgtra = new Tra[32];
        Drt drt;

        public Nis a = new Nis("a");
        public Nis b = new Nis("b");
        public Nis c = new Nis("c");
        public Nis d = new Nis("d");
        public Nis e = new Nis("e");

        public LogTest()
        {
            for (int i = 0; i < 32; i++)
            {
                rgtra[i] = new Tra(this, i);
            }
        }

        public void Setup()
        {
            drt = new Drt();
            nTestState = 0;
        }

        [Test]
        public void DrtTest()
        {
            {
                Setup();
                drt.WatchPair(a, b, Tcd.tcdTransferLeft, rgtra[0]);
                drt.WatchPair(a, c, Tcd.tcdTransferLeft, rgtra[1]);
                drt.WatchPair(b, c, Tcd.tcdTransferLeft, rgtra[2]);
                drt.Report(Tcd.tcdTransferLeft, a, b, null);
                Assert.AreEqual(nTestState, 1);
            }
            {
                Setup();
                drt.WatchPair(a, b, Tcd.tcdTransferLeft, rgtra[0]);
                drt.WatchPair(a, c, Tcd.tcdTransferLeft, rgtra[1]);
                drt.WatchPair(b, c, Tcd.tcdTransferLeft, rgtra[2]);
                drt.Report(Tcd.tcdTransferLeft, a, b, null);
                drt.Report(Tcd.tcdTransferLeft, a, c, null);
                Assert.AreEqual(nTestState, 1+2);
            }
            {
                Setup();
                drt.WatchPair(a, b, Tcd.tcdTransferRight, rgtra[0]);
                drt.Report(Tcd.tcdTransferRight, a, b, null);
                Assert.AreEqual(nTestState, 1);
            }
            {
                Setup();
                drt.WatchTarget(a, Tcd.tcdTransferRight, rgtra[0]);
                drt.Report(Tcd.tcdTransferRight, a, b, null);
                Assert.AreEqual(nTestState, 1);
            }
            {
                Setup();
                drt.WatchTarget(a, Tcd.tcdTransferRight, rgtra[0]);  // watch for TransferRight
                drt.Report(Tcd.tcdTransferLeft, a, b, null);
                Assert.AreEqual(nTestState, 0);
            }
            {
                Setup();
                drt.WatchTarget(a, Tcd.tcdTransferRight, rgtra[0]);
                drt.Report(Tcd.tcdTransferRight, a, b, null);
                Assert.AreEqual(nTestState, 1);
            }
            {
                Setup();
                drt.WatchTarget(a, Tcd.tcdTransferRight, rgtra[0]);
                drt.WatchInput(b, Tcd.tcdTransferRight, rgtra[1]);
                drt.Report(Tcd.tcdTransferRight, a, b, null);
                Assert.AreEqual(nTestState, 1+2);
            }
        }
    }
}

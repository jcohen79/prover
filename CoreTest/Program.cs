using GraphMatching;
using reslab;
using reslab.test;

namespace GraphMain
{
    class Program
    {
        static void Main(string[] args)
        {

            WebServer._Main(null);

#if false
            NgcTest test = new NgcTest();
            test.NgcRefuteNew();
#endif

#if false
            EqExamplesSlow ex = new EqExamplesSlow();
            ex.ProveEq3Para();
#endif
            // REPL.L();

            //    SatTest.EvalGenSequent(true, 10000000000L);
#if false
            EqExamples eqe = new EqExamples();
            eqe.ProveEq4Para();
#endif
        }
    }
}

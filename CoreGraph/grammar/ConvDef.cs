using System.Collections.Generic;
using System.Text;
using GraphMatching;
using Irony.Parsing;
using GrammarDLL;

namespace ConsoleApplication1
{
    public class TermInfo
    {

        public readonly BnfTerm bnfTerm;
        public readonly Vertex vDef;
        public readonly TokenInfo tokenInfo;
        public readonly EdgeDescriptor[] edgedefsBuild;

        public TermInfo(BnfTerm bnfTerm, Vertex vDef, TokenInfo tokenInfo, EdgeDescriptor[] edgedefsBuild)
        {
            this.bnfTerm = bnfTerm;
            this.vDef = vDef;
            this.edgedefsBuild = edgedefsBuild;
            this.tokenInfo = tokenInfo;
        }
        public Terminal K { get { return tokenInfo.term; } }
    }

    public interface IGraphGrammarBuilder<V, E> where V : class
    {
        V vertexNonterminal(BnfTerm term);
        V vertexTerminal(V vPrev, BnfTerm term, int line, int column, int length, string text);
        V vertexNonGrammar(V vPrev, BnfTerm term, int line, int column, int length, string text);
        void edgeForTerm(V vParent, V vChild, BnfTerm term, int positionNum);
        CommentTerminal getComment();
    }

    public class GenConfig
    {
        public string stPrefix;
        public EdgeDescriptor edGetTemplate;
        public Vertex vFindTemplatePath;

        public GenConfig(string stPrefix, EdgeDescriptor edGetTemplate, Vertex vFindTemplatePath)
        {
            this.stPrefix = stPrefix;
            this.edGetTemplate = edGetTemplate;
            this.vFindTemplatePath = vFindTemplatePath;
        }

        public virtual Vertex vGenPush(ParseGraphBuilder gb, Vertex vStart)
        {
            Edge ePush = new Edge(gb.ec.edEmitAtom, vStart, gb.ec.vEmitPrettyPush, gb.parseTreeContext);
            gb.graph.Add(ePush);
            return vStart;
        }

        public virtual Vertex vGenPop(ParseGraphBuilder gb, Vertex vPrev)
        {
            Vertex vPop = new Vertex("vPop");
            Edge ePop = new Edge(gb.ec.edEmitAtom, vPop, gb.ec.vEmitPrettyPop, gb.parseTreeContext);
            gb.graph.Add(ePop);
            Edge ePopNext = new Edge(gb.ec.edNextStep, vPrev, vPop, gb.parseTreeContext);
            gb.graph.Add(ePopNext);
            return vPop;
        }

        public virtual void Header(ParseGraphBuilder gb, ref Vertex vPrev, BnfTerm term) { }

        public virtual void Footer(ParseGraphBuilder gb, ref Vertex vPrev, BnfTerm term) { }
    }

    public class GenConfigSet
    {
        public GenConfig gcCode;
        public GenConfig gcLsx;

        public GenConfigSet(GenConfig gcCode, GenConfig gcLsx)
        {
            this.gcCode = gcCode;
            this.gcLsx = gcLsx;
        }
    }

    public class GenConfigCode : GenConfig
    {
        public GenConfigCode(string stPrefix, EdgeDescriptor edGetTemplate, Vertex vFindTemplatePath)
            : base(stPrefix, edGetTemplate, vFindTemplatePath)
        { }

        public override void Header(ParseGraphBuilder gb, ref Vertex vPrev, BnfTerm term)
        {
            vPrev = gb.ec.AddLiteralStep(term.Name, gb.vTokenLabel, gb.graph, gb.parseTreeContext, vPrev);
            vPrev = gb.ec.AddLiteralStep("(", gb.termLeftParenthesis.vDef, gb.graph, gb.parseTreeContext, vPrev);
        }

        public override void Footer(ParseGraphBuilder gb, ref Vertex vPrev, BnfTerm term)
        {
            vPrev = gb.ec.AddLiteralStep(")", gb.termRightParenthesis.vDef, gb.graph, gb.parseTreeContext, vPrev);
        }
    }

    public class GenConfigLsx : GenConfig
    {
        public GenConfigLsx(string stPrefix, EdgeDescriptor edGetTemplate, Vertex vFindTemplatePath)
            : base(stPrefix, edGetTemplate, vFindTemplatePath)
        { }

        public override void Header(ParseGraphBuilder gb, ref Vertex vPrev, BnfTerm term)
        {
            if (term == gb.grammar.LogicExpr2Seq)
                vPrev = gb.ec.AddLiteralStep("list", gb.termIdentifier.vDef, gb.graph, gb.parseTreeContext, vPrev);
        }
    }

    public class GenConfigLsxList : GenConfig
    {
        public GenConfigLsxList(string stPrefix, EdgeDescriptor edGetTemplate, Vertex vFindTemplatePath)
            : base(stPrefix, edGetTemplate, vFindTemplatePath)
        { }

        public override Vertex vGenPush(ParseGraphBuilder gb, Vertex vStart)
        {
            return null;
        }

        public override Vertex vGenPop(ParseGraphBuilder gb, Vertex vPrev)
        {
            return vPrev;
        }

    }
}
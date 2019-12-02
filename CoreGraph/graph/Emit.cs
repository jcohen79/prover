using System;
using System.Collections.Generic;

namespace GraphMatching
{
    /// <summary>
    /// Base class for template atoms. There are derived classes for literal, indent, invoke etc.
    /// Each Atom is stored as the payload on a vertex that is referenced by template via a edge labelled EmitConfig.edEmitAtom
    /// </summary>
    public abstract class EmitAtom : ValueBase
    {
        public abstract void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl);

        public override bool SatisfiedBy(Expression valCandidate)
        {
            throw new NotImplementedException();
        }
        public override object Evaluate(SymbolTable dict, Restrictor context)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Output the literal on the template step vertex
    /// </summary>
    public class EmitPayload : EmitAtom
    {
        public readonly bool fOnBasis;

        public EmitPayload(bool fOnBasis)
        {
            this.fOnBasis = fOnBasis;
        }

        public override void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl)
        {
            ValueBase vb;
            string stText;
            if (fOnBasis)
            {
                Vertex vBasis = symtbl.vLookup(emitter.emitConfig.vBasis);
                vb = (ValueBase)vBasis.Value;
            }
            else
            {
                vb = (ValueBase)vTemplateStep.Value;
            }
            ValueLiteral vl = (ValueLiteral)vb;
            stText = (string)vl.V;

            Edge eTokenDef = vTemplateStep.eaOutOnly(emitter.context, emitter.emitConfig.edTokenInfo);
            ValueLiteral tokenValue = (ValueLiteral)eTokenDef.vHead.Value;
            GrammarDLL.TokenInfo tokenInfo = (GrammarDLL.TokenInfo)tokenValue.V;

            emitter.stackout.Add(stText, tokenInfo);
        }
    }

    public class EmitNewline : EmitAtom
    {
        public EmitNewline()
        {
        }

        public override void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl)
        {
            emitter.stackout.Newline();
        }
    }
    public class EmitPrettyPush : EmitAtom
    {

        public EmitPrettyPush()
        {
        }

        public override void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl)
        {
            emitter.stackout.Push();
        }
    }
    public class EmitPrettyPop : EmitAtom
    {

        public EmitPrettyPop()
        {
        }

        public override void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl)
        {
            emitter.stackout.Pop();
        }
    }

    public class EmitApply : EmitAtom
    {
        public EmitApply()
        {
        }

        public override void Emit(Emitter emitter, Vertex vTemplateStep, SymbolTable symtbl)
        {
            EmitConfig ec = emitter.emitConfig;

            // find edge sequence for finding function to call
            Edge eTemplateSeq = vTemplateStep.eaOutOnly(emitter.context, ec.edEmitFunctionSeq);
            Vertex vTemplateSeq = eTemplateSeq.vHead;
            ValueLiteral vlTemplateSeq = (ValueLiteral)vTemplateSeq.Value;
            EdgeDescriptor[] edTemplateSeq = (EdgeDescriptor[])vlTemplateSeq.V;

            // evaluate arguments. The one that is basis is used to select the template to invoke
            SymbolTable symtblLocal = new SymbolTable(null);
            foreach (Edge eOut in vTemplateStep.eaOutgoing(emitter.context, ec.edArgument))
            {
                Vertex vArg = eOut.vHead;
                Expression exprArg = (Expression) vArg.Value;
                object objValue = exprArg.Evaluate(symtbl, emitter.context);
                if (objValue != null)
                {
                    Vertex vParm = vArg.eaOutOnly(emitter.context, ec.edParameterVal).vHead;
                    symtblLocal.Add(vParm, (Vertex)objValue);
                }
            }

            // get function (is like virtual fun lookup)
            Vertex vTemplate = symtblLocal.vLookup(emitter.emitConfig.vBasis);
            foreach (EdgeDescriptor ed in edTemplateSeq)
            {
                vTemplate = vTemplate.eaOutOnly(emitter.context, ed).vHead;
            }

            // get lexical symbol table, if any
            Edge eSymtbl = vTemplate.eaOutOpt(emitter.context, ec.edSymtbl);
            if (eSymtbl != null)
            {
                SymbolTable symtblBase = (SymbolTable)eSymtbl.vHead.Value;
                symtblLocal.parent = symtblBase;
            }

            emitter.Invoke(symtblLocal, vTemplate);
        }
    }

    /// <summary>
    /// Emitters share a config. The config holds the edgdefs used. 
    /// </summary>
    public class EmitConfig
    {
        public EdgeDescriptor edNextStep;  // step -> next steps in template
        public EdgeDescriptor edEmitAtom;  // step -> vertex whose payload is the EmitAtom that generates the output
        public EdgeDescriptor edEmitFunctionSeq;  // step -> vertex that has payload with array of edgedefs to to to template, starting from basis
        public EdgeDescriptor edParameterDef;  // template -> vertex that is used as key in symbol table to lookup value
        public EdgeDescriptor edArgument;  // step -> vertex whose payload is expression
        public EdgeDescriptor edParameterVal;  // step -> vertex that is used as key in symbol table to store value
        public EdgeDescriptor edSymtbl;  // optional template -> vertex whose payload is lexical symbol table
        public EdgeDescriptor edTokenInfo;  // step -> TokenInfo for spacing, newline etc.

        public Vertex vEmitLiteral;
        public Vertex vEmitBasisPayload;
        public Vertex vEmitNewline;
        public Vertex vEmitApply;
        public Vertex vEmitPrettyPush;
        public Vertex vEmitPrettyPop;

        public Vertex vBasis;

        public EmitConfig() { }

        private Vertex DefAtom(SymbolTable symtab, string stName, EmitAtom ea)
        {
            Vertex vEmit = new Vertex(stName, ea);
            symtab.Add(stName, vEmit);
            return vEmit;
        }

        public Vertex AddLiteralStep(string stText, Vertex vTokenInfo, Graph graph, Restrictor context, Vertex vPrev)
        {
            EmitConfig ec = this;

            Vertex vLabel = new Vertex("vLabel", new ValueLiteral(stText));
            Edge eToken = new Edge(ec.edTokenInfo, vLabel, vTokenInfo, context);
            graph.Add(eToken);
            Edge e1 = new Edge(ec.edEmitAtom, vLabel, ec.vEmitLiteral, context);
            graph.Add(e1);
            Edge eStartNext = new Edge(ec.edNextStep, vPrev, vLabel, context);
            graph.Add(eStartNext);

            return vLabel;
        }

        // Create objects needed to build templates, store reference to them in symbol table so lookup can find them.
        public void Setup(SymbolTable symtab)
        {
            edNextStep = new EdgeDescriptor("edNextStep", ColorDef.Black);
            edEmitAtom = new EdgeDescriptor("edEmitAtom", ColorDef.Green);
            edEmitFunctionSeq = new EdgeDescriptor("edEmitFunctionSeq", ColorDef.Red);
            edParameterDef = new EdgeDescriptor("edParameterDef", ColorDef.Yellow);
            edArgument = new EdgeDescriptor("edArgument", ColorDef.Gray);
            edParameterVal = new EdgeDescriptor("edParameterVal", ColorDef.Blue);
            edSymtbl = new EdgeDescriptor("edSymtbl", ColorDef.Black);
            edTokenInfo = new EdgeDescriptor("edTokenInfo", ColorDef.Cyan);

            vBasis = new Vertex("basis");

            vEmitLiteral = DefAtom(symtab, "EmitLiteral", new EmitPayload(false));
            vEmitBasisPayload = DefAtom(symtab, "EmitBasisPayload", new EmitPayload(true));
            vEmitNewline = DefAtom(symtab, "vEmitNewline", new EmitNewline());
            vEmitApply = DefAtom(symtab, "vEmitApply", new EmitApply());
            vEmitPrettyPush = DefAtom(symtab, "vEmitPrettyPush", new EmitPrettyPush());
            vEmitPrettyPop = DefAtom(symtab, "vEmitPrettyPop", new EmitPrettyPop());
        }
    }

    /// <summary>
    /// Avoid recursive calls in Emitter
    /// </summary>
    public class EmitState
    {
        public SymbolTable symtbl;
        public Vertex vTemplatePlace;

        public EmitState(SymbolTable symtbl, Vertex vTemplatePlace)
        {
            this.symtbl = symtbl;
            this.vTemplatePlace = vTemplatePlace;
        }
    }

    /// <summary>
    /// Generate output by applying atoms in template to a basis vertex
    /// </summary>
    public class Emitter
    {
        public readonly EmitConfig emitConfig;
        public readonly IStackOut stackout;
        public readonly Vertex vTemplate;
        public readonly Restrictor context;
        private Stack<EmitState> rgesPending;

        public Emitter(EmitConfig emitConfig, IStackOut stackout, Vertex vTemplate, SymbolTable symtbl, Restrictor context)
        {
            this.emitConfig = emitConfig;
            this.stackout = stackout;
            this.vTemplate = vTemplate;
            this.context = context;
            rgesPending = new Stack<EmitState>();
            Invoke(symtbl, vTemplate);
        }

        public void Invoke(SymbolTable symtbl, Vertex vTemplate)
        {
            rgesPending.Push(new EmitState(symtbl, vTemplate));
        }

        // perform output
        public void Emit ()
        {
            while (rgesPending.Count > 0)
            {
                EmitState esCurrent = rgesPending.Pop();

                // add next steps from outgoing transitions, they will be executed after the ones pushed on by this step
                foreach (Edge eOut in esCurrent.vTemplatePlace.eaOutgoing(context, emitConfig.edNextStep))
                {
                    Vertex vNext = eOut.vHead;
                    rgesPending.Push(new EmitState(esCurrent.symtbl, vNext));
                }

                // perform the step
                Edge eAtom = esCurrent.vTemplatePlace.eaOutOpt(context, emitConfig.edEmitAtom);
                if (eAtom != null)
                {
                    Vertex vAtom = eAtom.vHead;
                    ValueBase vbAtom = (ValueBase)vAtom.Value;
                    EmitAtom ea = (EmitAtom)vbAtom;
                    ea.Emit(this, esCurrent.vTemplatePlace, esCurrent.symtbl);
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Reflection;
using GrammarDLL;
using Irony.Parsing;

namespace GraphMatching
{
    /// <summary>
    /// Write formatted output
    /// </summary>
    public class Fwt
    {
        private StringBuilder sb;
        public int indentation = 0;
        private bool atNewline = false;
        public List<Object> rgAlready = new List<object>();

        public Fwt(StringBuilder sb)
        {
            this.sb = sb;
        }

        public Fwt()
        {
            this.sb = new StringBuilder();
        }

        public void Append(object s)
        {
            Append(s == null ? "null" : s.ToString());
        }

        public void AppendLine(string s)
        {
            Append(s);
            Newline();
        }

        public void Append(string s)
        {
            if (atNewline)
            {
                atNewline = false;
                for (int i = 0; i < indentation; i++)
                    sb.Append(' ');
            }
            sb.Append(s);
        }

        public void Newline()
        {
            atNewline = true;
            sb.Append("\n");
        }

        public void Indent(int iSpaces = 4)
        {
            indentation += iSpaces;
        }

        public void Unindent(int iSpaces = 4)
        {
            indentation -= iSpaces;
        }

        public string stString()
        {
            return sb.ToString();
        }
    }

    public class TokenItem
    {
        public string stText;
        public TokenInfo info;
        public TokenItem tokenNext;
        public bool fNewLineBefore = false;
        public bool fSpaceBefore = false;
        public bool fIndentBefore = false;
        public bool fUnindentBefore = false;
    }

    public class FormatStack
    {
        public FormatStack stackPrev;
        public TokenItem tokenStart;
        public bool fIndented = false;

        public static FormatStack stackCountBack(FormatStack stack, int nDepth)
        {
            while (nDepth > 0
                    && stack != null)
            {
                stack = stack.stackPrev;
                nDepth--;
            }
            return stack;
        }
    }

    public interface IStackOut
    {
        void Push();
        void Pop();
        void Add(string stText, TokenInfo info);
        void Newline();

    }

    public class PrettyFormatter : IStackOut
    {
        protected Fwt formatter;
        protected TokenItem tokenFirst;
        protected TokenItem tokenLast;
        public int nMaxLength = 80;

        public static TokenInfo tokenIdentifier = new TokenInfo(SpacingType.spcIdentifier);
        public static TokenInfo tokenOperator = new TokenInfo(SpacingType.spcOperator);
        public static TokenInfo tokenLeft = new TokenInfo(SpacingType.spcLeft);
        public static TokenInfo tokenRight = new TokenInfo(SpacingType.spcRight);
        public static TokenInfo tokenDot = new TokenInfo(SpacingType.spcDot);
        public static TokenInfo tokenComma = new TokenInfo(SpacingType.spcComma);
        public static TokenInfo tokenColon = new TokenInfo(SpacingType.spcColon);
        public static TokenInfo tokenKeyword = new TokenInfo(SpacingType.spcKeyword);
        public static TokenInfo tokenPop = new TokenInfo(SpacingType.spcPop);
        public static TokenInfo tokenPush = new TokenInfo(SpacingType.spcPush);


        public const int nSplitOnDepth = 3;

        public PrettyFormatter(Fwt formatter)
        {
            this.formatter = formatter;
        }

        public void Add (string stText, TokenInfo info)
        {
            TokenItem item = new TokenItem();
            item.stText = stText;
            item.info = info;
            if (tokenLast != null)
                tokenLast.tokenNext = item;
            else
                tokenFirst = item;
            tokenLast = item;
        }

        public void Identifier(string stText) { Add(stText, tokenIdentifier); }
        public void Operator(string stText) { Add(stText, tokenOperator); }
        public void Left(string stText) { Add(stText, tokenLeft); }
        public void Right(string stText) { Add(stText, tokenRight); }
        public void Dot(string stText) { Add(stText, tokenDot); }
        public void Comma(string stText) { Add(stText, tokenComma); }
        public void Colon(string stText) { Add(stText, tokenColon); }
        public void Keyword(string stText) { Add(stText, tokenKeyword); }
        public void Push() { Add(null, tokenPush); }
        public void Pop() { Add(null, tokenPop); }


        public void FieldValue(string stLabel, Base baseValue)
        {
            Push();
            Keyword(stLabel);
            Colon(":");
            Push();
            baseValue.FormatSubobject(this);
            Pop();
            Pop();
        }
        public void FieldString(string stLabel, string stValue)
        {
            Push();
            Keyword(stLabel);
            Colon(":");
            Push();
            Identifier(stValue);
            Pop();
            Pop();
        }

        public void FieldList(string stLabel, Base[] rgBase)
        {
            Push();
            Keyword(stLabel);
            Colon(":");
            Push();
            foreach (var b in rgBase)
                b.FormatSubobject(this);
            Pop();
            Pop();
        }
        public void FieldNameList(string stLabel, Named[] rgBase)
        {
            Push();
            Keyword(stLabel);
            Colon(":");
            Push();
            foreach (var b in rgBase)
                Identifier(b.Name);
            Pop();
            Pop();
        }

        private void AddSpacing(TokenItem leftToken, TokenItem rightToken)
        {
            SpacingType leftSpacing = leftToken.info.spacing;
            SpacingType rightSpacing = rightToken.info.spacing;
            bool fSpaceBefore = false;

            switch (rightSpacing)
            {
                case SpacingType.spcIdentifier:
                    if ((leftSpacing == SpacingType.spcLeft)
                        || (leftSpacing == SpacingType.spcDot))
                    { }
                    else
                        fSpaceBefore = true;
                    break;
                case SpacingType.spcOperator:
                    fSpaceBefore = true;
                    break;
                case SpacingType.spcLeft:
                    if (leftSpacing == SpacingType.spcLeft) {}
                    else
                        fSpaceBefore = true;
                    break;
                case SpacingType.spcRight:
                    if ((leftSpacing == SpacingType.spcIdentifier)
                        || (leftSpacing == SpacingType.spcLeft)
                        || (leftSpacing == SpacingType.spcRight))
                    {}
                    else
                        fSpaceBefore = true;
                    break;
                case SpacingType.spcDot:
                    if ((leftSpacing == SpacingType.spcLeft)
                        || (leftSpacing == SpacingType.spcComma))
                        fSpaceBefore = true;
                    break;
                case SpacingType.spcColon:
                    break;
                case SpacingType.spcComma:
                    if ((leftSpacing == SpacingType.spcLeft))
                        fSpaceBefore = true;
                    break;
                case SpacingType.spcKeyword:
                    if ((leftSpacing == SpacingType.spcLeft)
                        || (leftSpacing == SpacingType.spcDot))
                    { }
                    else
                        fSpaceBefore = true;
                    break;
                default:
                    break;
            }

            if (fSpaceBefore)
                rightToken.fSpaceBefore = true;
        }

        public void Organize()
        {
            FormatStack stack = null;
            TokenItem token = tokenFirst;
            TokenItem tokenPrev = null;
            int iPos = formatter.indentation;
            bool fNextTokenNewline = false;
            bool fNextTokenUnindent = false;

            while (token != null)
            {
                if (token.info.fStartNewLine
                    || ((token.stText != null) && (iPos + token.stText.Length > nMaxLength)))
                {
                    token.fNewLineBefore = true;
                    iPos = formatter.indentation;
                    tokenPrev = null;
                }

                if (token.info.spacing == SpacingType.spcPush)
                {
                    FormatStack newStack = new FormatStack();
                    newStack.tokenStart = token;
                    newStack.stackPrev = stack;
                    stack = newStack;

                    FormatStack stackPrev = FormatStack.stackCountBack(stack, nSplitOnDepth);
                    if (stackPrev != null)
                    {
                        stackPrev.tokenStart.fNewLineBefore = true;
                        stackPrev.tokenStart.fIndentBefore = true; 
                    }
                }
                else if (token.info.spacing == SpacingType.spcPop)
                {
                    if (stack.tokenStart.fNewLineBefore)
                        fNextTokenNewline = true;
                    if (stack.fIndented)
                        fNextTokenUnindent = true;
                    stack = stack.stackPrev;
                }
                else
                {
                    if (fNextTokenNewline && token.info.spacing != SpacingType.spcRight)
                    {
                        token.fNewLineBefore = true;
                        fNextTokenNewline = false;
                    }
                    if (fNextTokenUnindent)
                    {
                        token.fUnindentBefore = true;
                        fNextTokenUnindent = false;
                    }
                }

                if (tokenPrev != null)
                    AddSpacing (tokenPrev, token);

                tokenPrev = token;
                token = token.tokenNext;
            }
        }

        public void Send()
        {
            TokenItem token = tokenFirst;
            while (token != null)
            {
                if (token.fNewLineBefore)
                    formatter.Newline();
                else if (token.fSpaceBefore)
                    formatter.Append(" ");

                if (token.fIndentBefore)
                    formatter.Indent();
                else if (token.fIndentBefore)
                    formatter.Unindent();

                if (token.stText != null)
                    formatter.Append(token.stText);
                token = token.tokenNext;
            }
        }

        public void Newline()
        {
            Organize();
            Send();
            tokenFirst = null;
            tokenLast = null;
        }
    }
}

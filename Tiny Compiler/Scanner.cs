using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

public enum Token_Class
{
    Int, If, Float, String, Read, Write, Repeat, Until, Elseif, Else, Then, Return, Endl,
    Semicolon, Comma, LParanthesis, RParanthesis, LCurlyBracket, RCurlyBracket,
    EqualOp, LessThanOp, GreaterThanOp, NotEqualOp, AssignmentOp, AndOp, OrOp,
    PlusOp, MinusOp, MultiplyOp, DivideOp, Idenifier, Number, Comment, SingleQuote, DoubleQuote, Main, End, assEqual
}
namespace TINY_Compiler
{


    public class Token
    {
        public string lex;
        public Token_Class token_type;
    }

    public class Scanner
    {
        public List<Token> Tokens = new List<Token>();
        Dictionary<string, Token_Class> ReservedWords = new Dictionary<string, Token_Class>();
        Dictionary<string, Token_Class> Operators = new Dictionary<string, Token_Class>();

        public Scanner()
        {
            ReservedWords.Add("int", Token_Class.Int);
            ReservedWords.Add("if", Token_Class.If);
            ReservedWords.Add("float", Token_Class.Float);
            ReservedWords.Add("string", Token_Class.String);
            ReservedWords.Add("read", Token_Class.Read);
            ReservedWords.Add("write", Token_Class.Write);
            ReservedWords.Add("repeat", Token_Class.Repeat);
            ReservedWords.Add("until", Token_Class.Until);
            ReservedWords.Add("elseif", Token_Class.Elseif);
            ReservedWords.Add("else", Token_Class.Else);
            ReservedWords.Add("then", Token_Class.Then);
            ReservedWords.Add("return", Token_Class.Return);
            ReservedWords.Add("endl", Token_Class.Endl);
            ReservedWords.Add("main", Token_Class.Main);
            ReservedWords.Add("end", Token_Class.End);

            // List of Operators
            Operators.Add(";", Token_Class.Semicolon);
            Operators.Add(",", Token_Class.Comma);
            Operators.Add("(", Token_Class.LParanthesis);
            Operators.Add(")", Token_Class.RParanthesis);
            Operators.Add("{", Token_Class.LCurlyBracket);
            Operators.Add("}", Token_Class.RCurlyBracket);

            Operators.Add("=", Token_Class.EqualOp);
            Operators.Add("<", Token_Class.LessThanOp);
            Operators.Add(">", Token_Class.GreaterThanOp);
            Operators.Add("<>", Token_Class.NotEqualOp);

            Operators.Add(":=", Token_Class.AssignmentOp);

            Operators.Add("+", Token_Class.PlusOp);
            Operators.Add("-", Token_Class.MinusOp);
            Operators.Add("/", Token_Class.DivideOp);
            Operators.Add("*", Token_Class.MultiplyOp);
            Operators.Add("&&", Token_Class.AndOp);
            Operators.Add("||", Token_Class.OrOp);

            Operators.Add("'", Token_Class.SingleQuote);
            Operators.Add("\"", Token_Class.DoubleQuote);
        }

        public List<string> ErrorList = new List<string>();

        public void StartScanning(string SourceCode)
        {
            string pattern_comment = @"/\*((.|\n|\r)*?)\*/";
            SourceCode = Regex.Replace(SourceCode, pattern_comment, string.Empty);
            for (int i = 0; i < SourceCode.Length; i++)
            {
                int j = i;
                char CurrentChar = SourceCode[i];
                string CurrentLexeme = CurrentChar.ToString();

                if (CurrentChar == ' ' || CurrentChar == '\r' || CurrentChar == '\n')
                    continue;
                // Handle operators
                if (CurrentChar == ':' && j < SourceCode.Length && SourceCode[j + 1] == '=')
                {
                    CurrentLexeme = ":=";
                    FindTokenClass(CurrentLexeme);
                    i = j + 1;
                }
                //string
                else if (CurrentChar == '"')
                {
                    j++;
                    while (j < SourceCode.Length && SourceCode[j] != '"')
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    if (j < SourceCode.Length && SourceCode[j] == '"')
                        CurrentLexeme += SourceCode[j];

                    FindTokenClass(CurrentLexeme);
                    i = j;
                }
                else if (CurrentChar == '&' && j < SourceCode.Length && SourceCode[j + 1] == '&')
                {
                    CurrentLexeme += SourceCode[j + 1];
                    FindTokenClass(CurrentLexeme);
                    i = j + 1;
                }
                else if (CurrentChar == '|' && j < SourceCode.Length && SourceCode[j + 1] == '|')
                {
                    CurrentLexeme += SourceCode[j + 1];
                    FindTokenClass(CurrentLexeme);
                    i = j + 1;
                }
                else if (CurrentChar == '<' && j < SourceCode.Length && SourceCode[j + 1] == '>')
                {
                    CurrentLexeme += SourceCode[j + 1];
                    FindTokenClass(CurrentLexeme);
                    i = j + 1;
                }


                // Handle numbers
                else if (char.IsDigit(CurrentChar))
                {
                    j++;
                    while (j < SourceCode.Length &&
                           (char.IsLetter(SourceCode[j]) || char.IsDigit(SourceCode[j]) || SourceCode[j] == '.'))
                    {
                        if (SourceCode[j] == ' ')
                        {
                            break; // Stop at a space
                        }
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    // Now classify the full lexeme
                    FindTokenClass(CurrentLexeme);

                    // Adjust the main loop index
                    i = j - 1;
                }


                // Handle identifiers and keywords
                else if (char.IsLetter(CurrentChar))
                {
                    j++;
                    while (j < SourceCode.Length &&
                           (char.IsLetter(SourceCode[j]) || char.IsDigit(SourceCode[j])))
                    {
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }
                    FindTokenClass(CurrentLexeme);
                    i = j - 1;
                }




                /*else if (char.IsDigit(CurrentChar))
                {
                    bool isNumberValid = true;
                    bool hasDecimalPoint = false;

                    j++;
                    while (j < SourceCode.Length &&
                           (char.IsDigit(SourceCode[j]) || SourceCode[j] == '.' || (char.IsLetter(SourceCode[j]))))
                    {
                        if (SourceCode[j] == '.')
                        {
                            if (hasDecimalPoint)
                            {
                                isNumberValid = false;
                                break;
                            }
                            hasDecimalPoint = true;
                        }
                        CurrentLexeme += SourceCode[j];
                        j++;
                    }

                    if (isNumberValid)
                    {
                        FindTokenClass(CurrentLexeme);
                    }
                    else
                    {
                        ErrorList.Add(CurrentLexeme);
                    }
                    i = j - 1;
                }*/

                // Handle single-character tokens
                else
                {
                    FindTokenClass(CurrentLexeme);
                }
            }

            TINY_Compiler.TokenStream = Tokens;
        }



        void FindTokenClass(string Lex)
        {
            Token_Class TC;
            Token Tok = new Token();
            Tok.lex = Lex;
            //Is it a reserved word?
            var id = new Regex(@"^[a-zA-Z_][a-zA-Z0-9_]*$", RegexOptions.Compiled);

            //var id = new Regex(@"\b[a-zA-Z][a-zA-Z0-9]*\b", RegexOptions.Compiled);
            var num = new Regex(@"^[0-9]+(\.[0-9]+)?$", RegexOptions.Compiled);
            var str = new Regex(@"^""[^""]*""$", RegexOptions.Compiled);

            if (ReservedWords.ContainsKey(Lex))
            {
                Tok.token_type = ReservedWords[Lex];
                Tokens.Add(Tok);
                return;
            }

            //Is it an string?
            else if (str.IsMatch(Lex))
            {
                Tok.token_type = Token_Class.String;
                Tokens.Add(Tok);
            }


            //Is it a Number?
            else if (num.IsMatch(Lex))
            {
                Tok.token_type = Token_Class.Number;
                Tokens.Add(Tok);
                return;
            }
            //Is it an operator?
            else if (Operators.ContainsKey(Lex))
            {
                Tok.token_type = Operators[Lex];
                Tokens.Add(Tok);
            }
            //Is it an identifier?

            else if (id.IsMatch(Lex))
            {
                Tok.token_type = Token_Class.Idenifier;
                Tokens.Add(Tok);
                return;
            }
            //Is it an undefined?
            else
            {
                Errors.Error_List.Add(Lex);
                return;
            }

        }

    }
}
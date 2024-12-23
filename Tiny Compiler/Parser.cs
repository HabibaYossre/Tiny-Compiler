using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Security.Cryptography;
using System.Windows.Forms;
using TINY_Compiler;
using static System.Windows.Forms.AxHost;

namespace TINY_Compiler
{
    public class Node
    {
        public List<Node> Children = new List<Node>();
        public string Name;

        public Node(string name)
        {
            Name = name;
        }
    }

    public class Parser
    {
        int InputPointer = 0;
        List<Token> TokenStream;
        public Node root;
        bool inFunctionBody = false;

        public Node StartParsing(List<Token> TokenStream)
        {
            this.InputPointer = 0;
            this.TokenStream = TokenStream;
            root = new Node("Program");
            root.Children.Add(Program());
            return root;
        }

        public Node Program()
        {
            // Program → FunctionList MainFunc

            Node program = new Node("Program");
            if (InputPointer < TokenStream.Count && isItADataType() && InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type != Token_Class.Main)
                program.Children.Add(FunctionList());

            program.Children.Add(MainFunc());
            return program;

        }
        
        Node MainFunc()
        {
            // MainFunc → DataType main() FuncBody
            Node mainFunc = new Node("MainFunc");
            mainFunc.Children.Add(DataType());
            mainFunc.Children.Add(Match(Token_Class.Main));
            mainFunc.Children.Add(Match(Token_Class.LParanthesis));
            mainFunc.Children.Add(Match(Token_Class.RParanthesis));
            mainFunc.Children.Add(FuncBody());
            return mainFunc;
        }
        Node DataType()
        {
            // DataType → int | float | string
            Node dataType = new Node("DataType");
            if (InputPointer < TokenStream.Count)
            {
                Token currentToken = TokenStream[InputPointer];
                if (currentToken.token_type == Token_Class.Int ||
                    currentToken.token_type == Token_Class.Float ||
                    currentToken.token_type == Token_Class.String)
                {
                    dataType.Children.Add(Match(currentToken.token_type));
                }
            }
            return dataType;
        }
        Node FunctionList()
        {
            // FunctionList → FuncDef FunctionList| epsilon

            Node funcListNode = new Node("function_list");

            if (InputPointer < TokenStream.Count)
            {
                if (TokenStream[InputPointer].token_type == Token_Class.Int || TokenStream[InputPointer].token_type == Token_Class.Float || TokenStream[InputPointer].token_type == Token_Class.String)
                {
                    ++InputPointer;
                    if (InputPointer < TokenStream.Count)
                    {
                        if (TokenStream[InputPointer].token_type != Token_Class.Main) //if it is not main
                        {
                            --InputPointer;
                            funcListNode.Children.Add(FuncDef());
                            funcListNode.Children.Add(FunctionList());
                            return funcListNode;
                        }
                    }
                    --InputPointer;
                }
                else
                {
                    return null;
                }
            }
            return funcListNode;
        }
        
        private Node FuncDef()
        {
            // FuncDef → FuncDecl FuncBody

            Node funcDefNode = new Node("FuncDef");
            funcDefNode.Children.Add(FuncDecl());
            funcDefNode.Children.Add(FuncBody());
            return funcDefNode;
        }
         
        private Node FuncDecl()
        {
            // FuncDecl  → DataType FuncName(FuncParams)

            Node function_decl_node = new Node("FuncDecl");
            function_decl_node.Children.Add(DataType());
            function_decl_node.Children.Add(FuncName());
            function_decl_node.Children.Add(Match(Token_Class.LParanthesis)); 
            function_decl_node.Children.Add(FuncParams());
            function_decl_node.Children.Add(Match(Token_Class.RParanthesis));

            return function_decl_node;
        }
        
        Node FuncName()
        {
            // FuncName → identifier
            Node funcNameNode = new Node("FuncName");
            funcNameNode.Children.Add(Match(Token_Class.Idenifier));
            return funcNameNode;
        }

        Node FuncParams()
        {
            // FuncParams → ParamDecl MoreParams | epsilon
            Node funcParamsNode = new Node("FuncParams");

            if (InputPointer < TokenStream.Count)
            {

                if (TokenStream[InputPointer].token_type == Token_Class.Int ||
                    TokenStream[InputPointer].token_type == Token_Class.Float ||
                    TokenStream[InputPointer].token_type == Token_Class.String)
                {

                    funcParamsNode.Children.Add(ParamDecl());

                    funcParamsNode.Children.Add(MoreParams());
                }
            }


            return funcParamsNode;
        }

        Node ParamDecl()
        {
            // ParamDecl → DataType identifier
            Node paramDeclNode = new Node("ParamDecl");
            paramDeclNode.Children.Add(DataType());
            paramDeclNode.Children.Add(Match(Token_Class.Idenifier));
            return paramDeclNode;
        }

        Node MoreParams()
        {
            // MoreParams → , FuncParams MoreParams | epsilon
            Node moreParamsNode = new Node("MoreParams");

            if (TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                moreParamsNode.Children.Add(Match(Token_Class.Comma));
                moreParamsNode.Children.Add(FuncParams());
            }


            return moreParamsNode;
        }
        
        Node FuncBody()
        {
            // FuncBody → { StatementList  ReturnStatement |  ReturnStatement }

            inFunctionBody = true;

            Node FuncBody_node = new Node("FuncBody");

            // Match the opening curly bracket
            FuncBody_node.Children.Add(Match(Token_Class.LCurlyBracket));

            // Check if there is a StatementList
            if (TokenStream[InputPointer].token_type != Token_Class.Return) // If it's not a return statement, we expect a statement list
            {
                FuncBody_node.Children.Add(StatementList());
            }

            // Add the return statement (either with or without statement list)
            if (TokenStream[InputPointer].token_type == Token_Class.Return)
            {
                FuncBody_node.Children.Add(ReturnStatement());
            }

            // Match the closing curly bracket
            FuncBody_node.Children.Add(Match(Token_Class.RCurlyBracket));


            return FuncBody_node;
        }
 
        Node StatementList()
        {
            // StatementList → Statement State

            Node statments = new Node("Statements");
            statments.Children.Add(Statement());
            statments.Children.Add(State());
            return statments;
        }

        Node State()
        {
            // State → Statement State | ε

            Node state = new Node("State");
            if (InputPointer < TokenStream.Count && isItAStartOfStatement())
            {
                state.Children.Add(Statement());
                state.Children.Add(State());
                return state;
            }
            else
            {
                return null;
            }

        }
        
        Node Statement()
        {
            // Statement → IfStatement| ReturnStatement| ReadStmt| WriteStmt | RepeatStatement| DeclarationStatement| AssignStmt;| FuncCall; | ε
            Node statement = new Node("Statement");

            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.If)
            {
                statement.Children.Add(IfStatement());
                return statement;
            }
            

            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Read)
            {
                statement.Children.Add(ReadStmt());
                return statement;
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Write)
            {
                statement.Children.Add(WriteStmt());
                return statement;
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Repeat)
            {
                statement.Children.Add(RepeatStatement());
                return statement;
            }

            else if (InputPointer < TokenStream.Count && (TokenStream[InputPointer].token_type == Token_Class.Float
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Int
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.String))
            {
                statement.Children.Add(DeclarationStatement());
                return statement;
            }

            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Return && !inFunctionBody)
            {
                statement.Children.Add(ReturnStatement());
                return statement;
            }

            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                if (InputPointer + 1 < TokenStream.Count && TokenStream[InputPointer + 1].token_type == Token_Class.AssignmentOp)
                    statement.Children.Add(AssignStmt());
                else
                    statement.Children.Add(FuncCall());
                statement.Children.Add(Match(Token_Class.Semicolon));
                return statement;
            }
            else
            {
                return null;
            }


        }

        Node AssignStmt()
        {
            // AssignStmt → identifier := Expr

            // Create the node for assignment statement
            Node assignNode = new Node("Assignment");

            // Ensure the next token is an identifier
            if (TokenStream[InputPointer].token_type == Token_Class.Idenifier)
            {
                // Parse identifier (variable name)
                assignNode.Children.Add(Match(Token_Class.Idenifier));

                // Ensure the next token is ':=' (assignment operator)
                if (TokenStream[InputPointer].token_type == Token_Class.AssignmentOp)
                {
                    // Parse ':=' (assignment operator)
                    assignNode.Children.Add(Match(Token_Class.AssignmentOp));

                    // Parse the expression on the right-hand side
                    assignNode.Children.Add(Expr());

                    return assignNode;
                }
                else
                {
                    // Error handling if the assignment operator is not found
                    // (Optional, depending on how you want to handle errors)
                    Console.WriteLine("Expected ':=' after identifier.");
                    return null;
                }
            }

            // If the token is not an identifier, return null
            return null;
        }

        Node ReadStmt()
        {
            // ReadStmt → read identifier;
            Node readStmt = new Node("ReadStmt");
            readStmt.Children.Add(Match(Token_Class.Read));
            readStmt.Children.Add(Match(Token_Class.Idenifier));
            readStmt.Children.Add(Match(Token_Class.Semicolon));
            return readStmt;
        }

        Node WriteStmt() //statement 
        {
            // WriteStmt → write WriteTail;

            Node write_Statement = new Node("WriteStmt");

            write_Statement.Children.Add(Match(Token_Class.Write));
            write_Statement.Children.Add(WriteTail());
            write_Statement.Children.Add(Match(Token_Class.Semicolon));

            return write_Statement;
        }

        Node WriteTail() //statement 
        {
            // WriteTail -> Expr | endl

            Node write_tail = new Node("WriteTail");

            if (TokenStream[InputPointer].token_type == Token_Class.Endl)
            {
                write_tail.Children.Add(Match(Token_Class.Endl));

            }
            else
            {
                write_tail.Children.Add(Expr());
            }

            return write_tail;
        }

        Node DeclarationStatement()
        {
            // DeclarationStatement → DataType VarsDeclartion;

            Node declaration_Statement = new Node("DeclarationStatement");
            declaration_Statement.Children.Add(DataType());
            declaration_Statement.Children.Add(VarsDeclation());
            declaration_Statement.Children.Add(Match(Token_Class.Semicolon));
            return declaration_Statement;
        }

        Node VarsDeclation()
        {
            // VarsDeclartion → identifier Initialization Declartions

            Node varsDeclation = new Node("VarsDeclation");
            varsDeclation.Children.Add(Match(Token_Class.Idenifier));
            varsDeclation.Children.Add(Initialization());
            varsDeclation.Children.Add(Declartions());
            return varsDeclation;
        }

        Node Initialization()
        {
            // Initialization → := Expr | ε

            Node initialization = new Node("Initialization");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.AssignmentOp)
            {
                initialization.Children.Add(Match(Token_Class.AssignmentOp));
                initialization.Children.Add(Expr());
                return initialization;
            }
            else
            {
                return null;
            }
        }
        Node Declartions()
        {
            // Declartions → , identifier Initialization Declartions | ε

            Node declartions = new Node("Declartions");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                declartions.Children.Add(Match(Token_Class.Comma));
                declartions.Children.Add(Match(Token_Class.Idenifier));
                declartions.Children.Add(Initialization());
                declartions.Children.Add(Declartions());
                return declartions;
            }
            else
            {
                return null;
            }
        }

        Node FuncCall()
        {
            // FuncCall → identifier(ArgList);
            Node funcCall = new Node("FuncCall");


            funcCall.Children.Add(Match(Token_Class.Idenifier));
            funcCall.Children.Add(Match(Token_Class.LParanthesis));
            funcCall.Children.Add(ArgList());
            funcCall.Children.Add(Match(Token_Class.RParanthesis));

            return funcCall;
        }
        Node ArgList()
        {
            //ArgList → Expr MoreArgs | epsilon

            Node args = new Node("ArgList");
            // Check if Expr is present (Identifier or number for simplicity)
            if (InputPointer < TokenStream.Count &&
                (TokenStream[InputPointer].token_type == Token_Class.Idenifier ||
                 TokenStream[InputPointer].token_type == Token_Class.Number))
            {
                args.Children.Add(Expr()); // Add the expression node
                args.Children.Add(MoreArgs()); // Handle additional arguments
            }
            else
            {
                // epsilon (return empty node or null if no arguments)
                return null;
            }
            return args;
        }

        Node MoreArgs()
        {
            //MoreArgs→ , Expr MoreArgs | epsilon

            Node more_args = new Node("MoreArgs");
            if (InputPointer < TokenStream.Count &&
        TokenStream[InputPointer].token_type == Token_Class.Comma)
            {
                more_args.Children.Add(Match(Token_Class.Comma));
                more_args.Children.Add(Expr());
                more_args.Children.Add(MoreArgs());
            }
            else
            {
                return null;
            }
            return more_args;

        }
       
        Node Expr()
        {

            // Expr -> string | Term | Equation

            Node expr = new Node("Expr");

            if (InputPointer < TokenStream.Count)
            {
                Token currentToken = TokenStream[InputPointer];

                if (currentToken.token_type == Token_Class.String)
                {

                    expr.Children.Add(Match(Token_Class.String));
                    return expr;
                }
                else if (currentToken.token_type == Token_Class.Number || currentToken.token_type == Token_Class.Idenifier)
                {
                    ++InputPointer;
                    if (InputPointer < TokenStream.Count)
                    {


                        if (currentToken.token_type == Token_Class.LParanthesis ||
                       currentToken.token_type == Token_Class.Number ||
                       currentToken.token_type == Token_Class.Idenifier)
                        {
                            --InputPointer;
                            expr.Children.Add(Equation());
                            return expr;
                        }
                    }
                    --InputPointer;
                    expr.Children.Add(Term());
                    return expr;

                }
            }
            return expr;
        }
        Node Equation()
        {
            // Equation -> Term EqOp Term | (Equation) EqOp Term | Term

            Node equation = new Node("Equation");

            // Case 1: If the current token is an opening parenthesis, handle (Equation)
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
            {
                equation.Children.Add(Match(Token_Class.LParanthesis));  // Match '('
                equation.Children.Add(Equation());  // Parse the expression inside the parentheses
                equation.Children.Add(Match(Token_Class.RParanthesis));  // Match ')'
                equation.Children.Add(Operator_Equation());  // Match the operator after the parentheses
            }
            else
            {
                // Case 2: Regular Term EqOp Term
                equation.Children.Add(Term());  // Parse the first term
                equation.Children.Add(Operator_Equation());  // Parse the operator
                equation.Children.Add(Term());  // Parse the second term
            }

            return equation;
        }

        Node Operator_Equation()
        {
            // Operator_Equation → Arthematic_Operator Equation Operator_Equation | ε

            Node operatorEquation = new Node("Operator_Equation");

            // Check for Arithmetic_Operator
            if (InputPointer < TokenStream.Count && IsArithmeticOperator(TokenStream[InputPointer].token_type))
            {
                operatorEquation.Children.Add(Arithmetic_Operator()); // Match operator
                operatorEquation.Children.Add(Equation());            // Parse the next Equation
                operatorEquation.Children.Add(Operator_Equation());   // Recursively parse Operator_Equation
            }
            else
            {
                // epsilon case (no more operator or equation)
                return null;
            }

            return operatorEquation;
        }
        Node Arithmetic_Operator()
        {
            Node arithmeticOperator = new Node("Arithmetic_Operator");

            if (InputPointer < TokenStream.Count && IsArithmeticOperator(TokenStream[InputPointer].token_type))
            {
                arithmeticOperator.Children.Add(Match(TokenStream[InputPointer].token_type)); // Match operator
            }

            return arithmeticOperator;
        }
        Node ReturnStatement()
        {
            // ReturnStatement → return Expr ;

            Node returnNode = new Node("Return");
            inFunctionBody = false;
            returnNode.Children.Add(Match(Token_Class.Return));
            returnNode.Children.Add(Expr());
            returnNode.Children.Add(Match(Token_Class.Semicolon));

            return returnNode;

        }

        Node RepeatStatement()
        {
            // RepeatStatement → repeat StatementList until ConditionStatement

            Node repeatStatement = new Node("RepeatStatement");
            inFunctionBody = false;
            repeatStatement.Children.Add(Match(Token_Class.Repeat));
            repeatStatement.Children.Add(StatementList());
            repeatStatement.Children.Add(Match(Token_Class.Until));
            repeatStatement.Children.Add(ConditionStatement());
            return repeatStatement;
        }
        Node Term()
        {
            // Term → number | identifier | FuncCall
            Node term = new Node("Term");

            if (InputPointer < TokenStream.Count)
            {

                if (TokenStream[InputPointer].token_type == Token_Class.Number)
                {
                    term.Children.Add(Match(Token_Class.Number));
                    return term;
                }
                else if (TokenStream[InputPointer].token_type == Token_Class.Idenifier) //AMBUGUITY
                {
                    ++InputPointer;
                    if (InputPointer < TokenStream.Count)
                    {
                        if (TokenStream[InputPointer].token_type == Token_Class.LParanthesis)
                        {
                            --InputPointer;
                            term.Children.Add(FuncCall());
                            return term;
                        }
                    }

                    --InputPointer;
                    term.Children.Add(Match(Token_Class.Idenifier));
                    return term;

                }

            }

            return term;

        }

        

        Node ConOp()
        {
            // ConOp -> < | > | != | ==
            Node conOp = new Node("ConOp");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.EqualOp)
            {
                conOp.Children.Add(Match(Token_Class.EqualOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.NotEqualOp)
            {
                conOp.Children.Add(Match(Token_Class.NotEqualOp));
            }
            else if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.GreaterThanOp)
            {
                conOp.Children.Add(Match(Token_Class.GreaterThanOp));
            }
            else
            {
                conOp.Children.Add(Match(Token_Class.LessThanOp));
            }
            return conOp;
        }

        
        Node BoolOp()
        {
            // BoolOp →  || | && 

            Node boolOp = new Node("BoolOp");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.OrOp)
            {
                boolOp.Children.Add(Match(Token_Class.OrOp));
            }
            else
            {
                boolOp.Children.Add(Match(Token_Class.AndOp));
            }
            return boolOp;
        }
        
        Node Condition()
        {
            // Condition → identifier ConOp term

            Node condition = new Node("Condition");
            condition.Children.Add(Match(Token_Class.Idenifier));
            condition.Children.Add(ConOp());
            condition.Children.Add(Term());
            return condition;
        }


        Node ConditionStatement()
        {
            // ConditionStatement → Condition Conditions

            Node conditionStatement = new Node("ConditionStatement");
            conditionStatement.Children.Add(Condition());
            conditionStatement.Children.Add(Conditions());
            return conditionStatement;
        }

        Node Conditions()
        {
            // Conditions → Boolop Condition Conditions |  ε
            Node conditions = new Node("Conditions");
            if (InputPointer < TokenStream.Count && isItAStartOfAnotherCondition())
            {
                conditions.Children.Add(BoolOp());
                conditions.Children.Add(Condition());
                conditions.Children.Add(Conditions());
                return conditions;
            }
            else
            {
                return null;
            }
        }
        
        Node IfStatement()
        {
            // IfStatement → if ConditionStatement then StatementList ElseifStatements ElseStatement end
            Node ifStatement = new Node("IfStatement");
            inFunctionBody = false;
            ifStatement.Children.Add(Match(Token_Class.If));
            ifStatement.Children.Add(ConditionStatement());
            ifStatement.Children.Add(Match(Token_Class.Then));
            ifStatement.Children.Add(StatementList());
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Elseif)
            {
                ifStatement.Children.Add(ElseIfStatements());
            }
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Else)
            {
                ifStatement.Children.Add(ElseStatement());
            }
            ifStatement.Children.Add(Match(Token_Class.End));
            return ifStatement;
        }
        Node ElseIfStatements()
        {
            // ElseIfStatements → elseif ConditionStatement then StatementList ElseIfStatements | ε
            Node elseIfStatements = new Node("ElseIfStatements");
            if (InputPointer < TokenStream.Count && TokenStream[InputPointer].token_type == Token_Class.Elseif)
            {
                elseIfStatements.Children.Add(Match(Token_Class.Elseif));
                elseIfStatements.Children.Add(ConditionStatement());
                elseIfStatements.Children.Add(Match(Token_Class.Then));
                elseIfStatements.Children.Add(StatementList());
                elseIfStatements.Children.Add(ElseIfStatements());
                return elseIfStatements;
            }
            else
            {
                return null;
            }

        }
        Node ElseStatement()
        {
            // ElseStatement → else StatementList

            Node elseStatement = new Node("ElseStatement");
            elseStatement.Children.Add(Match(Token_Class.Else));
            elseStatement.Children.Add(StatementList());
            return elseStatement;
        }

        bool isItAStartOfStatement()
        {
            return TokenStream[InputPointer].token_type == Token_Class.Comment
                            ||
                        TokenStream[InputPointer].token_type == Token_Class.If
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Read
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Write
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Repeat
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.If
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Idenifier
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Float
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Int
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.String;
        }

        bool isItAStartOfAnotherCondition()
        {
            return (TokenStream[InputPointer].token_type == Token_Class.AndOp || TokenStream[InputPointer].token_type == Token_Class.OrOp);
        }

        bool IsArithmeticOperator(Token_Class tokenType)
        {
            return tokenType == Token_Class.PlusOp || tokenType == Token_Class.MinusOp || tokenType == Token_Class.MultiplyOp || tokenType == Token_Class.DivideOp;
        }
        bool isItADataType()
        {
            return TokenStream[InputPointer].token_type == Token_Class.Float
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.Int
                            ||
                            TokenStream[InputPointer].token_type == Token_Class.String;
        }
        public Node Match(Token_Class ExpectedToken)
        {

            if (InputPointer < TokenStream.Count)
            {
                if (ExpectedToken == TokenStream[InputPointer].token_type)
                {
                    InputPointer++;
                    Node newNode = new Node(ExpectedToken.ToString());

                    return newNode;

                }

                else
                {
                    Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + " and " +
                        TokenStream[InputPointer].token_type.ToString() +
                        "  found\r\n");
                    InputPointer++;
                    return null;
                }
            }
            else
            {
                Errors.Error_List.Add("Parsing Error: Expected "
                        + ExpectedToken.ToString() + "\r\n");
                InputPointer++;
                return null;
            }
        }
        public static TreeNode PrintParseTree(Node root)
        {
            TreeNode tree = new TreeNode("Parse Tree");
            TreeNode treeRoot = PrintTree(root);
            if (treeRoot != null)
                tree.Nodes.Add(treeRoot);
            return tree;
        }
        static TreeNode PrintTree(Node root)
        {
            if (root == null || root.Name == null)
                return null;
            TreeNode tree = new TreeNode(root.Name);
            if (root.Children.Count == 0)
                return tree;
            foreach (Node child in root.Children)
            {
                if (child == null)
                    continue;
                tree.Nodes.Add(PrintTree(child));
            }
            return tree;
        }
    }
}



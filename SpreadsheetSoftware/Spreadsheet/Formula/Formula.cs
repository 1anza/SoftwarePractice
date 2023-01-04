// Skeleton written by Joe Zachary for CS 3500, September 2013
// Read the entire skeleton carefully and completely before you
// do anything else!

// Version 1.1 (9/22/13 11:45 a.m.)

// Change log:
//  (Version 1.1) Repaired mistake in GetTokens
//  (Version 1.1) Changed specification of second constructor to
//                clarify description of how validation works

// (Daniel Kopta) 
// Version 1.2 (9/10/17) 

// Change log:
//  (Version 1.2) Changed the definition of equality with regards
//                to numeric tokens

// Author: Alivia Liljenquist
// uID: u1348865
// Filled in skeleton code for PS3.
// Last Updated: 9-17-21

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SpreadsheetUtilities
{

    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  The allowed symbols are non-negative numbers written using double-precision 
    /// floating-point syntax (without unary preceeding '-' or '+'); 
    /// variables that consist of a letter or underscore followed by 
    /// zero or more letters, underscores, or digits; parentheses; and the four operator 
    /// symbols +, -, *, and /.  
    /// 
    /// Spaces are significant only insofar that they delimit tokens.  For example, "xy" is
    /// a single variable, "x y" consists of two variables "x" and y; "x23" is a single variable; 
    /// and "x 23" consists of a variable "x" and a number "23".
    /// 
    /// Associated with every formula are two delegates:  a normalizer and a validator.  The
    /// normalizer is used to convert variables into a canonical form, and the validator is used
    /// to add extra restrictions on the validity of a variable (beyond the standard requirement 
    /// that it consist of a letter or underscore followed by zero or more letters, underscores,
    /// or digits.)  Their use is described in detail in the constructor and method comments.
    /// </summary>
    public class Formula
    {
        private List<string> tokens;
        private HashSet<string> variables;
        private List<string> hashList; //Keeps track of tokens list with numbers change to doubles

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically invalid,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer is the identity function, and the associated validator
        /// maps every string to true.  
        /// </summary>
        public Formula(String formula) :
            this(formula, s => s, s => true)
        {
        }

        /// <summary>
        /// Creates a Formula from a string that consists of an infix expression written as
        /// described in the class comment.  If the expression is syntactically incorrect,
        /// throws a FormulaFormatException with an explanatory Message.
        /// 
        /// The associated normalizer and validator are the second and third parameters,
        /// respectively.  
        /// 
        /// If the formula contains a variable v such that normalize(v) is not a legal variable, 
        /// throws a FormulaFormatException with an explanatory message. 
        /// 
        /// If the formula contains a variable v such that isValid(normalize(v)) is false,
        /// throws a FormulaFormatException with an explanatory message.
        /// 
        /// Suppose that N is a method that converts all the letters in a string to upper case, and
        /// that V is a method that returns true only if a string consists of one letter followed
        /// by one digit.  Then:
        /// 
        /// new Formula("x2+y3", N, V) should succeed
        /// new Formula("x+y3", N, V) should throw an exception, since V(N("x")) is false
        /// new Formula("2x+y3", N, V) should throw an exception, since "2x+y3" is syntactically incorrect.
        /// </summary>
        public Formula(String formula, Func<string, string> normalize, Func<string, bool> isValid)
        {
            tokens = new List<string>(GetTokens(formula).ToArray());
            variables = new HashSet<string>();
            hashList = new List<string>();

            // Checks if formula has at least 1 token
            if (tokens.Count != 0)
            {
                string varNumOpen = "^[a-zA-Z_][a-zA-Z0-9'_']*$|^[0-9]+$|^['(']$";
                string varNumClosed = "^[a-zA-Z_][a-zA-Z0-9'_']*$|^[0-9]+$|^[')']$";
                bool validStartingToken = IsDoubleOrPattern(tokens[0], varNumOpen); //Retrun true is token is a double, Valid variable, number, or opening parenthesis.
                bool validEndingToken = IsDoubleOrPattern(tokens[tokens.Count - 1], varNumClosed); //Retrun true is token is a double, Valid variable, number, or closing parenthesis.
                int openPar = 0;
                int closingPar = 0;

                // Checks if the starting and ending tokens are valid (without normailer).
                if (!IsTokenValid(tokens[0]) || !validStartingToken)
                {
                    throw new FormulaFormatException("Invalid first token. Start equation with number, open parethesis, or variable.");
                }

                if (!IsTokenValid(tokens[tokens.Count - 1]) || !validEndingToken)
                {
                    throw new FormulaFormatException("Invalid last token. End equation with number, closing parethesis, or variable.");
                }

                for (int i = 0; i < tokens.Count; i++)
                {
                    //Normalizes token and checks if it is still a valid variable.
                    //Adds variable to 'variables' list
                    string cur = tokens[i];
                    if (Regex.IsMatch(cur, "^[a-zA-Z_][a-zA-Z0-9'_']*$"))
                    {
                        string n = normalize(cur);
                        if (IsTokenValid(n) && isValid(n))
                        {
                            tokens[i] = n;
                        }
                        else
                        {
                            throw new FormulaFormatException("Variables did not pass validator or there is an invalid token.");
                        }

                        cur = tokens[i];

                        if (!(variables.Contains(cur)))
                            variables.Add(cur);
                    }

                    cur = tokens[i];
                    // Checks if all tokens are valid
                    if (IsTokenValid(cur))
                    {
                        // Checks correct parenthesis implementation
                        if (cur == "(")
                        {
                            openPar++;
                        }

                        if (cur == ")")
                        {
                            closingPar++;
                        }

                        if (closingPar > openPar)
                        {
                            throw new FormulaFormatException("Invaild use of parenthesis. There should be more opening parenthesis than closing.");
                        }

                        //Changes numbers and add them to token list 'hashList' with doubles for numbers
                        double num_parse;
                        if (double.TryParse(cur, out num_parse))
                        {
                            hashList.Add(num_parse.ToString());
                        }
                        else
                        {
                            hashList.Add(cur);
                        }

                        if (i != 0)
                        {
                            string prev = hashList[i - 1];
                            bool followingOperator = IsDoubleOrPattern(cur, varNumOpen);

                            // Checks if token following an operator or open parenthesis is either a variable, number, or open parenthesis.
                            if (prev == "(" || prev == "+" || prev == "-" || prev == "*" || prev == "/")
                            {
                                if (!followingOperator)
                                    throw new FormulaFormatException("Token following '(' or operator was not valid.");

                            }
                            // Checks if token following a number, a variable, or a closing parenthesis is an operator or closing parenthesis
                            else if (IsDoubleOrPattern(prev, varNumClosed) && i == tokens.Count - 1)
                            {
                                if (cur != ")")
                                {
                                    throw new FormulaFormatException("Invalid extra operator.");
                                }
                                else
                                {
                                    continue;
                                }
                            }
                            else if (IsDoubleOrPattern(prev, varNumClosed))
                            {
                                if (!(cur == "+" || cur == "-" || cur == "*" || cur == "/" || cur == ")"))
                                {
                                    throw new FormulaFormatException("There has to be a number after operator.");
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }

                // Checks parenthesis balance
                if (openPar != closingPar)
                {
                    throw new FormulaFormatException("Number of opening parentheses does not match number of closing parentheses.");
                }
            }
            else
            {
                throw new FormulaFormatException("No tokens found. Formula empty.");
            }

        }

        /// <summary>
        /// Evaluates this Formula, using the lookup delegate to determine the values of
        /// variables.  When a variable symbol v needs to be determined, it should be looked up
        /// via lookup(normalize(v)). (Here, normalize is the normalizer that was passed to 
        /// the constructor.)
        /// 
        /// For example, if L("x") is 2, L("X") is 4, and N is a method that converts all the letters 
        /// in a string to upper case:
        /// 
        /// new Formula("x+7", N, s => true).Evaluate(L) is 11
        /// new Formula("x+7").Evaluate(L) is 9
        /// 
        /// Given a variable symbol as its parameter, lookup returns the variable's value 
        /// (if it has one) or throws an ArgumentException (otherwise).
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, the value is returned.  Otherwise, a FormulaError is returned.  
        /// The Reason property of the FormulaError should have a meaningful explanation.
        ///
        /// This method should never throw an exception.
        /// </summary>
        public object Evaluate(Func<string, double> lookup)
        {
            Stack<double> values = new Stack<double>();
            Stack<string> operations = new Stack<string>();

            foreach (string token in hashList)
            {
                double num;
                bool isNumeric = double.TryParse(token, out num);
                // Preforms operations if token is a double
                if (isNumeric && values.Count == 0)
                {
                    values.Push(num);
                }
                else if (isNumeric && operations.Count > 0)
                {
                    values.Push(num);

                    if (operations.Peek().Equals("*") || operations.Peek().Equals("/"))
                    {
                        string op = operations.Pop();
                        if (op.Equals("/") && values.Peek().Equals(0))
                            return new FormulaError();
                        else
                            values.Push(BasicOperation(op, values));
                    }
                }
                // Preforms operations if token is a variable
                else if (GetVariables().Contains(token) && values.Count == 0)
                {
                    try
                    {
                        double varNum = lookup(token);
                        values.Push(varNum);
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError();
                    }

                }
                else if (GetVariables().Contains(token) && operations.Count > 0)
                {
                    try
                    {
                        double varNum = lookup(token);
                        values.Push(varNum);
                    }
                    catch (ArgumentException)
                    {
                        return new FormulaError();
                    }

                    if (operations.Peek().Equals("*") || operations.Peek().Equals("/"))
                    {
                        string op = operations.Pop();
                        if (op.Equals("/") && values.Peek().Equals(0))
                            return new FormulaError();
                        else
                            values.Push(BasicOperation(op, values));
                    }
                }

                // Preforms operations if token is an opertor
                if (token == "*" || token == "/" || token == "(")
                {
                    operations.Push(token);
                }
                else if (token == "+" || token == "-")
                {
                    if (operations.Count != 0)
                    {
                        if (operations.Peek().Equals("+") || operations.Peek().Equals("-"))
                        {
                            string op = operations.Pop();
                            values.Push(BasicOperation(op, values));
                            operations.Push(token);
                        }
                        else
                        {
                            operations.Push(token);
                        }
                    }
                    else
                    {
                        operations.Push(token);
                    }

                }
                else if (token == ")")
                {
                    if (values.Count == 1 || operations.Count == 0)
                    {
                        continue;
                    }
                    else
                    {
                        if (operations.Peek().Equals("+") || operations.Peek().Equals("-"))
                        {
                            string op = operations.Pop();
                            values.Push(BasicOperation(op, values));
                        }

                        if (operations.Peek().Equals("("))
                            operations.Pop();

                        if (operations.Count != 0)
                        {
                            if (operations.Peek().Equals("*") || operations.Peek().Equals("/"))
                            {
                                string op = operations.Pop();
                                if (op.Equals("/") && values.Peek().Equals(0))
                                    return new FormulaError();
                                else
                                    values.Push(BasicOperation(op, values));
                            }
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
            }

            // Finishes solving formula
            if (operations.Count != 0 && values.Count != 1)
            {
                while (operations.Count > 0)
                {
                    string op = operations.Pop();
                    if (op.Equals("/") && values.Peek().Equals(0))
                        return new FormulaError();
                    else
                        values.Push(BasicOperation(op, values));
                }

                return (double)values.Pop();
            }
            else
            {
                return (double)values.Pop();
            }
        }

        /// <summary>
        /// Enumerates the normalized versions of all of the variables that occur in this 
        /// formula.  No normalization may appear more than once in the enumeration, even 
        /// if it appears more than once in this Formula.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x+y*z", N, s => true).GetVariables() should enumerate "X", "Y", and "Z"
        /// new Formula("x+X*z", N, s => true).GetVariables() should enumerate "X" and "Z".
        /// new Formula("x+X*z").GetVariables() should enumerate "x", "X", and "z".
        /// </summary>
        public IEnumerable<String> GetVariables()
        {
            return variables;
        }

        /// <summary>
        /// Returns a string containing no spaces which, if passed to the Formula
        /// constructor, will produce a Formula f such that this.Equals(f).  All of the
        /// variables in the string should be normalized.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        /// 
        /// new Formula("x + y", N, s => true).ToString() should return "X+Y"
        /// new Formula("x + Y").ToString() should return "x+Y"
        /// </summary>
        public override string ToString()
        {
            return string.Join("", hashList); //Returns string with numbers reduces to doubles
        }

        /// <summary>
        /// If obj is null or obj is not a Formula, returns false.  Otherwise, reports
        /// whether or not this Formula and obj are equal.
        /// 
        /// Two Formulae are considered equal if they consist of the same tokens in the
        /// same order.  To determine token equality, all tokens are compared as strings 
        /// except for numeric tokens and variable tokens.
        /// Numeric tokens are considered equal if they are equal after being "normalized" 
        /// by C#'s standard conversion from string to double, then back to string. This 
        /// eliminates any inconsistencies due to limited floating point precision.
        /// Variable tokens are considered equal if their normalized forms are equal, as 
        /// defined by the provided normalizer.
        /// 
        /// For example, if N is a method that converts all the letters in a string to upper case:
        ///  
        /// new Formula("x1+y2", N, s => true).Equals(new Formula("X1  +  Y2")) is true
        /// new Formula("x1+y2").Equals(new Formula("X1+Y2")) is false
        /// new Formula("x1+y2").Equals(new Formula("y2+x1")) is false
        /// new Formula("2.0 + x7").Equals(new Formula("2.000 + x7")) is true
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Formula))
                return false;
            else
                return string.Join("", this.hashList) == string.Join("", ((Formula)obj).hashList);
        }

        /// <summary>
        /// Reports whether f1 == f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return true.  If one is
        /// null and one is not, this method should return false.
        /// </summary>
        public static bool operator ==(Formula f1, Formula f2)
        {
            if (ReferenceEquals(f1, null))
                return ReferenceEquals(f2, null);
            else
                return f1.Equals(f2);
        }

        /// <summary>
        /// Reports whether f1 != f2, using the notion of equality from the Equals method.
        /// Note that if both f1 and f2 are null, this method should return false.  If one is
        /// null and one is not, this method should return true.
        /// </summary>
        public static bool operator !=(Formula f1, Formula f2)
        {
            if (ReferenceEquals(f1, null))
                return !(ReferenceEquals(f2, null));
            else
                return !(f1.Equals(f2));
        }

        /// <summary>
        /// Returns a hash code for this Formula.  If f1.Equals(f2), then it must be the
        /// case that f1.GetHashCode() == f2.GetHashCode().  Ideally, the probability that two 
        /// randomly-generated unequal Formulae have the same hash code should be extremely small.
        /// </summary>
        public override int GetHashCode()
        {
            return string.Join("", hashList).GetHashCode();
        }

        /// <summary>
        /// Given an expression, enumerates the tokens that compose it.  Tokens are left paren;
        /// right paren; one of the four operator symbols; a string consisting of a letter or underscore
        /// followed by zero or more letters, digits, or underscores; a double literal; and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z_](?: [a-zA-Z_]|\d)*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: [eE][\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }

        }

        /// <summary>
        /// Helper method that checks if token is a correctly formatted varible, number, or operator
        /// </summary>
        private static bool IsTokenValid(string s)
        {
            double num;
            bool isNumeric = double.TryParse(s, out num);
            if (isNumeric || s == "(" || s == ")" || s == "+" || s == "-" || s == "*" || s == "/")
            {
                return true;
            }
            // Check for valid variable format
            else if (Regex.IsMatch(s, "^[a-zA-Z_][a-zA-Z0-9'_']*$"))
            {
                return true;
            }
            else
            {
                throw new FormulaFormatException("Invalid token. Make sure values entered are (, ), +, -, *, /, variables, and/or real numbers");
            }
        }

        /// <summary>
        /// Helper method that checks if something is either a double or a specific condition.
        /// Used to easily check what token goes after what in Formula.
        /// </summary>
        private static bool IsDoubleOrPattern(string var, string pattern)
        {
            double num;
            bool IsDouble = double.TryParse(var, out num);
            bool IsPattern = Regex.IsMatch(var, pattern);
            if (IsDouble || IsPattern)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Helper method that preforms a basic operation based on the passed in value.
        /// </summary>
        private static double BasicOperation(string op, Stack<double> values)
        {
            double v1 = values.Pop();
            double v2 = values.Pop();

            switch (op)
            {
                case "+":
                    return v2 + v1;
                case "-":
                    return v2 - v1;
                case "*":
                    return v2 * v1;
                case "/":
                    return v2 / v1;
                default:
                    return 0;
            }
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the argument to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message)
            : base(message)
        {
        }
    }

    /// <summary>
    /// Used as a possible return value of the Formula.Evaluate method.
    /// </summary>
    public struct FormulaError
    {
        /// <summary>
        /// Constructs a FormulaError containing the explanatory reason.
        /// </summary>
        /// <param name="reason"></param>
        public FormulaError(String reason)
            : this()
        {
            Reason = reason;
        }

        /// <summary>
        ///  The reason why this FormulaError was created.
        /// </summary>
        public string Reason { get; private set; }
    }
}
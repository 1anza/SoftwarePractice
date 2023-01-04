using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;

namespace FormulaEvaluator
{
    public static class Evaluator
    {
        public delegate int Lookup(String v);
        public static int Evaluate(String exp, Lookup variableEvaluator)
        {
            string[] substrings = Regex.Split(exp, "(\\()|(\\))|(-)|(\\+)|(\\*)|(/)");
            Stack values = new Stack();
            Stack operations = new Stack();

            foreach (string s in substrings)
            {
                string val = Regex.Replace(s, @"\s+", "");
                int num;
                bool isNumeric = int.TryParse(val, out num);

      
                if(isNumeric)
                {
                    if (operations.Count > 0 && values.Count > 0)
                    {
                        if (operations.Peek().Equals("*"))
                        {
                            operations.Pop();
                            int popped = (int)values.Pop();
                            values.Push(num * popped);
                        }
                        else if (operations.Peek().Equals("/"))
                        {
                            operations.Pop();
                            int popped = (int)values.Pop();
                            if(num == 0)
                            {
                                throw new ArgumentException("You can not divide by zero.");
                            }
                            else
                            {
                                values.Push(popped / num);
                            }
                        }
                        else if (operations.Peek().Equals("+") || operations.Peek().Equals("-") || operations.Peek().Equals("("))
                        {
                            values.Push(num);
                        }
                        else
                        {
                            throw new ArgumentException("Invalid operator.");
                        } 
                    }
                    else
                    {
                        values.Push(num);
                    }
                }
                else
                {
                    if(val=="+" || val=="-")
                    {
                        if(values.Count >= 2 && operations.IsOnTop("+"))
                        {
                            int var1 = (int)values.Pop();
                            int var2 = (int)values.Pop();
                            operations.Push(val);
                            values.Push(var1 + var2);
                        }
                        else if (values.Count >= 2 && operations.IsOnTop("-"))
                        {
                            int var1 = (int)values.Pop();
                            int var2 = (int)values.Pop();
                            operations.Push(val);
                            values.Push(var2 - var1);
                        }
                        else
                        {
                            operations.Push(val);
                        }
                    }
                    else if (val=="*" || val=="/" || val=="(")
                    {
                        operations.Push(val);
                    }
                    else if (val==")" && operations.Contains("("))
                    {
                        while(!operations.Peek().Equals("("))
                        {
                            if (values.Count >= 2 && operations.IsOnTop("+"))
                            {
                                values.Push(PushResult(values, operations, "+"));
                            }
                            else if (values.Count >= 2 && operations.IsOnTop("-"))
                            {
                                values.Push(PushResult(values, operations, "-"));
                            }
                            else if (values.Count >= 2 && operations.IsOnTop("*"))
                            {
                                values.Push(PushResult(values, operations, "*"));
                            }
                            else if (values.Count >= 2 && operations.IsOnTop("/"))
                            {
                                values.Push(PushResult(values, operations, "/"));
                            }
                        }


                        if (operations.Count > 0 && operations.Peek().Equals("("))
                        {
                            operations.Pop();
                        }

                    }
                    else if(Regex.IsMatch(val, "^[a-z]+[0-9]+$|^[A-Z]+[0-9]+$"))
                    {
                        int varNum = variableEvaluator(val);
                        if (values.Count > 0 && operations.Count > 0)
                        {
                            if (operations.Peek().Equals("*"))
                            {
                                operations.Pop();
                                int popped = (int)values.Pop();
                                values.Push(varNum * popped);
                            }
                            else if (operations.Peek().Equals("/"))
                            {
                                operations.Pop();
                                int popped = (int)values.Pop();
                                if (varNum == 0)
                                {
                                    throw new ArgumentException("You can not divide by zero.");
                                }
                                else
                                {
                                    values.Push(popped / varNum);
                                }
                            }
                            else
                            {
                               values.Push(varNum);
                            }
                        }
                        else
                        {
                            values.Push(varNum);
                        }
                    }
                    else if (val == "" || val == " ")
                    {
                        continue;
                    }
                    else
                    {
                        throw new ArgumentException("Unexpected non-numerator or extra operator.");
                    }
                }
            }


            if (operations.Count == 0)
            {
                if (values.Count == 1)
                {
                    return (int)values.Pop();
                }
                else
                {
                    throw new ArgumentException("More than one number in final value.");
                }
            } 
            else if (values.Count>1)
            {   
                while(values.Count > 1 && operations.Count > 0)
                {
                    if (operations.Peek().Equals("+") && values.Count == 2)
                    {
                        int var1 = (int)values.Pop();
                        int var2 = (int)values.Pop();
                        operations.Pop();
                        return var1 + var2;
                    }
                    else if (operations.Peek().Equals("-") && values.Count == 2)
                    {
                        int var1 = (int)values.Pop();
                        int var2 = (int)values.Pop();
                        operations.Pop();
                        return var2 - var1;
                    }
                    else if (operations.Peek().Equals("*") && values.Count == 2)
                    {
                        operations.Pop();
                        int var1 = (int)values.Pop();
                        int var2 = (int)values.Pop();
                        values.Push(var1 * var2);
                    }
                    else if (operations.Peek().Equals("/") && values.Count == 2)
                    {
                        operations.Pop();
                        int var1 = (int)values.Pop();
                        int var2 = (int)values.Pop();
                        if (var1 == 0)
                        {
                            throw new ArgumentException("You can not divide by zero.");
                        }
                        else
                        {
                            values.Push(var2 / var1);
                        }
                    }
                }

                return (int)values.Pop();
            }
            else
            {
                if (operations.Count > 0)
                {
                    throw new ArgumentException("Extra operator");
                }

                return (int)values.Pop();
            }
        }

        public static int PushResult(Stack a, Stack b, string op)
        {
            int var1 = (int)a.Pop();
            int var2 = (int)a.Pop();
            b.Pop();
            if (op.Equals("+"))
            {
                return var1 + var2;
            }
            else if (op.Equals("-"))
            {
                return var2 - var1;
            }
            else if (op.Equals("*"))
            {
                return var1 * var2;
            }
            else if (op.Equals("/"))
            {
                if (var1 == 0)
                {
                    throw new ArgumentException("You can not divide by zero.");
                }
                else
                {
                    return var2 / var1;
                }
            }
            else
            {
                throw new ArgumentException("Invalid Operator");
            }
        }

        public static bool IsOnTop(this Stack s, string c)
        {
            return s.Count > 0 && s.Peek().Equals(c);
        }

    }
}

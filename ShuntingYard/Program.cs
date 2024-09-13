namespace ShuntingYard
{
    enum TokenType
    {
        Numeric,
        Operator,
        Function
    }
    struct Token
    {
        public string token;
        public TokenType type;
    }

    internal class Program
    {
        static Token PopToken(string expression, out string popped) // Pops a token from the start of the string
        {
            Token result = new Token();

            expression = expression.TrimStart();
            
            char[] operators = { '+', '-', '*', '/', '^', '(', ')', ','};
            char[] numbers = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };

            if (operators.Contains(expression[0])) {
                result.token = new string(expression[0], 1); // Create a string of the operator
                result.type = TokenType.Operator;
                popped = expression.Substring(1);

                return result;
            }else if (numbers.Contains(expression[0]))
            {
                int numLength = 1;
                
                while (float.TryParse(expression.Substring(0, numLength), out float x))
                {
                    numLength++;
                    if (numLength >= expression.Length)
                    {
                        break;
                    }
                }

                result.token = expression.Substring(0, numLength-1);
                result.type = TokenType.Numeric;
                popped = expression.Substring(numLength-1);
                
                return result;
            }
            else
            {
                int funcNameLength = 0;
                while (funcNameLength < expression.Length && expression[funcNameLength] != ' ' && expression[funcNameLength] != '(' && expression[funcNameLength] != ')' && !operators.Contains(expression[funcNameLength]))
                    funcNameLength++;

                result.token = expression.Substring(0, funcNameLength);
                result.type = TokenType.Function;

                if (funcNameLength == 2 && expression.Substring(0, 2) == "pi") // Special case for pi
                    result.type = TokenType.Numeric;
                
                popped = expression.Substring(funcNameLength);


                return result;
                //throw new Exception($"Unrecognised token {expression[0]}");
            }
        }

        public static string ShuntingYard(string expression)
        {
            string[] operators = { "^", "*", "/", "+", "-" }; // Only the real operators, no brackets or comma

            Stack<string> op = new Stack<string>();
            string result = "";

            while (expression.Length > 0)
            {
                Token next = PopToken(expression, out string popped);
                expression = popped;

                switch (next.type)
                {
                    case TokenType.Numeric:
                        result = result + " " + next.token;
                        break;
                    case TokenType.Function:
                        op.Push(next.token);
                        break;
                    case TokenType.Operator:
                        bool opIsNotEmpty = op.TryPeek(out string peekNext);

                        switch (next.token)
                        {
                            case "^":
                                op.Push("^");
                                break;

                            case "*":
                                while (opIsNotEmpty && (peekNext == "^" || peekNext == "*" || peekNext == "/")) // These 2 have the same precedence as *
                                { 
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                op.Push("*");
                                break;

                            case "/":
                                while (opIsNotEmpty && (peekNext == "^" || peekNext == "*" || peekNext == "/")) // These 2 have the same precedence as /
                                { 
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                op.Push("/");
                                break;

                            case "+":
                                while (opIsNotEmpty && (peekNext == "^" || peekNext == "*" || peekNext == "/" || peekNext == "-" || peekNext == "+"))
                                {
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                op.Push("+");
                                break;

                            case "-":
                                while (opIsNotEmpty && (peekNext == "^" || peekNext == "*" || peekNext == "/" || peekNext == "-" || peekNext == "+"))
                                {
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                op.Push("-");
                                break;

                            case "(":
                                op.Push("(");
                                break;

                            case ")":
                                while (opIsNotEmpty && peekNext != "(")
                                {
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                if (!opIsNotEmpty)
                                    throw new Exception("Mismatched brackets");

                                op.Pop(); // This is the open bracket

                                if (opIsNotEmpty && !operators.Contains(peekNext)) // Use lazy evaluation to avoid peekNext being wrong
                                { 
                                    result = result + " " + op.Pop(); // Pop the function
                                }

                                break;

                            case ",":
                                while (opIsNotEmpty && peekNext != "(")
                                {
                                    result = result + " " + op.Pop();
                                    opIsNotEmpty = op.TryPeek(out peekNext);
                                }
                                if (!opIsNotEmpty)
                                    throw new Exception("Why is there a random comma");

                                break;
                        }
                        break;
                    default:
                        throw new Exception("Achievement unlocked: How did we get here?");
                }
            }

            
            while (op.TryPeek(out string peekNext))
            {
                result = result + " " + op.Pop();
            }

            return result;
        }

        public static double EvaluateRPN(string expression)
        {
            Stack<double> values = new Stack<double>();

            while (expression.Length > 0)
            {
                Token next = PopToken(expression, out string popped);
                expression = popped;

                if (next.type == TokenType.Numeric)
                {
                    if (next.token == "pi")
                        values.Push(Math.PI);
                    else
                    {
                        values.Push(double.Parse(next.token));
                    }
                } else
                {
                    double first = values.Pop();
                    bool stackIsNotEmpty = values.TryPeek(out double second);

                    switch (next.token)
                    {
                        case "+":
                            second = values.Pop();
                            values.Push(first + second);
                            break;

                        case "-":
                            second = values.Pop();
                            values.Push(second - first);
                            break;

                        case "*":
                            second = values.Pop();
                            values.Push(first * second);
                            break;

                        case "/":
                            second = values.Pop();
                            values.Push(second / first);
                            break;

                        case "^":
                            second = values.Pop();
                            values.Push(Math.Pow(second, first));
                            break;

                        case "exp":
                            values.Push(Math.Exp(first));
                            break;

                        case "sin":
                            values.Push(Math.Sin(first));
                            break;

                        case "cos":
                            values.Push(Math.Cos(first));
                            break;

                        case "tan":
                            values.Push(Math.Tan(first));
                            break;

                        case "abs":
                            values.Push(Math.Abs(first));
                            break;

                        case "log":
                            values.Push(Math.Log10(first));
                            break;

                        case "ln":
                            values.Push(Math.Log(first));
                            break;

                        default:
                            throw new NotImplementedException($"Function {next.token} not yet implemented");
                    }
                }

            }
            return values.Pop();
        }
        static void Main(string[] args)
        {
            string a = "2*cos(pi/4)+log(10^3)";
            string b = ShuntingYard(a);
            Console.WriteLine(b);
            Console.WriteLine(EvaluateRPN(b));
            
        }
    }
}

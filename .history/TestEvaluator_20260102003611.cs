using System;
using System.Globalization;

public class TestEvaluator
{
    static void Main()
    {
        // Test expression evaluation
        double result = 6500000 + 980000 + (6500000/22/8*24*2) + 60000 + 710000;
        Console.WriteLine($"Expected result: {result}");
        
        // Test the failing expression after parentheses removal
        // Expression: 6500000 + 980000 +(6500000/22/8*24*2) + 60000 + 710000
        // After removing parentheses: 6500000 + 980000 +295454.54545... + 60000 + 710000
        
        double div1 = 6500000 / 22;
        Console.WriteLine($"6500000 / 22 = {div1}");
        
        double div2 = div1 / 8;
        Console.WriteLine($"... / 8 = {div2}");
        
        double mult1 = div2 * 24;
        Console.WriteLine($"... * 24 = {mult1}");
        
        double mult2 = mult1 * 2;
        Console.WriteLine($"... * 2 = {mult2}");
        
        // After parentheses eval: 6500000 + 980000 + [result] + 60000 + 710000
        string exprAfterParens = $"6500000 + 980000 + {mult2.ToString("G17", CultureInfo.InvariantCulture)} + 60000 + 710000";
        Console.WriteLine($"\nExpression after parentheses removal: {exprAfterParens}");
        
        // Test simpler expressions
        Console.WriteLine("\nTesting simpler expressions:");
        Console.WriteLine($"100 + 50 = {SimpleEval("100+50")}");
        Console.WriteLine($"100 * 2 = {SimpleEval("100*2")}");
        Console.WriteLine($"10 / 2 = {SimpleEval("10/2")}");
        Console.WriteLine($"2 ^ 3 = {SimpleEval("2^3")}");
        Console.WriteLine($"100 + 50 * 2 = {SimpleEval("100+50*2")}");
    }
    
    static double SimpleEval(string expr)
    {
        expr = expr.Trim();
        
        // Handle ^ first
        while (expr.Contains("^"))
        {
            int idx = expr.IndexOf("^");
            
            int leftEnd = idx - 1;
            while (leftEnd > 0 && (char.IsDigit(expr[leftEnd - 1]) || expr[leftEnd - 1] == '.'))
                leftEnd--;
            
            int rightStart = idx + 1;
            while (rightStart < expr.Length - 1 && (char.IsDigit(expr[rightStart + 1]) || expr[rightStart + 1] == '.'))
                rightStart++;
            
            string leftStr = expr.Substring(leftEnd, idx - leftEnd);
            string rightStr = expr.Substring(idx + 1, rightStart - idx);
            
            double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
            double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
            double result = Math.Pow(leftVal, rightVal);
            
            expr = expr.Substring(0, leftEnd) + result.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightStart + 1);
        }
        
        // Handle * and /
        while (expr.Contains("*") || expr.Contains("/"))
        {
            int mulIdx = expr.IndexOf("*");
            int divIdx = expr.IndexOf("/");
            int idx = -1;
            char op = ' ';
            
            if (mulIdx != -1 && divIdx != -1)
                idx = mulIdx < divIdx ? mulIdx : divIdx;
            else if (mulIdx != -1)
                idx = mulIdx;
            else
                idx = divIdx;
            
            op = expr[idx];
            
            int leftEnd = idx - 1;
            while (leftEnd > 0 && (char.IsDigit(expr[leftEnd - 1]) || expr[leftEnd - 1] == '.'))
                leftEnd--;
            
            int rightStart = idx + 1;
            while (rightStart < expr.Length - 1 && (char.IsDigit(expr[rightStart + 1]) || expr[rightStart + 1] == '.'))
                rightStart++;
            
            string leftStr = expr.Substring(leftEnd, idx - leftEnd);
            string rightStr = expr.Substring(idx + 1, rightStart - idx);
            
            double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
            double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
            double result = op == '*' ? leftVal * rightVal : leftVal / rightVal;
            
            expr = expr.Substring(0, leftEnd) + result.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightStart + 1);
        }
        
        // Handle + and -
        int startIdx = 0;
        if (expr.Length > 0 && expr[0] == '-')
            startIdx = 1;
        
        while (true)
        {
            int opIdx = -1;
            char opChar = ' ';
            
            for (int i = startIdx; i < expr.Length; i++)
            {
                if (expr[i] == '+' || expr[i] == '-')
                {
                    opIdx = i;
                    opChar = expr[i];
                    break;
                }
            }
            
            if (opIdx == -1)
                break;
            
            string leftStr = expr.Substring(0, opIdx);
            string rightStr = expr.Substring(opIdx + 1);
            
            double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
            double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
            double result = opChar == '+' ? leftVal + rightVal : leftVal - rightVal;
            
            expr = result.ToString("G17", CultureInfo.InvariantCulture);
            startIdx = 0;
        }
        
        return double.Parse(expr, CultureInfo.InvariantCulture);
    }
}

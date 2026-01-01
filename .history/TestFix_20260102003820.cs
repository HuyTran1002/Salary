using System;
using System.Globalization;

public class QuickTest
{
    static void Main()
    {
        // Test the problematic expression
        double result = SimpleEval("6500000+980000+1772724+60000+710000");
        Console.WriteLine($"6500000+980000+1772724+60000+710000 = {result}");
        Console.WriteLine($"Expected: 10022724");
        Console.WriteLine();
        
        // Test with division results
        result = SimpleEval("100/3+50");
        Console.WriteLine($"100/3+50 = {result}");
        Console.WriteLine($"Expected: ~83.33333");
    }
    
    static double SimpleEval(string expr)
    {
        expr = expr.Trim();
        
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
            
            // Find left number
            int leftEnd = idx - 1;
            while (leftEnd > 0 && (char.IsDigit(expr[leftEnd - 1]) || expr[leftEnd - 1] == '.'))
                leftEnd--;
            
            // Find right number - FIXED: Go through entire number including last digit
            int rightEnd = idx + 1;
            while (rightEnd < expr.Length && (char.IsDigit(expr[rightEnd]) || expr[rightEnd] == '.'))
                rightEnd++;
            rightEnd--;  // Move back to last digit
            
            string leftStr = expr.Substring(leftEnd, idx - leftEnd);
            string rightStr = expr.Substring(idx + 1, rightEnd - idx);
            
            Console.WriteLine($"Processing: {leftStr} {op} {rightStr}");
            
            double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
            double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
            double res = op == '*' ? leftVal * rightVal : leftVal / rightVal;
            
            expr = expr.Substring(0, leftEnd) + res.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
            Console.WriteLine($"Result: {expr}");
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
            
            Console.WriteLine($"Processing: {leftStr} {opChar} {rightStr}");
            
            double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
            double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
            double res = opChar == '+' ? leftVal + rightVal : leftVal - rightVal;
            
            expr = res.ToString("G17", CultureInfo.InvariantCulture);
            Console.WriteLine($"Result: {expr}");
            startIdx = 0;
        }
        
        return double.Parse(expr, CultureInfo.InvariantCulture);
    }
}

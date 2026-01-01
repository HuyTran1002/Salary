using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Windows.Forms;

namespace SalaryCalculator
{
    // Professional calculator UI inspired by real calculators
    public class CalculatorForm : Form
    {
        private Label displayLabel;
        private string expression = "0";  // Biểu thức toàn bộ
        private string lastAnswer = "0";
        private bool newNumber = true;
        private List<string> formulaHistory = new List<string>();  // Lưu công thức
        private int historyIndex = -1;

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Máy Tính";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 420;
            this.Height = 680;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(40, 40, 40);
            this.KeyPreview = true;
            this.KeyDown += CalculatorForm_KeyDown;

            // Display screen - tăng tương phản
            displayLabel = new Label();
            displayLabel.Text = "0";
            displayLabel.Font = new Font("Segoe UI", 36, FontStyle.Bold);
            displayLabel.ForeColor = Color.FromArgb(255, 140, 0);  // Màu cam
            displayLabel.BackColor = Color.Black;   // Nền đen tuyệt đối
            displayLabel.BorderStyle = BorderStyle.FixedSingle;
            displayLabel.TextAlign = ContentAlignment.MiddleRight;
            displayLabel.Location = new Point(10, 10);
            displayLabel.Size = new Size(380, 90);
            displayLabel.Tag = "0";  // Lưu giá trị hiện tại
            this.Controls.Add(displayLabel);

            // Button layout: 4 columns x 8 rows (remote TV style + number pad + long equals)
            var table = new TableLayoutPanel();
            table.ColumnCount = 4;
            table.RowCount = 8;
            table.Width = 400;
            table.Height = 560;
            table.Location = new Point(10, 110);
            table.BackColor = Color.FromArgb(40, 40, 40);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            
            for (int c = 0; c < table.ColumnCount; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < table.RowCount; r++)
            {
                // Row cuối cùng (=) sẽ chiếm 15%, các row khác chia đều
                if (r == table.RowCount - 1)
                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
                else
                    table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 8f));
            }

            this.Controls.Add(table);

            // Button factory with professional styling
            Button AddBtn(string text, int col, int row, int colSpan = 1, int rowSpan = 1, string tag = null, Color? bgColor = null)
            {
                var b = new Button();
                b.Dock = DockStyle.Fill;
                b.Text = text;
                b.Tag = tag ?? text;
                b.Font = new Font("Segoe UI", 16, FontStyle.Bold);
                b.ForeColor = Color.White;
                b.BackColor = bgColor ?? Color.FromArgb(80, 80, 80);
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderColor = Color.FromArgb(100, 100, 100);
                b.FlatAppearance.BorderSize = 1;
                b.Cursor = Cursors.Hand;
                b.Click += CalcButton_Click;
                
                b.MouseEnter += (s, e) => b.BackColor = bgColor.HasValue 
                    ? DarkenColor((Color)bgColor) 
                    : Color.FromArgb(100, 100, 100);
                b.MouseLeave += (s, e) => b.BackColor = bgColor ?? Color.FromArgb(80, 80, 80);
                
                table.Controls.Add(b, col, row);
                if (colSpan > 1) table.SetColumnSpan(b, colSpan);
                if (rowSpan > 1) table.SetRowSpan(b, rowSpan);
                return b;
            }

            // Color definitions
            Color acColor = Color.FromArgb(200, 50, 50);
            Color opColor = Color.FromArgb(200, 100, 50);
            Color eqColor = Color.FromArgb(50, 150, 50);
            Color navColor = Color.FromArgb(100, 140, 200);  // Xanh dương cho nút điều hướng

            // Row 0: ↑ (centered) - Navigation Up
            AddBtn("↑", 1, 0, colSpan: 2, tag: "UP", bgColor: navColor);

            // Row 1: ← ↓ → (Remote TV style)
            AddBtn("←", 0, 1, tag: "LEFT", bgColor: navColor);
            AddBtn("↓", 1, 1, colSpan: 2, tag: "DOWN", bgColor: navColor);
            AddBtn("→", 3, 1, tag: "RIGHT", bgColor: navColor);

            // Row 2: AC, DEL, (, )
            AddBtn("AC", 0, 2, tag: "AC", bgColor: acColor);
            AddBtn("DEL", 1, 2, tag: "DEL", bgColor: acColor);
            AddBtn("(", 2, 2, tag: "(");
            AddBtn(")", 3, 2, tag: ")");

            // Row 3: 7, 8, 9, /
            AddBtn("7", 0, 3);
            AddBtn("8", 1, 3);
            AddBtn("9", 2, 3);
            AddBtn("÷", 3, 3, tag: "/", bgColor: opColor);

            // Row 4: 4, 5, 6, *
            AddBtn("4", 0, 4);
            AddBtn("5", 1, 4);
            AddBtn("6", 2, 4);
            AddBtn("×", 3, 4, tag: "*", bgColor: opColor);

            // Row 5: 1, 2, 3, -
            AddBtn("1", 0, 5);
            AddBtn("2", 1, 5);
            AddBtn("3", 2, 5);
            AddBtn("−", 3, 5, tag: "-", bgColor: opColor);

            // Row 6: 0 (spans 2 cols), ., +
            AddBtn("0", 0, 6, colSpan: 2);
            AddBtn(".", 2, 6, tag: ".");
            AddBtn("+", 3, 6, tag: "+", bgColor: opColor);

            // Row 7: %, Ans, ^ và = (dài nhất)
            AddBtn("%", 0, 7, tag: "%", bgColor: opColor);
            AddBtn("Ans", 1, 7, tag: "Ans", bgColor: opColor);
            AddBtn("^", 2, 7, tag: "^", bgColor: opColor);
            AddBtn("=", 3, 7, tag: "=", bgColor: eqColor);

            try { Theme.ApplyEcommerceTheme(this); } catch { }
            
            // Đảm bảo displayLabel có màu cam sau khi theme được áp dụng
            displayLabel.ForeColor = Color.FromArgb(255, 140, 0);  // Màu cam
            displayLabel.BackColor = Color.Black;   // Nền đen
        }

        private Color DarkenColor(Color color)
        {
            return Color.FromArgb(
                Math.Max(0, color.R - 30),
                Math.Max(0, color.G - 30),
                Math.Max(0, color.B - 30)
            );
        }

        private void CalcButton_Click(object sender, EventArgs e)
        {
            if (!(sender is Button b)) return;
            string key = (b.Tag as string) ?? b.Text;

            // History navigation
            if (key == "UP")
            {
                if (historyIndex < formulaHistory.Count - 1)
                {
                    historyIndex++;
                    expression = formulaHistory[formulaHistory.Count - 1 - historyIndex];
                    UpdateDisplay();
                }
                return;
            }

            if (key == "DOWN")
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    expression = formulaHistory[formulaHistory.Count - 1 - historyIndex];
                    UpdateDisplay();
                }
                else if (historyIndex == 0)
                {
                    historyIndex = -1;
                    expression = "0";
                    UpdateDisplay();
                }
                return;
            }

            if (key == "AC")
            {
                expression = "0";
                newNumber = true;
                UpdateDisplay();
                return;
            }

            if (key == "DEL")
            {
                if (expression.Length > 1)
                    expression = expression.Substring(0, expression.Length - 1);
                else
                    expression = "0";
                UpdateDisplay();
                return;
            }

            if (key == "Ans")
            {
                if (expression == "0")
                {
                    expression = lastAnswer;
                }
                else
                {
                    expression += lastAnswer;
                }
                newNumber = true;  // Bấm số tiếp theo sẽ thay thế, không thêm vào
                UpdateDisplay();
                return;
            }

            if (key == "=")
            {
                try
                {
                    string originalExpression = expression;  // Lưu công thức gốc trước khi tính
                    double result = EvaluateExpression(expression);
                    lastAnswer = result.ToString(CultureInfo.InvariantCulture);
                    expression = lastAnswer;
                    
                    // Save formula to history (not the result)
                    formulaHistory.Add(originalExpression);
                    historyIndex = -1;
                    
                    newNumber = true;
                    UpdateDisplay();
                }
                catch
                {
                    expression = "Lỗi";
                    UpdateDisplay();
                }
                return;
            }

            // Handle operators: +, -, *, /
            if (key == "+" || key == "-" || key == "*" || key == "/")
            {
                // Thêm operator vào biểu thức
                if (expression != "0" && expression != "Lỗi")
                {
                    expression += key;
                }
                newNumber = true;
                UpdateDisplay();
                return;
            }

            // Handle percentage
            if (key == "%")
            {
                try
                {
                    double val = EvaluateExpression(expression);
                    val = val / 100;
                    expression = val.ToString(CultureInfo.InvariantCulture);
                    UpdateDisplay();
                }
                catch { }
                return;
            }

            // Handle power
            if (key == "^")
            {
                expression += "^";
                newNumber = true;
                UpdateDisplay();
                return;
            }

            // Handle parentheses
            if (key == "(" || key == ")")
            {
                expression += key;
                newNumber = true;
                UpdateDisplay();
                return;
            }

            // Handle decimal point
            if (key == ".")
            {
                if (newNumber)
                {
                    expression = "0.";
                    newNumber = false;
                }
                else if (!expression.EndsWith(".") && !expression.Contains("."))
                {
                    expression += ".";
                }
                UpdateDisplay();
                return;
            }

            // Handle digit input
            if (char.IsDigit(key[0]))
            {
                // Nếu expression là "0" hoặc "Lỗi", thay thế; nếu không thì thêm vào
                if (expression == "0" || expression == "Lỗi")
                {
                    expression = key;
                }
                else
                {
                    expression += key;
                }
                newNumber = false;
                UpdateDisplay();
                return;
            }
        }

        private void UpdateDisplay()
        {
            // Điều chỉnh font size dựa trên độ dài chữ
            float fontSize = 36;
            if (expression.Length > 18)
                fontSize = 18;
            else if (expression.Length > 15)
                fontSize = 24;
            else if (expression.Length > 12)
                fontSize = 28;
            else if (expression.Length > 10)
                fontSize = 32;
            
            displayLabel.Font = new Font("Segoe UI", fontSize, FontStyle.Bold);
            displayLabel.Text = expression;
        }

        // Evaluate expression with parentheses and operators
        private double EvaluateExpression(string expr)
        {
            try
            {
                // Remove spaces
                expr = expr.Replace(" ", "");
                
                // Replace operators
                expr = expr.Replace("÷", "/").Replace("×", "*").Replace("−", "-");
                
                // If contains parentheses, evaluate them first
                while (expr.Contains("("))
                {
                    int lastOpen = expr.LastIndexOf("(");
                    int firstClose = expr.IndexOf(")", lastOpen);
                    if (firstClose == -1) throw new Exception("Mismatched parentheses");
                    
                    string innerExpr = expr.Substring(lastOpen + 1, firstClose - lastOpen - 1);
                    double innerResult = SimpleEvaluate(innerExpr);
                    expr = expr.Substring(0, lastOpen) + innerResult.ToString(CultureInfo.InvariantCulture) + expr.Substring(firstClose + 1);
                }
                
                return SimpleEvaluate(expr);
            }
            catch
            {
                throw new Exception("Invalid expression");
            }
        }

        // Simple evaluator for +, -, *, /, ^
        private double SimpleEvaluate(string expr)
        {
            // Handle ^ first (highest precedence)
            while (expr.Contains("^"))
            {
                int idx = expr.IndexOf("^");
                int leftStart = idx - 1;
                while (leftStart > 0 && (char.IsDigit(expr[leftStart - 1]) || expr[leftStart - 1] == '.'))
                    leftStart--;
                int rightEnd = idx + 1;
                while (rightEnd < expr.Length - 1 && (char.IsDigit(expr[rightEnd + 1]) || expr[rightEnd + 1] == '.'))
                    rightEnd++;
                
                string leftStr = expr.Substring(leftStart, idx - leftStart);
                string rightStr = expr.Substring(idx + 1, rightEnd - idx);
                
                double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                double result = Math.Pow(leftVal, rightVal);
                
                expr = expr.Substring(0, leftStart) + result.ToString(CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
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
                
                // Find numbers
                int leftStart = idx - 1;
                while (leftStart > 0 && (char.IsDigit(expr[leftStart - 1]) || expr[leftStart - 1] == '.'))
                    leftStart--;
                int rightEnd = idx + 1;
                while (rightEnd < expr.Length - 1 && (char.IsDigit(expr[rightEnd + 1]) || expr[rightEnd + 1] == '.'))
                    rightEnd++;
                
                string leftStr = expr.Substring(leftStart, idx - leftStart);
                string rightStr = expr.Substring(idx + 1, rightEnd - idx);
                
                double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                double result = op == '*' ? leftVal * rightVal : leftVal / rightVal;
                
                expr = expr.Substring(0, leftStart) + result.ToString(CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
            }
            
            // Handle + and -
            // Skip first char if it's minus (negative number)
            int startIdx = 0;
            if (expr.Length > 0 && expr[0] == '-')
                startIdx = 1;
            
            for (int i = startIdx; i < expr.Length; i++)
            {
                if (expr[i] == '+' || expr[i] == '-')
                {
                    int leftStart = 0;
                    int rightEnd = expr.Length - 1;
                    
                    string leftStr = expr.Substring(leftStart, i - leftStart);
                    string rightStr = expr.Substring(i + 1);
                    
                    double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                    double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                    double result = expr[i] == '+' ? leftVal + rightVal : leftVal - rightVal;
                    
                    return result;
                }
            }
            
            return double.Parse(expr, CultureInfo.InvariantCulture);
        }

        private void CalculatorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            string key = "";
            
            if (char.IsDigit((char)e.KeyCode))
                key = ((char)e.KeyCode).ToString();
            else if (e.KeyCode == Keys.Add)
                key = "+";
            else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
                key = "-";
            else if (e.KeyCode == Keys.Multiply)
                key = "*";
            else if (e.KeyCode == Keys.Divide || e.KeyCode == Keys.OemQuestion)
                key = "/";
            else if (e.KeyCode == Keys.Decimal || e.KeyCode == Keys.OemPeriod)
                key = ".";
            else if (e.KeyCode == Keys.Return)
                key = "=";
            else if (e.KeyCode == Keys.Back)
                key = "DEL";
            else if (e.KeyCode == Keys.Delete)
                key = "AC";

            if (!string.IsNullOrEmpty(key))
            {
                // Trigger button click
                foreach (Control ctrl in ((TableLayoutPanel)this.Controls[1]).Controls)
                {
                    if (ctrl is Button btn && (btn.Tag as string) == key)
                    {
                        btn.PerformClick();
                        e.Handled = true;
                        e.SuppressKeyPress = true;
                        break;
                    }
                }
            }
        }
    }
}

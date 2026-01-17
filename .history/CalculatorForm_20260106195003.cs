using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Media;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

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
        private int cursorPos = 1;  // Vị trí con trỏ (1 = sau "0" ban đầu)
        private Timer cursorBlinkTimer;  // Timer để nhấp nháy con trỏ
        private bool cursorVisible = true;  // Trạng thái hiển thị con trỏ

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
            displayLabel.TextAlign = ContentAlignment.MiddleLeft;  // Align trái (để hiển thị cursor)
            displayLabel.AutoSize = false;  // Không tự động resize
            displayLabel.Location = new Point(10, 10);
            displayLabel.Size = new Size(380, 90);
            displayLabel.Tag = "0";  // Lưu giá trị hiện tại
            this.Controls.Add(displayLabel);
            // ===== PANEL CHO 4 NÚT MŨI TÊN (D-PAD) - CANH GIỮA =====
            var navPanel = new TableLayoutPanel();
            navPanel.ColumnCount = 3;
            navPanel.RowCount = 3;
            navPanel.Width = 120;
            navPanel.Height = 120;
            navPanel.Location = new Point(150, 110);  // Canh giữa (420-120)/2 = 150
            navPanel.BackColor = Color.FromArgb(40, 40, 40);
            navPanel.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            
            for (int c = 0; c < 3; c++)
                navPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 3f));
            for (int r = 0; r < 3; r++)
                navPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 3f));

            this.Controls.Add(navPanel);

            // ===== PANEL CHO PHÍM SỐ VÀ PHÉP TÍNH - GIẢM KÍCH THƯỚC =====
            var mainTable = new TableLayoutPanel();
            mainTable.ColumnCount = 4;
            mainTable.RowCount = 6;  // 6 hàng cho phím số (từ AC-DEL-() đến hàng Ans-^-=)
            mainTable.Width = 360;  // Giảm từ 400 xuống 360
            mainTable.Height = 390;  // Giảm từ 440 xuống 390
            mainTable.Location = new Point(30, 240);  // Canh giữa hơn
            mainTable.BackColor = Color.FromArgb(40, 40, 40);
            mainTable.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            
            for (int c = 0; c < mainTable.ColumnCount; c++)
                mainTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < mainTable.RowCount; r++)
            {
                // Row cuối cùng (=) sẽ chiếm 15%, các row khác chia đều
                if (r == mainTable.RowCount - 1)
                    mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 15f));
                else
                    mainTable.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 5f));
            }

            this.Controls.Add(mainTable);

            // Button factory with professional styling - cho navPanel
            Button AddNavBtn(string text, int col, int row, string tag = null, Color? bgColor = null)
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
                
                navPanel.Controls.Add(b, col, row);
                return b;
            }

            // Button factory for main table - cho mainTable
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
                
                mainTable.Controls.Add(b, col, row);
                if (colSpan > 1) mainTable.SetColumnSpan(b, colSpan);
                if (rowSpan > 1) mainTable.SetRowSpan(b, rowSpan);
                return b;
            }

            // Color definitions
            Color acColor = Color.FromArgb(200, 50, 50);
            Color opColor = Color.FromArgb(200, 100, 50);
            Color eqColor = Color.FromArgb(50, 150, 50);
            Color navColor = Color.FromArgb(100, 140, 200);  // Xanh dương cho nút điều hướng

            // ===== 4 NÚT MŨI TÊN =====
            // Row 0: ↑ (col 1)
            AddNavBtn("↑", 1, 0, tag: "UP", bgColor: navColor);

            // Row 1: ← | → (col 0 và 2)
            AddNavBtn("←", 0, 1, tag: "LEFT", bgColor: navColor);
            AddNavBtn("→", 2, 1, tag: "RIGHT", bgColor: navColor);

            // Row 2: ↓ (col 1)
            AddNavBtn("↓", 1, 2, tag: "DOWN", bgColor: navColor);

            // ===== PHÍM SỐ VÀ PHÉP TÍNH =====
            // Row 0: AC, DEL, (, )
            AddBtn("AC", 0, 0, tag: "AC", bgColor: acColor);
            AddBtn("DEL", 1, 0, tag: "DEL", bgColor: acColor);
            AddBtn("(", 2, 0, tag: "(");
            AddBtn(")", 3, 0, tag: ")");

            // Row 1: 7, 8, 9, /
            AddBtn("7", 0, 1);
            AddBtn("8", 1, 1);
            AddBtn("9", 2, 1);
            AddBtn("÷", 3, 1, tag: "/", bgColor: opColor);

            // Row 2: 4, 5, 6, *
            AddBtn("4", 0, 2);
            AddBtn("5", 1, 2);
            AddBtn("6", 2, 2);
            AddBtn("×", 3, 2, tag: "*", bgColor: opColor);

            // Row 3: 1, 2, 3, -
            AddBtn("1", 0, 3);
            AddBtn("2", 1, 3);
            AddBtn("3", 2, 3);
            AddBtn("−", 3, 3, tag: "-", bgColor: opColor);

            // Row 4: 0, ., +, %
            AddBtn("0", 0, 4);
            AddBtn(".", 1, 4, tag: ".");
            AddBtn("+", 2, 4, tag: "+", bgColor: opColor);
            AddBtn("%", 3, 4, tag: "%", bgColor: opColor);

            // Row 5: Ans, ^, = (dài 2 cột)
            AddBtn("Ans", 0, 5, tag: "Ans", bgColor: opColor);
            AddBtn("^", 1, 5, tag: "^", bgColor: opColor);
            AddBtn("=", 2, 5, colSpan: 2, tag: "=", bgColor: eqColor);

            try { Theme.ApplyEcommerceTheme(this); } catch { }
            
            // Đảm bảo displayLabel có màu cam sau khi theme được áp dụng
            displayLabel.ForeColor = Color.FromArgb(255, 140, 0);  // Màu cam
            displayLabel.BackColor = Color.Black;   // Nền đen

            // Khởi tạo Timer để nhấp nháy con trỏ
            cursorBlinkTimer = new Timer();
            cursorBlinkTimer.Interval = 500;  // Nhấp nháy mỗi 500ms
            cursorBlinkTimer.Tick += (s, e) =>
            {
                cursorVisible = !cursorVisible;  // Toggle trạng thái
                UpdateDisplay();  // Cập nhật hiển thị
            };
            cursorBlinkTimer.Start();
            
            // Đảm bảo form giữ focus để nhận keyboard events
            this.Focus();
            this.ActiveControl = null;  // Không để focus vào control nào cả
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
                    cursorPos = expression.Length;  // Đặt cursor ở cuối
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
                    cursorPos = expression.Length;  // Đặt cursor ở cuối
                    UpdateDisplay();
                }
                else if (historyIndex == 0)
                {
                    historyIndex = -1;
                    expression = "0";
                    cursorPos = 1;  // Reset cursor
                    UpdateDisplay();
                }
                return;
            }

            // Cursor navigation - LEFT
            if (key == "LEFT")
            {
                if (cursorPos > 0)
                    cursorPos--;
                UpdateDisplay();
                return;
            }

            // Cursor navigation - RIGHT
            if (key == "RIGHT")
            {
                if (cursorPos < expression.Length)
                    cursorPos++;
                UpdateDisplay();
                return;
            }

            if (key == "AC")
            {
                expression = "0";
                cursorPos = 1;  // Reset cursor về sau "0"
                newNumber = true;
                UpdateDisplay();
                return;
            }

            if (key == "DEL")
            {
                // Xóa ký tự trước cursor (backspace)
                if (cursorPos > 0)
                {
                    expression = expression.Substring(0, cursorPos - 1) + expression.Substring(cursorPos);
                    cursorPos--;
                    
                    // Nếu expression trở thành rỗng, set về "0" và đặt cursor ở vị trí 1 (sau "0")
                    if (expression == "")
                    {
                        expression = "0";
                        cursorPos = 1;  // Cursor ở phải, sau "0"
                    }
                }
                newNumber = false;
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
                cursorPos = expression.Length;  // Đặt cursor ở cuối phải
                newNumber = true;  // Bấm số tiếp theo sẽ thay thế, không thêm vào
                UpdateDisplay();
                return;
            }
            if (key == "=")
            {
                string originalExpression = expression;  // Lưu công thức gốc trước khi tính
                try
                {
                    double result = EvaluateExpression(expression);
                    lastAnswer = result.ToString(CultureInfo.InvariantCulture);
                    expression = lastAnswer;
                    cursorPos = expression.Length;  // Đặt cursor ở cuối
                    
                    // Save formula to history (not the result)
                    formulaHistory.Add(originalExpression);
                    historyIndex = -1;
                    
                    newNumber = true;
                    UpdateDisplay();
                }
                catch (Exception ex)
                {
                    expression = "Lỗi: " + ex.Message;
                    cursorPos = expression.Length;
                    UpdateDisplay();
                    System.Diagnostics.Debug.WriteLine("Calculator Error: " + ex.Message + "\nExpression: " + originalExpression);
                }
                return;
            }

            // Handle operators: +, -, *, /
            if (key == "+" || key == "-" || key == "*" || key == "/")
            {
                // Chèn operator tại vị trí cursor
                if (expression != "0" && !expression.Contains("Lỗi"))
                {
                    expression = expression.Substring(0, cursorPos) + key + expression.Substring(cursorPos);
                    cursorPos++;
                }
                newNumber = true;
                UpdateDisplay();
                return;
            }

            // Handle percentage - chỉ tính % cho số cuối cùng
            if (key == "%")
            {
                try
                {
                    // Tìm vị trí của operator cuối cùng
                    int lastOpPos = -1;
                    for (int i = expression.Length - 1; i >= 0; i--)
                    {
                        char c = expression[i];
                        if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')')
                        {
                            lastOpPos = i;
                            break;
                        }
                    }
                    
                    // Tách số cuối cùng
                    string lastNumber = "";
                    if (lastOpPos == -1)
                    {
                        // Không có operator, toàn bộ là một số
                        lastNumber = expression;
                    }
                    else if (lastOpPos == expression.Length - 1)
                    {
                        // Operator ở cuối, không có số sau nó
                        return;
                    }
                    else
                    {
                        // Số từ vị trí sau operator đến cuối
                        lastNumber = expression.Substring(lastOpPos + 1);
                    }
                    
                    // Tính percentage
                    double numVal = double.Parse(lastNumber, CultureInfo.InvariantCulture);
                    double percentVal = numVal / 100;
                    string percentStr = percentVal.ToString(CultureInfo.InvariantCulture);
                    
                    // Thay thế số cuối bằng kết quả percentage
                    if (lastOpPos == -1)
                    {
                        expression = percentStr;
                    }
                    else
                    {
                        expression = expression.Substring(0, lastOpPos + 1) + percentStr;
                    }
                    
                    cursorPos = expression.Length;
                    UpdateDisplay();
                }
                catch { }
                return;
            }

            // Handle power
            if (key == "^")
            {
                // Chèn ^ tại vị trí cursor
                if (expression == "0" || expression.Contains("Lỗi"))
                    expression = "^";
                else
                {
                    expression = expression.Substring(0, cursorPos) + "^" + expression.Substring(cursorPos);
                    cursorPos++;
                }
                newNumber = true;
                UpdateDisplay();
                return;
            }

            // Handle parentheses
            if (key == "(" || key == ")")
            {
                // Chèn parenthesis tại vị trí cursor
                if (expression == "0")
                {
                    expression = key;
                    cursorPos = 1;
                }
                else if (!expression.Contains("Lỗi"))
                {
                    expression = expression.Substring(0, cursorPos) + key + expression.Substring(cursorPos);
                    cursorPos++;
                }
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
                    cursorPos = 2;  // Cursor sau "0."
                    newNumber = false;
                }
                else if (!expression.EndsWith(".") && !expression.Contains("."))
                {
                    expression += ".";
                    cursorPos = expression.Length;  // Cursor cuối, sau "."
                }
                UpdateDisplay();
                return;
            }
            // Handle digit input - nhập tại vị trí cursor (chèn)
            if (char.IsDigit(key[0]))
            {
                // Nếu expression là "0" hoặc "Lỗi", thay thế; nếu không thì chèn tại cursor
                if (expression == "0" || expression.Contains("Lỗi"))
                {
                    expression = key;
                    cursorPos = 1;
                }
                else
                {
                    // Chèn số tại vị trí cursor
                    expression = expression.Substring(0, cursorPos) + key + expression.Substring(cursorPos);
                    cursorPos++;
                }
                newNumber = false;
                UpdateDisplay();
                return;
            }
        }

        private void UpdateDisplay()
        {
            // Bảo vệ cursorPos không vượt quá expression.Length
            if (cursorPos > expression.Length)
                cursorPos = expression.Length;
            if (cursorPos < 0)
                cursorPos = 0;
            
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
            
            // Tạo display text với con trỏ
            string displayText = expression.Substring(0, cursorPos) + 
                                 (cursorVisible ? "|" : " ") + 
                                 expression.Substring(cursorPos);
            
            // Nếu text quá dài (> 52 ký tự), cuộn để hiển thị phần nơi cursor ở
            int maxChars = 52;  // Số ký tự tối đa có thể hiển thị
            if (displayText.Length > maxChars)
            {
                // Tính startIndex để cursor luôn nằm trong display
                // Ưu tiên hiển thị từ cursor trở về cuối
                int displayCursorPos = cursorPos + 1;  // +1 vì có dấu nháy
                
                int startIdx = Math.Max(0, displayCursorPos - maxChars / 2);  // Đặt cursor ở giữa nếu có thể
                if (startIdx + maxChars > displayText.Length)
                    startIdx = Math.Max(0, displayText.Length - maxChars);
                
                displayText = displayText.Substring(startIdx, maxChars);
                
                // Thêm "..." ở đầu nếu không phải từ đầu
                if (startIdx > 0)
                    displayText = "..." + displayText.Substring(3);
            }
            
            displayLabel.Text = displayText;
        }
        // Evaluate expression with parentheses and operators
        private double EvaluateExpression(string expr)
        {
            try
            {
                // Remove spaces and cursor
                expr = expr.Replace(" ", "").Replace("|", "");
                
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
                    // Dùng G17 format để tránh scientific notation
                    string innerResultStr = innerResult.ToString("G17", CultureInfo.InvariantCulture);
                    expr = expr.Substring(0, lastOpen) + innerResultStr + expr.Substring(firstClose + 1);
                    // Debug: Show expression after parentheses removal
                    System.Diagnostics.Debug.WriteLine($"After removing parentheses: {expr}");
                }
                
                return SimpleEvaluate(expr);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Expression evaluation error: {ex.Message}");
                throw new Exception("Invalid expression: " + ex.Message);
            }
        }

        // Simple evaluator for +, -, *, /, ^
        private double SimpleEvaluate(string expr)
        {
            expr = expr.Trim();
            if (string.IsNullOrEmpty(expr))
                throw new Exception("Empty expression");
            
            // Handle ^ first (highest precedence)
            while (expr.Contains("^"))
            {
                int idx = expr.IndexOf("^");
                
                // Find left number - go back from idx-1
                int leftEnd = idx - 1;
                while (leftEnd > 0 && (char.IsDigit(expr[leftEnd - 1]) || expr[leftEnd - 1] == '.'))
                    leftEnd--;
                
                // Find right number - go forward from idx+1
                int rightEnd = idx + 1;
                while (rightEnd < expr.Length && (char.IsDigit(expr[rightEnd]) || expr[rightEnd] == '.'))
                    rightEnd++;
                rightEnd--;  // Move back to last digit/decimal point
                
                string leftStr = expr.Substring(leftEnd, idx - leftEnd);
                string rightStr = expr.Substring(idx + 1, rightEnd - idx);
                
                double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                double result = Math.Pow(leftVal, rightVal);
                
                expr = expr.Substring(0, leftEnd) + result.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
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
                
                // Find left number
                int leftEnd = idx - 1;
                while (leftEnd > 0 && (char.IsDigit(expr[leftEnd - 1]) || expr[leftEnd - 1] == '.'))
                    leftEnd--;
                
                // Find right number
                int rightEnd = idx + 1;
                while (rightEnd < expr.Length && (char.IsDigit(expr[rightEnd]) || expr[rightEnd] == '.'))
                    rightEnd++;
                rightEnd--;  // Move back to last digit/decimal point
                
                string leftStr = expr.Substring(leftEnd, idx - leftEnd);
                string rightStr = expr.Substring(idx + 1, rightEnd - idx);
                
                double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                double result = op == '*' ? leftVal * rightVal : leftVal / rightVal;
                
                expr = expr.Substring(0, leftEnd) + result.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
            }
            
            // Handle + and - (left to right)
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
                
                // Find right number - only take the next number, not everything after operator
                int rightEnd = opIdx + 1;
                while (rightEnd < expr.Length && (char.IsDigit(expr[rightEnd]) || expr[rightEnd] == '.'))
                    rightEnd++;
                rightEnd--;  // Move back to last digit
                
                string rightStr = expr.Substring(opIdx + 1, rightEnd - opIdx);
                
                double leftVal = double.Parse(leftStr, CultureInfo.InvariantCulture);
                double rightVal = double.Parse(rightStr, CultureInfo.InvariantCulture);
                double result = opChar == '+' ? leftVal + rightVal : leftVal - rightVal;
                
                expr = result.ToString("G17", CultureInfo.InvariantCulture) + expr.Substring(rightEnd + 1);
                startIdx = 0;
            }
            
            return double.Parse(expr, CultureInfo.InvariantCulture);
        }

        private void CalculatorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            string key = "";
            
            // Số (0-9) từ hàng ngang hoặc numpad
            if (char.IsDigit((char)e.KeyCode))
            {
                // Số từ hàng ngang
                if (e.Shift)
                {
                    if (e.KeyCode == Keys.D9)
                        key = "(";
                    else if (e.KeyCode == Keys.D0)
                        key = ")";
                    else if (e.KeyCode == Keys.D6)
                        key = "^";
                    else if (e.KeyCode == Keys.D5)
                        key = "%";
                    else
                        key = ((char)e.KeyCode).ToString();
                }
                else
                    key = ((char)e.KeyCode).ToString();
            }
            // Số từ numpad
            else if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9)
            {
                key = ((int)(e.KeyCode - Keys.NumPad0)).ToString();
            }
            // Phép toán
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
            // Backspace = xóa
            else if (e.KeyCode == Keys.Back)
            {
                e.SuppressKeyPress = true;
                key = "DEL";
            }
            // Delete = xóa hết
            else if (e.KeyCode == Keys.Delete)
            {
                e.SuppressKeyPress = true;
                key = "AC";
            }
            // Escape = xóa hết
            else if (e.KeyCode == Keys.Escape)
            {
                e.SuppressKeyPress = true;
                key = "AC";
            }
            // Suppress Enter key completely
            else if (e.KeyCode == Keys.Return)
            {
                e.SuppressKeyPress = true;
                e.Handled = true;
                return;
            }

            if (!string.IsNullOrEmpty(key))
            {
                ClickButtonByTag(key);
                if (e.Handled == false)
                {
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            }
        }

        private void ClickButtonByTag(string tag)
        {
            foreach (Control ctrl in this.Controls)
            {
                if (ctrl is TableLayoutPanel panel)
                {
                    foreach (Control panelCtrl in panel.Controls)
                    {
                        if (panelCtrl is Button btn && (btn.Tag as string) == tag)
                        {
                            btn.PerformClick();
                            return;
                        }
                    }
                }
            }
        }

        private void MoveCursorLeft()
        {
            if (cursorPos > 1)
            {
                cursorPos--;
                UpdateDisplay();
            }
        }

        private void MoveCursorRight()
        {
            if (cursorPos < (expression?.Length ?? 0) + 1)
            {
                cursorPos++;
                UpdateDisplay();
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Make Enter key work like "=" button
            if (keyData == Keys.Return)
            {
                ClickButtonByTag("=");
                return true;  // Indicate we handled it, suppress default behavior
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

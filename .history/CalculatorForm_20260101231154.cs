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
        private string currentInput = "0";
        private string lastAnswer = "0";
        private string lastOperator = "";
        private double lastValue = 0;
        private bool newNumber = true;

        public CalculatorForm()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            this.Text = "Máy Tính";
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.Width = 350;
            this.Height = 500;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(50, 50, 50);
            this.KeyPreview = true;
            this.KeyDown += CalculatorForm_KeyDown;

            // Display screen
            displayLabel = new Label();
            displayLabel.Text = "0";
            displayLabel.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            displayLabel.ForeColor = Color.White;
            displayLabel.BackColor = Color.FromArgb(30, 30, 30);
            displayLabel.BorderStyle = BorderStyle.FixedSingle;
            displayLabel.TextAlign = ContentAlignment.MiddleRight;
            displayLabel.Location = new Point(10, 10);
            displayLabel.Size = new Size(320, 80);
            this.Controls.Add(displayLabel);

            // Button layout: 4 columns x 5 rows
            var table = new TableLayoutPanel();
            table.ColumnCount = 4;
            table.RowCount = 5;
            table.Width = 330;
            table.Height = 390;
            table.Location = new Point(10, 100);
            table.BackColor = Color.FromArgb(50, 50, 50);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            
            for (int c = 0; c < table.ColumnCount; c++)
                table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
            for (int r = 0; r < table.RowCount; r++)
                table.RowStyles.Add(new RowStyle(SizeType.Percent, 20f));

            this.Controls.Add(table);

            // Button factory with professional styling
            Button AddBtn(string text, int col, int row, int colSpan = 1, int rowSpan = 1, string tag = null, Color? bgColor = null)
            {
                var b = new Button();
                b.Dock = DockStyle.Fill;
                b.Text = text;
                b.Tag = tag ?? text;
                b.Font = new Font("Segoe UI", 14, FontStyle.Bold);
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

            // Row 0: AC, DEL, %, /
            AddBtn("AC", 0, 0, tag: "AC", bgColor: acColor);
            AddBtn("DEL", 1, 0, tag: "DEL", bgColor: acColor);
            AddBtn("%", 2, 0, tag: "%", bgColor: opColor);
            AddBtn("÷", 3, 0, tag: "/", bgColor: opColor);

            // Row 1: 7, 8, 9, ×
            AddBtn("7", 0, 1);
            AddBtn("8", 1, 1);
            AddBtn("9", 2, 1);
            AddBtn("×", 3, 1, tag: "*", bgColor: opColor);

            // Row 2: 4, 5, 6, −
            AddBtn("4", 0, 2);
            AddBtn("5", 1, 2);
            AddBtn("6", 2, 2);
            AddBtn("−", 3, 2, tag: "-", bgColor: opColor);

            // Row 3: 1, 2, 3, +
            AddBtn("1", 0, 3);
            AddBtn("2", 1, 3);
            AddBtn("3", 2, 3);
            AddBtn("+", 3, 3, tag: "+", bgColor: opColor);

            // Row 4: 0 (spans 2 cols), ., =
            AddBtn("0", 0, 4, colSpan: 2);
            AddBtn(".", 2, 4, tag: ".");
            AddBtn("=", 3, 4, tag: "=", bgColor: eqColor);

            try { Theme.ApplyEcommerceTheme(this); } catch { }
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

            if (key == "AC")
            {
                currentInput = "0";
                lastValue = 0;
                lastOperator = "";
                newNumber = true;
                UpdateDisplay();
                return;
            }

            if (key == "DEL")
            {
                if (currentInput.Length > 1)
                    currentInput = currentInput.Substring(0, currentInput.Length - 1);
                else
                    currentInput = "0";
                UpdateDisplay();
                return;
            }

            if (key == "=")
            {
                Calculate();
                newNumber = true;
                lastOperator = "";
                return;
            }

            // Handle operators: +, -, *, /
            if (key == "+" || key == "-" || key == "*" || key == "/")
            {
                if (!string.IsNullOrEmpty(lastOperator) && !newNumber)
                {
                    Calculate();
                }
                lastValue = double.Parse(currentInput, CultureInfo.InvariantCulture);
                lastOperator = key;
                newNumber = true;
                return;
            }

            // Handle percentage
            if (key == "%")
            {
                double val = double.Parse(currentInput, CultureInfo.InvariantCulture);
                val = val / 100;
                currentInput = val.ToString(CultureInfo.InvariantCulture);
                UpdateDisplay();
                return;
            }

            // Handle decimal point
            if (key == ".")
            {
                if (!currentInput.Contains("."))
                {
                    currentInput += ".";
                    newNumber = false;
                    UpdateDisplay();
                }
                return;
            }

            // Handle digit input
            if (char.IsDigit(key[0]))
            {
                if (newNumber)
                {
                    currentInput = key;
                    newNumber = false;
                }
                else
                {
                    currentInput += key;
                }
                UpdateDisplay();
                return;
            }
        }

        private void Calculate()
        {
            if (string.IsNullOrEmpty(lastOperator)) return;

            try
            {
                double current = double.Parse(currentInput, CultureInfo.InvariantCulture);
                double result = 0;

                switch (lastOperator)
                {
                    case "+": result = lastValue + current; break;
                    case "-": result = lastValue - current; break;
                    case "*": result = lastValue * current; break;
                    case "/": 
                        if (current == 0)
                        {
                            currentInput = "Lỗi";
                            UpdateDisplay();
                            return;
                        }
                        result = lastValue / current; 
                        break;
                }

                currentInput = result.ToString(CultureInfo.InvariantCulture);
                lastAnswer = currentInput;
                UpdateDisplay();
            }
            catch
            {
                currentInput = "Lỗi";
                UpdateDisplay();
            }
        }

        private void UpdateDisplay()
        {
            if (currentInput.Length > 15)
                displayLabel.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            else
                displayLabel.Font = new Font("Segoe UI", 32, FontStyle.Bold);
            
            displayLabel.Text = currentInput;
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
            caretIndicator.Height = GetLineHeight();
            caretIndicator.BackColor = Color.Orange;
            caretIndicator.Visible = true;
            this.Controls.Add(caretIndicator);
            caretIndicator.BringToFront();

            caretTimer = new Timer();
            caretTimer.Interval = 500;
            caretTimer.Tick += (s, ev) => 
            {
                // toggle visibility for blinking
                caretVisibleState = !caretVisibleState;
                caretIndicator.Visible = caretVisibleState;
                try { caretIndicator.BringToFront(); } catch { }
            };
            caretTimer.Start();
            // update caret position when user clicks or form gains focus; hide native caret
            screen.MouseUp += (s, ev) => { SetCaretFromMouseXY(ev.X, ev.Y); UpdateCaretIndicatorPosition(); this.Focus(); };
            screen.GotFocus += (s, ev) => { UpdateCaretIndicatorPosition(); };
            this.GotFocus += (s, ev) => { UpdateCaretIndicatorPosition(); };

            var table = new TableLayoutPanel();
            table.ColumnCount = 4;
            table.RowCount = 6;
            table.Width = this.ClientSize.Width - 24;
            table.Height = this.ClientSize.Height - screen.Bottom - 24;
            table.Location = new Point(12, screen.Bottom + 8);
            table.CellBorderStyle = TableLayoutPanelCellBorderStyle.Single;
            for (int c = 0; c < table.ColumnCount; c++) table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / table.ColumnCount));
            for (int r = 0; r < table.RowCount; r++) table.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / table.RowCount));

            this.Controls.Add(table);

            // Button factory
                Button AddBtn(string text, int col, int row, int colSpan = 1, int rowSpan = 1, string tag = null)
                {
                    var b = new Button();
                    b.Dock = DockStyle.Fill;
                    b.Text = text;
                    b.Tag = tag ?? text;
                    b.Font = new Font("Segoe UI", 12, FontStyle.Bold);
                    b.BackColor = Color.White;
                    b.Click += CalcButton_Click;
                    // Ensure arrow keys are treated as input keys when a button has focus
                    b.PreviewKeyDown += (ss, ee) => {
                        if (ee.KeyCode == Keys.Left || ee.KeyCode == Keys.Right)
                        {
                            ee.IsInputKey = true;
                        }
                    };
                    // Auto-size font on resize
                    b.Resize += (ss, ee) =>
                    {
                        try
                        {
                            float fs = 12f;
                            var padding = 8;
                            while (fs > 6f)
                            {
                                using (var f = new Font(b.Font.FontFamily, fs, b.Font.Style))
                                {
                                    var size = TextRenderer.MeasureText(b.Text, f);
                                    if (size.Width + padding <= b.ClientSize.Width && size.Height + padding <= b.ClientSize.Height) { b.Font = new Font(b.Font.FontFamily, fs, b.Font.Style); break; }
                                }
                                fs -= 0.5f;
                            }
                        }
                        catch { }
                    };
                    table.Controls.Add(b, col, row);
                    if (colSpan > 1) table.SetColumnSpan(b, colSpan);
                    if (rowSpan > 1) table.SetRowSpan(b, rowSpan);
                    return b;
                }

            // Compact layout: rows of digits and required operators only
            // Row0: ( ) DEL AC
            AddBtn("(", 0, 0);
            AddBtn(")", 1, 0);
            AddBtn("DEL", 2, 0, tag: "DEL");
            AddBtn("AC", 3, 0, tag: "AC");

            // Row1: 7 8 9 /
            AddBtn("7", 0, 1);
            AddBtn("8", 1, 1);
            AddBtn("9", 2, 1);
            AddBtn("/", 3, 1, tag: "/");

            // Row2: 4 5 6 *
            AddBtn("4", 0, 2);
            AddBtn("5", 1, 2);
            AddBtn("6", 2, 2);
            AddBtn("*", 3, 2, tag: "*");

            // Row3: 1 2 3 -
            AddBtn("1", 0, 3);
            AddBtn("2", 1, 3);
            AddBtn("3", 2, 3);
            AddBtn("-", 3, 3, tag: "-");

            // Row4: 0 . % +
            AddBtn("0", 0, 4);
            AddBtn(".", 1, 4, tag: ".");
            AddBtn("%", 2, 4, tag: "%");
            AddBtn("+", 3, 4, tag: "+");

            // Row5: Ans ^ = (Ans spans col 0, ^ in col1, = spans col 2-3)
            var ansBtn = AddBtn("Ans", 0, 5, colSpan: 1, tag: "Ans");
            var powBtn = AddBtn("^", 1, 5, tag: "^");
            var eqBtn = AddBtn("=", 2, 5, colSpan: 2, tag: "=");

            try { Theme.ApplyEcommerceTheme(this); } catch { }
            // Theme may recolor panels; ensure caret stays orange and on top
            try { caretIndicator.BackColor = Color.Orange; caretIndicator.BringToFront(); caretIndicator.Visible = true; } catch { }
        }

        private void CalcButton_Click(object sender, EventArgs e)
        {
            if (!(sender is Button b)) return;
            string key = (b.Tag as string) ?? b.Text;

            if (key == "DEL")
            {
                // Delete character to the left of the caret (like a backspace)
                int pos = Math.Max(0, caretIndex);
                if (!string.IsNullOrEmpty(expressionBacking) && expressionBacking.Length > 0 && caretIndex > 0)
                {
                    string candidate = expressionBacking.Remove(caretIndex - 1, 1);
                    if (string.IsNullOrEmpty(candidate)) candidate = "0";
                    if (TrySetExpressionWithLimit(candidate))
                    {
                        caretIndex = Math.Max(0, caretIndex - 1);
                        if (expressionBacking == "0") caretIndex = 1; // place caret after the single 0
                        UpdateVisibleText();
                        ResetCaretBlink();
                        UpdateCaretIndicatorPosition();
                    }
                }
                return;
            }
            if (key == "AC")
            {
                    expressionBacking = "0";
                    UpdateScreenToEnd();
                return;
            }
            if (key == "Ans")
            {
                int pos = Math.Max(0, caretIndex);
                if (expressionBacking == "0")
                {
                    if (!TrySetExpressionWithLimit(lastAnswer)) return;
                    pos = expressionBacking.Length;
                }
                else
                {
                    string candidate = expressionBacking.Insert(Math.Min(pos, expressionBacking.Length), lastAnswer);
                    if (!TrySetExpressionWithLimit(candidate)) return;
                    pos += lastAnswer.Length;
                }
                UpdateVisibleText();
                caretIndex = pos;
                ResetCaretBlink();
                UpdateCaretIndicatorPosition();
                return;
            }
            if (key == "=")
            {
                EvaluateAndShow();
                return;
            }

            if (key == "%")
            {
                // find current number around caretIndex and divide by 100
                int i = caretIndex - 1;
                while (i >= 0 && (char.IsDigit(expressionBacking[i]) || expressionBacking[i] == '.')) i--;
                int numStart = i + 1;
                int len = caretIndex - numStart;
                if (len <= 0) return;
                string numStr = expressionBacking.Substring(numStart, len);
                    if (double.TryParse(numStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var num))
                    {
                        double newVal = num / 100.0;
                        string newStr = newVal.ToString(CultureInfo.InvariantCulture);
                        string candidate = expressionBacking.Remove(numStart, len).Insert(numStart, newStr);
                        if (TrySetExpressionWithLimit(candidate))
                        {
                            caretIndex = numStart + newStr.Length;
                            UpdateVisibleText();
                            ResetCaretBlink();
                            UpdateCaretIndicatorPosition();
                        }
                    }
                return;
            }

            // Handle some special tags
            if (key == "FRAC") { /* removed */ }
            if (key == "pi") key = Math.PI.ToString(CultureInfo.InvariantCulture);
            if (key == "e") key = "*10^"; // user pressed x10^x -> insert *10^
            if (key == "^2")
            {
                string candidate = expressionBacking.Insert(Math.Min(caretIndex, expressionBacking.Length), "^2");
                if (TrySetExpressionWithLimit(candidate))
                {
                    caretIndex += 2;
                    UpdateVisibleText();
                    ResetCaretBlink();
                    UpdateCaretIndicatorPosition();
                }
                return;
            }
            if (key == "inv")
            {
                string candidate = expressionBacking.Insert(Math.Min(caretIndex, expressionBacking.Length), "^-1");
                if (TrySetExpressionWithLimit(candidate))
                {
                    caretIndex += 3;
                    UpdateVisibleText();
                    ResetCaretBlink();
                    UpdateCaretIndicatorPosition();
                }
                return;
            }

            // Append textual content to the screen (simple behavior)
            if (key == "Ans") key = lastAnswer;

            // Insert at caret position rather than always appending
            if (key == "Ans") key = lastAnswer;

            // Use caret position for insertion so user can click to edit anywhere
            int insertPos = Math.Max(0, caretIndex);
            if (insertPos > expressionBacking.Length) insertPos = expressionBacking.Length;
            if (key == ".")
            {
                // If caret is at 0 and expression is "0", make "0."
                if (expressionBacking == "0")
                {
                    if (TrySetExpressionWithLimit("0.")) insertPos = expressionBacking.Length;
                }
                else
                {
                    // find start of current number (search left from insertPos)
                    int i = insertPos - 1;
                    while (i >= 0 && (char.IsDigit(expressionBacking[i]) || expressionBacking[i] == '.')) i--;
                    int numStart = i + 1;
                    string currentNumber = expressionBacking.Substring(numStart, insertPos - numStart);
                    if (!currentNumber.Contains("."))
                    {
                            string candidate = expressionBacking.Insert(insertPos, ".");
                            if (TrySetExpressionWithLimit(candidate)) insertPos += 1;
                    }
                }
            }
            else
            {
                // For normal keys, insert or replace initial zero
                bool isSingleDigit = key.Length == 1 && char.IsDigit(key[0]);
                if (expressionBacking == "0")
                {
                    string candidate;
                    if (isSingleDigit) candidate = key;
                    else if (key == ".") candidate = "0.";
                    else candidate = "0" + key;
                    if (TrySetExpressionWithLimit(candidate)) insertPos = expressionBacking.Length;
                }
                else
                {
                    string candidate = expressionBacking.Insert(insertPos, key);
                    if (TrySetExpressionWithLimit(candidate)) insertPos += key.Length;
                }
            }
            UpdateVisibleText();
            caretIndex = insertPos;
            ResetCaretBlink();
            UpdateCaretIndicatorPosition();
        }

        private void EvaluateAndShow()
        {
            try
            {
                string expr = expressionBacking;
                expr = expr.Replace("×", "*").Replace("÷", "/");
                expr = expr.Replace("Ans", lastAnswer);
                var val = EvaluateExpression(expr);
                var resultText = val.ToString(CultureInfo.InvariantCulture);
                lastAnswer = resultText;
                // update backing expression and place caret at end so user can continue
                expressionBacking = resultText;
                UpdateScreenToEnd();
                caretIndex = expressionBacking.Length;
                UpdateCaretIndicatorPosition();
                ResetCaretBlink();
            }
            catch
            {
                // show error briefly in backing
                expressionBacking = "ERR";
                UpdateScreenToEnd();
            }
        }
        private void UpdateScreenToEnd()
        {
            if (screen == null) return;
            caretIndex = expressionBacking?.Length ?? 0;
            UpdateVisibleText();
            try { UpdateCaretIndicatorPosition(); } catch { }
        }

        private void UpdateCaretIndicatorPosition()
        {
            if (screen == null || caretIndicator == null) return;
            int caret = Math.Max(0, caretIndex);
            using (Graphics g = screen.CreateGraphics())
            {
                var lines = (screen.Tag as List<string>) ?? new List<string> { "" };
                int lineHeight = GetLineHeight(); // tighter spacing

                // determine which line and x offset the caret is on
                int remaining = Math.Max(0, caret - visibleStartIndex);
                int caretLine = 0;
                int caretInLine = 0;
                for (int li = 0; li < lines.Count; li++)
                {
                    if (remaining <= lines[li].Length) { caretLine = li; caretInLine = remaining; break; }
                    remaining -= lines[li].Length;
                }

                string lineBefore = lines.Count > caretLine ? lines[caretLine].Substring(0, Math.Min(caretInLine, lines[caretLine].Length)) : "";
                var sizeBefore = g.MeasureString(lineBefore, screen.Font);
                int xOffset = (int)sizeBefore.Width;

                // compute startX for this particular line to achieve right-aligned look
                var lineWidth = (int)g.MeasureString(lines[caretLine], screen.Font).Width;
                int controlInnerWidth = screen.ClientSize.Width - 12;
                int startX = screen.Left + 6 + Math.Max(0, controlInnerWidth - lineWidth);

                caretIndicator.Height = screen.Font.Height + 2;
                // align caret vertically to baseline of the line: use screen.Top + padding + visibleTopOffset + (lineIndex * lineHeight)
                caretIndicator.Top = screen.Top + screen.Padding.Top + visibleTopOffset + caretLine * lineHeight;
                caretIndicator.Left = startX + xOffset;
            }
            try { caretIndicator.BringToFront(); } catch { }
        }

        private void UpdateVisibleText()
        {
            if (screen == null) return;
            // Show a rightmost window of the expression so new input scrolls left
            string s = expressionBacking ?? "";
            if (s.Length == 0) { screen.Tag = new List<string> { "" }; visibleStartIndex = 0; screen.Invalidate(); return; }
            using (Graphics g = screen.CreateGraphics())
            {
                int controlInnerWidth = screen.ClientSize.Width - screen.Padding.Left - screen.Padding.Right - 6; // inner padding
                int maxLines = 4;

                // First compute how many trailing characters can be shown in maxLines
                int probePos = s.Length;
                int shownChars = 0;
                for (int line = 0; line < maxLines && probePos > 0; line++)
                {
                    int take = 0;
                    for (int len = 1; len <= probePos; len++)
                    {
                        string sub = s.Substring(probePos - len, len);
                        if (g.MeasureString(sub, screen.Font).Width <= controlInnerWidth) take = len;
                        else break;
                    }
                    if (take == 0) take = 1;
                    shownChars += take;
                    probePos -= take;
                }

                int start = Math.Max(0, s.Length - shownChars);

                // Now build lines top-down starting at 'start'
                var lines = new List<string>();
                int pos = start;
                for (int line = 0; line < maxLines && pos < s.Length; line++)
                {
                    int take = 0;
                    int remaining = s.Length - pos;
                    for (int len = 1; len <= remaining; len++)
                    {
                        string sub = s.Substring(pos, len);
                        if (g.MeasureString(sub, screen.Font).Width <= controlInnerWidth) take = len;
                        else break;
                    }
                    if (take == 0) take = 1;
                    lines.Add(s.Substring(pos, Math.Min(take, s.Length - pos)));
                    pos += take;
                }

                if (lines.Count == 0) lines.Add("");

                visibleStartIndex = start;
                // Center the block of lines vertically inside the control
                int lineHeight = screen.Font.Height + 2;
                int contentHeight = lines.Count * lineHeight;
                int innerHeight = screen.ClientSize.Height - screen.Padding.Top - screen.Padding.Bottom;
                visibleTopOffset = Math.Max(0, (innerHeight - contentHeight) / 2);

                // Build text as usual; the Label control will still draw from top, so we keep Text
                screen.Tag = lines;
                screen.Invalidate();
            }
        }

        // Returns true if `s` can be wrapped into `maxLines` or fewer lines
        // using the same wrapping logic as UpdateVisibleText.
        private bool FitsInMaxLines(string s, int maxLines)
        {
            if (screen == null) return false;
            if (s == null) s = "";
            using (Graphics g = screen.CreateGraphics())
            {
                int controlInnerWidth = screen.ClientSize.Width - screen.Padding.Left - screen.Padding.Right - 6; // inner padding

                int pos = 0;
                int linesCount = 0;
                while (pos < s.Length && linesCount < maxLines)
                {
                    int remaining = s.Length - pos;
                    int take = 0;
                    for (int len = 1; len <= remaining; len++)
                    {
                        string sub = s.Substring(pos, len);
                        if (g.MeasureString(sub, screen.Font).Width <= controlInnerWidth) take = len;
                        else break;
                    }
                    if (take == 0) take = 1;
                    pos += take;
                    linesCount++;
                }
                return pos >= s.Length && linesCount <= maxLines;
            }
        }

        // Try to set expressionBacking to newExpr only if it fits in `maxLines`.
        // Plays a beep and returns false when it doesn't fit.
        private bool TrySetExpressionWithLimit(string newExpr, int maxLines = 4)
        {
            if (FitsInMaxLines(newExpr, maxLines))
            {
                expressionBacking = newExpr;
                return true;
            }
            // Beep suppressed by user request: previously SystemSounds.Beep.Play();
            return false;
        }

        private void CalculatorForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
            {
                // Move caret one character at a time and perform minimal scrolling so caret stays visible
                if (e.KeyCode == Keys.Left)
                {
                    caretIndex = Math.Max(0, caretIndex - 1);
                }
                else // Right
                {
                    caretIndex = Math.Min(expressionBacking.Length, caretIndex + 1);
                }

                caretNavScrolling = true;
                UpdateVisibleText();
                caretNavScrolling = false;

                ResetCaretBlink();
                UpdateCaretIndicatorPosition();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void SetCaretFromMouseXY(int mouseX, int mouseY)
        {
            // Map mouse X/Y into caret index across multiple lines
            using (Graphics g = screen.CreateGraphics())
            {
                int controlInnerWidth = screen.ClientSize.Width - 12;
                var lines = (screen.Tag as List<string>) ?? new List<string> { "" };
                int lineHeight = GetLineHeight();
                int relY = mouseY - screen.Padding.Top - visibleTopOffset;
                int line = Math.Max(0, Math.Min(lines.Count - 1, relY / lineHeight));
                // compute X start for right-aligned line
                string textLine = lines[line];
                int textWidth = (int)g.MeasureString(textLine, screen.Font).Width;
                int startX = 6 + Math.Max(0, controlInnerWidth - textWidth);
                int relX = mouseX - startX;
                if (relX <= 0) { caretIndex = visibleStartIndex; return; }
                int best = 0;
                for (int i = 1; i <= textLine.Length; i++)
                {
                    var w = (int)g.MeasureString(textLine.Substring(0, i), screen.Font).Width;
                    if (w >= relX) { best = i; break; }
                }
                // Convert line-local index to global caretIndex
                int globalIndex = visibleStartIndex;
                for (int li = 0; li < line; li++) globalIndex += lines[li].Length;
                globalIndex += best;
                caretIndex = Math.Max(0, Math.Min(expressionBacking.Length, globalIndex));
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            try { caretTimer?.Stop(); caretTimer?.Dispose(); } catch { }
            base.OnFormClosed(e);
        }

        private void ResetCaretBlink()
        {
            try
            {
                caretVisibleState = true; 
                caretIndicator.Visible = true; 
                
                if (caretTimer != null)
                {
                    caretTimer.Stop();
                    caretTimer.Start();
                }
            }
            catch { }
        }

        // Shunting-yard + RPN evaluator with functions
        private double EvaluateExpression(string input)
        {
            var tokens = Tokenize(input);
            var rpn = ToRPN(tokens);
            double res = EvalRPN(rpn);
            return res;
        }

        private enum TokenType { Number, Operator, Function, LeftParen, RightParen, Comma }
        private class Token { public string Text; public TokenType Type; public int Precedence; public bool RightAssociative; }

        private List<Token> Tokenize(string s)
        {
            var list = new List<Token>();
            string pattern = @"(\d+\.\d+|\d+|pi|PI|Ans|[A-Za-z_]+|\^|\+|\-|\*|/|\(|\)|,|!)";
            foreach (Match m in Regex.Matches(s, pattern))
            {
                string t = m.Value;
                if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out _)) list.Add(new Token { Text = t, Type = TokenType.Number });
                else if (t == "pi" || t == "PI") list.Add(new Token { Text = Math.PI.ToString(CultureInfo.InvariantCulture), Type = TokenType.Number });
                else if (t == ",") list.Add(new Token { Text = t, Type = TokenType.Comma });
                else if (t == "(") list.Add(new Token { Text = t, Type = TokenType.LeftParen });
                else if (t == ")") list.Add(new Token { Text = t, Type = TokenType.RightParen });
                else if (t == "+" || t == "-" || t == "*" || t == "/" || t == "^")
                {
                    int prec = t == "+" || t == "-" ? 2 : (t == "^" ? 4 : 3);
                    bool right = t == "^";
                    list.Add(new Token { Text = t, Type = TokenType.Operator, Precedence = prec, RightAssociative = right });
                }
                else if (t == "!")
                {
                    list.Add(new Token { Text = t, Type = TokenType.Operator, Precedence = 5, RightAssociative = false });
                }
                else
                {
                    // Function name
                    list.Add(new Token { Text = t, Type = TokenType.Function });
                }
            }
            return list;
        }

        private List<Token> ToRPN(List<Token> tokens)
        {
            var output = new List<Token>();
            var ops = new Stack<Token>();
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.Number)
                {
                    output.Add(token);
                }
                else if (token.Type == TokenType.Function)
                {
                    ops.Push(token);
                }
                else if (token.Type == TokenType.Operator)
                {
                    while (ops.Count > 0 && (ops.Peek().Type == TokenType.Operator || ops.Peek().Type == TokenType.Function))
                    {
                        var top = ops.Peek();
                        if (top.Type == TokenType.Function || (top.Type == TokenType.Operator && ((top.Precedence > token.Precedence) || (top.Precedence == token.Precedence && !token.RightAssociative))))
                        {
                            output.Add(ops.Pop());
                            continue;
                        }
                        break;
                    }
                    ops.Push(token);
                }
                else if (token.Type == TokenType.Comma)
                {
                    while (ops.Count > 0 && ops.Peek().Type != TokenType.LeftParen) output.Add(ops.Pop());
                }
                else if (token.Type == TokenType.LeftParen)
                {
                    ops.Push(token);
                }
                else if (token.Type == TokenType.RightParen)
                {
                    while (ops.Count > 0 && ops.Peek().Type != TokenType.LeftParen) output.Add(ops.Pop());
                    if (ops.Count == 0) throw new Exception("Mismatched parentheses");
                    ops.Pop(); // remove left paren
                    if (ops.Count > 0 && ops.Peek().Type == TokenType.Function) output.Add(ops.Pop());
                }
            }
            while (ops.Count > 0) output.Add(ops.Pop());
            return output;
        }

        private double EvalRPN(List<Token> rpn)
        {
            var st = new Stack<double>();
            foreach (var tk in rpn)
            {
                if (tk.Type == TokenType.Number)
                {
                    st.Push(double.Parse(tk.Text, CultureInfo.InvariantCulture));
                }
                else if (tk.Type == TokenType.Operator)
                {
                    if (tk.Text == "!")
                    {
                        var tmp = st.Pop();
                        st.Push(Factorial(tmp));
                        continue;
                    }
                    var b = st.Pop();
                    var a = st.Pop();
                    switch (tk.Text)
                    {
                        case "+": st.Push(a + b); break;
                        case "-": st.Push(a - b); break;
                        case "*": st.Push(a * b); break;
                        case "/": st.Push(a / b); break;
                        case "^": st.Push(Math.Pow(a, b)); break;
                    }
                }
                else if (tk.Type == TokenType.Function)
                {
                    string name = tk.Text.ToLower();
                    if (name == "sin") { var v = st.Pop(); st.Push(Math.Sin(v)); }
                    else if (name == "cos") { var v = st.Pop(); st.Push(Math.Cos(v)); }
                    else if (name == "tan") { var v = st.Pop(); st.Push(Math.Tan(v)); }
                    else if (name == "sqrt") { var v = st.Pop(); st.Push(Math.Sqrt(v)); }
                    else if (name == "ln") { var v = st.Pop(); st.Push(Math.Log(v)); }
                    else if (name == "log") { var v = st.Pop(); st.Push(Math.Log10(v)); }
                    else if (name == "abs") { var v = st.Pop(); st.Push(Math.Abs(v)); }
                    else if (name == "ans") { st.Push(double.Parse(lastAnswer, CultureInfo.InvariantCulture)); }
                    else throw new Exception("Unknown function: " + tk.Text);
                }
            }
            if (st.Count != 1) throw new Exception("Invalid expression");
            return st.Pop();
        }

        private static double Factorial(double a)
        {
            if (a < 0) throw new Exception("Factorial negative");
            int n = (int)Math.Floor(a);
            double res = 1;
            for (int i = 2; i <= n; i++) res *= i;
            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections; // For array list
using System.IO; // Streamreader

namespace AddStrip
{
    public partial class AddStripForm : Form
    {
        private Calculation calculation1;
        private bool newFile =true;
        private bool changeMade = false;
        private bool cleanText = false; //If true, wipe textbox during key release

        /// <summary>
        /// Initialize the form
        /// </summary>
        public AddStripForm()
        {
            InitializeComponent();
            calculation1 = new Calculation(lstDisplay);
        }

        /// <summary> method: changeIsMade
        /// A method to run whenever a change is made (true), or when it is refreshed (false) 
        /// True = change the bool changeMade=True and add "*" on the activeform.text/title
        /// False = change the bool changeMade=False
        /// </summary>
        /// <param name="change"></param>
        private void changeIsMade(bool change)
        {
            if (change)
            {
                changeMade = true;
                int fileLength = ActiveForm.Text.Length;
                if (ActiveForm.Text[fileLength - 1] != '*')
                {
                    ActiveForm.Text += '*';//Pointer to show file has been changed
                }
            }
            else
            {
                changeMade = false;
            }
        }

        /// <summary> method: textBox1_KeyDown
        /// When termination key is pressed in textbox1 run accordingly
        /// The code is written with the qwerty keyboard in mind and not using the numpad, so there will be compatibility issues with numpad.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
        //Termination Key
             if ((textBox1.Text != "") //Check if it is not the first character
                && ((e.KeyCode == Keys.Oemplus && e.Modifiers == Keys.Shift) //   +   key
                || (e.KeyData == Keys.OemMinus)                              //   -   Key
                || (e.KeyCode == Keys.OemQuestion)                           //   /   key
                || (e.KeyCode == Keys.D8 && e.Modifiers == Keys.Shift)       //   *   key
                || (e.KeyCode == Keys.D3 && e.Modifiers == Keys.Shift)       //   #   key
                || (e.KeyCode == Keys.Oemplus)                               //   =   Key
                || (e.KeyCode == Keys.Enter)                                 // Enter Key
                ))
            {
                try
                {
                    changeIsMade(true);
                    string Text = textBox1.Text;
                    calculation1.Add(new CalcLine(Text));
                    textBox1.Clear();
                    if (e.KeyCode == Keys.Oemplus && e.Modifiers == Keys.Shift)
                    {
                        lstDisplay.Items.Add("+");
                    }
                    else if(e.KeyData == Keys.OemMinus)
                    {
                        lstDisplay.Items.Add("-");
                    }
                    else if(e.KeyCode == Keys.OemQuestion)
                    {
                        lstDisplay.Items.Add("/");
                    }
                    else if (e.KeyCode == Keys.D8 && e.Modifiers == Keys.Shift)
                    {
                        lstDisplay.Items.Add("*");
                    }
                    else if (((e.KeyCode == Keys.Oemplus)|| (e.KeyCode == Keys.Enter)))
                    {
                        calculation1.Add(new CalcLine("="));
                        cleanText = true;
                    }
                    else if (e.KeyCode == Keys.D3 && e.Modifiers == Keys.Shift)
                    {
                        calculation1.Add(new CalcLine("#"));
                        cleanText = true;
                    }
                }
                catch
                {
                    MessageBox.Show("The first character must be an operator (+,-,*,/,#,=), the next character must be a numeric character(0-9), followed up by another number or a termination character (+,-,*,/,#,=) \r\n Example 1: +2# \r\n Example 2: -9*", "Error");
                }
            }
            else if(((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Oemplus && e.Modifiers == Keys.None)) && textBox1.Text == "")
            {
                changeIsMade(true);
                calculation1.Add(new CalcLine("="));
                cleanText = true;
            }
            else if ((e.KeyCode == Keys.D3 && e.Modifiers == Keys.Shift) && textBox1.Text == "")
            {
                changeIsMade(true);
                calculation1.Add(new CalcLine("#"));
                cleanText = true;
            }
        }


        /// <summary> method: textBox1_KeyPress
        /// when key is pressed, limit keys that can be entered on the textbox
        /// IF (textBox1.Text == "" && lstDisplay.Items.Count == 0) For the very first item on the list
        /// Else IF (textBox1.Text =="") for the first character that is not the first item in the list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (textBox1.Text == "" && lstDisplay.Items.Count == 0)
            {
                // Check for a naughty character in the KeyDown event.
                if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^+^\-]"))//source: https://stackoverflow.com/questions/12607087/only-allow-specific-characters-in-textbox
                {
                    e.Handled = true;// Stop the character from being entered into the control since it is illegal.
                }
                if (e.KeyChar == (char)Keys.Back) //Handle backspace
                {
                    e.Handled = false;
                }
            }
            else if (textBox1.Text =="") {
                // Check for a naughty character in the KeyDown event.
                if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^+^\-^\/^\*^\=^\#]"))
                {
                    e.Handled = true;
                }
                if ((e.KeyChar == (char)Keys.Back)||(e.KeyChar == (char)Keys.Enter)) //Handle backspace and enter
                {
                    e.Handled = false;
                }
            }
            else
            {
               if (System.Text.RegularExpressions.Regex.IsMatch(e.KeyChar.ToString(), @"[^0-9^+^\-^\/^\*^\=^\#]"))
                {
                    e.Handled = true;
                }
                if ((e.KeyChar == (char)Keys.Back) || (e.KeyChar == (char)Keys.Enter))
                {
                    e.Handled = false;
                }
            }
        }

        /// <summary> method: mnuNew_Click
        /// When new is clicked on the toolstrip
        /// prompt if user want to save or not if changes is made
        /// clear listbox
        /// clear arraylist
        /// clear texbox
        /// reset form title
        /// set newfile variable and run change is made method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuNew_Click(object sender, EventArgs e)
        {
            promptSave(sender, e);
            lstDisplay.Items.Clear();
            calculation1.Clear();
            textBox1.Text = "";
            textBox2.Text = "";
            ActiveForm.Text = "Add Strip Form - New File";
            newFile = true;
            changeIsMade(false);
        }

        /// <summary> method: mnuOpen_Click
        /// When open is clicked on the toolstrip
        /// prompt if user want to save or not if changes is made
        /// run calculation load from file
        /// change form title
        /// set newfile variable and run change is made method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void mnuOpen_Click(object sender, EventArgs e)
        {
            promptSave(sender, e);
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                calculation1.LoadFromFile(openFileDialog1.FileName);
                AddStripForm.ActiveForm.Text = "Add Strip Form - " + openFileDialog1.FileName;
                newFile = false;
                changeIsMade(false);
            }
        }

        /// <summary> method: saveToolStripMenuItem_Click
        /// when save is clicked
        /// if its a new file, run save as method
        /// run calculation save to file method
        /// set variable & title
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(newFile)
            {
                saveAsToolStripMenuItem_Click(sender, e);
            }
            else
            {
                saveFileDialog1.FileName = openFileDialog1.FileName;
                calculation1.SaveToFile(saveFileDialog1.FileName);
                newFile = false;
                changeIsMade(false);
                AddStripForm.ActiveForm.Text = "Add Strip Form - " + openFileDialog1.FileName;
            }
        }

        /// <summary> method: saveAsToolStripMenuItem_Click
        /// when save as clicked
        /// run calculation save to file method
        /// set variable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                calculation1.SaveToFile(saveFileDialog1.FileName);
                openFileDialog1.FileName = saveFileDialog1.FileName;
                newFile = false;
                changeIsMade(false);
                AddStripForm.ActiveForm.Text = "Add Strip Form - " + openFileDialog1.FileName;
            }
        }

        /// <summary> method: printToolStripMenuItem_Click
        /// open print preview dialog
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printToolStripMenuItem_Click(object sender, EventArgs e)
        {
            printPreviewDialog1.ShowDialog();
        }


        /// <summary> method: exitToolStripMenuItem_Click
        /// close the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        /// <summary>
        /// The method below runs when program is closed regardless of how you close it
        /// </summary>
        private void AddStripForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            promptSave(sender, e);
        }

        /// <summary> method: lstDisplay_SelectedIndexChanged
        /// when item in listbox is selected return selected value
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lstDisplay_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                string selected = lstDisplay.GetItemText(lstDisplay.SelectedItem);
                if (selected[0] == '=')
                {
                    textBox2.Text = "=";
                }
                else if (selected[0] == '#')
                {
                    textBox2.Text = "#";
                }
                else
                {
                    textBox2.Text = selected;
                }
            }
            catch
            {
                MessageBox.Show("Line does not exist, select another line","Error");
            }
        }

        /// <summary> method:btnUpdate_Click
        /// when update button is clicked
        /// check if selected index exist
        /// check textbox2 string
        /// run calculation replace method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (lstDisplay.SelectedIndex == -1)
                {
                    MessageBox.Show("Please select an item you want to update from the listbox", "Error");
                }
                else
                {
                    string text = textBox2.Text;
                    if ((text[0] == '+') || (text[0] == '-') || (text[0] == '*') || (text[0] == '/') || (text[0] == '#') || (text[0] == '='))
                    {
                        int n = lstDisplay.SelectedIndex;
                        calculation1.Replace(new CalcLine(text), n);
                        changeIsMade(true);
                    }
                    else
                    {
                        MessageBox.Show("First letter must be + - * / = or #");
                    }
                }
            }
            catch{
                MessageBox.Show("File cannot be updated", "Error");
            }
        }

        /// <summary> method:btnDelete_Click
        /// when delete button is clicked
        /// check if selected index exist
        /// try to delete it, catch error
        /// run calculation delete method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstDisplay.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an item you want to delete from the listbox","Error");
            }
            else
            {
                try {
                    string text = textBox2.Text;
                    DialogResult dialogResult = MessageBox.Show("Do you want to delete " + text + "?", "Delete Line", MessageBoxButtons.YesNo);
                    if (dialogResult == DialogResult.Yes)
                    {
                        int n = lstDisplay.SelectedIndex;
                        calculation1.Delete(n);
                        changeIsMade(true);
                    }
                }
                catch
                {
                    MessageBox.Show("Selected line cannot be deleted.", "Error");
                }
            }
        }

        /// <summary> method:btnInsert_Click
        /// when insert button is clicked
        /// check if selected index exist
        /// check operator
        /// try to insert it, catch error
        /// run calculation insert method
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInsert_Click(object sender, EventArgs e)
        {
            if (lstDisplay.SelectedIndex == -1)
            {
                MessageBox.Show("Please select an item you want to update from the listbox", "Error");
            }
            else
            {
                try
                {
                    string text = textBox2.Text;
                    if ((text[0] == '+') || (text[0] == '-') || (text[0] == '*') || (text[0] == '/') || (text[0] == '#') || (text[0] == '='))
                    {
                        int n = lstDisplay.SelectedIndex;
                        calculation1.Insert(new CalcLine(text), n);
                        changeIsMade(true);
                    }
                    else
                    {
                        MessageBox.Show("First letter must be + - * / = or #");
                    }
                }
                catch
                {
                    MessageBox.Show("The first character must be an operator (+,-,*,/,#,=), the next character must be a numeric character(0-9), followed up by another number or a termination character (+,-,*,/,#,=) \r\n Example 1: +2# \r\n Example 2: -9*", "Error");
                }
            }
        }

        /// <summary> method: promptSave
        /// IF bool change made is true
        /// prompt if user want to save or not, if yes run saveToolStripMenuItem_Click()
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void promptSave(object sender, EventArgs e)
        {
            if (changeMade == true)
            {
                DialogResult dialogResult = MessageBox.Show("Changes have been made recently, do you want to save?", "Delete Line", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    saveToolStripMenuItem_Click(sender, e);
                }
            }
        }

        /// <summary> method:printDocument1_PrintPage
        /// draw string on print preview dialogs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void printDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics g = e.Graphics;
            int linesSoFarHeading = 0;
            Font textFont = new Font("Arial", 12, FontStyle.Regular);
            Font headingFont = new Font("Arial", 14, FontStyle.Regular);
            Brush brush = new SolidBrush(Color.Black);

            //margins
            int leftMargin = e.MarginBounds.Left;
            int topMargin = e.MarginBounds.Top;
            int headingLeftMargin = 50;
            int topMarginDetails = topMargin + 70;
            int rightMargin = e.MarginBounds.Right;

            int length =calculation1.GetLength();
            for(int i=0;i<length; i++)
            {
                string line= calculation1.Find(i).ToString();
                g.DrawString(line, textFont, brush, leftMargin + headingLeftMargin,
                topMargin + (linesSoFarHeading * textFont.Height));
                linesSoFarHeading++;
            }
        }

        /// <summary> method:textBox1_KeyUp
        /// when key is released
        /// if cleantext is true, wipe textbox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void textBox1_KeyUp(object sender, KeyEventArgs e)
        {
            if(cleanText)
            {
                cleanText = false;
                textBox1.Clear();
            }
        }

        /// <summary> method:button1_Click
        /// when button is clicked hide/show help information for the user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            if (panel2.Visible == true)
            {
                panel2.Hide();
            }
            else
            {
                panel2.Show();
            }
        }
    }

    public class Calculation
    {
        ArrayList theCalcs;
        ListBox lstDisplay;

        /// <summary>
        /// Constructor initializes the reference to the listbox and starts a new ArrayList.
        /// </summary>
        /// <param name="lb"></param>
        public Calculation(ListBox lb)
        {
            lstDisplay = lb;
            theCalcs = new ArrayList();
        }

        /// <summary>
        /// Add a CalcLine object to the ArrayList then redisplay the calculations.
        /// </summary>
        /// <param name="cl"></param>
        public void Add(CalcLine cl)
        {
            theCalcs.Add(cl);
            Redisplay();
        }

        /// <summary>
        /// Clear the ArrayList and the listbox.
        /// </summary>
        public void Clear()
        {
            theCalcs.Clear();
            lstDisplay.Items.Clear();
        }

        /// <summary>
        /// Clear the listbox and then for each line in the calculation, 
        /// if the line is an ordinary calculation add the text version of the CalcLine to the listbox and calculate the result of the calculation so far. 
        /// If the line is for a total or subtotal add the text for the total or subtotal to the listbox. 
        /// If the line is for a total, the result of the calculation so far is reset to zero.
        /// </summary>
        public void Redisplay()
        {
            double total = 0;
            lstDisplay.Items.Clear();
            foreach (var item in theCalcs)
            {
                CalcLine calc1 = new CalcLine(item.ToString());
                Operator op = calc1.Op;

                if (op == Operator.subtotal)
                {
                    lstDisplay.Items.Add("# " + total + " < subtotal");
                }

                else if (op==Operator.total)
                {
                    lstDisplay.Items.Add("= " +total +" << total");
                    total = 0;// reset total
                }
                else //Ordinary calculation
                {
                    lstDisplay.Items.Add(item);
                    total = calc1.NextResult(total);
                }
            }
        }

        /// <summary>
        /// Return a reference to the nth CalcLine object in the ArrayList
        /// </summary>
        public CalcLine Find(int n)
        {
            int loop = 0;
            CalcLine calc1 = new CalcLine("+0");
            foreach (var item in theCalcs)
            {
                if (loop == n)
                {
                    calc1 = new CalcLine(item.ToString());
                }
                loop++;
            }
            return calc1;
        }

        /// <summary>
        /// Replace the nth CalcLine object in the ArrayList with the newCalc object, and then redisplay the calculations.
        /// </summary>
        /// <param name="newCalc"></param>
        /// <param name="n"></param>
        public void Replace(CalcLine newCalc, int n)
        {
            theCalcs[n] = newCalc;
            Redisplay();
        }

        /// <summary>
        /// Insert the newCalc CalcLine object in the ArrayList immediately before the nth object, and then redisplay the calculations.
        /// </summary>
        /// <param name="newCalc"></param>
        /// <param name="n"></param>
        public void Insert(CalcLine newCalc, int n)
        {
            theCalcs.Insert(n, newCalc);
            Redisplay();
        }

        /// <summary>
        /// Delete the nth CalcLine object in the ArrayList, and then redisplay the calculations.
        /// </summary>
        /// <param name="n"></param>
        public void Delete(int n)
        {
            theCalcs.RemoveAt(n);
            Redisplay();
        }

        /// <summary>
        /// Save all the CalcLine objects in the ArrayList as lines of text in the given file.
        /// </summary>
        /// <param name="filename"></param>
        public void SaveToFile(string filename)
        {
            StreamWriter sw = new StreamWriter(filename);
            foreach (var item in theCalcs)
            {
                string text = item.ToString();
                sw.Write(text+"\r\n");

            }
            sw.Close();
        }

        /// <summary>
        /// Clear the ArrayList and then open the given file and convert the lines of the file to a set of CalcLine objects held in the ArrayList. Then redisplay the calculations.
        /// </summary>
        /// <param name="filename"></param>
        public void LoadFromFile(string filename)
        {
            Clear();

            StreamReader sr = new StreamReader(filename);

            string line;
            while ((line = sr.ReadLine()) != null)
            {
                theCalcs.Add(line);
            }
            sr.Close();

            Redisplay();
        }

        /// <summary>
        /// custom method to steal length of array from calculation object
        /// </summary>
        /// <returns></returns>
        public int GetLength ()
        {
            return theCalcs.Count;
        }
   }


    public enum Operator { plus, minus, times, divide, subtotal, total, error };
    //data type that holds literal code for allowable operators

    /// <summary> class CalcLine
    /// class to represent individual lines in an adding strip calculation
    /// </summary>
    public class CalcLine
    {
        private Operator op;
        private double number;

        /// <summary> constructor CalcLine
        /// create a CalcLine object from an Operator value and a double value
        /// </summary>
        public CalcLine(Operator OP, double num)
        {
            op = OP;
            number = num;
        }

        /// <summary> constructor CalcLine
        /// create a CalcLine object from an Operator value
        /// </summary>
        public CalcLine(Operator OP)
        {
            op = OP;
            number = 0;
        }

        /// <summary> constructor CalcLine
        /// create a CalcLine object from a string value 
        /// </summary>
        public CalcLine(string calcLine)
        //assume format 123, +123, = (total) or # (subtotal)
        {
            char ch = calcLine[0];
            if (ch == '=')
            {
                op = Operator.total;
                number = 0;
            }
            else if (ch == '#')
            {
                op = Operator.subtotal;
                number = 0;
            }
            else if ((ch >= (char)48) && (ch <= (char)57)) //numeric
            {
                op = Operator.plus;
                number = Convert.ToDouble(calcLine);
            }
            else
            {
                op = CharToOp(ch);
                string snum = calcLine.Substring(1);
                number = Convert.ToDouble(snum);
            }
        }

        public Operator Op //public property that gives access to the private op data field
        {
            set { op = value; }
            get { return op; }
        }

        /// <summary> method ToString
        /// return a string giving the operator and number of CalcLine object
        /// or just the operator if the operation is for a total or subtotal
        /// </summary>
        public override string ToString()
        {
            if (op == Operator.total)
            {
                return "=";
            }
            else if (op == Operator.subtotal)
            {
                return "#";
            }
            else
            {
                return (OpToString(op) + " " + Convert.ToString(number));
            }
        }

        /// <summary> method NextResult
        /// return a double giving the result of applying the CalcLine object
        /// to the value passed in the double parameter ResultSoFar
        /// 
        /// Used to get the cumulative result of a set of CalcLine objects
        /// </summary>
        public double NextResult(double ResultSoFar)
        {
            if (op == Operator.plus) return ResultSoFar + number;
            else if (op == Operator.minus) return ResultSoFar - number;
            else if (op == Operator.times) return ResultSoFar * number;
            else if (op == Operator.divide) return ResultSoFar / number;
            else if (op == Operator.subtotal) return ResultSoFar;
            else if (op == Operator.total) return ResultSoFar;
            else return -1;
        }

        /// <summary> method OpToString
        /// return a string giving the string version of the operator held
        /// by the CalcLine object
        /// </summary>
        public static string OpToString(Operator OP)
        {
            if (OP == Operator.plus) return "+";
            else if (OP == Operator.minus) return "-";
            else if (OP == Operator.times) return "*";
            else if (OP == Operator.divide) return "/";
            else if (OP == Operator.subtotal) return "#";
            else if (OP == Operator.total) return "=";
            else return "Error";
        }

        /// <summary> method CharToOp
        /// return the Operator value matching a given char value
        /// 
        /// Can be used to check whether a char converts to a valid Operator
        /// </summary>
        public static Operator CharToOp(char strOp)
        {
            if (strOp == '+') return Operator.plus;
            else if (strOp == '-') return Operator.minus;
            else if (strOp == '*') return Operator.times;
            else if (strOp == '/') return Operator.divide;
            else if (strOp == '#') return Operator.subtotal;
            else if (strOp == '=') return Operator.total;
            else return Operator.error;
        }

    }
}

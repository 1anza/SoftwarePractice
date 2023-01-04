using SpreadsheetUtilities;
using SS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace SpreadsheetGUI
{
    public partial class Form1 : Form
    {
        // Variable that stores spreadsheet data
        private Spreadsheet spreadsheet;
        // Variable stores stack of all selected cells in (col, row) format
        private Stack<int[]> selectionHistory;

        public Form1()
        {
            InitializeComponent();
            // Creates new spreasheet with s.ToUpper normaizer to correspond with spreadsheetPanel cell names
            spreadsheet = new Spreadsheet(s=>true, s=>s.ToUpper(), "");
            FilenameText.Text = "untitled.sprd";

            // Sets defualt cell to "A1"
            spreadsheetPanel1.SetSelection(0, 0);
            selectionHistory = new Stack<int[]>();
            selectionHistory.Push(new int[] { 0, 0 });

            // Selection changed handler
            spreadsheetPanel1.SelectionChanged += OnSelected;

            CellInput.Focus();
            CellDataText.Text = "A1: ";

            // Closing form handler
            this.FormClosing += Form1_FormClosing;


            // Key press handler
            CellInput.KeyPress += new KeyPressEventHandler(EnterPressed);

            try
            {
                CellInput.KeyDown += new KeyEventHandler(Form1_KeyDown);
                CellInput.KeyUp += new KeyEventHandler(Form1_KeyUp);
            }
            catch (Exception exc)
            {

            }
        }

        // OnSelected: Runs everytime another cell is selected
        public void OnSelected(SpreadsheetPanel p)
        {
            // Adds data to cell previously selected.
            // Add data to cell when clicked away like in Excel
            if (CellInput.Text != "")
            {
                string name = CellName(selectionHistory.Peek()[0], selectionHistory.Peek()[1]);
                spreadsheet.SetContentsOfCell(name, CellInput.Text);
                p.SetValue(selectionHistory.Peek()[0], selectionHistory.Peek()[1], spreadsheet.GetCellValue(name).ToString());
                displayData();

                CellInput.Clear();
            }


            int row, col;
            p.GetSelection(out col, out row);
            selectionHistory.Push(new int[] { col, row });

            // Formula edit value: Adds '=' to the beginning of function in text editor
            if(spreadsheet.GetCellContents(CellName(col, row)) is Formula)
            {
                CellInput.Text = "=" + spreadsheet.GetCellContents(CellName(col, row)).ToString();
            }
            else
            {
                CellInput.Text = spreadsheet.GetCellContents(CellName(col, row)).ToString();
            }

            // Catches Formula error and displays in value
            if (spreadsheet.GetCellValue(CellName(col, row)) is FormulaError) {
                CellDataText.Text = CellName(col, row) + ": " + (FormulaError)spreadsheet.GetCellValue(CellName(col, row));
            }
            else
            {
                CellDataText.Text = CellName(col, row) + ": " + spreadsheet.GetCellValue(CellName(col, row));
            }

            CellInput.Select(CellInput.Text.Length, 0);
        }

        // Lists through all non-empty cells in spreadsheet variable and displays them in spreadsheetPanel
        private void displayData()
        {
            CellInput.Text = spreadsheet.GetCellContents("A1").ToString();
            CellDataText.Text = "A1: " + spreadsheet.GetCellValue("A1");

            foreach (string name in spreadsheet.GetNamesOfAllNonemptyCells())
            {
                // Changes name to col, row values
                int row = Int32.Parse(Regex.Match(name, @"\d+").Value) - 1;
                char letter = name[0];
                int col = char.ToUpper(letter) - 65;

                spreadsheetPanel1.SetValue(col, row, spreadsheet.GetCellValue(name).ToString());
            }
        }

        // Helper method that converts a cell name to col, row format
        private string CellName(int col, int row)
        {
            char columnLetter = Convert.ToChar(65 + col);
            int currentRow = row;
            return (columnLetter + "" + ++currentRow);
        }

        // Enter key method - moves selected cell to the cell below
        private void EnterPressed(Object o, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                int row, col;
                spreadsheetPanel1.GetSelection(out col, out row);
               
                spreadsheetPanel1.SetSelection(col, row + 1);
                OnSelected(spreadsheetPanel1);


                e.Handled = true;
            }
        }

        // Updates cell value as textbox is changed
        private void CellInput_TextChanged(object sender, EventArgs e)
        {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            spreadsheetPanel1.SetValue(col, row, CellInput.Text);
        }

        // Opens file button dropdown
        private void FileButton_Click(object sender, EventArgs e)
        {
            Point screenPoint = FileButton.PointToScreen(new Point(FileButton.Left, FileButton.Bottom));
            if (screenPoint.Y + FileDropDown.Size.Height > Screen.PrimaryScreen.WorkingArea.Height)
            {
                FileDropDown.Show(FileButton, new Point(0, -FileDropDown.Size.Height));
            }
            else
            {
                FileDropDown.Show(FileButton, new Point(0, FileButton.Height));
            }
        }

        // On form load - puts cusor in cell editor
        private void Form1_Load(object sender, EventArgs e)
        {
            CellInput.Select();
            this.ActiveControl = CellInput;
            CellInput.Focus();
        }

        // Moves arrow keys
        void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            int col, row;
            spreadsheetPanel1.GetSelection(out col, out row);
            switch (e.KeyCode)
            {
                // handle up/down/left/right
                case Keys.Up:
                    spreadsheetPanel1.SetSelection(col, --row);
                    break;
                case Keys.Down:
                    spreadsheetPanel1.SetSelection(col, ++row);
                    break;
                case Keys.Right:
                    spreadsheetPanel1.SetSelection(++col, row);
                    break;
                case Keys.Left:
                    spreadsheetPanel1.SetSelection(--col, row);
                    break;
                default: return;  // ignore other keys
            }
            OnSelected(spreadsheetPanel1);
        }

        // These are supposed to be empty - Gives error is deleted
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // undo what was done by KeyDown
        }

        private void FileDropDown_Opening(object sender, CancelEventArgs e)
        {
        }

        // Form closing 'X' button - Shows message asking to save if the spreadsheet has been changed
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            int row, col;
            spreadsheetPanel1.GetSelection(out col, out row);
            if (CellInput.Text != "")
            {
                string name = CellName(col, row);
                spreadsheet.SetContentsOfCell(name, CellInput.Text);
                spreadsheetPanel1.SetValue(col, row, spreadsheet.GetCellValue(name).ToString());
                displayData();
            }


            if (spreadsheet.Changed)
            {
                DialogResult result = MessageBox.Show("Are you sure? Save before you close?", "Spreadsheet", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);

                if (result.Equals(DialogResult.Yes))
                    SaveButton_Click(sender, e);
                    e.Cancel = false;

                if (result.Equals(DialogResult.No))
                    e.Cancel = false;

                if (result.Equals(DialogResult.Cancel))
                    e.Cancel = true;
            }
            else
            {
                e.Cancel = false;
            }
        }


        // NEW BUTTON - Opens new empty spreadsheet
        private void buttonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SpreadsheetApplicationContext.getAppContext().RunForm(new Form1());
        }

        // Opens and displays data of selected file in spreadsheet
        private void OpenButton_Click(object sender, EventArgs e)
        {
            if (spreadsheet.Changed)
            {
                DialogResult result = MessageBox.Show("Are you sure? Save before you open new file?", "Spreadsheet", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Exclamation);
                if (result == DialogResult.Yes)
                    SaveButton_Click(sender, e);
              
                if (result == DialogResult.Cancel)
                    return;

            }

            OpenFileDialog openFileDialog1 = new OpenFileDialog
            {
                InitialDirectory = @"D:\",
                Title = "Browse Text Files",

                CheckFileExists = true,
                CheckPathExists = true,

                DefaultExt = "sprd",
                Filter = "sprd files (*.sprd)|*.sprd",
                FilterIndex = 2,
                RestoreDirectory = true,

                ReadOnlyChecked = true,
                ShowReadOnly = true
            };


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string version = spreadsheet.GetSavedVersion(openFileDialog1.FileName);
                spreadsheet = new Spreadsheet(openFileDialog1.FileName, s => true, s => s, version);
                displayData();
                FilenameText.Text = openFileDialog1.FileName;
            }
            
        }

        // Saves data as .sprd file
        private void SaveButton_Click(object sender, EventArgs e)
        {
            //Initialize new savefile dialogue
            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "Spreadsheet Files (*.sprd)|*.sprd|All Files (*.*)|*.*";
            save.DefaultExt = ".sprd"; save.AddExtension = true;
            save.FileName = "New Default Spreadsheet"; save.OverwritePrompt = false;

            DialogResult result = save.ShowDialog();

            //If result is yes, save the file
            if (result.Equals(DialogResult.OK))
                spreadsheet.Save(save.FileName);
                FilenameText.Text = save.FileName;


            //Otherwise cancel potential new save
            if (result.Equals(DialogResult.Cancel))
                save.Dispose();
        }

        private void HelpButton_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Change Selection: To change selection you click on a new cell. \n\nEdit Cell Contents: To edit cell contents, click on cell and enter cell contetnts into textbox above spreadsheet panel. Data will automatically add to the spreadsheet by clicking away or pressing enter. \n\nHow to Close: Clicking on close button will ask to save data. Must click on 'Close' button instead of the x. \n\nExtra Feature: See current filename.");
        }
    }
}

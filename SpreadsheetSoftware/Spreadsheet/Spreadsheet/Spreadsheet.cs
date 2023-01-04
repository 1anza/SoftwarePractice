/// Finalized PS5 - 10/4/21
/// 
/// Author: Alivia Liljenquist
/// UID: 1348865
/// 
/// <summary>
/// An AbstractSpreadsheet object represents the state of a simple spreadsheet.  A 
/// spreadsheet consists of an infinite number of named cells.
/// 
/// A string is a valid cell name if and only if:
///   (1) its first character is an underscore or a letter
///   (2) its remaining characters (if any) are underscores and/or letters and/or digits
/// Note that this is the same as the definition of valid variable from the PS3 Formula class.
/// 
/// For example, "x", "_", "x2", "y_15", and "___" are all valid cell  names, but
/// "25", "2x", and "&" are not.  Cell names are case sensitive, so "x" and "X" are
/// different cell names.
/// 
/// A spreadsheet contains a cell corresponding to every possible cell name.  (This
/// means that a spreadsheet contains an infinite number of cells.)  In addition to 
/// a name, each cell has a contents and a value.  The distinction is important.
/// 
/// The contents of a cell can be (1) a string, (2) a double, or (3) a Formula.  If the
/// contents is an empty string, we say that the cell is empty.  (By analogy, the contents
/// of a cell in Excel is what is displayed on the editing line when the cell is selected.)
/// 
/// In a new spreadsheet, the contents of every cell is the empty string.
///  
/// The value of a cell can be (1) a string, (2) a double, or (3) a FormulaError.  
/// (By analogy, the value of an Excel cell is what is displayed in that cell's position
/// in the grid.)
/// 
/// If a cell's contents is a string, its value is that string.
/// 
/// If a cell's contents is a double, its value is that double.
/// 
/// If a cell's contents is a Formula, its value is either a double or a FormulaError,
/// as reported by the Evaluate method of the Formula class.  The value of a Formula,
/// of course, can depend on the values of variables.  The value of a variable is the 
/// value of the spreadsheet cell it names (if that cell's value is a double) or 
/// is undefined (otherwise).
/// 
/// Spreadsheets are never allowed to contain a combination of Formulas that establish
/// a circular dependency.  A circular dependency exists when a cell depends on itself.
/// For example, suppose that A1 contains B1*2, B1 contains C1*2, and C1 contains A1*2.
/// A1 depends on B1, which depends on C1, which depends on A1.  That's a circular
/// dependency.
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using SpreadsheetUtilities;

namespace SS
{
    public class Spreadsheet : AbstractSpreadsheet
    {
        // Dictionary that keeps track of non-empty cells.
        private Dictionary<string, Cell> totalCells = new Dictionary<string, Cell>();
        // Keeps track of cell dependencies.
        private DependencyGraph cellMap = new DependencyGraph();

        //Keeps track is file has been changed.
        public override bool Changed { get; protected set; } = false;

        //Default constructor that saves default version.
        public Spreadsheet() : base(s => true, s => s, "default")
        {
        }

        //Constructor 1 that saves default version.
        public Spreadsheet(Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
        }

        //Constructor reads xml file per filename inserted.
        public Spreadsheet(string filepath, Func<string, bool> isValid, Func<string, string> normalize, string version) : base(isValid, normalize, version)
        {
            if (GetSavedVersion(filepath) != version)
            {
                throw new SpreadsheetReadWriteException($"Version {version} of {filepath} does not exist.");
            }

            try
            {
                using (XmlReader reader = XmlReader.Create(filepath))
                {
                    string name = "";
                    bool hasName = false;
                    string contents = "";
                    bool hasContents = false;
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            switch (reader.Name)
                            {
                                case "name":
                                    reader.Read();
                                    name = reader.Value;
                                    hasName = true;
                                    break;
                                case "contents":
                                    reader.Read();
                                    contents = reader.Value;
                                    hasContents = true;
                                    break;
                            }
                        }
                        if (hasName && hasContents)
                        {
                            this.SetContentsOfCell(name, contents);
                            hasName = false;
                            hasContents = false;
                        }
                    }
                }
            }
            catch (InvalidNameException)
            {
                throw new SpreadsheetReadWriteException("A name contained in the saved spreadsheet was invalid.");
            }
            catch (CircularException)
            {
                throw new SpreadsheetReadWriteException("A circular dependency in the saved spreadsheet was detected.");
            }
            catch (FormulaFormatException)
            {
                throw new SpreadsheetReadWriteException("An invalid formula was found in the saved spreadsheet.");
            }
            catch (XmlException)
            {
                throw new SpreadsheetReadWriteException("There was an error reading the XML of the given spreadsheet.");
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an error in your input.");
            }

            Changed = false;
        }

        public override string GetSavedVersion(string filename)
        {
            try
            {
                using (XmlReader reader = XmlReader.Create(filename))
                {
                    while (reader.Read())
                    {
                        if (reader.IsStartElement())
                        {
                            if (reader.Name == "spreadsheet")
                            {
                                string version = reader.GetAttribute("version");
                                if (version != null)
                                {
                                    return version;
                                }
                            }
                        }
                    }
                    throw new SpreadsheetReadWriteException("The version was not found.");
                }
            }
            catch (ArgumentNullException)
            {
                throw new SpreadsheetReadWriteException("A null filename was given.");
            }
            catch (System.IO.FileNotFoundException)
            {
                throw new SpreadsheetReadWriteException("File given was not found.");
            }
            catch (System.IO.DirectoryNotFoundException)
            {
                throw new SpreadsheetReadWriteException("Directory for file given was not found.");
            }

            catch (SpreadsheetReadWriteException)
            {
                throw;
            }
            catch (XmlException)
            {
                throw new SpreadsheetReadWriteException("There was an error reading the XML of the given spreadsheet.");
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an error reading the XML of the given spreadsheet.");
            }
        }

        public override void Save(string filename)
        {
            try
            {
                XmlWriterSettings settings = new XmlWriterSettings();
                settings.Indent = true;
                settings.IndentChars = "    ";
                using (XmlWriter writer = XmlWriter.Create(filename, settings))
                {
                    writer.WriteStartDocument();
                    writer.WriteStartElement("spreadsheet");
                    writer.WriteAttributeString("version", Version);


                    foreach (KeyValuePair<string, Cell> kvp in totalCells)
                    {
                        writer.WriteStartElement("cell");
                        writer.WriteElementString("name", kvp.Key);
                        writer.WriteElementString("contents", kvp.Value.getContents().ToString());
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                }

            }
            catch (ArgumentNullException)
            {
                throw new SpreadsheetReadWriteException("A null filename was given");
            }
            catch (XmlException)
            {
                throw new SpreadsheetReadWriteException("There was an error writing the XML spreadsheet file to the given filename");
            }
            catch
            {
                throw new SpreadsheetReadWriteException("There was an error in read/writing to this filename.");
            }

            Changed = false;
        }

        public override object GetCellValue(string name)
        {
            name = Normalize(name);
            if (name == null || !validCellName(name))
            {
                throw new InvalidNameException();
            }

            Cell cell;
            bool tryCell = totalCells.TryGetValue(name, out cell);
            if (tryCell)
            {
                return cell.getValue();
            }
            else
            {
                return "";
            }
        }

        public override IList<string> SetContentsOfCell(string name, string content)
        {
            Changed = true;
            name = Normalize(name);

            if (name is null || !validCellName(name))
            {
                throw new InvalidNameException();
            }

            if (content is null)
            {
                throw new ArgumentNullException();
            }


            IList<string> recalculate;

            double doub;
            if (double.TryParse(content, out doub))
            {
                recalculate = SetCellContents(name, doub);
            }
            else if (content.StartsWith("="))
            {
                try
                {
                    recalculate = SetCellContents(name, new Formula(content.Substring(1), Normalize, IsValid));
                }
                catch
                {
                    recalculate = SetCellContents(name, content);
                }
            }
            else
            {
                recalculate = SetCellContents(name, content);
            }

            UpdateCells(recalculate);

            return recalculate;
        }


        /// <summary>
        /// If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, returns the contents (as opposed to the value) of the named cell.  The return
        /// value should be either a string, a double, or a Formula.
        /// </summary>
        public override object GetCellContents(string name)
        {
            name = Normalize(name);
            if (name == null || !validCellName(name))
            {
                throw new InvalidNameException();
            }

            Cell cell;
            bool tryCell = totalCells.TryGetValue(name, out cell);
            if (tryCell)
            {
                return cell.getContents();
            }
            else
            {
                return "";
            }
        }

        public override IEnumerable<string> GetNamesOfAllNonemptyCells()
        {
            return new List<string>(totalCells.Keys);
        }

        /// <summary>
        /// Sets cell contents to number. Recalculates all cell dependencies and returns new direct and indirect dependents.
        /// 
        ///  If name is null or invalid, throws an InvalidNameException.
        /// 
        /// Otherwise, the contents of the named cell becomes number.  The method returns a
        /// list consisting of name plus the names of all other cells whose value depends, 
        /// directly or indirectly, on the named cell.
        /// 
        /// For example, if name is A1, B1 contains A1*2, and C1 contains B1+A1, the
        /// list {A1, B1, C1} is returned.
        /// </summary>
        protected override IList<string> SetCellContents(string name, double number)
        {
            if (totalCells.ContainsKey(name))
            {
                totalCells.Remove(name);
            }

            totalCells.Add(name, new Cell(number));
            cellMap.ReplaceDependees(name, new List<string> { });
            return GetCellsToRecalculate(name).ToList();
        }

        protected override IList<string> SetCellContents(string name, string text)
        {
            if (text != "")
            {
                if (totalCells.ContainsKey(name))
                {
                    totalCells.Remove(name);
                }

                if (text.StartsWith("="))
                {
                    Cell invalidFormula = new Cell(text, "FormulaFormatException: Invalid character.");
                    totalCells.Add(name, invalidFormula);
                }
                else
                {
                    totalCells.Add(name, new Cell(text));
                }
                cellMap.ReplaceDependees(name, new List<string> { });
                return GetCellsToRecalculate(name).ToList();
            }
            else
            {
                return new List<string> { };
            }
        }

        protected override IList<string> SetCellContents(string name, Formula formula)
        {

            List<string> dependees = cellMap.GetDependees(name).ToList();
            Cell prevCell;
            bool tryPrevVal = totalCells.TryGetValue(name, out prevCell);



            if (totalCells.ContainsKey(name))
            {
                totalCells.Remove(name);
            }

            try
            {
                totalCells.Add(name, EvaluateFormula(formula));
                cellMap.ReplaceDependees(name, formula.GetVariables());
                return GetCellsToRecalculate(name).ToList();
            }
            catch (CircularException e)
            {
                totalCells.Remove(name);
                //if (tryPrevVal)
                //{
                //    totalCells.Add(name, prevCell);
                //}
                //cellMap.ReplaceDependees(name, dependees);
                ////throw e;
                //return GetCellsToRecalculate(name).ToList();
                totalCells.Add(name, new Cell(formula, new FormulaError("Circular Dependency caught.")));
                return new List<string>();
            }
        }

        protected override IEnumerable<string> GetDirectDependents(string name)
        {
            name = Normalize(name);
            return cellMap.GetDependents(name);
        }

        private class Cell
        {
            private Object content;
            private Object value;
            public Cell()
            {
                content = "";
                value = "";
            }

            public Cell(string c)
            {
                content = c;
                double numVal;
                if (double.TryParse(c, out numVal))
                {
                    value = numVal;
                } 
                else
                {
                    value = c;
                }
            }

            public Cell(double c)
            {
                content = c;
                value = c;
            }

            public Cell(Formula c, Object evaluated)
            {
                content = c;
                double numVal;
                if (double.TryParse(evaluated.ToString(), out numVal))
                {
                    value = numVal;
                }
                else
                {
                    value = evaluated;
                }
            }

            public Cell(string c, string e)
            {
                content = c;
                value = e;
            }

            public Object getContents()
            {
                return content;
            }

            public Object getValue()
            {
                return value;
            }
        }

        private bool validCellName(String name)
        {
            if (!Regex.IsMatch(name, @"^[a-zA-Z]+[0-9]+$")) 
            {
                return false;
            }
            if (!IsValid(name))
            {
                return false;
            }
            return true;
        }

        private void UpdateCells(IList<string> list)
        {
            foreach (string name in list)
            {
                Cell cell;
                if (totalCells.TryGetValue(name, out cell))
                {
                    if (cell.getContents() is Formula)
                    {
                        Formula newForm = new Formula(cell.getContents().ToString());
                        totalCells[name] = EvaluateFormula(newForm);
                    }
                }
            }
        }


        //Helper method that is the lookup for formula evaluate method, looking for values from spreadsheet.
        private Cell EvaluateFormula(Formula newForm)
        {
            try
            {
                return new Cell((newForm), (Object)newForm.Evaluate(s => (double)totalCells[s].getValue()));
            }
            catch
            {
                return new Cell((newForm), (Object)new FormulaError("Variable not defined."));
            }
        }
    }
}

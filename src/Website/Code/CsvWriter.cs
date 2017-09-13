using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Diagnostics;
using System.IO;

namespace Csg.IO.TextData
{
    /// <summary>
    /// Writes data to a text file with a delimiter.
    /// </summary>
    public class CsvWriter
    {
        private char pDelimiter;
        private char pQuote;
        private string[] pFields;
        private string[] pHeaderNames;
        private TextWriter pWriter;
        private Hashtable pValues;
        private bool pEnableHeader;
        private bool pHeaderOutput;
        private bool pAlwaysQuote;

        private bool pStripNewline;

        /// <summary>
        /// Gets or sets a value that indicates if CrLf should be removed from values.
        /// </summary>
        public bool StripNewline
        {
            get { return pStripNewline; }
            set { pStripNewline = value; }
        }

        /// <summary>
        /// When set to true, encloses all fields in quote characters
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool AlwaysQuote
        {
            get { return pAlwaysQuote; }
            set { pAlwaysQuote = value; }
        }

        /// <summary>
        /// Gets or sets the character used to delimit fields in the output
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char Delimiter
        {
            get { return pDelimiter; }
            set { pDelimiter = value; }
        }

        /// <summary>
        /// Gets or sets the character used to enclose fields containing the delimiter character
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public char Quote
        {
            get { return pQuote; }
            set { pQuote = value; }
        }

        /// <summary>
        /// When set to true, the first output line will be a header containing a list of field names
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public bool EnableHeader
        {
            get { return pEnableHeader; }
            set { pEnableHeader = value; }
        }

        /// <summary>
        /// Gets or sets the list of fields included in the output
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[] Fields
        {
            get { return pFields; }
            set { pFields = value; }
        }

        /// <summary>
        /// Gets or sets the header names of each field
        /// </summary>
        /// <value></value>
        /// <returns></returns>
        /// <remarks></remarks>
        public string[] HeaderNames
        {
            get { return pHeaderNames; }
            set { pHeaderNames = value; }
        }

        /// <summary>
        /// Gets the current row's values.
        /// </summary>
        public Hashtable Values
        {
            get { return pValues; }
        }

        /// <summary>
        /// Returns a string representing this CsvWriter.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// If the CsvWriter was constructed without a stream or file, this method will return the
        /// CSV data.
        /// </remarks>
        public override string ToString()
        {
            if (pWriter is StringWriter)
            {
                return pWriter.ToString();                
            }
            else
            {
                return this.GetType().Name;
            }
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be stored in memory.
        /// </summary>
        /// <remarks></remarks>
        public CsvWriter()
            : this(new StringWriter(), null)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be stored in memory.
        /// </summary>
        /// <param name="Fields">Array of field names</param>
        /// <remarks></remarks>
        public CsvWriter(string[] Fields)
            : this(new StringWriter(), Fields)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified stream.
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <remarks></remarks>
        public CsvWriter(Stream stream)
            : this(new StreamWriter(stream), null)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified stream.
        /// </summary>
        /// <param name="stream">Destination stream</param>
        /// <param name="Fields">Array of field names</param>
        /// <remarks></remarks>
        public CsvWriter(Stream stream, string[] Fields)
            : this(new StreamWriter(stream), Fields)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified file.
        /// </summary>
        /// <param name="Path">Destination filename</param>
        /// <remarks>The specified file will be overwritten if it already exists.</remarks>
        public CsvWriter(string Path)
            : this(new StreamWriter(Path), null)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified file.
        /// </summary>
        /// <param name="Path">Destination filename</param>
        /// <param name="Fields">Array of field names</param>
        /// <remarks>The specified file will be overwritten if it already exists.</remarks>
        public CsvWriter(string Path, string[] Fields)
            : this(new StreamWriter(Path), Fields)
        {
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified file with the specified encoding.
        /// </summary>
        /// <param name="Path">Destination filename</param>
        /// <param name="encoding">Text encoding to use for output file</param>
        /// <remarks>The specified file will be overwritten if it already exists.</remarks>
        public CsvWriter(string Path, System.Text.Encoding encoding)
            : this(new StreamWriter(Path, false, encoding), null)
        {
        }

        /// <summary>
        /// Causes buffered data to be written to the underlying stream.
        /// </summary>
        /// <remarks></remarks>
        public void Flush()
        {
            pWriter.Flush();
        }

        /// <summary>
        /// Forces the header line to be output to the underlying stream.
        /// </summary>
        /// <remarks></remarks>
        public void WriteHeader()
        {
            pHeaderOutput = true;
            if (this.HeaderNames != null && this.HeaderNames.Length > 0)
            {
                WriteLine(this.HeaderNames);
            }
            else
            {
                WriteLine(pFields);
            }
        }

        /// <summary>
        /// Initializes a new CsvWriter class.  Output data will be written to the specified TextWriter.
        /// </summary>
        /// <param name="Writer">Destination TextWriter</param>
        /// <param name="Fields">Array of field names</param>
        /// <remarks></remarks>
        public CsvWriter(TextWriter Writer, string[] Fields)
        {
            pEnableHeader = true;
            pDelimiter = ',';
            pQuote = '"';
            pWriter = Writer;
            pValues = new Hashtable();
            pHeaderOutput = false;

            if ((Fields != null))
            {
                pFields = Fields;
            }
        }

        private string QuoteField(string field)
        {
            string s = field;

            if (s == null)
            {
                s = string.Empty;
            }

            if (pStripNewline)
            {
                s = s.Replace("\r\n", " ");
                s = s.Replace("\r", " ");
                s = s.Replace("\n", " ");
            }
            
            s = s.Replace(pQuote.ToString(), pQuote.ToString() + pQuote.ToString());
            if (AlwaysQuote || s.Contains(pDelimiter) || s.Contains("\r") || s.Contains("\n"))
            {
                s = pQuote + s + pQuote;
            }

            return s;
        }

        /// <summary>
        /// Writes fields to the underlying stream in the order they appear in the array.
        /// </summary>
        /// <param name="fields"></param>
        /// <remarks>Fields are written in the order the same order as they exist in the array.</remarks>
        public void WriteLine(string[] fields)
        {
            string[] local = null;
            string s = null;

            if (pEnableHeader & !pHeaderOutput)
            {
                WriteHeader();
            }

            local = fields;
            for (int i = 0; i <= local.GetLength(0) - 1; i++)
            {
                local[i] = QuoteField(local[i]);
            }

            s = string.Join(pDelimiter.ToString(), local);

            pWriter.WriteLine(s);
        }

        /// <summary>
        /// Writes values in an IDataRecord to the underlying stream.
        /// </summary>
        /// <param name="values">Values to write</param>
        /// <remarks></remarks>
        public void WriteLine(IDataRecord values)
        {
            List<string> a = new List<string>();
            int i = 0;

            if (pFields == null)
            {
                throw new Exception("Fields have not been initialized");
            }

            foreach (string s in pFields)
            {
                i = values.GetOrdinal(s);
                if (values.IsDBNull(i))
                {
                    a.Add("");
                }
                else
                {
                    a.Add(values.GetValue(i).ToString());
                }
            }

            WriteLine(a.ToArray());
        }

        /// <summary>
        /// Writes values in a DataRow to the underlying stream.
        /// </summary>
        /// <param name="values">Values to write</param>
        /// <remarks></remarks>
        public void WriteLine(DataRow values)
        {
            List<string> a = new List<string>();

            if (Fields == null)
            {
                throw new Exception("Fields have not been initialized");
            }

            foreach (string s in pFields)
            {
                if (values.IsNull(s))
                {
                    a.Add("");
                }
                else
                {
                    a.Add(values[s].ToString());
                }
            }

            WriteLine(a.ToArray());
        }

        /// <summary>
        /// Writes values in an IDictionay to the underlying stream
        /// </summary>
        /// <param name="values"></param>
        /// <remarks></remarks>
        public void WriteLine(IDictionary values)
        {
            List<string> a = new List<string>();

            if (Fields == null)
            {
                throw new Exception("Fields have not been initialized");
            }

            foreach (string s in pFields)
            {
                if (values.Contains(s))
                {
                    a.Add(values[s].ToString());
                }
                else
                {
                    a.Add("");
                }
            }

            WriteLine(a.ToArray());
        }

        /// <summary>
        /// Writes the values set in the <code>Values</code> property to the underlying stream
        /// </summary>
        /// <remarks></remarks>
        public void WriteLine()
        {
            WriteLine(pValues);
        }

        /// <summary>
        /// Closes the underlying stream.
        /// </summary>
        /// <remarks></remarks>
        public void Close()
        {
            pWriter.Close();
        }

    }
}
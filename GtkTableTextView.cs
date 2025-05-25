// GtkTableTextView.cs
/*
	It makes it easier to create a table of just text.
*/


using System.Collections.ObjectModel;


namespace GtkUtil;


/// <summary>
/// The TableView class offers a simplification of TreeView for
/// tables of text elements
/// </summary>
public class TableTextView
{
	/// <summary>
	/// Delegate used to signal to a method that the table has changed.
	/// </summary>
	public delegate void TableChangedDelegate(int row, int col, string value);

	/// <summary>
	/// Set this attribute to the method you want to have called when a cell changes.
	/// </summary>
	public TableChangedDelegate TableChanged = (a1, a2, a3) => {};

    /// <summary>
    /// It acts as a wrapper for average TreeView's
    /// </summary>
    /// <param name="tv">
    /// A <see cref="Gtk.TreeView"/> this object will wrap.
    /// </param>
    /// <param name="numCols">
    /// A <see cref="int"/> holding the number of columns.
    /// </param>
    public TableTextView(Gtk.TreeView tv, int numCols)
		:this(tv, numCols,
				(bool[])  Enumerable.Repeat( false, 1 )
							.Concat( Enumerable.Repeat( true, numCols ) )  )
	{
	}

	/// <summary>
	/// It acts as a wrapper for average TreeView's
	/// </summary>
	/// <param name="tv">
	/// A <see cref="Gtk.TreeView"/> this object will wrap.
	/// </param>
	/// <param name="numCols">
	/// A <see cref="System.Int32"/> holding the number of columns.
	/// </param>
	/// <param name="numFirstColsNotEditable">
	/// A <see cref="System.Int32"/> holding the number of beginning columns that are not editable.
	/// </param>
	public TableTextView(Gtk.TreeView tv, int numCols, int numFirstColsNotEditable)
		: this( tv, numCols,
				(bool[])Enumerable.Repeat(
									false, numFirstColsNotEditable )
							.Concat( Enumerable.Repeat( true, numCols ) ) )
	{
	}

	/// <summary>
	/// It acts as a wrapper for average TreeView's. Allows to determine whether columns are editable or not.
	/// </summary>
	/// <param name="tv">
	/// A <see cref="Gtk.TreeView"/> this object will wrap.
	/// </param>
	/// <param name="numCols">
	/// A <see cref="System.Int32"/> holding the number of columns.
	/// </param>
	/// <param name="mutability">
	/// A <see cref="System.Boolean"/> holding the mutability of each column.
	/// </param>
	public TableTextView(Gtk.TreeView tv, int numCols, bool[] mutability)
	{
		// Chk
		if ( this.NumCols < 1 ) {
			throw new ArgumentException( "number of cols must be > 0" );
		}

		if ( mutability.Length > numCols) {
			throw new ArgumentException( nameof(numCols)
					+ " < length of " + nameof( mutability ) );
		}

		this.NumCols = numCols;
		this.tvTable = tv;
		this.headers = new string[ this.NumCols ];

		// Create
		this.tvTable.Model = this.NewModel();

		// Delete existing columns
		while ( tvTable.Columns.Length > 0 ) {
			tvTable.RemoveColumn( tvTable.Columns[ 0 ] );
		}

		// Create index column
		var column = new Gtk.TreeViewColumn { Title = "#" };
		var cellRenderer = new Gtk.CellRendererText();
		column.PackStart( cellRenderer, true );

		if ( !mutability[ 0 ] ) {
			cellRenderer.Editable = false;
			cellRenderer.Foreground = "black";
			cellRenderer.Background = "light gray";
		} else {
			cellRenderer.Editable = true;
		}

		column.AddAttribute( cellRenderer, "text", 0 );
		tvTable.AppendColumn( column );

		// Add columns
		for (int colNum = 0; colNum < this.NumCols - 1; ++colNum) {
			column = new Gtk.TreeViewColumn { Title = this.Headers[colNum] };
			column.Sizing = Gtk.TreeViewColumnSizing.Autosize;
			cellRenderer = new Gtk.CellRendererText();
			column.PackStart( cellRenderer, true );

			cellRenderer.Editable = mutability[colNum + 1];
			if ( !cellRenderer.Editable ) {
				cellRenderer.Foreground = "black";
				cellRenderer.Background = "light gray";
			}

			column.AddAttribute( cellRenderer, "text", colNum + 1 );
			cellRenderer.Edited += OnCellEdited;
			tvTable.AppendColumn(column);
		}
	}

	/// <summary>
	/// Creates a new model, suitable for the TreeView.
	/// </summary>
	/// <returns>The model, a Gtk.ListStore.</returns>
	protected Gtk.ListStore NewModel()
	{
		return new Gtk.ListStore(
					Enumerable.Repeat( typeof( string ), this.NumCols ).ToArray());
	}

	/// <summary>
	/// Returns whether a given column is editabel or not.
	/// </summary>
	/// <param name="numCol">
	/// A <see cref="System.Int32"/> holding the column number.
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/> which is true if editable; false otherwise.
	/// </returns>
	public bool IsEditable(int numCol)
	{
		if ( numCol < 0
		  || numCol > this.NumCols)
		{
			string ncName = nameof( numCol );
			throw new ArgumentException( ncName
										+ " must be: 0 < "
										+ ncName + " < "
										+ this.NumCols );
		}

		return ( (Gtk.CellRendererText) this.tvTable.Columns[ numCol ].Cells[ 0 ] ).Editable;
	}

	/// <summary>
	/// Sets the mutability of a given column.
	/// </summary>
	/// <param name="numCol">
	/// A <see cref="System.Int32"/> holding the number of the column.
	/// </param>
	/// <param name="editable">
	/// A <see cref="System.Boolean"/> holding true when the column is editable; false otherwise.
	/// </param>
	public void SetEditable(int numCol, bool editable)
	{
		if ( numCol < 0
		  || numCol > this.NumCols )
		{
			string ncName = nameof( numCol );
			throw new ArgumentException( ncName
										+ " must be: 0 < "
										+ ncName + " < "
										+ this.NumCols );
		}

		var cellRenderer = (Gtk.CellRendererText) this.tvTable.Columns[ numCol ].Cells[ 0 ];
		cellRenderer.Editable = editable;

		if (!editable)
		{
			cellRenderer.Foreground = "black";
			cellRenderer.Background = "light gray";
		}
		else
		{
			cellRenderer.Foreground = "black";
			cellRenderer.Background = "white";
		}
	}

	/// <summary>
	/// Returns whether a given column is visible or not
	/// </summary>
	/// <param name="numCol">
	/// A <see cref="System.Int32"/> holding the column number.
	/// </param>
	/// <returns>
	/// A <see cref="System.Boolean"/> true when the volumn is visible, false otherwise.
	/// </returns>
	public bool IsVisible(int numCol)
	{
		if (numCol >= this.NumCols
			|| numCol < 0)
		{
			throw new ArgumentException("param numCol must be: 0 < numCol < " + this.NumCols);
		}

		return this.tvTable.Columns[numCol].Visible;
	}

	/// <summary>
	/// Sets a column visible.
	/// </summary>
	/// <param name="numCol">
	/// A <see cref="System.Int32"/> holding the column number
	/// </param>
	/// <param name="visible">
	/// A <see cref="System.Boolean"/> holding whether that column will be visible or not.
	/// </param>
	public void SetVisible(int numCol, bool visible)
	{
		if (numCol >= this.NumCols
			|| numCol < 0)
		{
			throw new ArgumentException("param numCol must be: 0 < numCol < " + this.NumCols);
		}

		this.tvTable.Columns[numCol].Visible = visible;
	}

	/// <summary>
	/// Set the contents of the tvTable
	/// </summary>
	/// <param name="row">
	/// A <see cref="System.Int32"/> with the row number of the cell to set
	/// </param>
	/// <param name="col"> with the column number of the cell to set
	/// A <see cref="System.Int32"/>
	/// </param>
	/// <param name="value">
	/// A <see cref="System.String"/> with the value of the cell to set
	/// </param>
	public void Set(int row, int col, string value)
	{
		var table = (Gtk.ListStore)this.tvTable.Model;

		// Chk
		if (row < 0
			|| row >= this.NumRows)
		{
			throw new ArgumentException("invalid row to set: " + row, nameof(row));
		}

		if (col < 0
			|| col >= this.NumCols)
		{
			throw new ArgumentException("invalid column to set: " + col, nameof(col));
		}

		// Find place
		Gtk.TreeIter itRow;
		table.GetIter(out itRow, new Gtk.TreePath(new[] { row }));

		// Set
		table.SetValue(itRow, col, value);
	}

	/// <summary>
	/// Sets the given values for the row at the given row index.
	/// </summary>
	/// <param name="row">The row index.</param>
	/// <param name="values">The values to set, as a string array.</param>
	public void SetRow(int row, string[] values)
	{
		var table = (Gtk.ListStore)this.tvTable.Model;

		// Chk
		if (row < 0
			|| row >= this.NumRows)
		{
			throw new ArgumentException("invalid row to set: " + row, nameof(row));
		}

		if (values.Length != this.NumCols)
		{
			throw new ArgumentException("invalid number of column values to set: "
											+ values.Length + " / " + this.NumCols);
		}

		// Find place
		Gtk.TreeIter itRow;
		table.GetIter(out itRow, new Gtk.TreePath(new[] { row }));

		// Set
		for (int i = 0; i < values.Length; ++i)
		{
			table.SetValue(itRow, i, values[i]);
		}
	}


	/// <summary>
	/// Get the value from row and column.
	/// </summary>
	/// <param name="row">
	/// A <see cref="System.Int32"/> holding the number of the row.
	/// </param>
	/// <param name="col">
	/// A <see cref="System.Int32"/> holding the number of the column.
	/// </param>
	/// <returns>
	/// A <see cref="System.String"/> with the value stored in that cell.
	/// </returns>
	public string Get(int row, int col)
	{
		var table = (Gtk.ListStore)this.tvTable.Model;

		// Chk
		if (row < 0
			|| row >= this.NumRows
			|| col < 1
			|| col >= this.NumCols)
		{
			throw new ArgumentException("invalid row or column index to get value from");
		}

		// Find place
		Gtk.TreeIter itRow;
		table.GetIter(out itRow, new Gtk.TreePath(new[] { row }));

		// Get
		return (string)table.GetValue(itRow, col);
	}

	/// <summary>
	/// Gets the row of given index.
	/// </summary>
	/// <returns>The row values, as a vector of strings.</returns>
	/// <param name="row">The row index.</param>
	public string[] GetRow(int row)
	{
		var toret = new string[this.NumCols];
		var table = (Gtk.ListStore)this.tvTable.Model;

		// Chk
		if (row < 0
			|| row >= this.NumRows)
		{
			throw new ArgumentException("invalid row index to get values from");
		}

		// Find place
		Gtk.TreeIter itRow;
		table.GetIter(out itRow, new Gtk.TreePath(new[] { row }));

		// Populate return vector with table's values
		for (int i = 0; i < toret.Length; ++i)
		{
			toret[i] = (string)table.GetValue(itRow, i);
		}

		return toret;
	}

	/// <summary>
	/// Appends a new row to the table, at the end.
	/// By default, the first column will hold the row number.
	/// </summary>
	public void AppendRow()
	{
		var table = (Gtk.ListStore)this.tvTable.Model;
		var values = new string[this.NumCols];
		values[0] = (this.NumRows + 1).ToString();

		table.AppendValues(values);
	}

	/// <summary>
	/// Updates the headers
	/// </summary>
	public void UpdateHeaders()
	{
		// Chk
		if (this.Headers.Count < tvTable.Columns.Length)
		{
			throw new ArgumentException("insufficient number of headers for available columns");
		}

		int i = 0;
		foreach (var c in this.tvTable.Columns)
		{
			c.Title = this.Headers[i];
			++i;
		}
	}

	private void OnCellEdited(object sender, Gtk.EditedArgs args)
	{
		int row;
		int col;

		// Get current position
		Gtk.TreePath rowPath;

		// Convert path in row and rowPointer
		rowPath = new Gtk.TreePath(args.Path);
		tvTable.Model.GetIter(out Gtk.TreeIter rowPointer, rowPath);
		row = rowPath.Indices[0];

		// Find out the column order
		tvTable.GetCursor(out rowPath, out Gtk.TreeViewColumn colPath);
		for (col = 0; col < tvTable.Columns.Length; ++col)
		{
			if (tvTable.Columns[col] == colPath)
			{
				break;
			}
		}

		// Store data
		tvTable.Model.SetValue(rowPointer, col, args.NewText);

		if (TableChanged != null)
		{
			TableChanged(row, col, args.NewText);
		}
	}

	/// <summary>
	/// Returns the current selected cell in the table.
	/// </summary>
	/// <param name="row">
	/// A <see cref="System.Int32"/> will hold the row number.
	/// </param>
	/// <param name="col">
	/// A <see cref="System.Int32"/> will hold the column number.
	/// </param>
	public void GetCurrentCell(out int row, out int col)
	{
		// Convert path in row and rowPointer
		tvTable.GetCursor(out Gtk.TreePath rowPath,
							out Gtk.TreeViewColumn colPath);

		if (rowPath != null
			&& colPath != null)
		{
			tvTable.Model.GetIter(out Gtk.TreeIter rowPointer, rowPath);
			row = rowPath.Indices[0];

			// Find out the column order
			for (col = 0; col < tvTable.Columns.Length; ++col)
			{
				if (tvTable.Columns[col] == colPath)
				{
					break;
				}
			}

			// Adapt column from UI
			col = Math.Max(0, col - 1);
		}
		else
		{
			row = col = 0;
		}
	}

	/// <summary>
	/// Sets the current selected cell in the table.
	/// </summary>
	/// <param name="row">
	/// A <see cref="System.Int32"/> holding the row number.
	/// </param>
	/// <param name="col">
	/// A <see cref="System.Int32"/> holding the column number.
	/// </param>
	public void SetCurrentCell(int row, int col)
	{
		Gtk.TreeViewColumn colPath;
		Gtk.TreePath rowPath;

		// Chk
		if (row < 0
			|| row >= this.NumRows)
		{
			throw new ArgumentException("parameter row should be 0 < row < " + this.NumRows);
		}

		if (col < 0
			|| col >= this.NumCols)
		{
			throw new ArgumentException("parameter row should be 0 < row < " + this.NumCols);
		}

		// Find out the column order
		colPath = tvTable.Columns[col];

		// Find out the row number
		rowPath = new Gtk.TreePath(new[] { row });

		// Set the cursor
		this.tvTable.ScrollToCell(
			rowPath,
			colPath,
			false,
			(float)0.0,
			(float)0.0
		);
		tvTable.SetCursor(rowPath, colPath, false);
		tvTable.GrabFocus();
		return;
	}

	/// <summary>
	/// Remove all rows in the table
	/// </summary>
	public void RemoveAllRows()
	{
		((Gtk.ListStore)this.tvTable.Model).Clear();
	}

	/// <summary>
	/// Get the number of rows in the table
	/// </summary>
	public int NumRows
	{
		get { return this.tvTable.Model.IterNChildren(); }
	}

	/// <summary>
	/// Get the number of columns in the table
	/// </summary>
	public int NumCols
	{
		get; private set;
	}

	/// <summary>
	/// Get and modify the headers in the document
	/// </summary>
	public ReadOnlyCollection<string> Headers
	{
		get { return new ReadOnlyCollection<string>(headers); }
		set
		{
			this.headers = new string[value.Count];
			value.CopyTo(headers, 0);
			this.UpdateHeaders();
		}
	}

	private readonly Gtk.TreeView tvTable;
	private string[] headers;
}

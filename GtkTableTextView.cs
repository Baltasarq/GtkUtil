using System;
using System.Collections.ObjectModel;
using Gtk;

namespace GtkUtil {
	
	/// <summary>
	/// The TableView class offers a simplification of TreeView for
	/// tables of text elements
	/// </summary>
	public class TableTextView {
		private TreeView tvTable;
		private int numCols;
		private string[] headers;
		
		/// <summary>
		/// Delegate used to signal to a method that the table has changed.
		/// </summary>
		public delegate void TableChangedDelegate(int row, int col, string value);
		
		/// <summary>
		/// Set this attribute to the method you want to have called when a cell changes. 
		/// </summary>
		public TableChangedDelegate tableChanged = null;
		
		/// <summary>
		/// Get the number of columns in the table
		/// </summary>
		public int NumCols {
			get { return this.numCols; } 
		}
		
		/// <summary>
		/// Get the number of rows in the table
		/// </summary>
		public int NumRows {
			get { return this.tvTable.Model.IterNChildren(); } 
		}
		
		/// <summary>
		/// Get and modify the headers in the document
		/// </summary>
		public ReadOnlyCollection<string> Headers {
			get { return new ReadOnlyCollection<string>( headers ); }
			set {
				this.headers = new string[ value.Count ];
				value.CopyTo( headers, 0 );
				this.UpdateHeaders();
			}
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
			  || numCol > this.NumCols )
			{
				throw new ArgumentException( "numCol must be 0 < numCol < " + this.NumCols.ToString() );
			}
			
			return ( (Gtk.CellRendererText) this.tvTable.Columns[ numCol ].CellRenderers[ 0 ] ).Editable;
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
				throw new ArgumentException( "numCol must be 0 < numCol < " + this.NumCols.ToString() );
			}
			
			var cellRenderer = ( (Gtk.CellRendererText) this.tvTable.Columns[ numCol ].CellRenderers[ 0 ] );
			cellRenderer.Editable = editable;
			
			if ( !editable ) {
				cellRenderer.Foreground = "black";
				cellRenderer.Background = "light gray";
			} else {
				cellRenderer.Foreground = "black";
				cellRenderer.Background = "white";
			}
		}
		
		/// <summary>
		/// It acts as a wrapper for average TreeView's
		/// </summary>
		/// <param name="tv">
		/// A <see cref="TreeView"/> this object will wrap.
		/// </param>
		/// <param name="numCols">
		/// A <see cref="System.Int32"/> holding the number of columns.
		/// </param>
		public TableTextView(TreeView tv, int numCols)
		{
			// Prepare mutability
			var mutability = new bool[ numCols ];
			mutability[ 0 ] = false;
			for(int i = 1; i < numCols; ++i) {
				mutability[ i ] = true;
			}
			
			this.Init( tv, numCols, mutability );
		}
		
		/// <summary>
		/// It acts as a wrapper for average TreeView's
		/// </summary>
		/// <param name="tv">
		/// A <see cref="TreeView"/> this object will wrap.
		/// </param>
		/// <param name="numCols">
		/// A <see cref="System.Int32"/> holding the number of columns.
		/// </param>
		/// <param name="numFirstColsNotEditable">
		/// A <see cref="System.Int32"/> holding the number of beginning columns that are not editable.
		/// </param>
		public TableTextView(TreeView tv, int numCols, int numFirstColsNotEditable)
		{
			// Chk
			if ( numFirstColsNotEditable > numCols ) {
				throw new ArgumentException(
				      "param numFirstColsNotEditable must be: 1 < numFirstColsNotEditable < numCol"
				);
			}
			
			// Prepare mutability
			var mutability = new bool[ numCols ];
			
			for(int i = 0; i < numFirstColsNotEditable; ++i) {         // Not editable columns
				mutability[ i ] = false;
			}
			
			for(int i = numFirstColsNotEditable; i < numCols; ++i) {   // Remaining columns
				mutability[ i ] = true;
			}
			
			this.Init( tv, numCols, mutability );
		}
		
		/// <summary>
		/// It acts as a wrapper for average TreeView's. Allows to determine whether columns are editable or not.
		/// </summary>
		/// <param name="tv">
		/// A <see cref="TreeView"/> this object will wrap.
		/// </param>
		/// <param name="numCols">
		/// A <see cref="System.Int32"/> holding the number of columns.
		/// </param>
		/// <param name="mutability">
		/// A <see cref="System.Boolean"/> holding the mutability of each column.
		/// </param>
		public TableTextView(TreeView tv, int numCols, bool[] mutability)
		{
			this.Init( tv, numCols, mutability );
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
			if ( numCol >= this.NumCols
			  || numCol < 0 )
			{
				throw new ArgumentException( "param numCol must be: 0 < numCol < " + this.NumCols.ToString() );
			}
			
			return this.tvTable.Columns[ numCol ].Visible;
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
			if ( numCol >= this.NumCols
			  || numCol < 0 )
			{
				throw new ArgumentException( "param numCol must be: 0 < numCol < " + this.NumCols.ToString() );
			}
			
			this.tvTable.Columns[ numCol ].Visible = visible;
		}

		/// <summary>
		/// Creates the number of columns needed.
		/// </summary>
		/// <param name="tv">
		/// A <see cref="Gtk.TreeView"/> that will be the base.
		/// </param>
		/// <param name="numCols">
		/// A <see cref="System.Int32"/> holding the number of columns.
		/// </param>
		/// <param name="mutability">
		/// A <see cref="System.Array"/> holding the mutability of columns.
		/// </param>
		protected void Init(Gtk.TreeView tv, int numCols, bool[] mutability)
		{
			this.numCols = numCols;
			this.tvTable = tv;
			this.headers = new string[ this.NumCols ];

			// Chk
			if ( this.NumCols < 1 ) {
				throw new ArgumentException( "number of cols must be > 0" );
			}
			
			// Create
			var types = new System.Type[ this.NumCols ];
			for(int i = 0; i < this.NumCols; ++i)
			{
				types[ i ] = typeof( string );
			}
			
			Gtk.ListStore listStore = new Gtk.ListStore( types ); 
			tvTable.Model = listStore;
			
			// Delete existing columns
			while(tvTable.Columns.Length > 0) {	
				tvTable.RemoveColumn( tvTable.Columns[ 0 ] );
			}
			
			// Create index column
			var column = new Gtk.TreeViewColumn();
			var cell = new Gtk.CellRendererText();
			column.Title = "#";
			column.PackStart( cell, true );
			
			if ( !mutability[ 0 ] ) {
				cell.Editable = false;
				cell.Foreground = "black";
				cell.Background = "light gray";
			} else {
				cell.Editable = true;
			}
			
			column.AddAttribute( cell, "text", 0 );
			tvTable.AppendColumn( column );
			
			// Add columns
			for(int colNum = 0; colNum < this.NumCols -1; ++colNum)
			{
				column = new Gtk.TreeViewColumn();
				cell = new Gtk.CellRendererText();
				column.Title = this.Headers[ colNum ];
				column.PackStart( cell, true );
				
				cell.Editable = mutability[ colNum +1 ];
				if ( !mutability[ colNum +1 ] ) {
					cell.Foreground = "black";
					cell.Background = "light gray";
				}
				
				column.AddAttribute( cell, "text", colNum + 1 );
				cell.Edited += OnCellEdited;
				tvTable.AppendColumn( column );
			}
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
			var table = (Gtk.ListStore) this.tvTable.Model;
			
			// Chk
			if( row < 0
			 || row >= this.NumRows )
			{
				throw new ArgumentException( "invalid row to set: " + row.ToString(), "row" );
			}
			
			if( col < 0
			 || col >= this.NumCols )
			{
				throw new ArgumentException( "invalid column to set: " + col.ToString(), "col" );
			}
			
			// Find place
			Gtk.TreeIter itRow = new Gtk.TreeIter();
			table.GetIter( out itRow, new TreePath( new int[] { row } ) );
			
			// Set
			table.SetValue( itRow, col, value );
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
			var table = (Gtk.ListStore) this.tvTable.Model;
			
			// Chk
			if ( row < 1
			  || row >= this.NumRows
			  || col < 1
			  || col >= this.NumCols )
			{
				throw new ArgumentException( "invalid row or column to get value from" );
			}
			
			// Find place
			Gtk.TreeIter itRow = new Gtk.TreeIter();
			table.GetIter( out itRow, new TreePath( new int[] { row } ) );
			
			// Get
			return (string) table.GetValue( itRow, col );
		}
		
		/// <summary>
		/// Appends a new row to the table, at the end. By default, the first column will hold the row number.
		/// </summary>
		public void AppendRow()
		{
			var table = (Gtk.ListStore) this.tvTable.Model;
			var values = new string[ this.NumCols ];
			values[ 0 ] = ( this.NumRows +1 ).ToString();
			
			table.AppendValues( values );
		}
		
		/// <summary>
		/// Updates the headers
		/// </summary>
		public void UpdateHeaders()
		{
			// Chk
			if ( this.Headers.Count < tvTable.Columns.Length ) {
				throw new ArgumentException( "insufficient number of headers for available columns" );
			}
			
			int i = 0;
			foreach(var c in this.tvTable.Columns) {
				c.Title = this.Headers[ i ];
				++i;
			}
		}
		
		private void OnCellEdited(object sender, EditedArgs args)
		{
			int row;
			int col;
				
			// Get current position
			TreePath rowPath;
			TreeIter rowPointer;
			TreeViewColumn colPath;
			
			// Convert path in row and rowPointer
			rowPath = new Gtk.TreePath( args.Path );
			tvTable.Model.GetIter( out rowPointer, rowPath );
			row = rowPath.Indices[ 0 ];
			 		
			// Find out the column order
			tvTable.GetCursor( out rowPath, out colPath );
			for(col = 0; col < tvTable.Columns.Length; ++col) {
				if ( tvTable.Columns[ col ] == colPath ) {
									break;
				}
			}
			
			// Store data
			tvTable.Model.SetValue( rowPointer, col, args.NewText );
			
			if ( tableChanged != null ) {
				tableChanged( row, col, args.NewText );
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
			TreePath rowPath;
			TreeIter rowPointer;
			TreeViewColumn colPath;
		
			// Convert path in row and rowPointer
			tvTable.GetCursor( out rowPath, out colPath );
			
			if ( rowPath != null
		 	  && colPath != null )
			{
				tvTable.Model.GetIter( out rowPointer, rowPath );
				row = rowPath.Indices[ 0 ];
					
				// Find out the column order
				for(col = 0; col < tvTable.Columns.Length; ++col)
		 		{
					if ( tvTable.Columns[ col ] == colPath ) {
						break;
					}
				}
		
				// Adapt column from UI
				--col;
				if ( col < 0 ) {
					col = 0;
				}
			} else {
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
			TreeViewColumn colPath;
			TreePath rowPath;
			
			// Chk
			if ( row < 0
			  || row >= this.NumRows )
			{
				throw new ArgumentException( "parameter row should be 0 < row < " + this.NumRows.ToString() );
			}
			
			if ( col < 0
			  || col >= this.NumCols )
			{
				throw new ArgumentException( "parameter row should be 0 < row < " + this.NumCols.ToString() );
			}
			
			// Find out the column order
			colPath = tvTable.Columns[ col ];
			
			// Find out the row number
			rowPath = new TreePath( new int[]{ row } );

			// Set the cursor
			this.tvTable.ScrollToCell(
				rowPath,
				colPath,
				false,
				(float) 0.0,
				(float) 0.0
			);
			tvTable.SetCursor( rowPath, colPath, false );
			tvTable.GrabFocus();
			return;
		}
		
		/// <summary>
		/// Remove all rows in the table
		/// </summary>
		public void RemoveAllRows()
		{			
			( (Gtk.ListStore) this.tvTable.Model ).Clear();
		}
	}
}


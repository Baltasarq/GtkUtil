// Util.cs
/*
    Author: Baltasar García Perez-Schofield
    e.mail: jbgacia@uvigo.es
    License: LGPL

    This is a tool class that is intended to ease repetitive jobs.
*/


using System;
using System.IO;
using Gtk;
using System.Text;
using System.Collections;
using System.Collections.Generic;

namespace GtkUtil {
	/// <summary>
	/// The class Util is the main class providing all utility services. 
	/// </summary>
	public class Util {
		/// Minimum width for any possible application window
		public const int MinWidth = 300;
		
		/// Minimum height for any possible application window
		public const int MinHeight = 200;
		
		/// <summary>
		/// This label is used in the configuration file in order to mark
		/// the last file loaded.
		/// </summary>
		public const string EtqLastFile = "lastfile";
		
		/// <summary>
		/// This label is used to store the width of the window in the
		/// configuration file.
		/// </summary>
		public const string EtqWidth    = "width";
		
		/// <summary>
		/// This label is used to store the height of the window in the
		/// configuration file.
		/// </summary>
		public const string EtqHeight   = "height";
		
		/// <summary>
		/// This label is used to store a list of recent files.
		/// </summary>
		public const string EtqRecent   = "recentfiles";
		
		/// <summary>
		/// Retrieves the correct path for the configuration file.
		/// </summary>
		/// <param name="cfgFileName">
		/// A <see cref="System.String"/> containing the name of the file.
		/// </param>
		/// <returns>
		/// A <see cref="System.String"/> with the complete, combined path.
		/// </returns>
		public static string GetCfgCompletePath(string cfgFileName)
		{
			string toret = (Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX)
					? Environment.GetEnvironmentVariable( "HOME" )
					: Environment.ExpandEnvironmentVariables( "%HOMEDRIVE%%HOMEPATH%" );
			toret = System.IO.Path.Combine( toret, cfgFileName );
			
			return toret;
		}
		
		/// <summary>
		/// Opens an open dialog on screen and lets the user choose a file.
		/// </summary>
		/// <param name="backend">
		/// A <see cref="System.String"/> containing a message.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title of the window.
		/// </param>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent for this dialog.
		/// </param>
		/// <param name="fileName">
		/// A <see cref="System.String"/> the last filename open.
		/// </param>
		/// <param name="filter">
		/// A <see cref="System.String"/> containing the filter to use, in a "*.jpg" fashion.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> referring whether a file was chosen or not.
		/// </returns>
		static public bool DlgOpen(string backend, string title, Window main, ref string fileName, string filter) {
			return DlgSelectFile( backend, title, main, FileChooserAction.Open, ref fileName, filter );
		}

		/// <summary>
		/// Opens a save dialog on screen and lets the user choose a file name.
		/// </summary>
		/// <param name="backend">
		/// A <see cref="System.String"/> containing a message.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title of the window.
		/// </param>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent for this dialog.
		/// </param>
		/// <param name="fileName">
		/// A <see cref="System.String"/> the last filename open.
		/// </param>
		/// <param name="filter">
		/// A <see cref="System.String"/> containing the filter to use, in a "*.jpg" fashion.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> referring whether a file was chosen or not.
		/// </returns>
		static public bool DlgSave(string backend, string title, Window main, ref string fileName, string filter) {
			return DlgSelectFile( backend, title, main, FileChooserAction.Save, ref fileName, filter );
		}

		/// <summary>
		/// Opens an save folder dialog on screen and lets the user choose a folder.
		/// </summary>
		/// <param name="backend">
		/// A <see cref="System.String"/> containing a message.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title of the window.
		/// </param>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent for this dialog.
		/// </param>
		/// <param name="fileName">
		/// A <see cref="System.String"/> the last filename open.
		/// </param>
		/// <param name="filter">
		/// A <see cref="System.String"/> containing the filter to use, in a "*.jpg" fashion.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> referring whether a file was chosen or not.
		/// </returns>
		static public bool DlgSaveFolder(string backend, string title, Window main, ref string fileName, string filter) {
			return DlgSelectFile( backend, title, main, FileChooserAction.CreateFolder, ref fileName, filter );
		}

		/// <summary>
		/// Opens an open folder dialog on screen and lets the user choose a file.
		/// </summary>
		/// <param name="backend">
		/// A <see cref="System.String"/> containing a message.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title of the window.
		/// </param>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent for this dialog.
		/// </param>
		/// <param name="fileName">
		/// A <see cref="System.String"/> the last filename open.
		/// </param>
		/// <param name="filter">
		/// A <see cref="System.String"/> containing the filter to use, in a "*.jpg" fashion.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> referring whether a file was chosen or not.
		/// </returns>
		static public bool DlgSelectFolder(string backend, string title, Window main, ref string fileName, string filter) {
			return DlgSelectFile( backend, title, main, FileChooserAction.SelectFolder, ref fileName, filter );
		}

		static private bool DlgSelectFile(string back, string title, Window main, FileChooserAction action, ref string fileName, string filter)
		{
			var dlg = new FileChooserDialog(
			          			back, title, main, action,
			                                "Cancel", ResponseType.Cancel,
                                      		"Ok", ResponseType.Ok
			);

			var fileFilter = new  FileFilter();

			try {
				fileFilter.Name = filter.Substring ( filter.IndexOf( "." ) + 1 ) + " files";
			} catch(Exception) {
				fileFilter.Name = filter;
			}

			fileFilter.AddPattern( filter );
			dlg.AddFilter( fileFilter );
			dlg.SetFilename( fileName );
			ResponseType result = (ResponseType) dlg.Run();

			fileName = dlg.Filename;
			dlg.Destroy();
			return ( result == ResponseType.Ok );
		}

		/// <summary>
		/// Shows a message dialog on the screen, with an error message. 
		/// </summary>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent of this dialog.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title for the dialog.
		/// </param>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message.
		/// </param>
		static public void MsgError(Window main, string title, string msg) {
			ShowMsg( main, title, msg, Gtk.MessageType.Error );
		}

		/// <summary>
		/// Shows a message dialog on the screen, with an info message. 
		/// </summary>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent of this dialog.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title for the dialog.
		/// </param>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the message.
		/// </param>
		static public void MsgInfo(Window main, string title, string msg) {
			ShowMsg( main, title, msg, Gtk.MessageType.Info );
		}

		/// <summary>
		/// Asks for confirmation through a dialog on the screen. 
		/// </summary>
		/// <param name="main">
		/// A <see cref="Window"/> that will be the parent of the dialog.
		/// </param>
		/// <param name="title">
		/// A <see cref="System.String"/> containing the title of the dialog.
		/// </param>
		/// <param name="msg">
		/// A <see cref="System.String"/> containing the question to show.
		/// </param>
		/// <returns>
		/// A <see cref="System.Boolean"/> containing true if the user chose yes, false otherwise
		/// </returns>
		static public bool Ask(Window main, string title, string msg)
		{
			var dlg = new MessageDialog(
			              		main,
			                    Gtk.DialogFlags.Modal,
								Gtk.MessageType.Question,
			                    Gtk.ButtonsType.YesNo,
			                    title
			 );

			dlg.Text = msg;
			dlg.Title = title + " Question";

			ResponseType res = (ResponseType) dlg.Run();
			dlg.Destroy();

			return ( res == ResponseType.Yes );
		}


		static private void ShowMsg(Window main, string title, string txt, Gtk.MessageType type)
		{
			var dlg = new MessageDialog(
			              		main,
			                    Gtk.DialogFlags.Modal,
								type,
			                    Gtk.ButtonsType.Ok,
			                    title
			 );

			dlg.Text = txt;
			dlg.Title = title;

			if ( type == MessageType.Error )
					dlg.Title += " Error";
			else	dlg.Title += " Information";

			dlg.Run();
			dlg.Destroy();
		}

		/// <summary>
		/// Updates the user interface executing all pending messages.
		/// </summary>
		static public void UpdateUI()
		{
			while ( Gtk.Application.EventsPending() ) {
				Gtk.Application.RunIteration();
			}
		}
		
		/// <summary>
		/// Reads a configuration file for this application. 
		/// </summary>
		/// <param name="window">
		/// A <see cref="Gtk.Window"/> that is the main window of the application.
		/// </param>
		/// <param name="cfgFileName">
		/// A <see cref="System.String"/> the name of the configuration file.
		/// </param>
		/// <returns>
		/// A <see cref="System.Array"/> with the list of recently used files.
		/// </returns>
		public static string[] ReadConfiguration(Gtk.Window window, string cfgFileName)
			{
				string lastFileName = "";
				string[] recentFiles = new string[]{};
				List<string> files = null;
				int width;
				int height;
				StreamReader file = null;
				string line;
				
				window.GetSize( out width, out height );
				
				try {
					try {
						file = new StreamReader( cfgFileName );
					} catch(Exception) {
						return null;
					}
					
					line = file.ReadLine();
					while( !file.EndOfStream ) {
						if ( line.ToLower().StartsWith( EtqLastFile ) ) {
							int pos = line.IndexOf( '=' );
							
							if ( pos > 0 ) {
								lastFileName = line.Substring( pos + 1 ).Trim();
							}
						}
						else
						if ( line.ToLower().StartsWith( EtqWidth ) ) {
							int pos = line.IndexOf( '=' );
	
							if ( pos > 0 ) {
								width = System.Convert.ToInt32( line.Substring( pos + 1 ).Trim() );
							}
						}
						else
						if ( line.ToLower().StartsWith( EtqHeight ) ) {
							int pos = line.IndexOf( '=' );
	
							if ( pos > 0 ) {
								height = System.Convert.ToInt32( line.Substring( pos + 1 ).Trim() );
							}
						}
						else
						if ( line.ToLower().StartsWith( EtqRecent ) ) {
							int pos = line.IndexOf( '=' );
	
							if ( pos > 0 ) {
								recentFiles = line.Substring( pos + 1 ).Split( ',' );
							}
						}
							
							line = file.ReadLine();
						}
					
					file.Close();
					
					// Now apply cfg
					ApplyNewSize( window, width, height );
				
					// Create list of recent files, plus the last file
					files = new List<string>();
					files.Add( lastFileName );
					foreach(var f in recentFiles) {
						files.Add( f );
					}
				
					return files.ToArray();
				} catch(Exception exc)
				{
					Util.MsgError( window, window.Title, exc.Message );
				}
			
				return null;
			}
			
			/// <summary>
			/// Writes the configuration file for this application. 
			/// </summary>
			/// <param name="window">
			/// A <see cref="Gtk.Window"/> that is the main application's window
			/// </param>
			/// <param name="cfgFileName">
			/// A <see cref="System.String"/> containing the name of the config file.
			/// </param>
			/// <param name="recentFiles">
			/// A <see cref="System.Array"/> containing the recently used files.
			/// The first one, lastFiles[ 0 ] is the last file used by the application.
			/// </param>
			public static void WriteConfiguration(Gtk.Window window, string cfgFileName, string[] recentFiles)
			{
				int width;
				int height;
				string lastFile = "";
				StringBuilder recentFilesLine = null;
			
				// Prepare window size
				window.GetSize( out width, out height );
				
				// Prepare last file name. It is the first of the recent files.
				if ( recentFiles.Length > 0 ) {
					lastFile = recentFiles[ 0 ];
				}

				// Prepare last files - first one is the current open
				if ( recentFiles.Length > 1 ) {
					recentFilesLine = new StringBuilder();
				
					for(int i = 1; i < recentFiles.Length; ++i) {
						recentFilesLine.Append( recentFiles[ i ] );
						recentFilesLine.Append( ',' );
					}
				
					// Remove last ','
					recentFilesLine.Remove( recentFilesLine.Length -1, 1 );
				}
			
				// Write configuration
				try {
					var file = new StreamWriter( cfgFileName );
				
					// Write window size
					file.WriteLine( "{0}={1}", EtqWidth, width );
					file.WriteLine( "{0}={1}", EtqHeight, height );
				
					// Write last file name
					if ( lastFile.Length > 0 ) {
						file.WriteLine( "{0}={1}", EtqLastFile, lastFile );
					}

					// Write list of recent files
					if ( recentFilesLine != null ) {
						file.WriteLine( "{0}={1}", EtqRecent, recentFilesLine.ToString() );
					}

					file.WriteLine();
					file.Close();
				} catch(Exception exc)
				{
					Util.MsgError( window, window.Title, exc.Message );
				}
			
				return;
			}
		
			private static void ApplyNewSize(Gtk.Window window, int width, int height)
			{
				if ( width > MinWidth
				  && height > MinHeight )
				{
					window.Resize( width, height );
				}
			}
	}
}

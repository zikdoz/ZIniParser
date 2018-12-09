using System;
using System.Runtime.InteropServices;
using System.Text;

namespace ZUtility
{
	public class ZIniParser
	{
#region [ Import ]

		[ DllImport( "kernel32", CharSet = CharSet.Unicode ) ]
		private static extern int GetPrivateProfileString(
			string        section,
			string        key,
			string        default_value,
			StringBuilder value,
			int           size,
			string        file_path );

		[ DllImport( "kernel32.dll", CharSet = CharSet.Unicode ) ]
		private static extern int GetPrivateProfileString(
			string             section,
			string             key,
			string             default_value,
			[ In, Out ] char[] value,
			int                size,
			string             file_path );

		[ DllImport( "kernel32.dll", CharSet = CharSet.Auto ) ]
		private static extern int GetPrivateProfileSection(
			string section,
			IntPtr key_value,
			int    size,
			string file_path );

		[ DllImport( "kernel32", CharSet = CharSet.Unicode, SetLastError = true ) ]
		[ return: MarshalAs( UnmanagedType.Bool ) ]
		private static extern bool WritePrivateProfileString(
			string section,
			string key,
			string value,
			string file_path );

#endregion

		public static int buffer_capacity = 512;

#region [ Read ]

		public static string readValue(
			string section,
			string key,
			string file_path,
			string default_value = "" )
		{
			var value = new StringBuilder( buffer_capacity );

			GetPrivateProfileString( section, key, default_value, value, value.Capacity, file_path );

			return value.ToString();
		}

		public static string[] readSections(
			string file_path )
		{
			// first line will not recognize if ini file is saved in UTF-8 with BOM 
			while ( true )
			{
				var chars = new char[ buffer_capacity ];
				var size = GetPrivateProfileString( null, null, "", chars, buffer_capacity, file_path );

				if ( size == 0 )
				{
					return null;
				}

				if ( size < buffer_capacity - 2 )
				{
					var result = new string( chars, 0, size );
					var sections = result.Split( new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries );

					return sections;
				}

				buffer_capacity <<= 1; // *2
			}
		}

		public static string[] readKeys(
			string section,
			string file_path )
		{
			// first line will not recognize if ini file is saved in UTF-8 with BOM 
			while ( true )
			{
				var chars = new char[ buffer_capacity ];
				var size = GetPrivateProfileString( section, null, "", chars, buffer_capacity, file_path );

				if ( size == 0 )
				{
					return null;
				}

				if ( size < buffer_capacity - 2 )
				{
					var result = new string( chars, 0, size );
					var keys = result.Split( new[] { '\0' }, StringSplitOptions.RemoveEmptyEntries );

					return keys;
				}

				buffer_capacity <<= 1; // *2
			}
		}

		public static string[] readKeyValuePairs(
			string section,
			string file_path )
		{
			while ( true )
			{
				var returned_string = Marshal.AllocCoTaskMem( buffer_capacity * sizeof( char ) );
				var size = GetPrivateProfileSection( section, returned_string, buffer_capacity, file_path );

				if ( size == 0 )
				{
					Marshal.FreeCoTaskMem( returned_string );

					return null;
				}

				if ( size < buffer_capacity - 2 )
				{
					var result = Marshal.PtrToStringAuto( returned_string, size - 1 );

					Marshal.FreeCoTaskMem( returned_string );

					var key_value_pairs = result.Split( '\0' );

					return key_value_pairs;
				}

				Marshal.FreeCoTaskMem( returned_string );

				buffer_capacity <<= 1; // *2
			}
		}

#endregion

#region [ Write ]

		// Sections and keys will be created, if they not exist
		public static bool writeValue(
			string section,
			string key,
			string value,
			string file_path )
			=> WritePrivateProfileString( section, key, value, file_path );

#endregion

#region [ Delete ]

		public static bool deleteSection(
			string section,
			string file_path )
			=> WritePrivateProfileString( section, null, null, file_path );

		public static bool deleteKey(
			string section,
			string key,
			string file_path )
			=> WritePrivateProfileString( section, key, null, file_path );

#endregion
	}
}
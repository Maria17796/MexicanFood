using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LasMarias.Dataservice
{
	public class Person
	{

		//*************************************************************************************
		#region constants

		protected const string TABLE = "lasmarias.person";
		protected const string COLUMNS = "person_id, name, vorname, benutzer_name, rolle_zahl, rolle_text, geb_dat, passwort_hash";
		protected const string DEFAULTSELECT = "select " + COLUMNS + " from " + TABLE;

		#endregion

		//*************************************************************************************
		#region static methods

		public static string PasswortHashErstellen(string passwort)
		{
			if (String.IsNullOrEmpty(passwort)) return string.Empty;
			else
			{
				SHA512 sha = SHA512.Create();
				return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(passwort)));
			}
		}

		public static List<Person> GetList(NpgsqlConnection connection)
		{
			connection.Open();
			NpgsqlCommand command = new();
			command.Connection = connection;
			command.CommandText = $"{DEFAULTSELECT} order by name, vorname, geb_dat desc";
			NpgsqlDataReader reader = command.ExecuteReader();
			
			List<Person> list = new List<Person>();
			while (reader.Read()) list.Add(new Person(reader));
			reader.Close();
			connection.Close();
			return list;
		}

		public static Person Get( NpgsqlConnection connection , string benutzerName, string passwort)
		{
			connection.Open();
			NpgsqlCommand command = new NpgsqlCommand();
			command.Connection = connection;
			//command.CommandText = $"{DEFAULTSELECT} where benutzer_name = '{benutzerName}' and passwort_hash = '{Person.PasswortHashErstellen(passwort)}'";
			command.CommandText = $"{DEFAULTSELECT} where benutzer_name = :bn and passwort_hash = :pwd";
			command.Parameters.AddWithValue("bn", benutzerName);
			command.Parameters.AddWithValue("pwd", Person.PasswortHashErstellen(passwort));
			NpgsqlDataReader reader = command.ExecuteReader();
			Person person = null;
			if (reader.Read()) person = new Person(reader);
			reader.Close();
			connection.Close();
			return person;
		}


		#endregion


		#region constructors
		public Person()
		{

		}

		public Person(NpgsqlDataReader reader)
		{
			this.PersonId = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
			this.Name = reader.IsDBNull(1) ? (string)null : reader.GetString(1);
			this.Vorname = reader.IsDBNull(2) ? (string)null : reader.GetString(2);
			this.BenutzerName = reader.IsDBNull(3) ? (string)null : reader.GetString(3);
			this.RolleZahl = reader.IsDBNull(4) ? (int?)null : reader.GetInt32(4);
			this.RolleText = reader.IsDBNull(5) ? (string)null : reader.GetString(5);
			this.GebDat = reader.IsDBNull(6) ? (DateTime?)null : reader.GetDateTime(6);
			this.PasswortHash = reader.IsDBNull(7) ? (string)null : reader.GetString(7);
		}


		#endregion

		//*************************************************************************************
		#region properties

		[JsonPropertyName("personid")]

		public long? PersonId { get; set; }

		[JsonPropertyName("name")]
		public string Name { get; set; }

		[JsonPropertyName("vorname")]
		public string Vorname { get; set; }

        [JsonPropertyName("rolle")]
        public int? RolleZahl { get; set; }

        [JsonPropertyName("rolletext")]
        public string RolleText { get; set; }

        [JsonPropertyName("gebdat")]
        public DateTime? GebDat { get; set; }

        [JsonPropertyName("benutzername")]
        public string BenutzerName { get; set; }

		[JsonIgnore()]
        public string PasswortHash { get; set; }

		#endregion

		//*************************************************************************************
		#region public methods

		public int Save(NpgsqlConnection connection)
		{

			connection.Open();
			NpgsqlCommand command = new NpgsqlCommand();
			command.Connection = connection;

			if (this.PersonId.HasValue) // update
			{
				command.CommandText = $"update {TABLE} set name = :name, vorname = :vorname, benutzer_name = :benutzername, rolle_zahl = :rollezahl, rolle_text = :rolletext, geb_dat = :gebdat, passwort_hash = :passworthash where person_id = :personid";
			}
			else // insert
			{
				command.CommandText = $"select nextval('{TABLE}_seq')";
				this.PersonId = (long)command.ExecuteScalar();
				command.CommandText = $"insert into {TABLE} ({COLUMNS}) values (:personid, :name, :vorname, :benutzername, :rollezahl, :rolletext, :gebdat, :passworthash)";
			}

			command.Parameters.AddWithValue("personid", this.PersonId.Value);
			command.Parameters.AddWithValue("name", String.IsNullOrEmpty(this.Name) ? DBNull.Value : (object)this.Name);
			command.Parameters.AddWithValue("vorname", String.IsNullOrEmpty(this.Vorname) ? DBNull.Value : (object)this.Vorname);
			command.Parameters.AddWithValue("benutzername", String.IsNullOrEmpty(this.BenutzerName) ? DBNull.Value : (object)this.BenutzerName);
			command.Parameters.AddWithValue("rollezahl", this.RolleZahl.HasValue ? (object)this.RolleZahl.Value : DBNull.Value);
			command.Parameters.AddWithValue("rolletext", String.IsNullOrEmpty(this.RolleText) ? DBNull.Value : (object)this.RolleText);
			command.Parameters.AddWithValue("gebdat", this.GebDat.HasValue ? (object)this.GebDat.Value : DBNull.Value);
			command.Parameters.AddWithValue("passworthash", String.IsNullOrEmpty(this.PasswortHash) ? DBNull.Value : (object)this.PasswortHash);

			int r = 0;
			try
			{
				r = command.ExecuteNonQuery();
				return r;
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				connection.Close();
			}

		}



		#endregion





	}
}

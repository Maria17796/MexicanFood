using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LasMarias.Dataservice
{
	public class Datei
	{

		//*************************************************************************************
		#region constants

		protected const string TABLE = "lasmarias.datei";
		protected const string COLUMNS = "datei_id, artikel_id, person_id, name, erweiterung, inhalt, medien_typ";
		protected const string DEFAULTSELECT = "select " + COLUMNS + " from " + TABLE;

		#endregion

		//*************************************************************************************
		#region static methods

		public static List<Datei> GetList(NpgsqlConnection connection, Artikel artikel)
		{
			connection.Open();
			NpgsqlCommand command = new NpgsqlCommand();
			command.Connection = connection;
			command.CommandText = $"{DEFAULTSELECT} where artikel_id = :aid";
			command.Parameters.AddWithValue("aid", artikel.ArtikelId);
			NpgsqlDataReader reader = command.ExecuteReader();

			List<Datei> list = new List<Datei>();
			while (reader.Read()) list.Add(new Datei(reader));
			reader.Close();
			connection.Close();
			return list;
		}

		public static Datei Get(NpgsqlConnection connection, int dateiId)
		{
			connection.Open();
			NpgsqlCommand command = new NpgsqlCommand();
			command.Connection = connection;
			command.CommandText = $"{DEFAULTSELECT} where datei_id = :did";
			command.Parameters.AddWithValue("did", dateiId);
			NpgsqlDataReader reader = command.ExecuteReader();
			Datei datei = null;
			if (reader.Read()) datei = new(reader);
			reader.Close();
			connection.Close();
			return datei;
		}

		#endregion

		//*************************************************************************************
		#region constructors
		public Datei()
		{

		}

		public Datei(NpgsqlDataReader reader)
		{
			this.DateiId = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
			this.ArtikelId = reader.IsDBNull(1) ? (long?)null : reader.GetInt64(1);
			this.PersonId = reader.IsDBNull(2) ? (long?)null : reader.GetInt64(2);
			this.Name = reader.IsDBNull(3) ? (string)null : reader.GetString(3);
			this.Erweiterung = reader.IsDBNull(4) ? (string)null : reader.GetString(4);
			this.Inhalt = reader.IsDBNull(5) ? (byte[])null : (byte[])reader.GetValue(5);
			this.MedienTyp = reader.IsDBNull(6) ? (string)null : reader.GetString(6);
		}


		#endregion

		//*************************************************************************************
		#region properties

		public long? DateiId { get; set; }
		public long? ArtikelId { get; set; }
		public long? PersonId { get; set; }
		public string Name { get; set; }
		public string Erweiterung { get; set; }
		public string MedienTyp { get; set; }

		[JsonIgnore()]
		public byte[] Inhalt { get; set; }

		#endregion

		//*************************************************************************************
		#region public methods

		public int Save(NpgsqlConnection connection) => Save(connection, null);
		public int Save(NpgsqlConnection connection, NpgsqlTransaction transaction)
		{
			if (connection.State != System.Data.ConnectionState.Open) connection.Open();
			NpgsqlCommand command = new NpgsqlCommand();
			command.Connection = connection;
			command.Transaction = transaction;	

			if (this.DateiId.HasValue) // update
			{
				command.CommandText = $"update {TABLE} set artikel_id = :aid, person_id = :pid, name = :name, erweiterung = :erweiterung, inhalt = :inhalt, medien_typ = :medientyp where datei_id = :did";
			}
			else // insert
			{
				command.CommandText = $"select nextval('{TABLE}_seq')";
				this.DateiId = (long)command.ExecuteScalar();
				command.CommandText = $"insert into {TABLE} ({COLUMNS}) values (:did, :aid, :pid, :name, :erweiterung, :inhalt, :medientyp)";
			}

			command.Parameters.AddWithValue("did", this.DateiId.Value);
			command.Parameters.AddWithValue("aid", this.ArtikelId.HasValue ? this.ArtikelId.Value : (object)DBNull.Value);
			command.Parameters.AddWithValue("pid", this.PersonId.HasValue ? this.PersonId.Value : (object)DBNull.Value);
			command.Parameters.AddWithValue("name", String.IsNullOrEmpty(this.Name) ? DBNull.Value : (object)this.Name);
			command.Parameters.AddWithValue("erweiterung", String.IsNullOrEmpty(this.Erweiterung) ? DBNull.Value : (object)this.Erweiterung);
			command.Parameters.AddWithValue("inhalt", this.Inhalt == null ?  (object)DBNull.Value : this.Inhalt);
			command.Parameters.AddWithValue("medientyp", String.IsNullOrEmpty(this.MedienTyp) ? DBNull.Value : (object)this.MedienTyp);

			try
			{
				return command.ExecuteNonQuery();
			}
			catch (Exception)
			{
				throw;
			}
			finally
			{
				if (transaction == null) connection.Close();
			}

		}

		public int Delete(NpgsqlConnection connection, NpgsqlTransaction transaction)
		{
			if (this.DateiId.HasValue)
			{
				NpgsqlCommand command = new NpgsqlCommand();

				if (connection.State != System.Data.ConnectionState.Open) connection.Open();
				command.Transaction = transaction;
				command.Connection = connection;

				try
				{
					command.CommandText = $"delete from {TABLE} where datei_id = :did";
					command.Parameters.AddWithValue("did", this.DateiId);
					return command.ExecuteNonQuery();
				}
				catch (Exception)
				{
					throw;
				}
				finally
				{
					if (transaction == null)	connection.Close();
				}
			}
			else return 1;
		}

		#endregion


	}
}

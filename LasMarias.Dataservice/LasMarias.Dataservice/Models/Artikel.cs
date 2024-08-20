using Npgsql;
using System;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Text.Json.Serialization;

namespace LasMarias.Dataservice
{
    public class Artikel
    {
        //*************************************************************************************
        #region constants

        protected const string TABLE = "lasmarias.artikel";
        protected const string COLUMNS = "artikel_id, name, preis, ekpreis, vkpreis, anlagedatum";
        protected const string DEFAULTSELECT = "select " + COLUMNS + " from " + TABLE;

        #endregion

        //*************************************************************************************
        #region static methods

        public static List<Artikel> GetList(NpgsqlConnection connection)
        {
            List<Artikel> list = new List<Artikel>();
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = $"{DEFAULTSELECT} order by name"
                };
                NpgsqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new Artikel(reader));
                }
                reader.Close();
            }
            finally
            {
                connection.Close();
            }
            return list;
        }

        public static Artikel Get(NpgsqlConnection connection, int id)
        {
            Artikel artikel = null;
            try
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand
                {
                    Connection = connection,
                    CommandText = $"{DEFAULTSELECT} where artikel_id = @id order by name"
                };
                command.Parameters.AddWithValue("id", id);
                NpgsqlDataReader reader = command.ExecuteReader();
                if (reader.Read())
                {
                    artikel = new Artikel(reader);
                }
                reader.Close();
            }
            finally
            {
                connection.Close();
            }
            return artikel;
        }

        #endregion

        //*************************************************************************************
        #region constructors

        public Artikel()
        {

        }

        public Artikel(NpgsqlDataReader reader)
        {
            this.ArtikelId = reader.IsDBNull(0) ? (long?)null : reader.GetInt64(0);
            //this.Nummer = reader.IsDBNull(1) ? null : reader.GetString(1);
            this.Name = reader.IsDBNull(1) ? null : reader.GetString(1);
            this.Preis = reader.IsDBNull(2) ? (double?)null : reader.GetDouble(2);
            this.ekpreis = reader.IsDBNull(3) ? (double?)null : reader.GetDouble(3);
            this.vkpreis = reader.IsDBNull(4) ? (double?)null : reader.GetDouble(4);
            this.AnlageDatum = reader.IsDBNull(5) ? (DateTime?)null : reader.GetDateTime(5);
          
        }

        #endregion

        //*************************************************************************************
        #region properties

        public long? ArtikelId { get; set; }
        public string Name { get; set; }
        public double? Preis { get; set; }
        public double? ekpreis { get; set; }
        public double? vkpreis { get; set; }
        public DateTime? AnlageDatum { get; set; }

        [JsonPropertyName("Bild")]
       public Datei Bild { get; set; }

        #endregion

        //*************************************************************************************

        public int Save(NpgsqlConnection connection)
        {
            connection.Open();
            NpgsqlTransaction transaction = connection.BeginTransaction();

            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            command.Transaction = transaction;

            if (this.ArtikelId.HasValue) // update
            {
                command.CommandText = $"update {TABLE} set nummer = :nummer, name = :name, Preis = :Preis, ekpreis = :ekpreis, vkpreis = :vkpreis, anlage_datum = :anlagedatum where artikel_id = :artikelid";
            }
            else // insert
            {
                command.CommandText = $"select nextval('{TABLE}_seq')";
                this.ArtikelId = (long)command.ExecuteScalar();
                command.CommandText = $"insert into {TABLE} ({COLUMNS}) values (:artikelid, :nummer, :name, :hersteller, :pgr, :peh, :anlagedatum)";
            }

            command.Parameters.AddWithValue("Artikelid", this.ArtikelId.Value);
            //command.Parameters.AddWithValue("nummer", string.IsNullOrEmpty(this.Nummer) ? DBNull.Value : (object)this.Nummer);
            command.Parameters.AddWithValue("name", string.IsNullOrEmpty(this.Name) ? DBNull.Value : (object)this.Name);
            command.Parameters.AddWithValue("Preis",this.Preis == null ? DBNull.Value : (object)this.Preis);
            command.Parameters.AddWithValue("ekpreis", this.ekpreis.HasValue ? (object)this.ekpreis: DBNull.Value);
            command.Parameters.AddWithValue("vkpreis", this.vkpreis ==null ? DBNull.Value : (object)this.vkpreis);
            command.Parameters.AddWithValue("anlagedatum", this.AnlageDatum.HasValue ? (object)this.AnlageDatum : DBNull.Value);

            int r = 0;
            try
            {
                r = command.ExecuteNonQuery();

                if (r == 1)
                {
                    command = new NpgsqlCommand();
                    command.Connection = connection;
                    command.Transaction = transaction;
                    command.CommandText = "delete from lasmarias.artikel where artikel_id = :aid";
                    command.Parameters.AddWithValue("aid", this.ArtikelId);
                    command.ExecuteNonQuery();

                    if (r == 1) transaction.Commit();
                    else transaction.Rollback();
                }

                return r;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                connection.Close();
            }

        }


        public int Delete(NpgsqlConnection cnn)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                NpgsqlCommand cmd = new NpgsqlCommand();
                cnn.Open();
                transaction = cnn.BeginTransaction();
                cmd.Connection = cnn;
                cmd.Transaction = transaction;

               // if (this.dateiList != null && this.dateiList.Count > 0)
              //  {
              //	foreach (Datei datei in this.dateiList) datei.Delete(cnn, transaction);
              //  }

                //if (this.bewegungList != null && this.bewegungList.Count > 0)
                //{
                //    foreach (Bewegung bewegung in this.bewegungList) bewegung.Delete(cnn, transaction);
                //}


                cmd.CommandText = "delete from lagerverwaltung.artikel_kategorie where artikel_id = :aid";
                cmd.Parameters.AddWithValue("aid", this.ArtikelId);
                cmd.ExecuteNonQuery();

                cmd.CommandText = "delete from lagerverwaltung.artikel where artikel_id = :aid";
                int r = cmd.ExecuteNonQuery();

                transaction.Commit();
                return r;
            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                cnn.Close();
            }
        }


    }
}

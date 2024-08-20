using Microsoft.AspNetCore.Mvc;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace LasMarias.Dataservice
{
    public enum PersonRolle
    {
        Keine = 0,
        Lieferant = 1,
        Kellner = 2,
        Admin = 3

    }


    public class Person
	{

		//*************************************************************************************
		#region constants

		protected const string TABLE = "lasmarias.person";
		protected const string COLUMNS = "person_id, name, vorname, benutzer_name, rolle_zahl, rolle_text, geb_dat, passwort_hash,login_code,login_gilt_bis, login_zuletzt, person_uid, del_datum";
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
			command.CommandText = $"{DEFAULTSELECT}where del_datum is null order by name, vorname, geb_dat desc";
            NpgsqlDataReader reader = command.ExecuteReader();
			
			List<Person> list = new List<Person>();
			while (reader.Read()) list.Add(new Person(reader));
            reader.Close();
            connection.Close();
            return list;
        }

        public static List<Person> GetList(NpgsqlConnection connection, int? sort, string filter)
        {
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = $"{DEFAULTSELECT} where del_datum is null";

            if (!String.IsNullOrEmpty(filter))
            {
                command.CommandText += " and (lower(name) like :p1 or lower(vorname) like :p1 or lower(benutzer_name) like :p1)";
                command.Parameters.AddWithValue("p1", $"%{filter.ToLower().Replace(" ", "%")}%");
            }

            if (sort.HasValue)
            {
                switch (sort.Value)
                {
                    case 1:
                        command.CommandText += " order by name desc, vorname desc";
                        break;
                    case 2:
                        command.CommandText += " order by geb_dat";
                        break;
                    case 3:
                        command.CommandText += " order by geb_dat desc";
                        break;
                    case 4:
                        command.CommandText += " order by rolle_zahl";
                        break;
                    case 5:
                        command.CommandText += " order by rolle_zahl desc";
                        break;
                    default:
                        command.CommandText += " order by name, vorname";
                        break;
                }
            }
        

            NpgsqlDataReader reader = command.ExecuteReader();

            List<Person> list = new List<Person>();
            while (reader.Read()) list.Add(new Person(reader));
            reader.Close();
            connection.Close();
            return list;
        }


        public static Person Get(NpgsqlConnection connection, ControllerBase controller)
        {
            string loginCode = controller.Request.Cookies["logincode"];
            if (String.IsNullOrEmpty(loginCode)) return null;
            else
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connection;
                //command.CommandText = $"{DEFAULTSELECT} where login_code = :lc and login_gilt_bis >= now() and del_datum is null";
                command.CommandText = $"{DEFAULTSELECT}";
                command.Parameters.AddWithValue("lc", loginCode);
                NpgsqlDataReader reader = command.ExecuteReader();
                Person person = null;
                if (reader.Read()) person = new Person(reader);
                reader.Close();
                connection.Close();
                return person;
            }
        }

        public static Person Get(NpgsqlConnection connection, string benutzerName, string passwort)
        {
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;

            command.CommandText = $"{DEFAULTSELECT} where benutzer_name = :bn and passwort_hash = :pwd and del_datum is null";
            command.Parameters.AddWithValue("bn", benutzerName);
            var pwd = Person.PasswortHashErstellen(passwort);
            command.Parameters.AddWithValue("pwd",pwd);
            //command.CommandText = $"{DEFAULTSELECT} where benutzer_name = '{benutzerName}' and passwort_hash = '{pwd}'";
            NpgsqlDataReader reader = command.ExecuteReader();
            Person person = null;
            if (reader.Read()) person = new Person(reader);
            reader.Close();
            connection.Close();
            return person;
        }

        public static Person Get(NpgsqlConnection connection, string id)
        {
            int tmpId = 0;

            if (int.TryParse(id, out tmpId)) return Get(connection, tmpId);
            else
            {
                connection.Open();
                NpgsqlCommand command = new NpgsqlCommand();
                command.Connection = connection;
                command.CommandText = $"{DEFAULTSELECT} where person_uid = :pid and del_datum is null";
                command.Parameters.AddWithValue("pid", id);
                NpgsqlDataReader reader = command.ExecuteReader();
                Person person = null;
                if (reader.Read()) person = new Person(reader);
                reader.Close();
                connection.Close();
                return person;
            }
        }

        public static Person Get(NpgsqlConnection connection, int id)
        {
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            command.CommandText = $"{DEFAULTSELECT} where person_id = :pid and del_datum is null";
            command.Parameters.AddWithValue("pid", id);
            NpgsqlDataReader reader = command.ExecuteReader();
            Person person = null;
            if (reader.Read()) person = new Person(reader);
            reader.Close();
            connection.Close();
            return person;
        }
        public static Person GetUserByUsername(NpgsqlConnection connection, string benutzerName)
        {
            connection.Open();
            NpgsqlCommand command = new NpgsqlCommand();
            command.Connection = connection;
            //command.CommandText = $"{DEFAULTSELECT} where benutzer_name = '{benutzerName}' and passwort_hash = '{Person.PasswortHashErstellen(passwort)}'";
            command.CommandText = $"{DEFAULTSELECT} where benutzer_name = :bn";
            command.Parameters.AddWithValue("bn", benutzerName);
            NpgsqlDataReader reader = command.ExecuteReader();
            Person person = null;
            if (reader.Read()) person = new Person(reader);
            reader.Close();
            connection.Close();
            return person;
        }

        //*************************************************************************************
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
            this.LoginCode = reader.IsDBNull(8) ? (string)null : reader.GetString(8);
            this.LoginGiltBis = reader.IsDBNull(9) ? (DateTime?)null : reader.GetDateTime(9);
            this.LoginZuletzt = reader.IsDBNull(10) ? (DateTime?)null : reader.GetDateTime(10);
            this.PersonUid = reader.IsDBNull(11) ? (string)null : reader.GetString(11);
            this.DelDatum = reader.IsDBNull(12) ? (DateTime?)null : reader.GetDateTime(12);
        }


        #endregion

        //*************************************************************************************
        #region properties

        [JsonPropertyName("personid")]
        public long? PersonId { get; set; }

        [JsonPropertyName("personuid")]
        public string PersonUid { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("vorname")]
        public string Vorname { get; set; }

        [JsonPropertyName("rolle")]
        //public PersonRolle Rolle
        //{
        //    get
        //    {
        //        return this.RolleZahl.HasValue ? (PersonRolle)this.RolleZahl.Value : PersonRolle.Keine;
        //    }
        //    set
        //    {
        //        this.RolleZahl = (int)value;
        //    }

        //}
        public PersonRolle? Rolle
        {
            get
            {
                return this.RolleZahl.HasValue ? (PersonRolle)this.RolleZahl.Value : PersonRolle.Keine;
            }
            set
            {
                this.RolleZahl = (int)value;
            }

        }

        [JsonIgnore()]
        public int? RolleZahl { get; set; }

        [JsonPropertyName("rolletext")]
        public string RolleText { get; set; }

        [JsonPropertyName("gebdat")]
        public DateTime? GebDat { get; set; }

        [JsonPropertyName("benutzername")]
        public string BenutzerName { get; set; }

        [JsonPropertyName("passwort")]
        public string Passwort { get; set; }

        [JsonIgnore()]
        public string PasswortHash { get; set; }

        [JsonIgnore()]
        public string LoginCode { get; set; }

        [JsonIgnore()]
        public DateTime? LoginGiltBis { get; set; }

        [JsonPropertyName("loginzuletzt")]
        public DateTime? LoginZuletzt { get; set; }

        [JsonIgnore()]
        public DateTime? DelDatum { get; set; }
        #endregion

        //*************************************************************************************
        #region public methods

        public int Save(NpgsqlConnection connection)
        {
            this.RolleText = this.Rolle.ToString();

            connection.Open();
            NpgsqlCommand command = new();
            command.Connection = connection;

            if (this.PersonId.HasValue) // update
            {
                command.CommandText = $"update {TABLE} set name = :name, vorname = :vorname, benutzer_name = :benutzername, rolle_zahl = :rollezahl, rolle_text = :rolletext, geb_dat = :gebdat, passwort_hash = :passworthash, login_code = :logincode, login_gilt_bis = :logingiltbis, login_zuletzt = :loginzuletzt, person_uid = :personuid, del_datum = :deldatum where person_id = :personid";
            }
            else // insert
            {
                command.CommandText = $"select nextval('{TABLE}_seq')";
                this.PersonId = (long)command.ExecuteScalar();
                this.PersonUid = Guid.NewGuid().ToString("N");
                command.CommandText = $"insert into {TABLE} ({COLUMNS}) values (:personid, :name, :vorname, :benutzername, :rollezahl, :rolletext, :gebdat, :passworthash, :logincode, :logingiltbis, :loginzuletzt, :personuid, :deldatum)";
            }

            command.Parameters.AddWithValue("personid", this.PersonId.Value);
            command.Parameters.AddWithValue("name", String.IsNullOrEmpty(this.Name) ? DBNull.Value : (object)this.Name);
            command.Parameters.AddWithValue("vorname", String.IsNullOrEmpty(this.Vorname) ? DBNull.Value : (object)this.Vorname);
            command.Parameters.AddWithValue("benutzername", String.IsNullOrEmpty(this.BenutzerName) ? DBNull.Value : (object)this.BenutzerName);
            command.Parameters.AddWithValue("rollezahl", this.RolleZahl.HasValue ? (object)this.RolleZahl.Value : DBNull.Value);
            command.Parameters.AddWithValue("rolletext", String.IsNullOrEmpty(this.RolleText) ? DBNull.Value : (object)this.RolleText);
            command.Parameters.AddWithValue("gebdat", this.GebDat.HasValue ? (object)this.GebDat.Value : DBNull.Value);
            this.PasswortHash = PasswortHashErstellen(this.Passwort);
            command.Parameters.AddWithValue("passworthash", String.IsNullOrEmpty(this.PasswortHash) ? DBNull.Value : (object)this.PasswortHash);
            command.Parameters.AddWithValue("logincode", String.IsNullOrEmpty(this.LoginCode) ? DBNull.Value : (object)this.LoginCode);
            command.Parameters.AddWithValue("logingiltbis", this.LoginGiltBis.HasValue ? (object)this.LoginGiltBis.Value : DBNull.Value);
            command.Parameters.AddWithValue("loginzuletzt", this.LoginZuletzt.HasValue ? (object)this.LoginZuletzt.Value : DBNull.Value);
            command.Parameters.AddWithValue("personuid", String.IsNullOrEmpty(this.PersonUid) ? DBNull.Value : (object)this.PersonUid);
            command.Parameters.AddWithValue("deldatum", this.DelDatum.HasValue ? (object)this.DelDatum.Value : DBNull.Value);

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

        public int Delete(NpgsqlConnection connection)
        {
            if (this.PersonId.HasValue) // update
            {
                this.DelDatum = DateTime.Now;
                return this.Save(connection);
            }
            return 1;
        }


        #endregion

    }
}

#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using System.Data.SqlClient;
using SSD_Assignment___Banking_Application;
using System.Data;

namespace Banking_Application
{
    public class Data_Access_Layer
    {

        private List<Bank_Account> accounts;
        private static String databaseName = "Banking Database.db";
        private static Data_Access_Layer instance = new Data_Access_Layer();

        private Data_Access_Layer()//Singleton Design Pattern (For Concurrency Control) - Use getInstance() Method Instead.
        {
            accounts = new List<Bank_Account>();
        }

        public static Data_Access_Layer getInstance()
        {
            return instance;
        }

        public SqliteConnection getDatabaseConnection()
        {

            String databaseConnectionString = new SqliteConnectionStringBuilder()
            {
                DataSource = Data_Access_Layer.databaseName,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ToString();

            return new SqliteConnection(databaseConnectionString);

        }

        private void initialiseDatabase() // No need for parameters.
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
                    CREATE TABLE IF NOT EXISTS Bank_Accounts(    
                        accountNo TEXT PRIMARY KEY,
                        name TEXT NOT NULL,
                        address_line_1 TEXT,
                        address_line_2 TEXT,
                        address_line_3 TEXT,
                        town TEXT NOT NULL,
                        balance REAL NOT NULL,
                        accountType INTEGER NOT NULL,
                        overdraftAmount REAL,
                        interestRate REAL
                    ) WITHOUT ROWID
                ";



                command.ExecuteNonQuery();

            }
        }

        private void loadBankAccounts()
        {
            if (!File.Exists(Data_Access_Layer.databaseName))
                initialiseDatabase();
            else
            {

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "SELECT * FROM Bank_Accounts"; // This should possibly be reduced to 1 account
                    //command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNo = @accountNo";
                    //command.Parameters.AddWithValue("@accountNo", accountNo);
                    SqliteDataReader dr = command.ExecuteReader();

                    while (dr.Read())
                    {

                        int accountType = dr.GetInt16(7);

                        if (accountType == Account_Type.Current_Account)
                        {
                            Current_Account ca = new Current_Account();
                            ca.accountNo = dr.GetString(0);
                            ca.name = dr.GetString(1);
                            ca.address_line_1 = dr.GetString(2);
                            ca.address_line_2 = dr.GetString(3);
                            ca.address_line_3 = dr.GetString(4);
                            ca.town = dr.GetString(5);
                            ca.balance = dr.GetDouble(6);
                            ca.overdraftAmount = dr.GetDouble(8);
                            accounts.Add(ca);
                        }
                        else
                        {
                            Savings_Account sa = new Savings_Account();
                            sa.accountNo = dr.GetString(0);
                            sa.name = dr.GetString(1);
                            sa.address_line_1 = dr.GetString(2);
                            sa.address_line_2 = dr.GetString(3);
                            sa.address_line_3 = dr.GetString(4);
                            sa.town = dr.GetString(5);
                            sa.balance = dr.GetDouble(6);
                            sa.interestRate = dr.GetDouble(9);
                            accounts.Add(sa);
                        }


                    }

                }

            }
        }

        public String addBankAccount(Bank_Account ba) // adding parameters
        {

            if (ba.GetType() == typeof(Current_Account))
                ba = (Current_Account)ba;
            else
                ba = (Savings_Account)ba;

            accounts.Add(ba);

            using (var connection = getDatabaseConnection())
            {
                int arbitrary_limit = 30;
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText =
                @"
            INSERT INTO Bank_Accounts VALUES(@accountNo, @name, @address_line_1, @address_line_2, @address_line_3, @town, @balance, @accountType, @overdraftAmount, @interestRate)";

                command.Parameters.AddWithValue("@accountNo", ba.accountNo);
                command.Parameters.AddWithValue("@name", ba.name).Size=arbitrary_limit;
                command.Parameters.AddWithValue("@address_line_1", ba.address_line_1).Size = arbitrary_limit;
                command.Parameters.AddWithValue("@address_line_2", ba.address_line_2).Size = arbitrary_limit;
                command.Parameters.AddWithValue("@address_line_3", ba.address_line_3).Size = arbitrary_limit;
                command.Parameters.AddWithValue("@town", ba.town).Size = arbitrary_limit;
                command.Parameters.AddWithValue("@balance", ba.balance);
                command.Parameters.AddWithValue("@accountType", ba.GetType() == typeof(Current_Account) ? 1 : 2);

                if (ba.GetType() == typeof(Current_Account))
                {
                    Current_Account ca = (Current_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", ca.overdraftAmount);
                    command.Parameters.AddWithValue("@interestRate", DBNull.Value);
                }

                else
                {
                    Savings_Account sa = (Savings_Account)ba;
                    command.Parameters.AddWithValue("@overdraftAmount", DBNull.Value);
                    command.Parameters.AddWithValue("@interestRate", sa.interestRate);
                }
                command.Prepare();
                command.ExecuteNonQuery();

            }

            return ba.accountNo;

        }

        public Bank_Account findBankAccountByAccNo(String accNo)
        {
            using (var connection = getDatabaseConnection())
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM Bank_Accounts WHERE accountNo = @accNo";
                command.Parameters.AddWithValue("@accNo", accNo);

                SqliteDataReader dr = command.ExecuteReader();

                while (dr.Read())
                {
                    int accountType = dr.GetInt16(7);
                    if (accountType == Account_Type.Current_Account)
                    {
                        Current_Account ca = new Current_Account();
                        ca.accountNo = dr.GetString(0);
                        ca.name = dr.GetString(1);
                        ca.address_line_1 = dr.GetString(2);
                        ca.address_line_2 = dr.GetString(3);
                        ca.address_line_3 = dr.GetString(4);
                        ca.town = dr.GetString(5);
                        ca.balance = dr.GetDouble(6);
                        ca.overdraftAmount = dr.GetDouble(8);
                        return ca;
                    }
                    else
                    {
                        Savings_Account sa = new Savings_Account();
                        sa.accountNo = dr.GetString(0);
                        sa.name = dr.GetString(1);
                        sa.address_line_1 = dr.GetString(2);
                        sa.address_line_2 = dr.GetString(3);
                        sa.address_line_3 = dr.GetString(4);
                        sa.town = dr.GetString(5);
                        sa.balance = dr.GetDouble(6);
                        sa.interestRate = dr.GetDouble(9);
                        return sa;
                    }
                }
            }

            return null;
        }

        public bool closeBankAccount(String accNo)
        {

            accounts.Add(findBankAccountByAccNo(accNo));
            Bank_Account toRemove = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    toRemove = ba;
                    break;
                }

            }

            if (toRemove == null)
                return false;
            else
            {
                accounts.Remove(toRemove);

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "DELETE FROM Bank_Accounts WHERE accountNo = '" + toRemove.accountNo + "'";
                    command.Prepare();
                    command.ExecuteNonQuery();

                }

                return true;
            }

        }

        public bool lodge(String accNo, double amountToLodge)
        {

            accounts.Add(findBankAccountByAccNo(accNo));
            Bank_Account toLodgeTo = null;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    ba.lodge(amountToLodge);
                    toLodgeTo = ba;
                    break;
                }

            }

            if (toLodgeTo == null)
                return false;
            else
            {
                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toLodgeTo.balance + " WHERE accountNo = '" + toLodgeTo.accountNo + "'";
                    //command.ExecuteNonQuery();

                
                    SqliteParameter idParam = new SqliteParameter("@newbal", SqliteType.Integer, 0);
                    SqliteParameter descParam =
                        new SqliteParameter("@desc", SqliteType.Text, 100);
                    idParam.Value = toLodgeTo.balance;
                    descParam.Value = toLodgeTo.accountNo;
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(descParam);
                    command.Prepare();
                    command.ExecuteNonQuery();
                }
                return true;
            }

        }

        public bool withdraw(String accNo, double amountToWithdraw)
        {
            accounts.Add(findBankAccountByAccNo(accNo));

            Bank_Account toWithdrawFrom = null;
            bool result = false;

            foreach (Bank_Account ba in accounts)
            {

                if (ba.accountNo.Equals(accNo))
                {
                    result = ba.withdraw(amountToWithdraw);
                    toWithdrawFrom = ba;
                    break;
                }

            }

            if (toWithdrawFrom == null || result == false)
                return false;
            else
            {

                /*using (SqlConnection connection = new SqlConnection(Data_Access_Layer.databaseName))
                {
                    var newbal = encryption.Decrypt(toWithdrawFrom.balance.ToString());

                    SqlCommand command = new SqlCommand(null, connection)
                    {

                        // Create and prepare an SQL statement.
                        CommandText =
                        "UPDATE Bank_Accounts SET balance =(bal) WHERE account = @accountNo" +
                        "VALUES (@newbal, @accno)"
                    };
                    SqlParameter idParam = new SqlParameter("@newbal", SqlDbType.Int, 0);
                    SqlParameter descParam =
                        new SqlParameter("@desc", SqlDbType.Text, 100);
                    idParam.Value = newbal;
                    descParam.Value = toWithdrawFrom.accountNo;
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(descParam);
                    command.Prepare();
                    command.ExecuteNonQuery();
               */

                using (var connection = getDatabaseConnection())
                {
                    connection.Open();
                    var command = connection.CreateCommand();
                    command.CommandText = "UPDATE Bank_Accounts SET balance = " + toWithdrawFrom.balance + " WHERE accountNo = '" + toWithdrawFrom.accountNo + "'";
                    SqliteParameter idParam = new SqliteParameter("@newbal", SqliteType.Integer, 0);
                    SqliteParameter descParam =
                        new SqliteParameter("@desc", SqliteType.Text, 100);
                    idParam.Value = toWithdrawFrom.balance;
                    descParam.Value = toWithdrawFrom.accountNo;
                    command.Parameters.Add(idParam);
                    command.Parameters.Add(descParam);
                    command.Prepare();

                    command.ExecuteNonQuery();

                }

                return true;

            }

            }

        }

    }

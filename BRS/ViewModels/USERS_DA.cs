using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

namespace BRS.ViewModels
{
    public class USERS_DA
    {
        SqlConnection CnLocal = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);
        public virtual DataSet GetMsUser(int page, string where)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            using (SqlCommand command = new SqlCommand("dbo.GetMsUser", CnLocal))
            {
                command.Parameters.AddWithValue("PageIndex", page);
                command.Parameters.AddWithValue("PageSize", ConfigurationManager.AppSettings["PageSize"]);
                command.Parameters.AddWithValue("Condition", where);
                command.CommandType = CommandType.StoredProcedure;
                CnLocal.Open();

                adapter.SelectCommand = command;
                adapter.Fill(ds, "UsersData");
                CnLocal.Close();
            }

            return ds;
        }

        public string insertMsUser(string userId, string userName, string password, int brandId, string role)
        {
            string errMessage = string.Empty;
            string query = string.Empty;

            CnLocal.Open();
            SqlTransaction sqltrans = CnLocal.BeginTransaction();
            try
            {
                //create users
                query = String.Format("INSERT INTO dbo.Users (UserId, UserName, Password, BrandId) VALUES " +
                                             "(@UserId, @UserName, @Password, @BrandId)");
                
                using (SqlCommand command = new SqlCommand(query, CnLocal))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    command.Parameters.AddWithValue("UserName", userName);
                    command.Parameters.AddWithValue("Password", password);
                    command.Parameters.AddWithValue("BrandId", brandId);
                    command.Transaction = sqltrans;
                    command.ExecuteScalar();
                }

                //create user role
                query = String.Format("INSERT INTO dbo.UserRole (UserId, RoleId) VALUES " +
                                             "(@UserId, @RoleId)");

                using (SqlCommand command = new SqlCommand(query, CnLocal))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    command.Parameters.AddWithValue("RoleId", role);
                    command.Transaction = sqltrans;
                    command.ExecuteScalar();
                }

                sqltrans.Commit();
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
                sqltrans.Rollback();
            }
            finally
            {
                CnLocal.Close();
            }

            return errMessage;
        }

        public string updateMsUser(string userId, string userName, int brandId, bool active)
        {
            string errMessage = string.Empty;
            
            try
            {
                //update users
                string query = String.Format(@"UPDATE dbo.Users SET UserName = @UserName, BrandId = @BrandId, [Status] = @Status, AuditUserName = @userId, AuditTime = GETDATE(), AuditActivity = 'U' WHERE UserId = @userId");

                using (SqlCommand command = new SqlCommand(query, CnLocal))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    command.Parameters.AddWithValue("UserName", userName);
                    command.Parameters.AddWithValue("BrandId", brandId);
                    command.Parameters.AddWithValue("Status", active);
                    CnLocal.Open();
                    command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            finally
            {
                CnLocal.Close();
            }

            return errMessage;
        }

        public string resetPassword(string userId)
        {
            string errMessage = string.Empty;

            try
            {
                //update users
                string query = String.Format(@"UPDATE dbo.Users SET Password = @Password, AuditUserName = @userId, AuditTime = GETDATE(), AuditActivity = 'U' WHERE UserId = @userId");

                using (SqlCommand command = new SqlCommand(query, CnLocal))
                {
                    command.Parameters.AddWithValue("UserId", userId);
                    command.Parameters.AddWithValue("Password", LOGIN_DA.Encrypt(ConfigurationManager.AppSettings["RPassword"]));
                    CnLocal.Open();
                    command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;
            }
            finally
            {
                CnLocal.Close();
            }

            return errMessage;
        }
    }
}
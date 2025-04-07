using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Text;
using BRS.Models;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Security.Cryptography;

namespace BRS.ViewModels
{
    public class LOGIN_DA
    {
        static readonly string PasswordHash = "P@@Sw0rd";
        static readonly string SaltKey = "S@LT&KEY";
        static readonly string VIKey = "@1B2c3D4e5F6g7H8";

        SqlConnection CnLocal = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);

        public static string Encrypt(string plainText)
        {
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(plainText);

            byte[] keyBytes = new Rfc2898DeriveBytes(PasswordHash, Encoding.ASCII.GetBytes(SaltKey)).GetBytes(256 / 8);
            var symmetricKey = new RijndaelManaged() { Mode = CipherMode.CBC, Padding = PaddingMode.Zeros };
            var encryptor = symmetricKey.CreateEncryptor(keyBytes, Encoding.ASCII.GetBytes(VIKey));

            byte[] cipherTextBytes;

            using (var memoryStream = new MemoryStream())
            {
                using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                {
                    cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                    cryptoStream.FlushFinalBlock();
                    cipherTextBytes = memoryStream.ToArray();
                    cryptoStream.Close();
                }
                memoryStream.Close();
            }
            return Convert.ToBase64String(cipherTextBytes);
        }

        public MenuResult getUserModule(string userId, string password)
        {
            MenuResult mr = new MenuResult
            {
                moduleCategories = new List<UserLibrary.UserModuleCategory>()
            };

            DataTable dt = new DataTable();

            try
            {
                using (SqlCommand command = new SqlCommand("dbo.GetUserModule", CnLocal))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("UserId", userId);
                    command.Parameters.AddWithValue("Password", password);
                    CnLocal.Open();
                    SqlDataAdapter adapter = new SqlDataAdapter();
                    adapter.SelectCommand = command;
                    adapter.Fill(dt);

                    if (dt.Rows.Count > 0)
                    {
                        mr.brandName = dt.Rows[0]["BrandName"].ToString();
                        var dict = dt.AsEnumerable()
                                   .GroupBy(row => row.Field<string>("ModuleCategoryName"))
                                   .ToDictionary(
                                        g => g.Key.ToString(),
                                        g => g.Select(row => new
                                        {
                                            ModuleId = row.Field<int>("ModuleId"),
                                            ModuleName = row.Field<string>("ModuleName"),
                                            Link = row.Field<string>("Link")
                                        }).ToList()
                                    );

                        int menuId = 0;
                        foreach (var mod in dict)
                        {
                            menuId++;
                            UserLibrary.UserModuleCategory umc = new UserLibrary.UserModuleCategory
                            {
                                ModuleCategory = mod.Key,
                                MenuCategoryId = menuId,
                                MenuCategoryCss = ""
                            };

                            foreach (var subMod in mod.Value)
                            {
                                umc.UserModules.Add(
                                    new UserLibrary.UserModule
                                    {
                                        ModuleId = subMod.ModuleId,
                                        ModuleName = subMod.ModuleName,
                                        Url = subMod.Link
                                    }
                                );
                            }

                            mr.moduleCategories.Add(umc);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mr.errorMessage = ex.Message;
            }
            finally
            {
                CnLocal.Close();
            }

            return mr;
        }

        public string UpdateLastLogin(string userId)
        {
            string errMessage = string.Empty;

            try
            {
                using (SqlCommand command = new SqlCommand(@"UPDATE dbo.Users SET LastLogin = GETDATE(), AuditUserName = @userId, AuditTime = GETDATE(), AuditActivity = 'U' WHERE UserId = @userId", CnLocal))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@userId", userId);
                    CnLocal.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errMessage = "Error: " + ex.Message;
            }
            finally
            {
                CnLocal.Close();
            }

            return errMessage;
        }

        public string ChangePassword(string userid, string password)
        {
            string errMessage = string.Empty;

            try
            {
                using (SqlCommand command = new SqlCommand(@"UPDATE dbo.Users SET [password] = @password, AuditUserName = @userName, AuditTime = GETDATE(), AuditActivity = 'U' WHERE UserId = @userName", CnLocal))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.AddWithValue("@userName", userid);
                    command.Parameters.AddWithValue("@password", password);
                    CnLocal.Open();
                    command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                errMessage = "Error: " + ex.Message;
            }
            finally
            {
                CnLocal.Close();
            }

            return errMessage;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;
using System.Data.OleDb;
using BRS.Models;
using System.Web.Mvc;

namespace BRS.ViewModels
{
    public class TRANS_DA
    {
        SqlConnection CnLocal = new SqlConnection(ConfigurationManager.ConnectionStrings["ConnString"].ConnectionString);

        public virtual DataSet GetMsItem(int page, string where, string sort)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            using (SqlCommand command = new SqlCommand("dbo.GetMsItem", CnLocal))
            {
                command.Parameters.AddWithValue("PageIndex", page);
                command.Parameters.AddWithValue("PageSize", ConfigurationManager.AppSettings["PageSize"]);
                command.Parameters.AddWithValue("Condition", where);
                command.Parameters.AddWithValue("Sort", sort);
                command.CommandType = CommandType.StoredProcedure;
                CnLocal.Open();

                adapter.SelectCommand = command;
                adapter.Fill(ds, "ItemsData");
                CnLocal.Close();
            }

            return ds;
        }

        public virtual string uploadMsItem(string _fileExt, string _path, string auditUserName, out int uploadRows)
        {
            string errMessage = string.Empty;
            string connStr = string.Empty;
            uploadRows = 0;

            if (_fileExt == ".xls" || _fileExt == ".xlsx")
            {
                connStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={_path};Extended Properties='Excel 12.0'";
                OleDbConnection conn = new OleDbConnection(connStr);

                try
                {
                    conn.Open();
                    DataTable excelSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    string s_table = string.Empty;
                    foreach (DataRow row in excelSchema.Rows)
                    {
                        if (!row["TABLE_NAME"].ToString().Contains("FilterDatabase"))
                        {
                            if (!string.IsNullOrEmpty(s_table))
                            {
                                errMessage = "Excel File contains multiple sheets. Please Upload Excel File with single sheet.";
                            }

                            s_table = row["TABLE_NAME"].ToString();
                        }
                    }

                    DataTable dtExcelData = new DataTable();
                    dtExcelData.Columns.AddRange(new DataColumn[14] {
                        new DataColumn("BARCODE", typeof(string)),
                        new DataColumn("BRAND", typeof(string)),
                        new DataColumn("GENDER", typeof(string)),
                        new DataColumn("TYPE", typeof(string)),
                        new DataColumn("CATEGORY", typeof(string)),
                        new DataColumn("ARTICLE", typeof(string)),
                        new DataColumn("COLOR", typeof(string)),
                        new DataColumn("SIZE", typeof(string)),
                        new DataColumn("FIT", typeof(string)),
                        new DataColumn("SEASON_YEAR", typeof(string)),
                        new DataColumn("BOARD_WH", typeof(string)) { AllowDBNull = true },
                        new DataColumn("TAG_PRICE", typeof(decimal)),
                        new DataColumn("RETAIL_PRICE", typeof(decimal)),
                        new DataColumn("COGS", typeof(decimal))
                    });

                    //cek untuk tag price = null atau tag price = 0
                    string s_excel_sql;
                    OleDbCommand command;
                    OleDbDataAdapter da;

                    s_excel_sql = $"SELECT * FROM [{System.IO.Path.GetFileNameWithoutExtension(s_table)}] WHERE ISNULL(TAG_PRICE) < 0";
                    command = new OleDbCommand(s_excel_sql, conn);
                    da = new OleDbDataAdapter(command);
                    da.Fill(dtExcelData);
                    if (dtExcelData.Rows.Count > 0)
                        throw new Exception($"There is {dtExcelData.Rows.Count} data with tag price is null, please fix it first!");

                    s_excel_sql = $"SELECT * FROM [{System.IO.Path.GetFileNameWithoutExtension(s_table)}] WHERE ISNULL(TAG_PRICE) = 0";
                    command = new OleDbCommand(s_excel_sql, conn);
                    da = new OleDbDataAdapter(command);
                    da.Fill(dtExcelData);

                    if (dtExcelData.Rows.Count > 0)
                    {
                        uploadRows = dtExcelData.Rows.Count;

                        using (SqlCommand sqlcom = new SqlCommand("CreateMsItem", CnLocal))
                        {
                            sqlcom.CommandType = CommandType.StoredProcedure;
                            sqlcom.Parameters.AddWithValue("items", dtExcelData).SqlDbType = SqlDbType.Structured;
                            sqlcom.Parameters.AddWithValue("auditUserName", auditUserName);
                            CnLocal.Open();
                            sqlcom.ExecuteScalar();
                            CnLocal.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
                errMessage = @"Invalid file for import data. only allow .xls / .xlsx";

            return errMessage;
        }

        public virtual DataSet GetAging(int page, string where, string sort)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            using (SqlCommand command = new SqlCommand("dbo.GetAging", CnLocal))
            {
                command.Parameters.AddWithValue("PageIndex", page);
                command.Parameters.AddWithValue("PageSize", ConfigurationManager.AppSettings["PageSize"]);
                command.Parameters.AddWithValue("Condition", where);
                command.Parameters.AddWithValue("Sort", sort);
                command.CommandType = CommandType.StoredProcedure;
                CnLocal.Open();

                adapter.SelectCommand = command;
                adapter.Fill(ds, "AgingData");
                CnLocal.Close();
            }

            return ds;
        }

        public int isAgingExists(string period, DataTable locations)
        {
            int result = 0;
            string errMessage = string.Empty;

            using (SqlCommand command = new SqlCommand("dbo.IsAgingExists", CnLocal))
            {
                try
                {
                    SqlParameter sqlparam = command.Parameters.Add("ReturnValue", SqlDbType.Int);
                    sqlparam.Direction = ParameterDirection.ReturnValue;
                    command.Parameters.AddWithValue("Period", period);
                    command.Parameters.AddWithValue("Locations", locations).SqlDbType = SqlDbType.Structured;
                    command.CommandType = CommandType.StoredProcedure;
                    CnLocal.Open();
                    command.ExecuteNonQuery();
                    result = (int)command.Parameters["ReturnValue"].Value;
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
                finally
                {
                    CnLocal.Close();
                }
            }

            return result;
        }

        public virtual string uploadAging(string period, string _fileExt, string _path, string auditUserName, bool ceklocations, out int uploadRows, out DataTable itemResult)
        {
            string errMessage = string.Empty;
            string connStr = string.Empty;
            uploadRows = 0;
            itemResult = new DataTable();

            if (_fileExt == ".xls" || _fileExt == ".xlsx")
            {
                connStr = $"Provider=Microsoft.ACE.OLEDB.12.0;Data Source={_path};Extended Properties='Excel 12.0'";
                OleDbConnection conn = new OleDbConnection(connStr);

                try
                {
                    conn.Open();
                    DataTable excelSchema = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                    string s_table = string.Empty;
                    foreach (DataRow row in excelSchema.Rows)
                    {
                        if (!row["TABLE_NAME"].ToString().Contains("FilterDatabase"))
                        {
                            if (!string.IsNullOrEmpty(s_table))
                            {
                                errMessage = "Excel File contains multiple sheets. Please Upload Excel File with single sheet.";
                            }

                            s_table = row["TABLE_NAME"].ToString();
                        }
                    }

                    DataTable dtExcelData = new DataTable();
                    dtExcelData.Columns.AddRange(new DataColumn[4] {
                        new DataColumn("RELEASE_DATE", typeof(string)),
                        new DataColumn("LOCATION", typeof(string)),
                        new DataColumn("BARCODE", typeof(string)),
                        new DataColumn("QUANTITY", typeof(int))
                    });

                    string s_excel_sql;
                    OleDbCommand command;
                    OleDbDataAdapter da;

                    if (ceklocations)
                    {
                        //cek locations is exists
                        DataTable dt = new DataTable();
                        s_excel_sql = $"SELECT DISTINCT LOCATION FROM [{System.IO.Path.GetFileNameWithoutExtension(s_table)}] WHERE ISNULL(RELEASE_DATE) = 0 and QUANTITY > 0";
                        command = new OleDbCommand(s_excel_sql, conn);
                        da = new OleDbDataAdapter(command);
                        da.Fill(dt);
                        int result = isAgingExists(period, dt);
                        if (result == 1)
                            throw new Exception("CONFIRM");
                    }

                    //cek release date is null
                    s_excel_sql = $"SELECT * FROM [{System.IO.Path.GetFileNameWithoutExtension(s_table)}] WHERE ISNULL(RELEASE_DATE) < 0";
                    command = new OleDbCommand(s_excel_sql, conn);
                    da = new OleDbDataAdapter(command);
                    da.Fill(dtExcelData);

                    if (dtExcelData.Rows.Count > 0)
                        throw new Exception($"There is {dtExcelData.Rows.Count} data with release date is null");

                    s_excel_sql = $"SELECT * FROM [{System.IO.Path.GetFileNameWithoutExtension(s_table)}] WHERE ISNULL(RELEASE_DATE) = 0 and QUANTITY > 0";
                    command = new OleDbCommand(s_excel_sql, conn);
                    da = new OleDbDataAdapter(command);
                    da.Fill(dtExcelData);

                    if (dtExcelData.Rows.Count > 0)
                    {
                        DataTable ir = GetItemsNotInMaster(dtExcelData);
                        if (ir.Rows.Count > 0)
                        {
                            itemResult = ir;
                            throw new Exception($"Found {ir.Rows.Count} of {dtExcelData.Rows.Count} barcode not found in master items");
                        }

                        uploadRows = dtExcelData.Rows.Count;

                        using (SqlCommand sqlcom = new SqlCommand("CreateAgingInventory", CnLocal))
                        {
                            sqlcom.CommandType = CommandType.StoredProcedure;
                            sqlcom.Parameters.AddWithValue("period", period);
                            sqlcom.Parameters.AddWithValue("agingInv", dtExcelData).SqlDbType = SqlDbType.Structured;
                            sqlcom.Parameters.AddWithValue("auditUserName", auditUserName);
                            CnLocal.Open();
                            sqlcom.ExecuteScalar();
                            CnLocal.Close();
                        }
                    }
                    else
                        throw new Exception("Upload data is empty");
                }
                catch (Exception ex)
                {
                    errMessage = ex.Message;
                }
                finally
                {
                    conn.Close();
                }
            }
            else
                errMessage = @"Invalid file for import data. only allow .xls / .xlsx";

            return errMessage;
        }

        private DataTable GetItemsNotInMaster(DataTable dt)
        {
            DataSet ds = new DataSet();
            using (SqlCommand command = new SqlCommand("dbo.GetItemsNotInMaster", CnLocal))
            {
                command.Parameters.AddWithValue("agingInv", dt).SqlDbType = SqlDbType.Structured;
                command.CommandType = CommandType.StoredProcedure;
                CnLocal.Open();
                SqlDataAdapter adapter = new SqlDataAdapter();
                adapter.SelectCommand = command;
                adapter.Fill(ds, "ItemsData");
                CnLocal.Close();
            }

            return ds.Tables[0];
        }

        public Dictionary<int, string> GetBrandList()
        {
            Dictionary<int, string> brandList = new Dictionary<int, string>();
            using (SqlCommand command = new SqlCommand("select BrandId, BrandName from dbo.Brands where BrandId > 0", CnLocal))
            {
                command.CommandType = CommandType.Text;
                CnLocal.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    brandList.Add(Convert.ToInt32(reader[0]), reader[1].ToString());
                }

                CnLocal.Close();
            }

            return brandList;
        }

        public virtual DataSet GetAgingReportData(string period, string locations, string brand, string condition)
        {
            SqlDataAdapter adapter = new SqlDataAdapter();
            DataSet ds = new DataSet();
            using (SqlCommand command = new SqlCommand("dbo.GetAgingReportData", CnLocal))
            {
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.AddWithValue("Period", period);
                command.Parameters.AddWithValue("Locations", locations);
                command.Parameters.AddWithValue("Brand", brand);
                command.Parameters.AddWithValue("Condition", condition);
                CnLocal.Open();

                adapter.SelectCommand = command;
                adapter.Fill(ds, "AgingReportData");
                CnLocal.Close();
            }

            return ds;
        }

        public Dictionary<string, string> GetLocationsList()
        {
            Dictionary<string, string> locationList = new Dictionary<string, string>();
            locationList.Add("ALL", "ALL");
            using (SqlCommand command = new SqlCommand("select distinct RTRIM(Locations) as Locations from dbo.AgingInventory", CnLocal))
            {
                command.CommandType = CommandType.Text;
                CnLocal.Open();
                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    locationList.Add(reader[0].ToString(), reader[0].ToString());
                }

                CnLocal.Close();
            }

            return locationList;
        }

        public string deleteItem(string barcode)
        {
            string errMessage = string.Empty;
            string query = String.Format("DELETE FROM dbo.Items where Barcode = @barcode");

            try
            {
                using (SqlCommand command = new SqlCommand(query, CnLocal))
                {
                    command.CommandType = CommandType.Text;
                    command.Parameters.Add("barcode", SqlDbType.Char, 15).Value = barcode;
                    command.CommandTimeout = 900;
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

        public List<SelectListItem> GetPopulateFilterList(string dimension, string condition)
        {
            List<SelectListItem> items = new List<SelectListItem>();

            try
            {
                using (SqlCommand command = new SqlCommand("dbo.GetPopulateFilterList", CnLocal))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("dimension", dimension);
                    command.Parameters.AddWithValue("condition", condition);
                    command.CommandTimeout = 900;
                    CnLocal.Open();
                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        items.Add(new SelectListItem {
                            Text = reader[0].ToString(),
                            Value = reader[0].ToString()
                        });
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return items;
        }
    }
}
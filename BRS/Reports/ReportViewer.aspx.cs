using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.Reporting.WebForms;

namespace BRS.Reports
{
    public partial class ReportViewer : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                string dparam = Request.QueryString["dparam"].ToString();
                string dtparam = Request.QueryString["dtparam"].ToString();
                LoadReport(dparam, dtparam);
            }
        }

        private void LoadReport(string dparam, string dtparam)
        {
            var reportparam = (dynamic)HttpContext.Current.Session["ReportParam"];
            if (reportparam != null && !string.IsNullOrEmpty(reportparam.RptFileName))
            {
                Page.Title = "Report | " + reportparam.ReportTitle;
                DataTable dt = reportparam.DataSource;
                if (dt.Rows.Count > 0)
                {
                    GenerateReportDocument(reportparam, dt, dparam, dtparam);
                }
                else
                {
                    ShowErrorMessage();
                }
            }
        }

        private void GenerateReportDocument(dynamic reportparam, DataTable data, string dparam, string dtparam)
        {
            string dsName = reportparam.DataSetName;
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource(dsName, data));
            ReportViewer1.LocalReport.ReportPath = Server.MapPath($@"rpt/{reportparam.RptFileName}");
            ReportViewer1.ZoomMode = ZoomMode.FullPage;
            ReportParameter[] parameters = new ReportParameter[3];
            parameters[0] = new ReportParameter("prmRowField", dparam, true);
            parameters[1] = new ReportParameter("prmDataField", dtparam, true);
            parameters[2] = new ReportParameter("prmLocation", reportparam.prmLocation);
            ReportViewer1.LocalReport.SetParameters(parameters);
            ReportViewer1.DataBind();
            ReportViewer1.LocalReport.Refresh();
        }

        private void ShowErrorMessage()
        {
            ReportViewer1.LocalReport.DataSources.Clear();
            ReportViewer1.LocalReport.DataSources.Add(new ReportDataSource("", new DataTable()));
            ReportViewer1.LocalReport.ReportPath = Server.MapPath("~" + $@"Reports/rpt/blank.rdlc");
            ReportViewer1.ZoomMode = ZoomMode.FullPage;
            ReportViewer1.DataBind();
            ReportViewer1.LocalReport.Refresh();
        }
    }
}
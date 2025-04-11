$(document).ready(function () {
    $("#btnGenerate").click(function () {
        ReportManager.GenerateReport();
    });
});

var ReportManager = {
    GenerateReport: function () {
        var period = document.getElementById("YMDate").value;
        var locarray = $("#locations").val();
        var dparam = document.getElementById("dimsparam").value;
        var dtparam = document.getElementById("dataparam").value;
        var filterfield = document.getElementById("filterField").value;
        var filtervalue = document.getElementById("filterValue").value;
        
        if (locarray.length == 0 || dparam == '' || dtparam == '')
            swal.fire('Error', 'Please select locations / dimension / data first!', 'error');
        else {
            var jsonParam = "'period':'" + period + "','locparam':'" + locarray + "','filterfield':'" + filterfield + "','filtervalue':'" + filtervalue + "'";
            var serviceUrl = "../Raging/GetAgingReport";
            
            ReportManager.GetReport(serviceUrl, jsonParam, dparam, dtparam, onFailed);
        }
        function onFailed(error) {
            swal.fire('Error', error, 'error');
        }
    },

    GetReport: function (serviceUrl, jsonParam, dparam, dtparam, errorCallback) {
        $.ajax({
            url: serviceUrl,
            async: false,
            type: "POST",
            data: "{" + jsonParam + "}",
            contentType: "application/json; charset=utf-8",
            success: function () {
                window.open('../Reports/ReportViewer.aspx?dparam=' + dparam + '&dtparam=' + dtparam, '_newtab');
            },
            error: errorCallback
        });
    }
};
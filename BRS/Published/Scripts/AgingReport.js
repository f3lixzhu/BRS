$(document).ready(function () {
    $("#btnGenerate").click(function () {
        ReportManager.GenerateReport();
    });
});

var ReportManager = {
    GenerateReport: function () {
        var locparam = document.getElementById("locparam").value;
        var dparam = document.getElementById("dimsparam").value;
        var dtparam = document.getElementById("dataparam").value;
        if (locparam == '' || dparam == '' || dtparam == '')
            swal.fire('Error', 'Please select locations / dimension / data first!', 'error');
        else {
            var jsonParam = "'locparam':'" + locparam + "'";
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
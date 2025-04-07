function sendForm(ctl, e, btnname) {
        e.preventDefault();
        swal.fire({
            title: "Confirm",
            html: "Are you sure?",
            type: "info",
            showCancelButton: true,
            confirmButtonColor: "#DD6B55"
        }).then(result => {
            if (result.isConfirmed) {
                //trigger button click
                $(ctl).prop("name", btnname);
                $(ctl).removeAttr("onclick");
                $(ctl).click();
            }
        });
}

function displayAlertSuccess(str) {
    swal.fire('Excellence', str, 'success');
}

function displayAlertError(str){
    swal.fire('Error', str, 'error');
}

function OpenModalPopUp() {
    var data = $(this).data("item");
    document.getElementById("userId").value = data.split(";")[0];
    document.getElementById("userName").value = data.split(";")[1];
    document.getElementById("brandId").value = data.split(";")[2];
    $("#active").prop("checked", (data.split(";")[3] === 'True'));
    $("#myBox2").modal();
}

$(document).ready(function () {
    $('.datepickerYM')
        .wrap('<div class="input-group">')
        .datepicker({
            maxDate: 0,
            dateFormat: 'yy.mm',
            changeMonth: true,
            changeYear: true,
            showOn: "both"
        })
        .next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select Date",
            text: false
        })
        .addClass("btn btn-default")
        .wrap('<span class="input-group-btn">')
        .find('ui-button-text')
        .css({
            'visibility': 'hidden',
            'display': 'inline'
        });

    $('.datepickerDYM')
        .wrap('<div class="input-group">')
        .datepicker({
            maxDate: 0,
            dateFormat: 'dd.mm.yy',
            changeDate: true,
            changeMonth: true,
            changeYear: true,
            showOn: "both"
        })
        .next("button").button({
            icons: { primary: "ui-icon-calendar" },
            label: "Select Date",
            text: false
        })
        .addClass("btn btn-default")
        .wrap('<span class="input-group-btn">')
        .find('ui-button-text')
        .css({
            'visibility': 'hidden',
            'display': 'inline'
        });

    $(document).on("click", ".btnSelectStore", function () {
        var id = $(this).attr("id");
        document.getElementById("store").value = id;
    });

    $(document).on("click", ".btnSelectShowroom", function () {
        var id = $(this).attr("id");
        $("#showroom").append('<option selected="selected">' + id + '</option>');
    });

    $(document).on("click", ".btnDeleteShowroom", function () {
        $("#showroom").value = null;
    });

    $(document).on("click", ".usrDetails", OpenModalPopUp);
});

jQuery(function ($) {
    $(document).ajaxSend(function () {
        $("#overlay").fadeIn(300);
    });

    $("#button").click(function () {
        $.ajax({
            type: 'GET',
            success: function (data) {
                console.log(data);
            }
        }).done(function () {
            setTimeout(function () {
                $("#overlay").fadeOut(300);
            }, 900000);
        });
    });

    $('#generate').click(function () {
        $.ajax({
            type: 'GET',
            success: function (data) {
                console.log(data);
            }
        }).done(function () {
            setTimeout(function () {
                $("#overlay").fadeOut(300);
            }, 2000);
        });
    });
});
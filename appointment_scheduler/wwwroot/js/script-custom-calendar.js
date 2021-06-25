var routeURL = location.protocol + "//" + location.host;
$(document).ready(function () {
    // this fills the start date input w/ a default value
    // and adds the date and time picker to the input as well
    $("#appointmentDate").kendoDateTimePicker({
        value: new Date(),
        dateInput: false
    });

    InitializeCalendar();
});
var calendar;
function InitializeCalendar() {
    try {


        var calendarEl = document.getElementById('calendar');
        if (calendarEl != null) {
            calendar = new FullCalendar.Calendar(calendarEl, {
                initialView: 'dayGridMonth',
                headerToolbar: {
                    left: 'prev,next,today',
                    center: 'title',
                    right: 'dayGridMonth,timeGridWeek,timeGridDay'
                },
                selectable: true,
                editable: false,
                // fires when a day on the calender is selected
                select: function (event) {
                    onShowModal(event, null);
                },
                eventDisplay: 'block', //displays the colored background around the
                                       // appointment being displayed on the calendar

                // i'm thinking that this event runs automatically
                events: function (fetchInfo, successCallback, failureCallback) {
                    $.ajax({
                        url: routeURL + '/api/Appointment/GetCalendarData?doctorId=' + $("#doctorId").val(),
                        type: 'GET',
                        dataType: 'JSON',
                        success: function (response) {
                            var events = [];
                            if (response.status === 1) {
                                $.each(response.dataenum, function (i, data) {
                                    events.push({
                                        title: data.title,
                                        description: data.description,
                                        start: data.startDate,
                                        end: data.endDate,
                                        backgroundColor: data.isDoctorApproved ? "#28a745" : "#dc3545",
                                        borderColor: "#162466",
                                        textColor: "white",
                                        id: data.id
                                    });
                                })
                            }
                            successCallback(events);
                        },
                        error: function (xhr) {
                            $.notify("Error", "error");
                        }
                    });
                },
                // runs when someone clicks on an appointment
                eventClick: function (info) {
                    getEventDetailsByEventId(info.event);
                }
            });
            calendar.render();
        }

    }
    catch (e) {
        alert(e);
    }

}


function onShowModal(obj, isEventDetail) {
    if (isEventDetail != null) {

        // these fill the inputs in the modal
        $("#title").val(obj.title);
        $("#description").val(obj.description);
        $("#appointmentDate").val(obj.startDate);
        $("#duration").val(obj.duration);
        $("#doctorId").val(obj.doctorId);
        $("#patientId").val(obj.patientId);
        $("#id").val(obj.id);
        $("#lblPatientName").html(obj.patientName);
        $("#lblDoctorName").html(obj.doctorName);

        if (obj.isDoctorApproved) {
            $("#lblStatus").html('Approved');
            $("#btnConfirm").addClass("d-none"); // hides the button
            $("#btnSubmit").addClass("d-none");
        }
        else {
            $("#lblStatus").html('Pending');
            $("#btnConfirm").removeClass("d-none");
            $("#btnSubmit").removeClass("d-none");
        }
        $("#btnDelete").removeClass("d-none");
    }
    else // if someone clicks on a day on the calendar to make an appointment 
    {
        // can use "moment" since we added it in _Layout
        
        // populating the start date in the modal with the date the user selects
        // on the calendar
        $("#appointmentDate").val(obj.startStr + " " + new moment().format("hh:mm A"));
        $("#id").val(0);
        $("#btnDelete").addClass("d-none");
        $("#btnSubmit").removeClass("d-none");
    }
    $("#appointmentInput").modal("show"); // "#appointmentInput" comes from 
}                                         // _AddEditAppointment.cshtml

function onCloseModal() {
    // clears all the values in the modal
    $("#apointmentForm")[0].reset();
    $("#id").val(0);
    $("#title").val('');
    $("#description").val('');
    $("#appointmentDate").val('');
   
    $("#appointmentInput").modal("hide");
}

function onSubmitForm() {
    if (checkValidation()) {
        var requestData = {
            // these values all come from _AddEditAppointment except for doctor
            // which comes from Appointment Controller Index page
            Id: parseInt($("#id").val()),
            Title: $("#title").val(),
            Description: $("#description").val(),
            StartDate: $("#appointmentDate").val(),
            Duration: $("#duration").val(),
            DoctorId: $("#doctorId").val(),
            PatientId: $("#patientId").val(),
        };

        $.ajax({
            url: routeURL + '/api/Appointment/SaveCalendarData',
            type: 'POST',
            data: JSON.stringify(requestData),
            contentType: 'application/json',
            success: function (response) {
                if (response.status === 1 || response.status === 2) {
                    calendar.refetchEvents(); // so the calendar is refreshed and the
                                              // newly added appointment appears right
                                              // away
                    $.notify(response.message, "success");
                    onCloseModal();
                }
                else {
                    $.notify(response.message, "error");
                }
            },
            error: function (xhr) {
                $.notify("Error", "error");
            }
        });
    }
}

function checkValidation() {
    var isValid = true;
    if ($("#title").val() === undefined || $("#title").val() === "") {
        isValid = false;
        $("#title").addClass('error');
    }
    else {
        $("#title").removeClass('error');
    }

    if ($("#appointmentDate").val() === undefined || $("#appointmentDate").val() === "") {
        isValid = false;
        $("#appointmentDate").addClass('error');
    }
    else {
        $("#appointmentDate").removeClass('error');
    }

    return isValid;

}

function getEventDetailsByEventId(info) {
    $.ajax({
        url: routeURL + '/api/Appointment/GetCalendarDataById/' + info.id,
        type: 'GET',
        dataType: 'JSON',
        success: function (response) {

            if (response.status === 1 && response.dataenum !== undefined) {
                onShowModal(response.dataenum, true);
            }
            successCallback(events);
        },
        error: function (xhr) {
            $.notify("Error", "error");
        }
    });
}

function onDoctorChange() {
    calendar.refetchEvents();
}

function onDeleteAppointment() {
    var id = parseInt($("#id").val());
    $.ajax({
        url: routeURL + '/api/Appointment/DeleteAppoinment/' + id,
        type: 'GET',
        dataType: 'JSON',
        success: function (response) {

            if (response.status === 1) {
                $.notify(response.message, "success");
                calendar.refetchEvents();
                onCloseModal();
            }
            else {

                $.notify(response.message, "error");
            }
        },
        error: function (xhr) {
            $.notify("Error", "error");
        }
    });
}

function onConfirm() {
    var id = parseInt($("#id").val());
    $.ajax({
        url: routeURL + '/api/Appointment/ConfirmEvent/' + id,
        type: 'GET',
        dataType: 'JSON',
        success: function (response) {

            if (response.status === 1) {
                $.notify(response.message, "success");
                calendar.refetchEvents();
                onCloseModal();
            }
            else {

                $.notify(response.message, "error");
            }
        },
        error: function (xhr) {
            $.notify("Error", "error");
        }
    });
}
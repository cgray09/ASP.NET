var dataTable;

$(document).ready(function () {
    var url = window.location.search;   // the websites url (the a-tags on the index page that calls this
                                        // appends to the url "approved", "pending" ect) 

    if (url.includes("approved")) {
        loadDataTable("GetAllApprovedOrders");
    }
    else {
        if (url.includes("pending")) {
            loadDataTable("GetAllPendingOrders");
        }
        else {
            loadDataTable("GetAllOrders");
        }

    }
});

function loadDataTable(url) {
    
    dataTable = $('#tblData').DataTable({
        "ajax": {
            "url": "/admin/order/" + url, // call to the methods in the controller using the url
            "type": "GET",
            "datatype": "json"
        },
        "columns": [
            { "data": "name", "width": "20%" },
            { "data": "phone", "width": "20%" },
            { "data": "email", "width": "15%" },
            { "data": "serviceCount", "width": "15%" },
            { "data": "status", "width": "15%" },
            {
                "data": "id",
                "render": function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/order/Details/${data}" class='btn btn-success text-white' style='cursor:pointer; width:100px;'>
                                    <i class='far fa-edit'></i> Details
                                </a>
                                
                                
                            </div>
                            `;
                }, "width": "15%"
            }
        ],
        "language": {
            "emptyTable":"No records found."
        },
        "width":"100%"
    });
}
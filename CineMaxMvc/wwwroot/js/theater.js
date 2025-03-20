$(document).ready(function () {
    // Initialize DataTable
    var table = $('#theatersTable').DataTable({
        procesing: true,
        ajax: {
            url: '/Admin/Theater/GetTheaters',
            type: 'POST',
            data: function (d) {
                return {
                    provinceId: $('#provinceFilter').val() || null
                };
            },
            
        },
        columns: [
            {
                data: 'name',
                render: function (data, type, row) {
                    return '<div class="d-flex flex-column">' +
                        '<span class="fw-bold">' + data + '</span>' +
                        '<small class="text-muted">' + row.address + '</small>' +
                        '</div>';
                }
            },
            { data: 'province.name' },
            {
                data: 'phone',
                render: function (data, type, row) {
                    return '<div class="d-flex flex-column">' +
                        '<span>' + data + '</span>' +
                        '<small class="text-muted">' + row.email + '</small>' +
                        '</div>';
                }
            },
            {
                data: null,
                render: function (data, type, row) {
                    if (row.openingTime && row.closingTime) {
                        return row.openingTime + ' - ' + row.closingTime;
                    } else {
                        return '<span class="text-muted">Not specified</span>';
                    }
                }
            },
            {
                data: 'isActive',
                render: function (data, type, row) {
                    return data ?
                        '<span class="badge bg-success">Active</span>' :
                        '<span class="badge bg-secondary">Inactive</span>';
                }
            },
            {
                data: 'createdAt',
                render: function (data) {
                    return new Date(data).toLocaleDateString();
                }
            },
            {
                data: 'id',
                render: function (data, type, row) {
                    return `<div class="btn-group">
                <a href="Theater/Detail/${data}" class="btn btn-sm btn-outline-primary" title="View Details">
                  <i class="fas fa-eye"></i>
                </a>
                <a href="Theater/Upsert/${data}" class="btn btn-sm btn-outline-primary" title="Edit">
                  <i class="fas fa-edit"></i>
                </a>
                <button type="button" class="btn btn-sm btn-outline-danger deleteBtn" title="Delete"
                  data-bs-toggle="modal" data-bs-target="#deleteModal" 
                  data-id="${data}" data-name="${row.name}">
                  <i class="fas fa-trash"></i>
                </button>
              </div>`;
                }
            }
        ],
        order: [[0, 'asc']],
        language: {
            search: "Search:",
            lengthMenu: "Show _MENU_ theaters per page",
            info: "Showing _START_ to _END_ of _TOTAL_ theaters",
            infoEmpty: "No theaters found",
            emptyTable: "No theaters available"
        }
    });

    // Filter event handling
    $('#provinceFilter').change(function () {
        table.ajax.reload();
    });

    // Refresh button
    $('#refreshTable').click(function () {
        table.ajax.reload();
    });

    // Delete theater functionality
    //var theaterId;

    //$(document).on('click', '.deleteBtn', function () {
    //    theaterId = $(this).data('id');
    //    $('#theaterNameToDelete').text($(this).data('name'));
    //});

    //$('#confirmDelete').click(function () {
    //    $.ajax({
    //        url: '@Url.Action("Delete", "Theaters")',
    //        type: 'POST',
    //        data: { id: theaterId },
    //        success: function (response) {
    //            if (response.success) {
    //                $('#deleteModal').modal('hide');
    //                table.ajax.reload();
    //                // Show success toast/notification
    //                toastr.success('Theater deleted successfully');
    //            } else {
    //                // Show error toast/notification
    //                toastr.error(response.message || 'Error deleting theater');
    //            }
    //        },
    //        error: function () {
    //            toastr.error('An error occurred while deleting the theater');
    //        }
    //    });
    //});
});
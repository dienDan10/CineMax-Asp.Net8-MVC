$(document).ready(function () {
    var dataTable = $('#concessionsTable').DataTable({
        ajax: {
            url: '/Admin/Concession/GetAll',
        },
        columns: [
            {
                data: 'imageUrl',
                render: function (data) {
                    return `<img src="${data || "/images/no-image.png"}" class="img-thumbnail" style="width:60px; height:60px; object-fit:cover;" />`;
                },
                orderable: false
            },
            { data: 'name' },
            {
                data: 'description',
                render: function (data) {
                    return data.length > 50 ? data.substring(0, 50) + '...' : data;
                }
            },
            {
                data: 'price',
                render: function (data) {
                    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(data);
                }
            },
            {
                data: 'isActive',
                render: function (data) {
                    return data ?
                        `<span class="badge rounded-pill bg-success px-3 py-2">Active</span>` :
                        `<span class="badge rounded-pill bg-danger px-3 py-2">Inactive</span>`;
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
                    return `
                        <div class="d-flex gap-2">
                            <a href="/Admin/Concession/Upsert/${data}" class="btn btn-sm btn-primary rounded-pill">
                                <i class="fas fa-edit"></i>
                            </a>
                            <button class="btn btn-sm btn-${row.isActive ? 'warning' : 'success'} rounded-pill toggle-status" 
                                    data-id="${data}" data-active="${row.isActive}">
                                <i class="fas fa-${row.isActive ? 'ban' : 'check'}"></i>
                            </button>
                        </div>
                    `;
                },
                orderable: false
            }
        ],
        language: { emptyTable: "No concession items found" },
        responsive: true,
        order: [[1, 'asc']],
        pagingType: "full_numbers"
    });

    // 🔄 Toggle Status with Swal Confirmation
    $('#concessionsTable').on('click', '.toggle-status', function () {
        var btn = $(this);
        var id = btn.data('id');
        var isActive = btn.data('active');

        Swal.fire({
            title: 'Are you sure?',
            text: `You are about to ${isActive ? 'disable' : 'enable'} this concession.`,
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#d33',
            cancelButtonColor: '#3085d6',
            confirmButtonText: `Yes, ${isActive ? 'disable' : 'enable'} it!`
        }).then((result) => {
            if (result.isConfirmed) {
                $.ajax({
                    url: '/Admin/Concession/ToggleStatus',
                    type: 'POST',
                    data: JSON.stringify(id),
                    contentType: 'application/json',
                    success: function (response) {
                        if (response.success) {
                            toastr.success(response.message);

                            // Update button text & color dynamically
                            btn.toggleClass('btn-warning btn-success')
                                .html(`<i class="fas fa-${isActive ? 'check' : 'ban'}"></i>`)
                                .data('active', !isActive);

                            // Update status badge dynamically
                            let statusCell = btn.closest('tr').find('td:nth-child(5) span');
                            statusCell.toggleClass('bg-success bg-danger')
                                .text(isActive ? 'Inactive' : 'Active');
                        } else {
                            toastr.error(response.message);
                        }
                    },
                    error: function () {
                        toastr.error('An error occurred while updating status.');
                    }
                });
            }
        });
    });
});
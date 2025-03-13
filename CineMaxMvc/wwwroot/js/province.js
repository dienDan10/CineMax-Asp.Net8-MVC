$(document).ready(function () {
    loadDataTable();
})

const loadDataTable = () => {
    $('#provinceTable').DataTable({
        ajax: '/admin/province/getall',
        columns: [
            { data: 'name', width: '70%' },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Province/Upsert/${data}" class="btn btn-warning btn-sm">
                                    <i class="fa-solid fa-pen-to-square"></i> Edit
                                </a>
                                <a onclick=Delete("/Admin/Province/Delete/${data}") class="btn btn-danger btn-sm">
                                    <i class="fa-solid fa-trash"></i> Delete
                                </a>
                            </div>`;
                }, width: '30%'
            }
        ]
    });
}

const Delete = (url) => {

    Swal.fire({
        title: "Are you sure?",
        text: "You won't be able to revert this!",
        icon: "warning",
        showCancelButton: true,
        confirmButtonColor: "#3085d6",
        cancelButtonColor: "#d33",
        confirmButtonText: "Yes, delete it!"
    }).then((result) => {
        if (result.isConfirmed) {
            // Call the delete method
            $.ajax({
                type: 'DELETE',
                url: url,
                success: function (data) {
                    if (data.success) {
                        $('#provinceTable').DataTable().ajax.reload();
                        toastr.success(data.message);
                    } else {
                        toastr.error(data.message);
                    }
                }
            });
        }
    });
}
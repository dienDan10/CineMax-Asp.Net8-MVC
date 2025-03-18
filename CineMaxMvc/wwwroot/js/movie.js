
$(document).ready(function () {
    // Initialize DataTable with custom styling
    var table = $('#movieTable').DataTable({
        ajax: {
            url: '/Admin/Movies/GetAll',
        },
        columns: [
            {
                data: null,
                render: function (data) {
                    return `<div class="d-flex align-items-center">
                      <div class="me-3">
                        <img src="${data.posterUrl || '/images/no-poster.jpg'}" class="rounded" width="50" alt="${data.title}">
                      </div>
                      <div>
                        <h6 class="mb-0">${data.title}</h6>
                        <small class="text-muted">Dir: ${data.director}</small>
                      </div>
                    </div>`;
                }
            },
            { data: 'genre' },
            {
                data: 'releaseDate',
                render: function (data) {
                    return new Date(data).toLocaleDateString();
                }
            },
            {
                data: 'isActive',
                className: 'text-center',
                render: function (data) {
                    return data ?
                        '<span class="badge bg-success rounded-pill px-3 py-2">Active</span>' :
                        '<span class="badge bg-secondary rounded-pill px-3 py-2">Inactive</span>';
                }
            },
            {
                data: 'id',
                className: 'text-center',
                render: function (data) {
                    return `<div class="btn-group" role="group">
                      <a href="/Admin/Movies/Upsert/${data}" class="btn btn-sm btn-outline-primary">
                        <i class="fas fa-edit"></i>
                      </a>
                      <button class="btn btn-sm btn-outline-danger delete-movie" data-id="${data}">
                        <i class="fas fa-trash-alt"></i>
                      </button>
                      <a href="/Admin/Movies/Detail/${data}" class="btn btn-sm btn-outline-info">
                        <i class="fas fa-info-circle"></i>
                      </a>
                    </div>`;
                }
            }
        ],
        language: {
            emptyTable: '<div class="text-center py-5"><i class="fas fa-film fa-3x text-muted mb-3"></i><p>No movies found</p></div>'
        },
        responsive: true,
        order: [[2, 'desc']], // Sort by release date, newest first
        dom: '<"row"<"col-sm-12"tr>><"row"<"col-sm-5"i><"col-sm-7"p>>',
        pageLength: 10,
        drawCallback: function () {
            $('#totalMovies').text(this.api().page.info().recordsTotal);
        }
    });

    // Custom search functionality
    $('#movieSearch').on('keyup', function () {
        table.search(this.value).draw();
    });

    // Filter buttons functionality
    $('#filterAll').on('click', function () {
        table.search('').columns(3).search('').draw();
        updateFilterButtons(this);
    });

    $('#filterActive').on('click', function () {
        table.columns(3).search('Active').draw();
        updateFilterButtons(this);
    });

    $('#filterInactive').on('click', function () {
        table.columns(3).search('Inactive').draw();
        updateFilterButtons(this);
    });

    // Refresh button
    $('#refreshTable').on('click', function () {
        table.ajax.reload();
    });

    // Delete movie confirmation
    $('#movieTable').on('click', '.delete-movie', function () {
        var id = $(this).data('id');
        Swal.fire({
            title: 'Are you sure?',
            text: "You won't be able to revert this!",
            icon: 'warning',
            showCancelButton: true,
            confirmButtonColor: '#3085d6',
            cancelButtonColor: '#d33',
            confirmButtonText: 'Yes, delete it!'
        }).then((result) => {
            if (result.isConfirmed) {
                // Send delete request
                $.ajax({
                    url: `/Admin/Movies/Delete/${id}`,
                    type: 'DELETE',
                    success: function () {
                        table.ajax.reload();
                        toastr.success('Movie deleted successfully');
                    },
                    error: function () {
                        toastr.error('Failed to delete movie');
                    }
                });
            }
        });
    });

    // Helper function to update filter button states
    function updateFilterButtons(activeButton) {
        $('#filterAll, #filterActive, #filterInactive').removeClass('active btn-primary').addClass('btn-outline-primary');
        $(activeButton).removeClass('btn-outline-primary').addClass('active btn-primary');
    }

    // Initialize with All filter active
    $('#filterAll').addClass('active btn-primary').removeClass('btn-outline-primary');
});


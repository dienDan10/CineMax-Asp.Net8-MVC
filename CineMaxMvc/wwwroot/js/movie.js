$(document).ready(function () {
    loadDataTable();
})

const loadDataTable = () => {
    $('#movieTable').DataTable({
        ajax: '/admin/movies/getall',
        columns: [
            {
                data: null,
                render: function (data) {
                    return `<div class="d-flex align-items-center">
                                <img src="${data.posterUrl}" alt="${data.title}" class="rounded" width="60" height="90" style="object-fit: cover; margin-right: 10px;">
                                <span>${data.title}</span>
                            </div>`;
                },
                width: '40%'
            },
            { data: 'genre', width: '15%' },
            { data: 'director', width: '15%' },
            { data: 'cast', width: '20%' },
            { data: 'releaseDate', width: '10%' },
            {
                data: 'id',
                render: function (data) {
                    return `<div class="text-center">
                                <a href="/Admin/Movies/Upsert/${data}" class="btn btn-warning btn-sm">
                                    <i class="fa-solid fa-pen-to-square"></i> Edit
                                </a>
                                
                            </div>`;
                },
                width: '10%'
            }
        ]
    });
}


$(document).ready(function () {
    loadDataTable();
})

const loadDataTable = () => {
    $('#userTable').DataTable({
        ajax: '/admin/user/getall',
        columns: [
            { data: 'name', width: '20%' },
            { data: 'email', width: '20%' },
            { data: 'phoneNumber', width: '20%' },
            { data: 'role', width: '20%' },
            {
                data: {id: 'id', lockoutEnd: 'lockoutEnd'},
                render: function (data) {
                    const lockoutEnd = data.lockoutEnd ? new Date(data.lockoutEnd) : null;
                    const isLocked = lockoutEnd && lockoutEnd > new Date();

                    return `
                        <div class="text-center">
                            <button onclick="toggleLock('${data.id}')" class="btn btn-sm ${isLocked ? 'btn-danger' : 'btn-success'}">
                                <i class="fa ${isLocked ? 'fa-lock' : 'fa-unlock'}"></i> ${isLocked ? 'Unlock' : 'Lock'}
                            </button>
                            <a href="/admin/user/RoleManagement/${data.id}" class="btn btn-warning btn-sm">
                        <i class="fas fa-user-cog"></i> Permission
                    </a>
                        </div>`;
                    
                }, width: '20%'
            }
        ]
    });
}


const toggleLock = (userId) => {
    Swal.fire({
        title: 'Are you sure?',
        text: "You are about to lock/unlock this user!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#d33',
        cancelButtonColor: '#3085d6',
        confirmButtonText: 'Yes, do it!'
    }).then((result) => {
        if (result.isConfirmed) {
            $.ajax({
                type: "POST",
                url: "/admin/user/lockunlock",
                data: JSON.stringify(userId),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    if (response.success) {
                        toastr.success(response.message);
                        $('#userTable').DataTable().ajax.reload();
                    } else {
                        toastr.error(response.message);
                    }
                }
            });
        }
    });
};

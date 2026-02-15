const { ref, onMounted } = Vue;

export default {
    setup() {
        const users = ref([]);
        const loading = ref(true);

        const fetchUsers = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/user/getall');
                if (res.ok) {
                    const result = await res.json();
                    users.value = result.data || result;
                }
            } finally { loading.value = false; }
        };

        const deleteUser = async (id) => {
            const result = await Swal.fire({
                title: 'Xóa người dùng?',
                text: "Hành động này không thể hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'Xóa ngay'
            });

            if (result.isConfirmed) {
                const res = await fetch(`/api/user/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    users.value = users.value.filter(u => u.id !== id);
                    Swal.fire('Đã xóa', '', 'success');
                } else {
                    Swal.fire('Lỗi', 'Không thể xóa (có thể là Super Admin)', 'error');
                }
            }
        };

        const resetPassword = async (id, name) => {
            const { value: newPassword } = await Swal.fire({
                title: `Đổi mật khẩu cho ${name}`,
                input: 'password',
                inputLabel: 'Nhập mật khẩu mới',
                inputPlaceholder: 'Nhập mật khẩu mới',
                showCancelButton: true,
                confirmButtonText: 'Đổi mật khẩu',
                cancelButtonText: 'Hủy'
            });

            if (newPassword) {
                try {
                    const res = await fetch(`/api/user/resetpassword/${id}`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ newPassword })
                    });

                    if (res.ok) {
                        Swal.fire('Thành công', 'Mật khẩu đã được thay đổi', 'success');
                    } else {
                        const err = await res.json();
                        Swal.fire('Lỗi', err.message || 'Không thể đổi mật khẩu', 'error');
                    }
                } catch (e) {
                    Swal.fire('Lỗi', 'Lỗi kết nối server', 'error');
                }
            }
        };

        onMounted(fetchUsers);
        return { users, loading, deleteUser, resetPassword };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">Người dùng hệ thống</h2>
                <router-link to="/Admin/User/Create" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90">
                    + Thêm người dùng
                </router-link>
            </div>
             <div class="rounded-sm border border-stroke bg-white shadow-default">
                <table class="w-full table-auto">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="py-4 px-4 font-medium text-black pl-8">Email</th>
                            <th class="py-4 px-4 font-medium text-black">Họ tên</th>
                            <th class="py-4 px-4 font-medium text-black">Vai trò</th>
                            <th class="py-4 px-4 font-medium text-black">Ngày tạo</th>
                            <th class="py-4 px-4 font-medium text-black text-right">Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="loading"><td colspan="5" class="p-4 text-center">Đang tải...</td></tr>
                        <tr v-else v-for="user in users" :key="user.id" class="border-b border-stroke hover:bg-gray-50">
                            <td class="py-4 px-4 pl-8 font-medium text-black">{{ user.email }}</td>
                            <td class="py-4 px-4 text-sm">{{ user.fullName || '---' }}</td>
                            <td class="py-4 px-4 text-sm">
                                <span class="bg-blue-100 text-blue-800 text-xs font-semibold px-2 py-1 rounded">{{ user.role }}</span>
                            </td>
                            <td class="py-4 px-4 text-sm text-slate-500">{{ new Date(user.createdDate).toLocaleDateString('vi-VN') }}</td>
                            <td class="py-4 px-4 text-right">
                                <button @click="resetPassword(user.id, user.fullName || user.email)" class="text-warning hover:underline mr-4 text-sm">
                                    <i class="bi bi-key"></i> Đổi MK
                                </button>
                                <router-link :to="'/Admin/User/Edit/' + user.id" class="text-primary hover:underline mr-4 text-sm">Sửa</router-link>
                                <button @click="deleteUser(user.id)" class="text-danger hover:underline text-sm">Xóa</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
             </div>
        </div>
    `
};

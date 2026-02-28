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
                title: 'XÃ³a ngÆ°á»i dÃ¹ng?',
                text: "HÃ nh Ä‘á»™ng nÃ y khÃ´ng thá»ƒ hoÃ n tÃ¡c!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'XÃ³a ngay'
            });

            if (result.isConfirmed) {
                const res = await fetch(`/api/user/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    users.value = users.value.filter(u => u.id !== id);
                    Swal.fire('ÄÃ£ xÃ³a', '', 'success');
                } else {
                    Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ xÃ³a (cÃ³ thá»ƒ lÃ  Super Admin)', 'error');
                }
            }
        };

        const resetPassword = async (id, name) => {
            const { value: newPassword } = await Swal.fire({
                title: `Äá»•i máº­t kháº©u cho ${name}`,
                input: 'password',
                inputLabel: 'Nháº­p máº­t kháº©u má»›i',
                inputPlaceholder: 'Nháº­p máº­t kháº©u má»›i',
                showCancelButton: true,
                confirmButtonText: 'Äá»•i máº­t kháº©u',
                cancelButtonText: 'Há»§y'
            });

            if (newPassword) {
                try {
                    const res = await fetch(`/api/user/resetpassword/${id}`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json' },
                        body: JSON.stringify({ newPassword })
                    });

                    if (res.ok) {
                        Swal.fire('ThÃ nh cÃ´ng', 'Máº­t kháº©u Ä‘Ã£ Ä‘Æ°á»£c thay Ä‘á»•i', 'success');
                    } else {
                        const err = await res.json();
                        Swal.fire('Lá»—i', err.message || 'KhÃ´ng thá»ƒ Ä‘á»•i máº­t kháº©u', 'error');
                    }
                } catch (e) {
                    Swal.fire('Lá»—i', 'Lá»—i káº¿t ná»‘i server', 'error');
                }
            }
        };

        onMounted(fetchUsers);
        return { users, loading, deleteUser, resetPassword };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">NgÆ°á»i dÃ¹ng há»‡ thá»‘ng</h2>
                <router-link to="/Admin/User/Create" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90">
                    + ThÃªm ngÆ°á»i dÃ¹ng
                </router-link>
            </div>
             <div class="rounded-sm border border-stroke bg-white shadow-default">
                <table class="w-full table-auto">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="py-4 px-4 font-medium text-black pl-8">Email</th>
                            <th class="py-4 px-4 font-medium text-black">Há» tÃªn</th>
                            <th class="py-4 px-4 font-medium text-black">Vai trÃ²</th>
                            <th class="py-4 px-4 font-medium text-black">NgÃ y táº¡o</th>
                            <th class="py-4 px-4 font-medium text-black text-right">HÃ nh Ä‘á»™ng</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="loading"><td colspan="5" class="p-4 text-center">Äang táº£i...</td></tr>
                        <tr v-else v-for="user in users" :key="user.id" class="border-b border-stroke hover:bg-gray-50">
                            <td class="py-4 px-4 pl-8 font-medium text-black">{{ user.email }}</td>
                            <td class="py-4 px-4 text-sm">{{ user.fullName || '---' }}</td>
                            <td class="py-4 px-4 text-sm">
                                <span class="bg-blue-100 text-blue-800 text-xs font-semibold px-2 py-1 rounded">{{ user.role }}</span>
                            </td>
                            <td class="py-4 px-4 text-sm text-slate-500">{{ new Date(user.createdDate).toLocaleDateString('vi-VN') }}</td>
                            <td class="py-4 px-4 text-right">
                                <button @click="resetPassword(user.id, user.fullName || user.email)" class="text-warning hover:underline mr-4 text-sm">
                                    <i class="bi bi-key"></i> Äá»•i MK
                                </button>
                                <router-link :to="'/Admin/User/Edit/' + user.id" class="text-primary hover:underline mr-4 text-sm">Sá»­a</router-link>
                                <button @click="deleteUser(user.id)" class="text-danger hover:underline text-sm">XÃ³a</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
             </div>
        </div>
    `
};


const { ref, reactive, onMounted, computed } = Vue;
const { useRoute, useRouter } = VueRouter;

export default {
    setup() {
        const route = useRoute();
        const router = useRouter();
        const isEdit = computed(() => !!route.params.id);
        const form = reactive({ email: '', password: '', fullName: '', role: 'User' });
        const loading = ref(false);

        onMounted(async () => {
            if (isEdit.value) {
                loading.value = true;
                try {
                    const res = await fetch(`/api/user/get/${route.params.id}`);
                    if (res.ok) {
                        const data = await res.json();
                        form.email = data.email;
                        form.fullName = data.fullName;
                        form.role = data.role;
                    } else {
                        Swal.fire('Lỗi', 'Không tìm thấy người dùng', 'error');
                        router.push('/Admin/User');
                    }
                } catch (e) {
                    Swal.fire('Lỗi', 'Lỗi khi tải dữ liệu người dùng', 'error');
                } finally {
                    loading.value = false;
                }
            }
        });

        const submit = async () => {
            if (!form.email || (!isEdit.value && !form.password)) {
                Swal.fire('Lỗi', 'Vui lòng nhập Email và Mật khẩu', 'error');
                return;
            }

            const url = isEdit.value ? `/api/user/update/${route.params.id}` : '/api/user/create';
            const method = 'POST'; // Assuming POST for both create and update

            try {
                const res = await fetch(url, {
                    method: method,
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(form)
                });
                if (res.ok) {
                    Swal.fire('Thành công', isEdit.value ? 'Người dùng đã được cập nhật' : 'Người dùng đã được tạo', 'success');
                    router.push('/Admin/User');
                } else {
                    const err = await res.json();
                    Swal.fire('Lỗi', err.message || 'Không thể lưu người dùng', 'error');
                }
            } catch (e) { Swal.fire('Lỗi', 'Lỗi server', 'error'); }
        };

        return { form, isEdit, loading, submit };
    },
    template: `
        <div class="max-w-xl mx-auto">
             <h2 class="text-2xl font-bold mb-6 text-black">{{ isEdit ? 'Chỉnh sửa người dùng' : 'Thêm thành viên mới' }}</h2>
             <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                <form @submit.prevent="submit" class="flex flex-col gap-4">
                     <div>
                        <label class="mb-2 block text-sm font-medium text-black">Email</label>
                        <input v-model="form.email" type="email" required class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" />
                    </div>
                    <div v-if="!isEdit">
                        <label class="mb-2 block text-sm font-medium text-black">Mật khẩu</label>
                        <input v-model="form.password" type="password" required class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" />
                    </div>
                     <div>
                        <label class="mb-2 block text-sm font-medium text-black">Họ tên</label>
                        <input v-model="form.fullName" type="text" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" />
                    </div>
                     <div>
                        <label class="mb-2 block text-sm font-medium text-black">Vai trò</label>
                        <select v-model="form.role" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none">
                            <option value="User">User</option>
                            <option value="Admin">Admin</option>
                        </select>
                    </div>
                    <div class="flex justify-end gap-3 mt-4">
                        <router-link to="/Admin/User" class="px-6 py-2 rounded border border-stroke hover:bg-gray-100">Hủy</router-link>
                        <button type="submit" class="px-6 py-2 rounded bg-primary text-white hover:bg-opacity-90">Lưu lại</button>
                    </div>
                </form>
             </div>
        </div>
    `
};
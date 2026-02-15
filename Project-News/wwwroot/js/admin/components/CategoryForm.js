const { ref, reactive, onMounted, computed } = Vue;
const { useRoute, useRouter } = VueRouter;

export default {
    setup() {
        const route = useRoute();
        const router = useRouter();
        const isEdit = computed(() => !!route.params.id);
        const form = reactive({ name: '', slug: '', description: '', parentId: null });
        const loading = ref(false);

        onMounted(async () => {
            if (isEdit.value) {
                loading.value = true;
                const res = await fetch(`/api/category/get/${route.params.id}`);
                if (res.ok) {
                    const data = await res.json();
                    Object.assign(form, data);
                }
                loading.value = false;
            }
        });

        const submit = async () => {
            const url = isEdit.value ? `/api/category/update/${route.params.id}` : '/api/category/create';

            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(form)
            });

            if (res.ok) {
                Swal.fire('Thành công', isEdit.value ? 'Đã cập nhật!' : 'Đã tạo mới!', 'success');
                router.push('/Admin/Category');
            } else {
                Swal.fire('Lỗi', 'Không thể lưu', 'error');
            }
        };

        return { form, isEdit, loading, submit };
    },
    template: `
        <div class="max-w-2xl mx-auto">
            <div class="mb-6 flex items-center gap-4">
                 <router-link to="/Admin/Category" class="text-black hover:text-primary">
                    <i class="bi bi-arrow-left text-2xl"></i>
                </router-link>
                <h2 class="text-2xl font-bold text-black">{{ isEdit ? 'Chỉnh sửa chuyên mục' : 'Thêm chuyên mục mới' }}</h2>
            </div>
            
            <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                <form @submit.prevent="submit" class="flex flex-col gap-4">
                    <div>
                        <label class="mb-2 block text-sm font-medium text-black">Tên chuyên mục</label>
                        <input v-model="form.name" type="text" required class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" />
                    </div>
                    <div>
                        <label class="mb-2 block text-sm font-medium text-black">Slug (Đường dẫn)</label>
                        <input v-model="form.slug" type="text" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" placeholder="tự-động-tạo-nếu-để-trống" />
                    </div>
                    <div>
                        <label class="mb-2 block text-sm font-medium text-black">Mô tả</label>
                        <textarea v-model="form.description" rows="3" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none"></textarea>
                    </div>
                    
                    <div class="flex justify-end gap-3 mt-4">
                        <router-link to="/Admin/Category" class="px-6 py-2 rounded border border-stroke hover:bg-gray-100">Hủy</router-link>
                        <button type="submit" class="px-6 py-2 rounded bg-primary text-white hover:bg-opacity-90">Lưu lại</button>
                    </div>
                </form>
            </div>
        </div>
    `
};

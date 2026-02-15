const { ref, onMounted } = Vue;

export default {
    setup() {
        const categories = ref([]);
        const loading = ref(true);

        const fetchCategories = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/category/getall');
                if (res.ok) categories.value = await res.json();
            } finally { loading.value = false; }
        };

        const deleteCategory = async (id) => {
            const result = await Swal.fire({
                title: 'Xóa danh mục?',
                text: "Không thể hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'Xóa'
            });

            if (result.isConfirmed) {
                const res = await fetch(`/api/category/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    categories.value = categories.value.filter(c => c.id !== id);
                    Swal.fire('Đã xóa', '', 'success');
                }
            }
        };

        onMounted(fetchCategories);
        return { categories, loading, deleteCategory };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">Danh mục</h2>
                <router-link to="/Admin/Category/Create" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90">
                    + Thêm danh mục
                </router-link>
            </div>
             <div class="rounded-sm border border-stroke bg-white shadow-default">
                <table class="w-full table-auto">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="py-4 px-4 font-medium text-black pl-8">Tên</th>
                            <th class="py-4 px-4 font-medium text-black">Slug</th>
                            <th class="py-4 px-4 font-medium text-black">Mô tả</th>
                            <th class="py-4 px-4 font-medium text-black text-right">Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="loading"><td colspan="4" class="p-4 text-center">Đang tải...</td></tr>
                        <tr v-else v-for="cat in categories" :key="cat.id" class="border-b border-stroke hover:bg-gray-50">
                            <td class="py-4 px-4 pl-8 font-medium text-black">{{ cat.name }}</td>
                            <td class="py-4 px-4 text-sm text-slate-500">{{ cat.slug }}</td>
                            <td class="py-4 px-4 text-sm">{{ cat.description }}</td>
                            <td class="py-4 px-4 text-right">
                                <router-link :to="'/Admin/Category/Edit/' + cat.id" class="text-primary hover:underline mr-4">Sửa</router-link>
                                <button @click="deleteCategory(cat.id)" class="text-danger hover:underline">Xóa</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
             </div>
        </div>
    `
};

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
                title: 'XÃ³a danh má»¥c?',
                text: "KhÃ´ng thá»ƒ hoÃ n tÃ¡c!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'XÃ³a'
            });

            if (result.isConfirmed) {
                const res = await fetch(`/api/category/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    categories.value = categories.value.filter(c => c.id !== id);
                    Swal.fire('ÄÃ£ xÃ³a', '', 'success');
                }
            }
        };

        onMounted(fetchCategories);
        return { categories, loading, deleteCategory };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">Danh má»¥c</h2>
                <router-link to="/Admin/Category/Create" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90">
                    + ThÃªm danh má»¥c
                </router-link>
            </div>
             <div class="rounded-sm border border-stroke bg-white shadow-default">
                <table class="w-full table-auto">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="py-4 px-4 font-medium text-black pl-8">TÃªn</th>
                            <th class="py-4 px-4 font-medium text-black">Slug</th>
                            <th class="py-4 px-4 font-medium text-black">MÃ´ táº£</th>
                            <th class="py-4 px-4 font-medium text-black text-right">HÃ nh Ä‘á»™ng</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="loading"><td colspan="4" class="p-4 text-center">Äang táº£i...</td></tr>
                        <tr v-else v-for="cat in categories" :key="cat.id" class="border-b border-stroke hover:bg-gray-50">
                            <td class="py-4 px-4 pl-8 font-medium text-black">{{ cat.name }}</td>
                            <td class="py-4 px-4 text-sm text-slate-500">{{ cat.slug }}</td>
                            <td class="py-4 px-4 text-sm">{{ cat.description }}</td>
                            <td class="py-4 px-4 text-right">
                                <router-link :to="'/Admin/Category/Edit/' + cat.id" class="text-primary hover:underline mr-4">Sá»­a</router-link>
                                <button @click="deleteCategory(cat.id)" class="text-danger hover:underline">XÃ³a</button>
                            </td>
                        </tr>
                    </tbody>
                </table>
             </div>
        </div>
    `
};


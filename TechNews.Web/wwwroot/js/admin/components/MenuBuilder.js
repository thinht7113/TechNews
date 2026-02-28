const { ref, reactive, onMounted } = Vue;

export default {
    setup() {
        const menuItems = ref([]);
        const loading = ref(true);

        const fetchMenu = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/menu/getall');
                menuItems.value = await res.json();
            } catch (e) { } finally { loading.value = false; }
        };

        const deleteItem = async (id) => {
            if (!confirm('XÃ³a menu nÃ y?')) return;
            await fetch(`/api/menu/delete/${id}`, { method: 'POST' });
            fetchMenu();
        };

        const form = reactive({ id: 0, title: '', url: '', parentId: null, order: 0, openInNewTab: false });
        const isEdit = ref(false);

        const editItem = (item) => {
            Object.assign(form, item);
            isEdit.value = true;
        };

        const resetForm = () => {
            Object.assign(form, { id: 0, title: '', url: '', parentId: null, order: 0, openInNewTab: false });
            isEdit.value = false;
        };

        const submit = async () => {
            const url = isEdit.value ? `/api/menu/update/${form.id}` : '/api/menu/create';
            try {
                const res = await fetch(url, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(form)
                });
                if (res.ok) {
                    fetchMenu();
                    resetForm();
                    Swal.fire('ThÃ nh cÃ´ng', 'ÄÃ£ lÆ°u menu', 'success');
                } else {
                    const err = await res.json();
                    Swal.fire('Lá»—i', 'Chi tiáº¿t: ' + JSON.stringify(err), 'error');
                }
            } catch (e) {
                Swal.fire('Lá»—i máº¡ng', e.message, 'error');
            }
        };

        onMounted(fetchMenu);
        return { menuItems, loading, deleteItem, editItem, form, isEdit, submit, resetForm };
    },
    template: `
        <div>
            <h2 class="text-2xl font-bold text-black mb-6">Quáº£n lÃ½ Menu</h2>
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <!-- Form -->
                <div class="lg:col-span-1">
                    <div class="bg-white p-6 shadow-sm rounded border border-stroke">
                        <h3 class="font-bold text-lg mb-4">{{ isEdit ? 'Sá»­a Menu' : 'ThÃªm Menu' }}</h3>
                        <form @submit.prevent="submit" class="flex flex-col gap-4">
                            <div>
                                <label class="block text-black font-medium mb-1">TiÃªu Ä‘á»</label>
                                <input v-model="form.title" type="text" class="w-full border border-stroke rounded px-3 py-2 outline-none focus:border-primary" required />
                            </div>
                            <div>
                                <label class="block text-black font-medium mb-1">URL</label>
                                <input v-model="form.url" type="text" class="w-full border border-stroke rounded px-3 py-2 outline-none focus:border-primary" />
                            </div>
                            <div>
                                <label class="block text-black font-medium mb-1">Thá»© tá»±</label>
                                <input v-model="form.order" type="number" class="w-full border border-stroke rounded px-3 py-2 outline-none focus:border-primary" />
                            </div>
                            <div class="flex items-center gap-2">
                                <input type="checkbox" v-model="form.openInNewTab" id="openInNewTab" />
                                <label for="openInNewTab">Má»Ÿ tab má»›i</label>
                            </div>
                            <div class="flex gap-2">
                                <button type="submit" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90 flex-1">{{ isEdit ? 'Cáº­p nháº­t' : 'ThÃªm má»›i' }}</button>
                                <button v-if="isEdit" type="button" @click="resetForm" class="bg-gray-200 text-black py-2 px-4 rounded hover:bg-gray-300">Há»§y</button>
                            </div>
                        </form>
                    </div>
                </div>

                <!-- List -->
                <div class="lg:col-span-2">
                    <div class="bg-white p-6 shadow-sm rounded border border-stroke">
                         <div v-if="loading" class="text-center">Äang táº£i...</div>
                         <div v-else>
                             <ul class="space-y-2">
                                 <li v-for="item in menuItems" :key="item.id" class="border border-stroke rounded p-3 flex justify-between items-center bg-gray-50">
                                     <div>
                                         <div class="font-bold text-black">{{ item.title }}</div>
                                         <div class="text-xs text-slate-500">{{ item.url }} (Order: {{ item.order }})</div>
                                     </div>
                                     <div class="flex gap-2">
                                         <button @click="editItem(item)" class="text-primary hover:underline text-sm">Sá»­a</button>
                                         <button @click="deleteItem(item.id)" class="text-red-500 hover:underline text-sm">XÃ³a</button>
                                     </div>
                                 </li>
                             </ul>
                         </div>
                    </div>
                </div>
            </div>
        </div>
    `
};


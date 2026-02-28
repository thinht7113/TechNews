const { ref, reactive, onMounted, watch } = Vue;

export default {
    setup() {
        const tags = ref([]);
        const loading = ref(true);

        const fetchTags = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/tag/getall');
                tags.value = await res.json();
            } catch (e) { } finally { loading.value = false; }
        };

        const deleteTag = async (id) => {
            if (!confirm('Xóa thẻ này?')) return;
            await fetch(`/api/tag/delete/${id}`, { method: 'POST' });
            fetchTags();
        };

        const form = reactive({ id: 0, name: '', slug: '' });
        const isEdit = ref(false);

        const editTag = (tag) => {
            Object.assign(form, tag);
            isEdit.value = true;
        };

        const resetForm = () => {
            Object.assign(form, { id: 0, name: '', slug: '' });
            isEdit.value = false;
        };

        watch(() => form.name, (val) => {
            if (!isEdit.value) form.slug = val.toLowerCase().replace(/ /g, '-').replace(/đ/g, 'd');
        });

        const submit = async () => {
            const url = isEdit.value ? `/api/tag/update/${form.id}` : '/api/tag/create';
            const res = await fetch(url, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(form)
            });
            if (res.ok) {
                fetchTags();
                resetForm();
            } else {
                Swal.fire('Lỗi', 'Có lỗi xảy ra', 'error');
            }
        };

        onMounted(fetchTags);
        return { tags, loading, deleteTag, editTag, form, isEdit, submit, resetForm };
    },
    template: `
        <div>
            <h2 class="text-2xl font-bold text-black mb-6">Quản lý Thẻ (Tags)</h2>
            <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                <!-- Form -->
                <div class="lg:col-span-1">
                    <div class="bg-white p-6 shadow-sm rounded border border-stroke">
                        <h3 class="font-bold text-lg mb-4">{{ isEdit ? 'Sửa Thẻ' : 'Thêm Thẻ Mới' }}</h3>
                        <form @submit.prevent="submit" class="flex flex-col gap-4">
                            <div>
                                <label class="block text-black font-medium mb-1">Tên thẻ</label>
                                <input v-model="form.name" type="text" class="w-full border border-stroke rounded px-3 py-2 outline-none focus:border-primary" required />
                            </div>
                            <div>
                                <label class="block text-black font-medium mb-1">Slug (URL)</label>
                                <input v-model="form.slug" type="text" class="w-full border border-stroke rounded px-3 py-2 outline-none focus:border-primary" required />
                            </div>
                            <div class="flex gap-2">
                                <button type="submit" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90 flex-1">{{ isEdit ? 'Cập nhật' : 'Thêm mới' }}</button>
                                <button v-if="isEdit" type="button" @click="resetForm" class="bg-gray-200 text-black py-2 px-4 rounded hover:bg-gray-300">Hủy</button>
                            </div>
                        </form>
                    </div>
                </div>

                <!-- List -->
                <div class="lg:col-span-2">
                    <div class="bg-white p-6 shadow-sm rounded border border-stroke">
                         <div v-if="loading" class="text-center">Đang tải...</div>
                         <div v-else>
                             <div class="flex flex-wrap gap-2">
                                 <div v-for="tag in tags" :key="tag.id" class="border border-stroke rounded px-3 py-2 flex items-center gap-2 bg-gray-50 hover:bg-white transition">
                                     <span class="font-medium text-black">{{ tag.name }}</span>
                                     <span class="text-xs bg-gray-200 px-1.5 rounded-full text-slate-600">{{ tag.count }}</span>
                                     <div class="flex flex-col text-xs ml-2 gap-1">
                                         <button @click="editTag(tag)" class="text-primary hover:underline">Sửa</button>
                                         <button @click="deleteTag(tag.id)" class="text-red-500 hover:underline">Xóa</button>
                                     </div>
                                 </div>
                             </div>
                         </div>
                    </div>
                </div>
            </div>
        </div>
    `
};

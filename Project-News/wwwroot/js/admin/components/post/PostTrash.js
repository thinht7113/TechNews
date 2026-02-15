const { ref, onMounted } = Vue;
const { useRoute } = VueRouter; // Not really needed unless linking back

export default {
    setup() {
        const posts = ref([]);
        const loading = ref(true);

        const fetchTrash = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/post/gettrash');
                if (res.ok) posts.value = await res.json();
            } finally { loading.value = false; }
        };

        const restorePost = async (id) => {
            try {
                const res = await fetch(`/api/post/restore/${id}`, { method: 'POST' });
                if (res.ok) {
                    posts.value = posts.value.filter(p => p.id !== id);
                    Swal.fire('Đã khôi phục', 'Bài viết đã được chuyển về danh sách chính.', 'success');
                }
            } catch (e) { Swal.fire('Lỗi', 'Không thể khôi phục', 'error'); }
        };

        const deleteForever = async (id) => {
            const result = await Swal.fire({
                title: 'Xóa vĩnh viễn?',
                text: "Hành động này KHÔNG THỀ hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'Xóa vĩnh viễn'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/post/permanentdelete/${id}`, { method: 'POST' });
                    if (res.ok) {
                        posts.value = posts.value.filter(p => p.id !== id);
                        Swal.fire('Đã xóa', '', 'success');
                    }
                } catch (e) { Swal.fire('Lỗi', 'Không thể xóa', 'error'); }
            }
        };

        onMounted(fetchTrash);
        return { posts, loading, restorePost, deleteForever };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">Thùng rác</h2>
                <router-link to="/Admin/Post" class="text-primary hover:underline">
                    <i class="bi bi-arrow-left"></i> Quay lại danh sách
                </router-link>
            </div>

            <div class="rounded-sm border border-stroke bg-white shadow-default">
                <div class="max-w-full overflow-x-auto">
                    <table class="w-full table-auto">
                         <thead>
                            <tr class="bg-gray-100 text-left">
                                <th class="py-4 px-4 font-medium text-black pl-8">Tiêu đề</th>
                                <th class="py-4 px-4 font-medium text-black">Chuyên mục</th>
                                <th class="py-4 px-4 font-medium text-black">Ngày xóa (gần đúng)</th>
                                <th class="py-4 px-4 font-medium text-black text-right">Hành động</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="loading"><td colspan="4" class="p-4 text-center">Đang tải...</td></tr>
                            <tr v-else-if="posts.length === 0"><td colspan="4" class="p-4 text-center text-slate-500">Thùng rác trống</td></tr>
                            <tr v-else v-for="item in posts" :key="item.id" class="border-b border-stroke hover:bg-gray-50">
                                <td class="py-4 px-4 pl-8 font-medium text-black">{{ item.title }}</td>
                                <td class="py-4 px-4 text-sm">{{ item.categoryName }}</td>
                                <td class="py-4 px-4 text-sm">{{ new Date(item.createdDate).toLocaleDateString('vi-VN') }}</td>
                                <td class="py-4 px-4 text-right">
                                    <button @click="restorePost(item.id)" class="text-primary hover:underline mr-4">Khôi phục</button>
                                    <button @click="deleteForever(item.id)" class="text-red-600 hover:underline">Xóa vĩnh viễn</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `
};

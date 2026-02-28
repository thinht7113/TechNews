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
                    Swal.fire('ÄÃ£ khÃ´i phá»¥c', 'BÃ i viáº¿t Ä‘Ã£ Ä‘Æ°á»£c chuyá»ƒn vá» danh sÃ¡ch chÃ­nh.', 'success');
                }
            } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ khÃ´i phá»¥c', 'error'); }
        };

        const deleteForever = async (id) => {
            const result = await Swal.fire({
                title: 'XÃ³a vÄ©nh viá»…n?',
                text: "HÃ nh Ä‘á»™ng nÃ y KHÃ”NG THá»€ hoÃ n tÃ¡c!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                confirmButtonText: 'XÃ³a vÄ©nh viá»…n'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/post/permanentdelete/${id}`, { method: 'POST' });
                    if (res.ok) {
                        posts.value = posts.value.filter(p => p.id !== id);
                        Swal.fire('ÄÃ£ xÃ³a', '', 'success');
                    }
                } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ xÃ³a', 'error'); }
            }
        };

        onMounted(fetchTrash);
        return { posts, loading, restorePost, deleteForever };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">ThÃ¹ng rÃ¡c</h2>
                <router-link to="/Admin/Post" class="text-primary hover:underline">
                    <i class="bi bi-arrow-left"></i> Quay láº¡i danh sÃ¡ch
                </router-link>
            </div>

            <div class="rounded-sm border border-stroke bg-white shadow-default">
                <div class="max-w-full overflow-x-auto">
                    <table class="w-full table-auto">
                         <thead>
                            <tr class="bg-gray-100 text-left">
                                <th class="py-4 px-4 font-medium text-black pl-8">TiÃªu Ä‘á»</th>
                                <th class="py-4 px-4 font-medium text-black">ChuyÃªn má»¥c</th>
                                <th class="py-4 px-4 font-medium text-black">NgÃ y xÃ³a (gáº§n Ä‘Ãºng)</th>
                                <th class="py-4 px-4 font-medium text-black text-right">HÃ nh Ä‘á»™ng</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="loading"><td colspan="4" class="p-4 text-center">Äang táº£i...</td></tr>
                            <tr v-else-if="posts.length === 0"><td colspan="4" class="p-4 text-center text-slate-500">ThÃ¹ng rÃ¡c trá»‘ng</td></tr>
                            <tr v-else v-for="item in posts" :key="item.id" class="border-b border-stroke hover:bg-gray-50">
                                <td class="py-4 px-4 pl-8 font-medium text-black">{{ item.title }}</td>
                                <td class="py-4 px-4 text-sm">{{ item.categoryName }}</td>
                                <td class="py-4 px-4 text-sm">{{ new Date(item.createdDate).toLocaleDateString('vi-VN') }}</td>
                                <td class="py-4 px-4 text-right">
                                    <button @click="restorePost(item.id)" class="text-primary hover:underline mr-4">KhÃ´i phá»¥c</button>
                                    <button @click="deleteForever(item.id)" class="text-red-600 hover:underline">XÃ³a vÄ©nh viá»…n</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `
};


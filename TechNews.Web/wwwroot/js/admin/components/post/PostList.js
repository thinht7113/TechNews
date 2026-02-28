const { ref, onMounted, computed, watch } = Vue;

export default {
    setup() {
        const posts = ref([]);
        const loading = ref(true);
        const searchQuery = ref('');
        const currentPage = ref(1);
        const totalPages = ref(1);
        const totalCount = ref(0);
        const pageSize = 20;

        const fetchPosts = async (page = 1) => {
            loading.value = true;
            try {
                const res = await fetch(`/api/post/getall?page=${page}&pageSize=${pageSize}`);
                const result = await res.json();
                posts.value = result.data;
                currentPage.value = result.page;
                totalPages.value = result.totalPages;
                totalCount.value = result.totalCount;
            } catch (e) { console.error(e); } finally {
                loading.value = false;
            }
        };

        const filteredPosts = computed(() => {
            if (!searchQuery.value) return posts.value;
            const lower = searchQuery.value.toLowerCase();
            return posts.value.filter(p => p.title.toLowerCase().includes(lower));
        });

        const deletePost = async (id) => {
            const result = await Swal.fire({
                title: 'Bá» vÃ o thÃ¹ng rÃ¡c?',
                text: "BÃ i viáº¿t nÃ y sáº½ bá»‹ xÃ³a!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#3C50E0',
                cancelButtonColor: '#D34053',
                confirmButtonText: 'XÃ³a ngay',
                cancelButtonText: 'Há»§y'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/post/delete/${id}`, { method: 'POST' });
                    if (res.ok) {
                        posts.value = posts.value.filter(p => p.id !== id);
                        Swal.fire('ÄÃ£ xÃ³a!', 'BÃ i viáº¿t Ä‘Ã£ Ä‘Æ°á»£c chuyá»ƒn vÃ o thÃ¹ng rÃ¡c.', 'success');
                    }
                } catch (err) { }
            }
        };

        const goToPage = (page) => {
            if (page >= 1 && page <= totalPages.value) fetchPosts(page);
        };

        onMounted(() => fetchPosts(1));

        return { posts, loading, deletePost, searchQuery, filteredPosts, currentPage, totalPages, totalCount, goToPage };
    },
    template: `
        <div>
            <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <h2 class="text-2xl font-bold text-black">Táº¥t cáº£ bÃ i viáº¿t <span class="text-sm font-normal text-slate-500">({{ totalCount }})</span></h2>
                <div class="flex gap-2">
                    <div class="relative">
                        <input v-model="searchQuery" type="text" placeholder="TÃ¬m kiáº¿m bÃ i viáº¿t..." class="rounded border border-stroke bg-white py-2 pl-4 pr-10 outline-none focus:border-primary" />
                        <span class="absolute right-4 top-2.5">
                            <i class="bi bi-search text-slate-400"></i>
                        </span>
                    </div>
                    <router-link to="/Admin/Post/Trash" class="inline-flex items-center justify-center rounded-md border border-stroke py-2 px-4 text-center font-medium text-black hover:bg-gray-50">
                        <i class="bi bi-trash mr-2"></i> ThÃ¹ng rÃ¡c
                    </router-link>
                    <router-link to="/Admin/Post/Create" class="inline-flex items-center justify-center rounded-md bg-primary py-2 px-6 text-center font-medium text-white hover:bg-opacity-90">
                        <i class="bi bi-plus-lg mr-2"></i> ThÃªm bÃ i má»›i
                    </router-link>
                </div>
            </div>

            <div class="rounded-sm border border-stroke bg-white shadow-default">
                <div class="max-w-full overflow-x-auto">
                    <table class="w-full table-auto">
                        <thead>
                            <tr class="bg-gray-100 text-left">
                                <th class="py-4 px-4 font-medium text-black xl:pl-8">TiÃªu Ä‘á»</th>
                                <th class="py-4 px-4 font-medium text-black">TÃ¡c giáº£</th>
                                <th class="py-4 px-4 font-medium text-black">ChuyÃªn má»¥c</th>
                                <th class="py-4 px-4 font-medium text-black">Tráº¡ng thÃ¡i</th>
                                <th class="py-4 px-4 font-medium text-black">Thá»i gian</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="loading"><td colspan="5" class="p-4 text-center">Äang táº£i...</td></tr>
                            <tr v-else-if="filteredPosts.length === 0"><td colspan="5" class="p-4 text-center text-slate-500">KhÃ´ng tÃ¬m tháº¥y bÃ i viáº¿t nÃ o</td></tr>
                            <tr v-else v-for="item in filteredPosts" :key="item.id" class="border-b border-stroke hover:bg-gray-50 group">
                                <td class="py-4 px-4 pl-8">
                                    <p class="text-black font-bold text-base mb-1">{{ item.title }}</p>
                                    <div class="flex items-center gap-3 text-xs opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                                        <router-link :to="'/Admin/Post/Edit/' + item.id" class="text-primary hover:underline">Chá»‰nh sá»­a</router-link>
                                        <span class="text-slate-300">|</span>
                                        <button @click="deletePost(item.id)" class="text-danger hover:underline">XÃ³a táº¡m</button>
                                        <span class="text-slate-300">|</span>
                                        <a :href="'/post/'+ (item.slug || 'detail')" target="_blank" class="text-slate-600 hover:underline">Xem</a>
                                    </div>
                                </td>
                                <td class="py-4 px-4 text-sm">Admin</td>
                                <td class="py-4 px-4 text-sm text-black">{{ item.categoryName || 'ChÆ°a phÃ¢n loáº¡i' }}</td>
                                <td class="py-4 px-4 text-sm">
                                    <span :class="{'bg-green-100 text-green-800': item.status===1, 'bg-gray-100 text-gray-800': item.status!==1}" class="px-2 py-1 rounded text-xs font-semibold">
                                        {{ item.status === 1 ? 'ÄÃ£ Ä‘Äƒng' : 'Báº£n nhÃ¡p' }}
                                    </span>
                                </td>
                                <td class="py-4 px-4 text-sm text-black">
                                    {{ new Date(item.createdDate).toLocaleDateString('vi-VN') }}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
                <!-- Pagination -->
                <div v-if="totalPages > 1" class="flex items-center justify-between border-t border-stroke px-6 py-4">
                    <p class="text-sm text-slate-500">Trang {{ currentPage }} / {{ totalPages }}</p>
                    <div class="flex gap-1">
                        <button @click="goToPage(currentPage - 1)" :disabled="currentPage <= 1" class="px-3 py-1 rounded border text-sm disabled:opacity-40 hover:bg-gray-100">â€¹</button>
                        <template v-for="p in totalPages" :key="p">
                            <button v-if="p <= 5 || p === totalPages || Math.abs(p - currentPage) <= 1" @click="goToPage(p)" :class="{'bg-primary text-white': p === currentPage, 'hover:bg-gray-100': p !== currentPage}" class="px-3 py-1 rounded border text-sm">{{ p }}</button>
                            <span v-else-if="p === 6 || p === totalPages - 1" class="px-1 text-slate-400">â€¦</span>
                        </template>
                        <button @click="goToPage(currentPage + 1)" :disabled="currentPage >= totalPages" class="px-3 py-1 rounded border text-sm disabled:opacity-40 hover:bg-gray-100">â€º</button>
                    </div>
                </div>
            </div>
        </div>
    `
};


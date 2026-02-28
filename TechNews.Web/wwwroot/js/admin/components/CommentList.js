const { ref, onMounted } = Vue;

export default {
    setup() {
        const comments = ref([]);
        const loading = ref(true);

        const fetchComments = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/comment/getall');
                const result = await res.json();
                comments.value = result.data || result;
            } catch (e) { console.error(e); } finally { loading.value = false; }
        };

        const approveComment = async (id) => {
            try {
                const res = await fetch(`/api/comment/approve/${id}`, { method: 'POST' });
                if (res.ok) {
                    const comment = comments.value.find(c => c.id === id);
                    if (comment) comment.isApproved = true;
                    Swal.fire('ThÃ nh cÃ´ng', 'ÄÃ£ duyá»‡t bÃ¬nh luáº­n', 'success');
                }
            } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ duyá»‡t', 'error'); }
        };

        const deleteComment = async (id) => {
            const result = await Swal.fire({
                title: 'XÃ³a bÃ¬nh luáº­n?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonText: 'XÃ³a',
                cancelButtonText: 'Há»§y'
            });

            if (result.isConfirmed) {
                try {
                    const res = await fetch(`/api/comment/delete/${id}`, { method: 'POST' });
                    if (res.ok) {
                        comments.value = comments.value.filter(c => c.id !== id);
                        Swal.fire('ThÃ nh cÃ´ng', 'ÄÃ£ xÃ³a bÃ¬nh luáº­n', 'success');
                    }
                } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ xÃ³a', 'error'); }
            }
        };

        onMounted(fetchComments);

        return { comments, loading, approveComment, deleteComment };
    },
    template: `
        <div>
            <h2 class="text-2xl font-bold text-black mb-6">Quáº£n lÃ½ bÃ¬nh luáº­n</h2>
            <div class="rounded-sm border border-stroke bg-white shadow-default">
                <div class="max-w-full overflow-x-auto">
                    <table class="w-full table-auto">
                        <thead>
                            <tr class="bg-gray-100 text-left">
                                <th class="py-4 px-4 font-medium text-black">NgÆ°á»i dÃ¹ng</th>
                                <th class="py-4 px-4 font-medium text-black">Ná»™i dung</th>
                                <th class="py-4 px-4 font-medium text-black">BÃ i viáº¿t</th>
                                <th class="py-4 px-4 font-medium text-black">Tráº¡ng thÃ¡i</th>
                                <th class="py-4 px-4 font-medium text-black">HÃ nh Ä‘á»™ng</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="loading"><td colspan="5" class="p-4 text-center">Äang táº£i...</td></tr>
                            <tr v-else v-for="c in comments" :key="c.id" class="border-b border-stroke">
                                <td class="py-4 px-4">
                                    <div class="flex items-center gap-2">
                                        <img :src="c.userAvatar || 'https://ui-avatars.com/api/?name='+c.userName" class="w-8 h-8 rounded-full" />
                                        <span class="text-sm font-medium">{{ c.userName }}</span>
                                    </div>
                                    <div class="text-xs text-slate-500 mt-1">{{ new Date(c.createdDate).toLocaleString('vi-VN') }}</div>
                                </td>
                                <td class="py-4 px-4 max-w-xs truncate" :title="c.content">{{ c.content }}</td>
                                <td class="py-4 px-4 text-sm"><a :href="'/post/'+c.postId" target="_blank" class="hover:underline text-primary">{{ c.postTitle }}</a></td>
                                <td class="py-4 px-4">
                                    <span v-if="c.isApproved" class="bg-green-100 text-green-800 text-xs px-2 py-1 rounded">ÄÃ£ duyá»‡t</span>
                                    <span v-else class="bg-yellow-100 text-yellow-800 text-xs px-2 py-1 rounded">Chá» duyá»‡t</span>
                                </td>
                                <td class="py-4 px-4 flex gap-2">
                                    <button v-if="!c.isApproved" @click="approveComment(c.id)" class="text-green-600 hover:underline text-sm">Duyá»‡t</button>
                                    <button @click="deleteComment(c.id)" class="text-red-600 hover:underline text-sm">XÃ³a</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    `
};


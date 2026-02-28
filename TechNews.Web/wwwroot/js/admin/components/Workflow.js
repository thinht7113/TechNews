const { ref, onMounted } = Vue;

export default {
    setup() {
        const pendingPosts = ref([]);
        const workflowLogs = ref([]);
        const selectedPostId = ref(null);
        const isLoading = ref(false);

        const statusLabels = {
            0: 'NhÃ¡p', 1: 'ÄÃ£ phÃ¡t hÃ nh', 2: 'LÆ°u trá»¯', 3: 'áº¨n',
            4: 'Chá» duyá»‡t', 5: 'Äang duyá»‡t', 6: 'Tá»« chá»‘i', 7: 'ÄÃ£ lÃªn lá»‹ch'
        };
        const statusColors = {
            0: 'bg-gray-100 text-gray-600', 1: 'bg-green-100 text-green-700', 2: 'bg-slate-100 text-slate-700',
            3: 'bg-gray-100 text-gray-500', 4: 'bg-yellow-100 text-yellow-700', 5: 'bg-blue-100 text-blue-700',
            6: 'bg-red-100 text-red-700', 7: 'bg-indigo-100 text-indigo-700'
        };

        const fetchPending = async () => {
            isLoading.value = true;
            try {
                const res = await fetch('/api/workflow/pending');
                if (res.ok) pendingPosts.value = await res.json();
            } catch (e) { console.error(e); }
            finally { isLoading.value = false; }
        };

        const viewLogs = async (postId) => {
            selectedPostId.value = postId;
            try {
                const res = await fetch(`/api/workflow/logs/${postId}`);
                if (res.ok) workflowLogs.value = await res.json();
            } catch (e) { console.error(e); }
        };

        const approve = async (postId) => {
            const result = await Swal.fire({
                title: 'Duyá»‡t bÃ i viáº¿t?', text: 'BÃ i sáº½ Ä‘Æ°á»£c phÃ¡t hÃ nh ngay sau khi duyá»‡t.',
                icon: 'question', showCancelButton: true, confirmButtonText: 'Duyá»‡t', cancelButtonText: 'Há»§y', confirmButtonColor: '#10b981',
                input: 'text', inputPlaceholder: 'Ghi chÃº (tÃ¹y chá»n)', inputAttributes: { autocapitalize: 'off' }
            });
            if (!result.isConfirmed) return;
            try {
                const res = await fetch(`/api/workflow/approve/${postId}`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ comment: result.value || '' })
                });
                if (res.ok) { Swal.fire({ icon: 'success', title: 'ÄÃ£ duyá»‡t!', timer: 1500, showConfirmButton: false }); fetchPending(); }
                else { const d = await res.json(); Swal.fire('Lá»—i', d.message || 'KhÃ´ng thá»ƒ duyá»‡t', 'error'); }
            } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ káº¿t ná»‘i server', 'error'); }
        };

        const reject = async (postId) => {
            const result = await Swal.fire({
                title: 'Tá»« chá»‘i bÃ i viáº¿t?', icon: 'warning', showCancelButton: true,
                confirmButtonText: 'Tá»« chá»‘i', cancelButtonText: 'Há»§y', confirmButtonColor: '#ef4444',
                input: 'textarea', inputPlaceholder: 'LÃ½ do tá»« chá»‘i (báº¯t buá»™c)...',
                inputValidator: (value) => { if (!value) return 'Vui lÃ²ng nháº­p lÃ½ do tá»« chá»‘i!'; }
            });
            if (!result.isConfirmed) return;
            try {
                const res = await fetch(`/api/workflow/reject/${postId}`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ comment: result.value })
                });
                if (res.ok) { Swal.fire({ icon: 'success', title: 'ÄÃ£ tá»« chá»‘i!', timer: 1500, showConfirmButton: false }); fetchPending(); }
                else { const d = await res.json(); Swal.fire('Lá»—i', d.message || 'Lá»—i', 'error'); }
            } catch (e) { Swal.fire('Lá»—i', 'KhÃ´ng thá»ƒ káº¿t ná»‘i server', 'error'); }
        };

        const schedule = async (postId) => {
            const result = await Swal.fire({
                title: 'LÃªn lá»‹ch xuáº¥t báº£n', icon: 'info', showCancelButton: true,
                confirmButtonText: 'LÃªn lá»‹ch', cancelButtonText: 'Há»§y', confirmButtonColor: '#3b82f6',
                html: '<input id="schedule-date" type="datetime-local" class="swal2-input" />',
                preConfirm: () => {
                    const date = document.getElementById('schedule-date').value;
                    if (!date) { Swal.showValidationMessage('Chá»n ngÃ y giá»!'); return false; }
                    if (new Date(date) <= new Date()) { Swal.showValidationMessage('Pháº£i chá»n thá»i Ä‘iá»ƒm trong tÆ°Æ¡ng lai!'); return false; }
                    return date;
                }
            });
            if (!result.isConfirmed) return;
            try {
                const res = await fetch(`/api/workflow/schedule/${postId}`, {
                    method: 'POST', headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ publishDate: result.value })
                });
                if (res.ok) { Swal.fire({ icon: 'success', title: 'ÄÃ£ lÃªn lá»‹ch!', timer: 1500, showConfirmButton: false }); fetchPending(); }
                else { const d = await res.json(); Swal.fire('Lá»—i', d.message || 'Lá»—i', 'error'); }
            } catch (e) { Swal.fire('Lá»—i', 'Lá»—i káº¿t ná»‘i', 'error'); }
        };

        onMounted(fetchPending);

        return { pendingPosts, workflowLogs, selectedPostId, isLoading, statusLabels, statusColors, approve, reject, schedule, viewLogs, fetchPending };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <div>
                    <h2 class="text-2xl font-bold text-black">Duyá»‡t bÃ i viáº¿t</h2>
                    <p class="text-sm text-slate-500 mt-1">Quáº£n lÃ½ quy trÃ¬nh biÃªn táº­p</p>
                </div>
                <button @click="fetchPending" class="flex items-center gap-2 px-4 py-2 bg-white border border-stroke rounded-lg text-sm hover:bg-gray-50">
                    <i class="bi bi-arrow-clockwise"></i> LÃ m má»›i
                </button>
            </div>

            <div v-if="isLoading" class="text-center py-10 text-slate-500">
                <div class="w-8 h-8 border-2 border-primary border-t-transparent rounded-full animate-spin mx-auto mb-2"></div>
                Äang táº£i...
            </div>

            <div v-else-if="pendingPosts.length === 0" class="text-center py-20 bg-white rounded-lg border border-stroke">
                <i class="bi bi-check-circle text-5xl text-green-500 mb-4 block"></i>
                <p class="text-lg font-medium text-black">KhÃ´ng cÃ³ bÃ i viáº¿t nÃ o chá» duyá»‡t</p>
                <p class="text-sm text-slate-500 mt-1">Táº¥t cáº£ bÃ i viáº¿t Ä‘Ã£ Ä‘Æ°á»£c xá»­ lÃ½</p>
            </div>

            <div v-else class="grid gap-4">
                <div v-for="post in pendingPosts" :key="post.id"
                    class="bg-white rounded-lg border border-stroke p-5 hover:shadow-md transition">
                    <div class="flex items-start justify-between">
                        <div class="flex-1">
                            <div class="flex items-center gap-2 mb-2">
                                <span class="px-2 py-0.5 rounded-full text-xs font-medium" :class="statusColors[post.status] || 'bg-gray-100'">
                                    {{ statusLabels[post.status] || 'N/A' }}
                                </span>
                                <span class="text-xs text-slate-400">ID: {{ post.id }}</span>
                            </div>
                            <h3 class="text-lg font-bold text-black mb-1">{{ post.title }}</h3>
                            <p class="text-sm text-slate-500 line-clamp-2">{{ post.shortDescription }}</p>
                            <div class="flex items-center gap-4 mt-3 text-xs text-slate-400">
                                <span><i class="bi bi-person"></i> {{ post.authorName || 'N/A' }}</span>
                                <span><i class="bi bi-calendar"></i> {{ new Date(post.createdDate).toLocaleDateString('vi-VN') }}</span>
                                <span v-if="post.categoryName"><i class="bi bi-folder"></i> {{ post.categoryName }}</span>
                            </div>
                        </div>
                        <div class="flex items-center gap-2 ml-4">
                            <button @click="approve(post.id)" title="Duyá»‡t"
                                class="p-2 rounded-lg bg-green-50 text-green-600 hover:bg-green-100 transition">
                                <i class="bi bi-check-lg text-lg"></i>
                            </button>
                            <button @click="reject(post.id)" title="Tá»« chá»‘i"
                                class="p-2 rounded-lg bg-red-50 text-red-600 hover:bg-red-100 transition">
                                <i class="bi bi-x-lg text-lg"></i>
                            </button>
                            <button @click="schedule(post.id)" title="LÃªn lá»‹ch"
                                class="p-2 rounded-lg bg-blue-50 text-blue-600 hover:bg-blue-100 transition">
                                <i class="bi bi-clock text-lg"></i>
                            </button>
                            <button @click="viewLogs(post.id)" title="Xem lá»‹ch sá»­"
                                class="p-2 rounded-lg bg-slate-50 text-slate-600 hover:bg-slate-100 transition">
                                <i class="bi bi-clock-history text-lg"></i>
                            </button>
                        </div>
                    </div>

                    <!-- Workflow Logs -->
                    <div v-if="selectedPostId === post.id && workflowLogs.length > 0"
                        class="mt-4 border-t border-stroke pt-4">
                        <h5 class="text-sm font-medium text-black mb-2"><i class="bi bi-clock-history"></i> Lá»‹ch sá»­ workflow</h5>
                        <div class="space-y-2 max-h-40 overflow-y-auto">
                            <div v-for="log in workflowLogs" :key="log.id" class="flex items-start gap-3 text-xs p-2 bg-gray-50 rounded">
                                <div class="w-6 h-6 bg-primary text-white rounded-full flex items-center justify-center text-[10px] flex-shrink-0 mt-0.5">
                                    <i class="bi bi-arrow-right"></i>
                                </div>
                                <div>
                                    <span class="font-medium">{{ statusLabels[log.fromStatus] }}</span>
                                    <i class="bi bi-arrow-right mx-1"></i>
                                    <span class="font-medium">{{ statusLabels[log.toStatus] }}</span>
                                    <p v-if="log.comment" class="text-slate-500 mt-0.5">{{ log.comment }}</p>
                                    <p class="text-slate-400">{{ new Date(log.createdDate).toLocaleString('vi-VN') }}</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `
};


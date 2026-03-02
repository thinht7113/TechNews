const { ref, onMounted, computed } = Vue;

export default {
    setup() {
        const subscribers = ref([]);
        const posts = ref([]);
        const loading = ref(true);
        const sending = ref(false);
        const activeTab = ref('subscribers');
        const totalCount = ref(0);
        const activeCount = ref(0);

        const subject = ref('');
        const content = ref('');
        const selectedPostIds = ref([]);

        const fetchSubscribers = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/newsletter/subscribers?pageSize=100');
                const result = await res.json();
                subscribers.value = result.data || [];
                totalCount.value = result.totalCount;
                activeCount.value = result.activeCount;
            } catch (e) { console.error(e); } finally { loading.value = false; }
        };

        const fetchPosts = async () => {
            try {
                const res = await fetch('/api/newsletter/posts');
                posts.value = await res.json();
            } catch (e) { console.error(e); }
        };

        const deleteSubscriber = async (id) => {
            const result = await Swal.fire({
                title: 'Xóa người đăng ký?',
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'Xóa',
                cancelButtonText: 'Hủy'
            });
            if (result.isConfirmed) {
                const res = await fetch(`/api/newsletter/subscribers/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    subscribers.value = subscribers.value.filter(s => s.id !== id);
                    totalCount.value--;
                    Swal.fire('Đã xóa!', '', 'success');
                }
            }
        };

        const togglePostSelection = (postId) => {
            const idx = selectedPostIds.value.indexOf(postId);
            if (idx > -1) selectedPostIds.value.splice(idx, 1);
            else selectedPostIds.value.push(postId);
        };

        const sendNewsletter = async () => {
            if (!subject.value.trim() || !content.value.trim()) {
                return Swal.fire('Lỗi', 'Vui lòng nhập tiêu đề và nội dung', 'error');
            }

            const confirm = await Swal.fire({
                title: 'Gửi Newsletter?',
                html: `Gửi đến <b>${activeCount.value}</b> người đăng ký?`,
                icon: 'question',
                showCancelButton: true,
                confirmButtonText: 'Gửi ngay',
                cancelButtonText: 'Hủy'
            });

            if (!confirm.isConfirmed) return;

            sending.value = true;
            try {
                const res = await fetch('/api/newsletter/send', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        subject: subject.value,
                        content: content.value,
                        postIds: selectedPostIds.value.length > 0 ? selectedPostIds.value : null
                    })
                });
                const data = await res.json();

                if (res.ok) {
                    Swal.fire('Thành công!', data.message, 'success');
                    subject.value = '';
                    content.value = '';
                    selectedPostIds.value = [];
                } else {
                    Swal.fire('Lỗi', data.message || 'Có lỗi xảy ra', 'error');
                }
            } catch {
                Swal.fire('Lỗi', 'Không thể kết nối server', 'error');
            } finally {
                sending.value = false;
            }
        };

        onMounted(() => {
            fetchSubscribers();
            fetchPosts();
        });

        return {
            subscribers, posts, loading, sending, activeTab,
            totalCount, activeCount, subject, content, selectedPostIds,
            deleteSubscriber, togglePostSelection, sendNewsletter
        };
    },

    template: `
        <div>
            <div class="mb-6 flex flex-col gap-3 sm:flex-row sm:items-center sm:justify-between">
                <h2 class="text-2xl font-bold text-black">Newsletter</h2>
                <div class="flex gap-2">
                    <button @click="activeTab = 'subscribers'" :class="activeTab === 'subscribers' ? 'bg-primary text-white' : 'bg-white text-black border border-stroke'" class="px-4 py-2 rounded-md text-sm font-medium transition-colors">
                        <i class="bi bi-people mr-1"></i> Người đăng ký
                    </button>
                    <button @click="activeTab = 'compose'" :class="activeTab === 'compose' ? 'bg-primary text-white' : 'bg-white text-black border border-stroke'" class="px-4 py-2 rounded-md text-sm font-medium transition-colors">
                        <i class="bi bi-send mr-1"></i> Soạn & Gửi
                    </button>
                </div>
            </div>

            <!-- Stats -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 mb-6">
                <div class="rounded-sm border border-stroke bg-white p-6 shadow-default">
                    <div class="flex items-center gap-4">
                        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-blue-50 text-primary"><i class="bi bi-envelope-fill text-xl"></i></div>
                        <div>
                            <h4 class="text-2xl font-bold text-black">{{ totalCount }}</h4>
                            <span class="text-sm text-slate-500">Tổng đăng ký</span>
                        </div>
                    </div>
                </div>
                <div class="rounded-sm border border-stroke bg-white p-6 shadow-default">
                    <div class="flex items-center gap-4">
                        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-green-50 text-green-600"><i class="bi bi-check-circle-fill text-xl"></i></div>
                        <div>
                            <h4 class="text-2xl font-bold text-black">{{ activeCount }}</h4>
                            <span class="text-sm text-slate-500">Đang hoạt động</span>
                        </div>
                    </div>
                </div>
                <div class="rounded-sm border border-stroke bg-white p-6 shadow-default">
                    <div class="flex items-center gap-4">
                        <div class="flex h-12 w-12 items-center justify-center rounded-full bg-red-50 text-red-500"><i class="bi bi-x-circle-fill text-xl"></i></div>
                        <div>
                            <h4 class="text-2xl font-bold text-black">{{ totalCount - activeCount }}</h4>
                            <span class="text-sm text-slate-500">Đã hủy</span>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Tab: Subscribers -->
            <div v-if="activeTab === 'subscribers'" class="rounded-sm border border-stroke bg-white shadow-default">
                <div class="max-w-full overflow-x-auto">
                    <table class="w-full table-auto">
                        <thead>
                            <tr class="bg-gray-100 text-left">
                                <th class="py-4 px-4 font-medium text-black">#</th>
                                <th class="py-4 px-4 font-medium text-black">Email</th>
                                <th class="py-4 px-4 font-medium text-black">Trạng thái</th>
                                <th class="py-4 px-4 font-medium text-black">Ngày đăng ký</th>
                                <th class="py-4 px-4 font-medium text-black">Hành động</th>
                            </tr>
                        </thead>
                        <tbody>
                            <tr v-if="loading"><td colspan="5" class="p-4 text-center">Đang tải...</td></tr>
                            <tr v-else-if="subscribers.length === 0"><td colspan="5" class="p-4 text-center text-slate-500">Chưa có người đăng ký nào</td></tr>
                            <tr v-else v-for="(s, idx) in subscribers" :key="s.id" class="border-b border-stroke hover:bg-gray-50">
                                <td class="py-3 px-4 text-sm">{{ idx + 1 }}</td>
                                <td class="py-3 px-4">
                                    <span class="font-medium text-black">{{ s.email }}</span>
                                </td>
                                <td class="py-3 px-4">
                                    <span v-if="s.isActive" class="bg-green-100 text-green-800 text-xs px-2 py-1 rounded font-semibold">Hoạt động</span>
                                    <span v-else class="bg-red-100 text-red-800 text-xs px-2 py-1 rounded font-semibold">Đã hủy</span>
                                </td>
                                <td class="py-3 px-4 text-sm text-slate-600">{{ new Date(s.createdDate).toLocaleDateString('vi-VN') }}</td>
                                <td class="py-3 px-4">
                                    <button @click="deleteSubscriber(s.id)" class="text-red-600 hover:underline text-sm"><i class="bi bi-trash"></i> Xóa</button>
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>

            <!-- Tab: Compose Newsletter -->
            <div v-if="activeTab === 'compose'" class="rounded-sm border border-stroke bg-white shadow-default p-6">
                <div class="grid grid-cols-1 lg:grid-cols-3 gap-6">
                    <!-- Left: Form -->
                    <div class="lg:col-span-2 space-y-5">
                        <div>
                            <label class="mb-2 block text-sm font-medium text-black">Tiêu đề email <span class="text-red-500">*</span></label>
                            <input v-model="subject" type="text" placeholder="VD: Bản tin tuần này từ TechNews..."
                                   class="w-full rounded border border-stroke bg-transparent py-3 px-5 font-medium outline-none transition focus:border-primary" />
                        </div>
                        <div>
                            <label class="mb-2 block text-sm font-medium text-black">Nội dung email <span class="text-red-500">*</span></label>
                            <textarea v-model="content" rows="8" placeholder="Viết nội dung bản tin..."
                                      class="w-full rounded border border-stroke bg-transparent py-3 px-5 font-medium outline-none transition focus:border-primary"></textarea>
                        </div>
                        <button @click="sendNewsletter" :disabled="sending"
                                class="inline-flex items-center justify-center rounded-md bg-primary py-3 px-8 text-center font-medium text-white hover:bg-opacity-90 disabled:opacity-50 transition-opacity">
                            <i class="bi bi-send mr-2"></i>
                            {{ sending ? 'Đang gửi...' : 'Gửi Newsletter' }}
                        </button>
                        <p class="text-sm text-slate-500">Sẽ gửi đến <b>{{ activeCount }}</b> người đăng ký đang hoạt động</p>
                    </div>

                    <!-- Right: Select Posts -->
                    <div>
                        <label class="mb-3 block text-sm font-medium text-black">Đính kèm bài viết (tùy chọn)</label>
                        <div class="max-h-96 overflow-y-auto border border-stroke rounded-md">
                            <div v-for="p in posts" :key="p.id"
                                 @click="togglePostSelection(p.id)"
                                 :class="selectedPostIds.includes(p.id) ? 'bg-blue-50 border-l-4 border-primary' : 'border-l-4 border-transparent hover:bg-gray-50'"
                                 class="px-4 py-3 cursor-pointer transition-colors border-b border-stroke">
                                <div class="flex items-center gap-2">
                                    <i :class="selectedPostIds.includes(p.id) ? 'bi-check-square-fill text-primary' : 'bi-square text-slate-400'" class="bi"></i>
                                    <span class="text-sm font-medium text-black line-clamp-2">{{ p.title }}</span>
                                </div>
                                <div class="text-xs text-slate-400 mt-1 ml-6">{{ new Date(p.createdDate).toLocaleDateString('vi-VN') }}</div>
                            </div>
                            <div v-if="posts.length === 0" class="p-4 text-sm text-slate-500 text-center">Chưa có bài viết</div>
                        </div>
                        <p v-if="selectedPostIds.length > 0" class="text-xs text-primary mt-2 font-medium">
                            Đã chọn {{ selectedPostIds.length }} bài viết
                        </p>
                    </div>
                </div>
            </div>
        </div>
    `
};
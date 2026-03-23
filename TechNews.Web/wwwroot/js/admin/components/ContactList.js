const { ref, onMounted } = Vue;

export default {
    setup() {
        const contacts = ref([]);
        const loading = ref(true);

        const fetchContacts = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/contact/getall');
                if (res.ok) contacts.value = await res.json();
            } finally { loading.value = false; }
        };

        const markRead = async (id) => {
            const res = await fetch(`/api/contact/markread/${id}`, { method: 'POST' });
            if (res.ok) {
                const c = contacts.value.find(x => x.id === id);
                if (c) c.isRead = true;
            }
        };

        const deleteContact = async (id) => {
            const result = await Swal.fire({
                title: 'Xóa liên hệ?',
                text: "Không thể hoàn tác!",
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#D34053',
                confirmButtonText: 'Xóa'
            });

            if (result.isConfirmed) {
                const res = await fetch(`/api/contact/delete/${id}`, { method: 'POST' });
                if (res.ok) {
                    contacts.value = contacts.value.filter(c => c.id !== id);
                    Swal.fire('Đã xóa', '', 'success');
                }
            }
        };

        onMounted(fetchContacts);
        return { contacts, loading, markRead, deleteContact };
    },
    template: `
        <div>
            <div class="mb-6 flex items-center justify-between">
                <h2 class="text-2xl font-bold text-black">Tin nhắn Liên hệ</h2>
            </div>
             <div class="rounded-sm border border-stroke bg-white shadow-default">
                <table class="w-full table-auto">
                    <thead>
                        <tr class="bg-gray-100 text-left">
                            <th class="py-4 px-4 font-medium text-black">Họ tên</th>
                            <th class="py-4 px-4 font-medium text-black">Email / Phone</th>
                            <th class="py-4 px-4 font-medium text-black">Tiêu đề - Nội dung</th>
                            <th class="py-4 px-4 font-medium text-black text-center">Trạng thái</th>
                            <th class="py-4 px-4 font-medium text-black">Ngày gửi</th>
                            <th class="py-4 px-4 text-right pr-6 font-medium text-black">Hành động</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr v-if="loading"><td colspan="6" class="p-4 text-center">Đang tải...</td></tr>
                        <tr v-else-if="contacts.length === 0"><td colspan="6" class="p-8 text-center text-slate-500">Chưa có liên hệ nào</td></tr>
                        <tr v-else v-for="c in contacts" :key="c.id" class="border-b border-stroke hover:bg-gray-50 transition-colors" :class="c.isRead ? '' : 'font-semibold bg-blue-50/30'">
                            <td class="py-4 px-4">
                                <span class="text-black">{{ c.name }}</span>
                            </td>
                            <td class="py-4 px-4 text-sm text-slate-500">
                                <div><i class="bi bi-envelope mr-1"></i> {{ c.email }}</div>
                                <div class="mt-1" v-if="c.phone"><i class="bi bi-telephone mr-1"></i> {{ c.phone }}</div>
                            </td>
                            <td class="py-4 px-4 text-sm max-w-[300px]">
                                <div class="text-black mb-1 truncate">{{ c.subject }}</div>
                                <div class="text-slate-500 line-clamp-2">{{ c.message }}</div>
                            </td>
                            <td class="py-4 px-4 text-center">
                                <span v-if="c.isRead" class="bg-gray-100 text-gray-500 text-xs px-2 py-1 rounded">Đã đọc</span>
                                <span v-else class="bg-blue-100 text-blue-700 text-xs px-2 py-1 rounded">Chưa đọc</span>
                            </td>
                            <td class="py-4 px-4 text-sm text-slate-500">
                                {{ new Date(c.createdDate).toLocaleString('vi-VN') }}
                            </td>
                            <td class="py-4 px-4 text-right pr-6 gap-2 flex justify-end items-center">
                                <button v-if="!c.isRead" @click="markRead(c.id)" class="text-primary hover:underline" title="Đánh dấu đã đọc"><i class="bi bi-check2-all text-lg"></i></button>
                                <button @click="deleteContact(c.id)" class="text-danger hover:underline"><i class="bi bi-trash text-lg"></i></button>
                            </td>
                        </tr>
                    </tbody>
                </table>
             </div>
        </div>
    `
};

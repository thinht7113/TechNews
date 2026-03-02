const { ref, onMounted } = Vue;

export default {
    setup() {
        const logs = ref([]);
        const loading = ref(false);

        const fetchLogs = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/system/logs');
                if (res.ok) logs.value = await res.json();
            } finally { loading.value = false; }
        };

        const exportBackup = () => {
            window.location.href = '/api/system/backup';
        };

        onMounted(fetchLogs);
        return { logs, loading, exportBackup, fetchLogs };
    },
    template: `
        <div class="max-w-4xl mx-auto">
            <h2 class="text-2xl font-bold mb-6 text-black">Hệ thống & Bảo trì</h2>
            
            <div class="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
                <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                    <h3 class="font-bold text-lg text-black mb-4">Sao lưu dữ liệu</h3>
                    <p class="text-sm text-slate-500 mb-4">Tải xuống toàn bộ dữ liệu (Bài viết, Danh mục, Cấu hình, Users) dưới dạng JSON.</p>
                    <button @click="exportBackup" class="bg-primary text-white py-2 px-4 rounded hover:bg-opacity-90 flex items-center gap-2">
                        <i class="bi bi-cloud-download"></i> Tải xuống Backup
                    </button>
                </div>
                <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                     <h3 class="font-bold text-lg text-black mb-4">Trạng thái Server</h3>
                     <div class="space-y-2 text-sm">
                        <p class="flex justify-between"><span class="text-slate-500">OS:</span> <span class="font-medium text-black">Windows</span></p>
                        <p class="flex justify-between"><span class="text-slate-500">Framework:</span> <span class="font-medium text-black">.NET 9.0</span></p>
                        <p class="flex justify-between"><span class="text-slate-500">Database:</span> <span class="font-medium text-green-600">Connected</span></p>
                        <p class="flex justify-between"><span class="text-slate-500">Serilog:</span> <span class="font-medium text-green-600">Active</span></p>
                     </div>
                </div>
            </div>

            <div class="rounded-sm border border-stroke bg-white shadow-default p-6">
                <div class="flex justify-between items-center mb-4">
                    <h3 class="font-bold text-lg text-black">Nhật ký hệ thống (Logs)</h3>
                    <button @click="fetchLogs" class="text-primary hover:underline text-sm"><i class="bi bi-arrow-repeat"></i> Làm mới</button>
                </div>
                <div class="bg-gray-900 text-gray-200 p-4 rounded h-96 overflow-y-auto font-mono text-xs">
                    <div v-if="loading" class="text-center py-4">Đang tải logs...</div>
                    <div v-else-if="logs.length === 0" class="text-center py-4 text-gray-500">Không có logs nào.</div>
                    <div v-else v-for="(line, idx) in logs" :key="idx" class="whitespace-pre-wrap border-b border-gray-800 pb-1 mb-1 last:border-0 hover:bg-gray-800">{{ line }}</div>
                </div>
            </div>
        </div>
    `
};

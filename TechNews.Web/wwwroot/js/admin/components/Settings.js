const { ref, onMounted, computed } = Vue;

export default {
    setup() {
        const settings = ref([]);
        const loading = ref(true);
        const isSaving = ref(false);

        const fetchSettings = async () => {
            loading.value = true;
            try {
                const res = await fetch('/api/setting/getall');
                if (res.ok) settings.value = await res.json();
            } finally { loading.value = false; }
        };

        const groupedSettings = computed(() => {
            const groups = {};
            settings.value.forEach(s => {
                if (!groups[s.groupName]) groups[s.groupName] = [];
                groups[s.groupName].push(s);
            });
            return groups;
        });

        const saveSettings = async () => {
            isSaving.value = true;
            try {
                const res = await fetch('/api/setting/update', {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(settings.value)
                });
                if (res.ok) {
                    Swal.fire('Thành công', 'Cấu hình đã được lưu', 'success');
                } else {
                    Swal.fire('Lỗi', 'Không thể lưu cấu hình', 'error');
                }
            } catch (e) {
                Swal.fire('Lỗi', 'Lỗi server', 'error');
            } finally {
                isSaving.value = false;
            }
        };

        onMounted(fetchSettings);
        return { settings, loading, isSaving, groupedSettings, saveSettings };
    },
    template: `
        <div class="max-w-4xl mx-auto">
            <div class="flex justify-between items-center mb-6">
                 <h2 class="text-2xl font-bold text-black">Cấu hình hệ thống</h2>
                 <button @click="saveSettings" :disabled="isSaving" class="bg-primary text-white py-2 px-6 rounded font-medium hover:bg-opacity-90 disabled:opacity-70 flex items-center gap-2">
                     <i v-if="isSaving" class="bi bi-arrow-repeat animate-spin"></i>
                     <span>{{ isSaving ? 'Đang lưu...' : 'Lưu thay đổi' }}</span>
                 </button>
            </div>

            <div v-if="loading" class="text-center py-8">Đang tải cấu hình...</div>
            
            <div v-else class="grid grid-cols-1 gap-6">
                <div v-for="(group, name) in groupedSettings" :key="name" class="rounded-sm border border-stroke bg-white shadow-default p-6">
                    <h3 class="font-bold text-lg text-black mb-4 border-b border-stroke pb-2">{{ name || 'Khác' }}</h3>
                    <div class="grid grid-cols-1 gap-4">
                        <div v-for="setting in group" :key="setting.key">
                            <label class="mb-2 block text-sm font-medium text-black">{{ setting.displayName || setting.key }}</label>
                            
                            <textarea v-if="setting.type === 'textarea'" v-model="setting.value" rows="3" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none"></textarea>
                            
                            <input v-else-if="setting.type === 'password'" type="password" v-model="setting.value" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" placeholder="••••••••" />
                            
                            <input v-else-if="setting.type === 'image'" type="text" v-model="setting.value" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" placeholder="Nhập URL ảnh..." />
                            
                            <input v-else v-model="setting.value" type="text" class="w-full rounded border border-stroke px-4 py-2 focus:border-primary outline-none" />
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `
};